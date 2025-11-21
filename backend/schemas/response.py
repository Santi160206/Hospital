from pydantic import BaseModel
from typing import Optional
from .medicamento_v2 import MedicamentoOut


class MessageOut(BaseModel):
    message: str


class DeleteOut(BaseModel):
    deleted: bool
    dependencias: int


class ReactivateOut(BaseModel):
    reactivated: bool
    medicamento: Optional[MedicamentoOut]


class TokenOut(BaseModel):
    access_token: str
    token_type: str
