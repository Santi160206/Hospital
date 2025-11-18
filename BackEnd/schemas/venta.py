from pydantic import BaseModel, Field
from uuid import UUID
from typing import List, Optional
from datetime import datetime
from enum import Enum


class DetalleVentaCreate(BaseModel):
    medicamento_id: UUID
    cantidad: int = Field(..., gt=0, description="Cantidad debe ser mayor a 0")
    precio_unitario: Optional[float] = Field(None, gt=0, description="Precio unitario debe ser mayor a 0")


class VentaCreate(BaseModel):
    detalles: List[DetalleVentaCreate] = Field(..., min_items=1, description="Debe haber al menos un medicamento en la venta")
    cliente: Optional[str] = Field(None, max_length=200)
    notas: Optional[str] = Field(None, max_length=500)


class DetalleVentaOut(BaseModel):
    id: UUID
    medicamento_id: UUID
    medicamento_nombre: str
    cantidad: int
    precio_unitario: float
    subtotal: float

    model_config = {"from_attributes": True}


class VentaOut(BaseModel):
    id: UUID
    usuario_id: UUID
    usuario_nombre: str
    total: float
    fecha: datetime
    cliente: Optional[str]
    notas: Optional[str]
    detalles: List[DetalleVentaOut]

    model_config = {"from_attributes": True}


# Para reportes
class ReporteVentaPeriodo(BaseModel):
    fecha_inicio: datetime
    fecha_fin: datetime


class VentasPorPeriodoOut(BaseModel):
    periodo: str
    total_ventas: float
    cantidad_ventas: int
    ventas_promedio: float


class ProductoMasVendidoOut(BaseModel):
    medicamento_id: UUID
    medicamento_nombre: str
    cantidad_vendida: int
    total_ventas: float


class ReporteCompletoOut(BaseModel):
    periodo: str
    total_ventas: float
    cantidad_ventas: int
    productos_mas_vendidos: List[ProductoMasVendidoOut]
    ventas_por_dia: List[VentasPorPeriodoOut]