from pydantic import BaseModel
from typing import Optional

class ProveedorShortOut(BaseModel):
    id: str
    nombre: str
    direccion: Optional[str]
    producto_ofrecido: Optional[str]

    model_config = {"from_attributes": True}
