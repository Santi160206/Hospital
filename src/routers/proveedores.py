from fastapi import APIRouter, Depends, HTTPException
from fastapi.responses import JSONResponse
from sqlalchemy.orm import Session
from uuid import UUID

import src.controller.Proveedor as proveedor_controller
from Database.conexion import get_db
from src.schemas.proveedor import ProveedorCreate, ProveedorResponse
from src.auth.middleware import get_current_user

router = APIRouter(
    prefix="/proveedores",
    tags=["Proveedores"],
    dependencies=[Depends(get_current_user)]
)

"""
RUTAS PARA PROVEEDORES
"""

@router.post("/", response_model=ProveedorResponse, tags=["Proveedores"])
def create_proveedor(proveedor: ProveedorCreate, db: Session = Depends(get_db)) -> JSONResponse:
    proveedor_creado = proveedor_controller.create_proveedor(db=db, proveedor=proveedor)
    if proveedor_creado is None:
        raise HTTPException(status_code=400, detail="Error al crear el Proveedor")
    else:
        return JSONResponse(
            status_code=201,
            content={
                "detail": "Proveedor creado correctamente",
                "Cuerpo de la respuesta": {
                    "Id_Proveedor": str(proveedor_creado.Id_Proveedor),
                    "Nombre": proveedor_creado.Nombre,
                    "Direccion": proveedor_creado.Direccion,
                    "Producto_Ofrecido": proveedor_creado.Producto_Ofrecido,
                    "Laboratorio": str(proveedor_creado.Laboratorio) if proveedor_creado.Laboratorio else None,
                },
            },
        )


@router.get("/", response_model=list[ProveedorResponse], tags=["Proveedores"])
def read_all_proveedores(db: Session = Depends(get_db)):
    proveedores = proveedor_controller.get_proveedores(db)
    if not proveedores:
        raise HTTPException(status_code=404, detail="No hay Proveedores registrados")
    return proveedores


@router.get("/{proveedor_id}", response_model=ProveedorResponse, tags=["Proveedores"])
def read_one_proveedor(proveedor_id: UUID, db: Session = Depends(get_db)):
    proveedor = proveedor_controller.get_proveedor(db, proveedor_id=proveedor_id)
    if proveedor is None:
        raise HTTPException(status_code=404, detail="Proveedor no encontrado")
    else:
        return JSONResponse(
            status_code=200,
            content={
                "detail": "Proveedor encontrado",
                "data": {
                    "Id_Proveedor": str(proveedor.Id_Proveedor),
                    "Nombre": proveedor.Nombre,
                    "Direccion": proveedor.Direccion,
                    "Producto_Ofrecido": proveedor.Producto_Ofrecido,
                    "Laboratorio": str(proveedor.Laboratorio) if proveedor.Laboratorio else None,
                },
            },
        )


@router.put("/{proveedor_id}", response_model=ProveedorResponse, tags=["Proveedores"])
def update_proveedor(proveedor_id: UUID, proveedor: ProveedorCreate, db: Session = Depends(get_db)):
    proveedor_actualizado = proveedor_controller.update_proveedor(
        db, proveedor_id=proveedor_id, proveedor=proveedor
    )
    if proveedor_actualizado is None:
        raise HTTPException(status_code=404, detail="Proveedor no encontrado")
    else:
        return JSONResponse(
            status_code=201,
            content={
                "detail": "Proveedor actualizado correctamente",
                "data": {
                    "Id_Proveedor": str(proveedor_actualizado.Id_Proveedor),
                    "Nombre": proveedor_actualizado.Nombre,
                    "Direccion": proveedor_actualizado.Direccion,
                    "Producto_Ofrecido": proveedor_actualizado.Producto_Ofrecido,
                    "Laboratorio": str(proveedor_actualizado.Laboratorio) if proveedor_actualizado.Laboratorio else None,
                },
            },
        )


@router.delete("/{proveedor_id}", response_model=ProveedorResponse, tags=["Proveedores"])
def delete_proveedor(proveedor_id: UUID, db: Session = Depends(get_db)):
    proveedor_eliminado = proveedor_controller.delete_proveedor(db, proveedor_id=proveedor_id)
    if proveedor_eliminado is None:
        raise HTTPException(status_code=404, detail="Proveedor no encontrado")
    else:
        return JSONResponse(
            status_code=200,
            content={
                "detail": "Proveedor eliminado correctamente",
                "data": {
                    "Id_Proveedor": str(proveedor_eliminado.Id_Proveedor),
                    "Nombre": proveedor_eliminado.Nombre,
                    "Direccion": proveedor_eliminado.Direccion,
                    "Producto_Ofrecido": proveedor_eliminado.Producto_Ofrecido,
                    "Laboratorio": str(proveedor_eliminado.Laboratorio) if proveedor_eliminado.Laboratorio else None,
                },
            },
        )
