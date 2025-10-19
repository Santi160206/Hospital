from fastapi import APIRouter, Depends, HTTPException, status, Response
from sqlalchemy.orm import Session
from database.connection import get_db, engine
from database import models
from schemas.medicamento_v2 import MedicamentoCreate, MedicamentoOut, MedicamentoUpdate
from auth.security import get_current_user, require_admin
from typing import List
from sqlalchemy.exc import IntegrityError
from sqlalchemy import func
import unicodedata

router = APIRouter()


def normalize_text(s: str) -> str:
    # remove accents and lower
    nkfd_form = unicodedata.normalize('NFKD', s)
    only_ascii = ''.join([c for c in nkfd_form if not unicodedata.combining(c)])
    return only_ascii.lower().strip()


@router.post("/", response_model=MedicamentoOut)
def crear_medicamento(payload: MedicamentoCreate, response: Response, db: Session = Depends(get_db), user: dict = Depends(get_current_user)):
    # validar duplicados
    # include lote in the search key so different lots are allowed
    search_key = f"{normalize_text(payload.nombre)}|{normalize_text(payload.presentacion)}|{normalize_text(payload.fabricante)}|{normalize_text(payload.lote)}"
    existing = db.query(models.Medicamento).filter(models.Medicamento.search_key == search_key, models.Medicamento.is_deleted == False).first()
    if existing:
        raise HTTPException(status_code=status.HTTP_409_CONFLICT, detail={"error":"Medicamento duplicado", "existing_id": str(existing.id)})

    # business rule: fecha_vencimiento must not be in the past
    from datetime import date, timedelta
    if payload.fecha_vencimiento < date.today():
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail='Fecha inválida: la fecha de vencimiento no puede ser anterior a hoy.')
    # if vencimiento within 30 days, add warning header
    if payload.fecha_vencimiento <= date.today() + timedelta(days=30):
        response.headers['X-Warning'] = 'Este medicamento está próximo a vencer, ¿Continuar?'

    # crear
    m = models.Medicamento(
        nombre=payload.nombre,
        fabricante=payload.fabricante,
        presentacion=payload.presentacion,
        lote=payload.lote,
        precio=payload.precio if hasattr(payload, 'precio') else None,
        fecha_vencimiento=payload.fecha_vencimiento,
        stock=payload.stock,
        minimo_stock=payload.minimo_stock,
        search_key=search_key,
        created_by=user.get('sub') if user else None
    )
    db.add(m)
    try:
        db.commit()
        db.refresh(m)
    except IntegrityError:
        db.rollback()
        raise HTTPException(status_code=500, detail="Error al crear medicamento")

    return m


@router.get("/", response_model=List[MedicamentoOut])
def listar_medicamentos(db: Session = Depends(get_db), user: dict = Depends(get_current_user)):
    meds = db.query(models.Medicamento).filter(models.Medicamento.is_deleted == False).all()
    return meds


@router.get("/{med_id}", response_model=MedicamentoOut)
def detalle_medicamento(med_id: str, db: Session = Depends(get_db), user: dict = Depends(get_current_user)):
    m = db.query(models.Medicamento).filter(models.Medicamento.id == med_id).first()
    if not m:
        raise HTTPException(status_code=404, detail="Medicamento no encontrado")
    return m


@router.put("/{med_id}")
def actualizar_medicamento(med_id: str, payload: MedicamentoUpdate, response: Response, db: Session = Depends(get_db), user: dict = Depends(get_current_user)):
    m = db.query(models.Medicamento).filter(models.Medicamento.id == med_id).first()
    if not m:
        raise HTTPException(status_code=404, detail="Medicamento no encontrado")

    # validar y preparar cambios
    # Before applying changes, compute potential new search_key and check duplicates
    data = payload.dict(exclude_unset=True)
    # business rule: if fecha_vencimiento provided, validate
    from datetime import date, timedelta
    if 'fecha_vencimiento' in data:
        fv = data.get('fecha_vencimiento')
        if fv < date.today():
            raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail='Fecha inválida: la fecha de vencimiento no puede ser anterior a hoy.')
        if fv <= date.today() + timedelta(days=30):
            response.headers['X-Warning'] = 'Este medicamento está próximo a vencer, ¿Continuar?'
    new_nombre = data.get('nombre', m.nombre)
    new_presentacion = data.get('presentacion', m.presentacion)
    new_fabricante = data.get('fabricante', m.fabricante)
    # include lote when computing potential new search_key
    new_lote = data.get('lote', m.lote)
    new_search_key = f"{normalize_text(new_nombre)}|{normalize_text(new_presentacion)}|{normalize_text(new_fabricante)}|{normalize_text(new_lote)}"
    dup = db.query(models.Medicamento).filter(models.Medicamento.search_key == new_search_key, models.Medicamento.is_deleted == False, models.Medicamento.id != m.id).first()
    if dup:
        raise HTTPException(status_code=status.HTTP_409_CONFLICT, detail={"error":"Medicamento duplicado con los nuevos valores", "existing_id": str(dup.id)})

    changes = []
    for field, value in payload.dict(exclude_unset=True).items():
        old = getattr(m, field)
        if value != old:
            changes.append((field, old, value))
            setattr(m, field, value)

    if len(changes) == 0:
        return {"message": "No se detectaron cambios, no se actualizó."}

    # aplicar y crear audit_log
    try:
        db.add(m)
        db.commit()
        db.refresh(m)
        # crear audit_log por cada cambio
        for field, old, new in changes:
            al = models.AuditLog(entidad='medicamentos', entidad_id=m.id, usuario_id=user.get('sub') if user else None, accion='UPDATE', campo=field, valor_anterior=str(old), valor_nuevo=str(new))
            db.add(al)
        db.commit()
    except Exception as e:
        db.rollback()
        raise HTTPException(status_code=500, detail=str(e))

    return {"message": "Actualización registrada correctamente."}


@router.delete("/{med_id}")
def eliminar_medicamento(med_id: str, db: Session = Depends(get_db), user: dict = Depends(require_admin)):
    m = db.query(models.Medicamento).filter(models.Medicamento.id == med_id).first()
    if not m:
        raise HTTPException(status_code=404, detail="Medicamento no encontrado")

    # contar movimientos
    count = db.query(func.count(models.Movimiento.id)).filter(models.Movimiento.medicamento_id == m.id).scalar()

    # soft-delete
    m.is_deleted = True
    m.estado = models.EstadoEnum.INACTIVO
    m.deleted_by = user.get('sub')
    m.deleted_at = func.now()

    try:
        db.add(m)
        db.commit()
        # crear audit log
        al = models.AuditLog(entidad='medicamentos', entidad_id=m.id, usuario_id=user.get('sub'), accion='DELETE_SOFT', metadatos={"dependencias": count})
        db.add(al)
        db.commit()
    except Exception as e:
        db.rollback()
        raise HTTPException(status_code=500, detail=str(e))

    msg = "Registro eliminado exitosamente"
    if count and count > 0:
        msg = f"Registro eliminado exitosamente. Este medicamento tiene {count} movimientos registrados"
    return {"message": msg}
