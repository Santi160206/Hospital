from pydantic import BaseModel
from typing import Optional

class ProveedorBase(BaseModel):
    nombre: str
    direccion: Optional[str] = None
    producto_ofrecido: Optional[str] = None
    laboratorio: Optional[int] = None  # id del medicamento (FK)

class ProveedorCreate(ProveedorBase):
    pass

class ProveedorUpdate(BaseModel):
    nombre: Optional[str] = None
    direccion: Optional[str] = None
    producto_ofrecido: Optional[str] = None
    laboratorio: Optional[int] = None

class ProveedorResponse(ProveedorBase):
    id: int

    class Config:
        orm_mode = True
