from pydantic import BaseModel, ConfigDict
from datetime import date
from typing import Optional
from uuid import UUID
from database.models import EstadoEnum


class LoteBase(BaseModel):
    Fecha_Vencimiento: date
    Cantidad_inicial: int
    Cantidad_disponible: int
    Estado: Optional[EstadoEnum] = EstadoEnum.ACTIVO
    # Id_reporte: UUID


class LoteCreate(LoteBase):
    pass


class LoteUpdate(BaseModel):
    Fecha_Vencimiento: Optional[date] = None
    Cantidad_disponible: Optional[int] = None
    Estado: Optional[EstadoEnum] = None


class LoteOut(LoteBase):
    id_lote: UUID

    # ✅ Pydantic v2 way
    model_config = ConfigDict(from_attributes=True)


class ReactivateLoteOut(BaseModel):
    reactivated: bool
    lote: Optional[LoteOut] = None

    # ✅ Pydantic v2 way
    model_config = ConfigDict(from_attributes=True)
