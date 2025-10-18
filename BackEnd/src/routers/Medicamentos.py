from fastapi import APIRouter, Depends, HTTPException
from fastapi.responses import JSONResponse
from sqlalchemy.orm import Session
from uuid import UUID

import src.controller.Medicamento as medicamento_controller
from Database.conexion import get_db
from src.schemas.medicamento import MedicamentoCreate, MedicamentoResponse
from src.auth.middleware import get_current_user

router = APIRouter(
    prefix="/medicamentos", tags=["Medicamentos"], dependencies=[Depends(get_current_user)]
)

"""
RUTAS PARA MEDICAMENTOS
"""


@router.post("/medicamentos/", response_model=MedicamentoResponse, tags=["Medicamentos"])
def create_medicamento(medicamento: MedicamentoCreate, db: Session = Depends(get_db)) -> JSONResponse:
    medicamento_creado = medicamento_controller.create_medicamento(db=db, medicamento=medicamento)
    if medicamento_creado is None:
        raise HTTPException(status_code=400, detail="Error al crear el Medicamento")
    else:
        return JSONResponse(
            status_code=201,
            content={
                "detail": "Medicamento creado correctamente",
                "Cuerpo de la respuesta": {
                    # "Id_Medicamento": str(medicamento.Id_Medicamento),
                    "Nombre_Comercial": medicamento.Nombre_Comercial,
                    "Principio_Activo": medicamento.Principio_Activo,
                    "Presentacion": medicamento.Presentacion,
                    "Unidad_Medida": medicamento.Unidad_Medida,
                    "Precio_Unitario": medicamento.Precio_Unitario,
                },
            },
        )


@router.get("/medicamentos/", response_model=list[MedicamentoResponse], tags=["Medicamentos"])
def read_all_medicamentos(db: Session = Depends(get_db)):
    medicamentos = medicamento_controller.get_medicamentos(db)
    if not medicamentos:
        raise HTTPException(status_code=404, detail="No hay Medicamentos registrados")
    return medicamentos


@router.get("/medicamentos/{medicamento_id}", response_model=MedicamentoResponse, tags=["Medicamentos"])
def read_one_medicamento(medicamento_id: UUID, db: Session = Depends(get_db)):
    medicamento = medicamento_controller.get_medicamento(db, medicamento_id=medicamento_id)
    if medicamento is None:
        raise HTTPException(status_code=404, detail="Medicamento no encontrado")
    else:
        return JSONResponse(
            status_code=200,
            content={
                "detail": "Medicamento encontrado",
                "data": {
                    "ID del Medicamento": str(medicamento.Id_Medicamento),
                    "Nombre_Comercial": medicamento.Nombre_Comercial,
                    "Principio_Activo": medicamento.Principio_Activo,
                    "Presentacion": medicamento.Presentacion,
                    "Unidad_Medida": medicamento.Unidad_Medida,
                    "Precio_Unitario": medicamento.Precio_Unitario,
                },
            },
        )


@router.put("/medicamentos/{medicamento_id}", response_model=MedicamentoResponse, tags=["Medicamentos"])
def update_medicamento(medicamento_id: UUID, medicamento: MedicamentoCreate, db: Session = Depends(get_db)):
    medicamento_actualizado = medicamento_controller.update_medicamento(
        db, medicamento_id=medicamento_id, medicamento=medicamento
    )
    if medicamento_actualizado is None:
        raise HTTPException(status_code=404, detail="Medicamento no encontrado")
    else:
        return JSONResponse(
            status_code=201,
            content={
                "detail": "Medicamento actualizado correctamente",
                "data": {
                    "ID del Medicamento": str(medicamento.Id_Medicamento),
                    "Nombre_Comercial": medicamento.Nombre_Comercial,
                    "Principio_Activo": medicamento.Principio_Activo,
                    "Presentacion": medicamento.Presentacion,
                    "Unidad_Medida": medicamento.Unidad_Medida,
                    "Precio_Unitario": medicamento.Precio_Unitario,
                },
            },
        )


@router.delete("/medicamentos/{medicamento_id}", response_model=MedicamentoResponse, tags=["Medicamentos"])
def delete_medicamento(medicamento_id: UUID, db: Session = Depends(get_db)):
    medicamento_eliminado = medicamento_controller.delete_medicamento(db, medicamento_id=medicamento_id)
    if medicamento_eliminado is None:
        raise HTTPException(status_code=404, detail="Medicamento no encontrado")
    else:
        return JSONResponse(
            status_code=200,
            content={
                "detail": "Medicamento eliminado correctamente",
                "data": {
                    "ID del Medicamento": str(medicamento_eliminado.Id_Medicamento),
                    "Nombre_Comercial": medicamento_eliminado.Nombre_Comercial,
                    "Principio_Activo": medicamento_eliminado.Principio_Activo,
                    "Presentacion": medicamento_eliminado.Presentacion,
                    "Unidad_Medida": medicamento_eliminado.Unidad_Medida,
                    "Precio_Unitario": medicamento_eliminado.Precio_Unitario,
                },
            },
        )
