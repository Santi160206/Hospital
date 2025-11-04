from pydantic import BaseModel, Field
from typing import Optional
from datetime import datetime
from uuid import UUID

class ProveedorBase(BaseModel):
    nombre: str = Field(..., max_length=200)
    direccion: Optional[str] = Field(None, max_length=300)
    producto_ofrecido: Optional[str] = Field(None, max_length=200)

class ProveedorCreate(ProveedorBase):
    pass

class ProveedorUpdate(BaseModel):
    nombre: Optional[str]
    direccion: Optional[str]
    producto_ofrecido: Optional[str]

class ProveedorOut(BaseModel):
    id: UUID
    nombre: str
    direccion: Optional[str]
    producto_ofrecido: Optional[str]
    estado: Optional[str]
    is_deleted: Optional[bool]
    created_by: Optional[str]
    created_at: Optional[datetime]
    updated_by: Optional[str]
    updated_at: Optional[datetime]

    model_config = {"from_attributes": True}
