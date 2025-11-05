from fastapi import APIRouter, Depends, HTTPException, status, Response
from sqlalchemy.orm import Session
from sqlalchemy.exc import IntegrityError
from typing import List, Optional
from datetime import date, timedelta

from database.connection import get_db
from database import models
from auth.security import get_current_user, require_admin, is_admin
from schemas.lote import LoteCreate, LoteOut, LoteUpdate
from schemas.response import MessageOut, DeleteOut, ReactivateOut
from services.lote_service import LoteService
from repositories.lote_repo import LoteRepository

router = APIRouter(prefix="/lotes", tags=["Lotes"])


def get_lote_service(db: Session = Depends(get_db)) -> LoteService:
    return LoteService(db)


# ========================
#        CREATE
# ========================
@router.post("/", response_model=LoteOut, status_code=status.HTTP_201_CREATED)
def crear_lote(
    payload: LoteCreate,
    response: Response,
    service: LoteService = Depends(get_lote_service),
    db: Session = Depends(get_db),
    user: dict = Depends(get_current_user)
):
    """
    Crea un nuevo lote para un medicamento.
    Valida duplicados por `id_reporte` (o según reglas de negocio).
    """
    repo = LoteRepository(db)

    # Validar fecha de vencimiento
    if payload.Fecha_Vencimiento < date.today():
        raise HTTPException(status_code=400, detail="La fecha de vencimiento no puede ser anterior a hoy.")
    
    if payload.Fecha_Vencimiento <= date.today() + timedelta(days=30):
        response.headers["X-Warning"] = "El lote está próximo a vencer."

    # Validar duplicado (ejemplo: un lote con el mismo Id_reporte)
    existing = repo.find_by_reporte(payload.Id_reporte)
    if existing:
        raise HTTPException(status_code=409, detail="Ya existe un lote asociado a este reporte de compra.")

    try:
        lote = service.create_lote(payload.model_dump(), user.get("sub") if user else None)
    except IntegrityError:
        raise HTTPException(status_code=409, detail="Error de integridad al crear el lote.")

    return lote


# ========================
#        LISTAR
# ========================
@router.get("/", response_model=List[LoteOut])
def listar_lotes(
    db: Session = Depends(get_db),
    user: dict = Depends(get_current_user),
    estado: Optional[str] = None,
    medicamento_id: Optional[str] = None,
    vencidos: Optional[bool] = None,
    limit: int = 100
):
    """
    Lista todos los lotes, con filtros opcionales.
    - estado: ACTIVO o INACTIVO
    - medicamento_id: filtra por medicamento
    - vencidos: True para mostrar solo los ya vencidos
    """
    q = db.query(models.Lote).filter(models.Lote.Estado != models.EstadoEnum.ELIMINADO)

    # Filtro por estado
    if estado:
        estado_upper = estado.upper()
        try:
            q = q.filter(models.Lote.Estado == models.EstadoEnum(estado_upper))
        except ValueError:
            raise HTTPException(status_code=400, detail="Estado inválido. Use: ACTIVO o INACTIVO")
    else:
        # Por defecto, usuarios no admin solo ven activos
        if not is_admin(user):
            q = q.filter(models.Lote.Estado == models.EstadoEnum.ACTIVO)

    # Filtro por medicamento
    if medicamento_id:
        q = q.filter(models.Lote.Id_reporte == medicamento_id)

    # Filtro por vencimiento
    if vencidos:
        q = q.filter(models.Lote.Fecha_Vencimiento < date.today())

    q = q.order_by(models.Lote.Fecha_Vencimiento.asc())

    return q.limit(limit).all()


# ========================
#        DETALLE
# ========================
@router.get("/{lote_id}", response_model=LoteOut)
def obtener_lote(
    lote_id: str,
    db: Session = Depends(get_db),
    user: dict = Depends(get_current_user)
):
    lote = db.query(models.Lote).filter(models.Lote.id_lote == lote_id).first()
    if not lote:
        raise HTTPException(status_code=404, detail="Lote no encontrado")
    return lote


# ========================
#        UPDATE
# ========================
@router.put("/{lote_id}", response_model=LoteOut)
def actualizar_lote(
    lote_id: str,
    payload: LoteUpdate,
    response: Response,
    service: LoteService = Depends(get_lote_service),
    db: Session = Depends(get_db),
    user: dict = Depends(get_current_user)
):
    lote = db.query(models.Lote).filter(models.Lote.id_lote == lote_id).first()
    if not lote:
        raise HTTPException(status_code=404, detail="Lote no encontrado")

    data = payload.model_dump(exclude_unset=True)

    # Validar fecha
    if "Fecha_Vencimiento" in data:
        fv = data["Fecha_Vencimiento"]
        if fv < date.today():
            raise HTTPException(status_code=400, detail="La fecha de vencimiento no puede ser anterior a hoy.")
        if fv <= date.today() + timedelta(days=30):
            response.headers["X-Warning"] = "El lote está próximo a vencer."

    updated = service.update_lote(lote_id, data, user.get("sub") if user else None)
    if not updated:
        raise HTTPException(status_code=404, detail="Lote no encontrado o sin cambios")

    return updated


# ========================
#        DELETE (soft)
# ========================
@router.delete("/{lote_id}", response_model=DeleteOut)
def eliminar_lote(
    lote_id: str,
    service: LoteService = Depends(get_lote_service),
    db: Session = Depends(get_db),
    user: dict = Depends(require_admin)
):
    res = service.delete_lote(lote_id, user.get("sub"))
    if res is None:
        raise HTTPException(status_code=404, detail="Lote no encontrado")
    return DeleteOut(deleted=True, dependencias=0)


# ========================
#      REACTIVAR
# ========================
@router.post("/{lote_id}/reactivar", response_model=ReactivateOut)
def reactivar_lote(
    lote_id: str,
    service: LoteService = Depends(get_lote_service),
    user: dict = Depends(require_admin)
):
    res = service.reactivar_lote(lote_id, user.get("sub"))
    if res is None:
        raise HTTPException(status_code=404, detail="Lote no encontrado")
    if res.get("reactivated") is False:
        raise HTTPException(status_code=400, detail="No se puede reactivar el lote (vencido o inactivo permanente)")
    return ReactivateOut(reactivated=True, lote=res.get("lote"))
