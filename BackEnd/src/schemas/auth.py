"""
Pydantic schemas for authentication.
"""

from datetime import datetime
from typing import Optional
from uuid import UUID

from pydantic import BaseModel, EmailStr


class UserBase(BaseModel):
    """Base schema for User with common fields."""

    Username: str
    Correo: EmailStr
    Telefono: str
    Nombre: str
    Rol: str = "Usuario"


class UserCreate(UserBase):
    """Schema for creating a new User."""

    password: str


class UserResponse(UserBase):
    """Schema for User response."""

    Id_Usuario: UUID
    Fecha_creacion: Optional[datetime] = None
    Fecha_actualizacion: Optional[datetime] = None
    Activo: bool = True

    class Config:
        from_attributes = True


class LoginRequest(BaseModel):
    """Schema for login request."""

    Username: str
    password: str


class LoginResponse(BaseModel):
    """Schema for login response."""

    access_token: str
    token_type: str = "bearer"
    user: UserResponse


class TokenData(BaseModel):
    """Schema for token data."""

    Username: Optional[str] = None
    user_id: Optional[UUID] = None
    Rol: Optional[str] = None
