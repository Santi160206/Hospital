from datetime import date, datetime
from typing import Optional
from uuid import UUID
from pydantic import BaseModel


class MedicamentoBase(BaseModel):

    # Id_Medicamento:Optional[str] = None
    Nombre_Comercial : str
    Principio_Activo : str
    Presentacion : str
    Unidad_Medida : str
    Precio_Unitario : float


class MedicamentoCreate(MedicamentoBase):

    pass


class MedicamentoResponse(MedicamentoBase):

    Id_usuario_creacion: Optional[UUID] = None
    Id_usuario_actualizacion: Optional[UUID] = None
    Fecha_creacion: Optional[datetime] = None
    Fecha_actualizacion: Optional[datetime] = None

    class Config:
        from_attributes = True
