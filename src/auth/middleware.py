"""
Authentication middleware for protecting endpoints.
"""

from fastapi import Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer
from sqlalchemy.orm import Session

from Database.conexion import get_db
from src.auth.jwt_handler import verify_token
from src.controller.auth_controller import get_user_by_id
from src.schemas.auth import UserResponse

oauth2_scheme = OAuth2PasswordBearer(tokenUrl="/auth/login")


def get_current_user(
    token: str = Depends(oauth2_scheme), db: Session = Depends(get_db)
) -> UserResponse:
    """Verificar token"""

    token_data = verify_token(token)
    user_id = token_data.get("user_id")

    if not user_id:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Token inválido o sin ID de usuario",
            headers={"WWW-Authenticate": "Bearer"},
        )

    """Buscar usuario en la base de datos"""

    user = get_user_by_id(db, user_id)
    if user is None:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Usuario no encontrado",
            headers={"WWW-Authenticate": "Bearer"},
        )

    if not user.Activo:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Usuario inactivo",
            headers={"WWW-Authenticate": "Bearer"},
        )

    """Retornar datos del usuario autenticado"""

    return UserResponse(
        Id_Usuario=user.Id_Usuario,
        Username=user.Username,
        Correo=user.Correo,
        Telefono=user.Telefono,
        Nombre=user.Nombre,
        Rol=user.Rol,
        Fecha_creacion=user.Fecha_creacion,
        Fecha_actualizacion=user.Fecha_actualizacion,
        Activo=user.Activo,
    )


def get_current_active_user(
    current_user: UserResponse = Depends(get_current_user),
) -> UserResponse:
    """Verifica si el usuario actual está activo"""
    if not current_user.Activo:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST, detail="Usuario inactivo"
        )
    return current_user


def require_role(required_role: str):
    """Dependency para validar roles"""

    def role_checker(
        current_user: UserResponse = Depends(get_current_active_user),
    ) -> UserResponse:
        if current_user.Rol != required_role and current_user.Rol != "admin":
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
                detail=f"Se requiere rol '{required_role}' o 'admin'",
            )
        return current_user

    return role_checker


"""Roles predefinidos"""

require_admin = require_role("admin")
require_bibliotecario = require_role("bibliotecario")
require_cliente = require_role("cliente")
