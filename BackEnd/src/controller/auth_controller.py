from datetime import datetime
from typing import Optional
from uuid import UUID, uuid4

from sqlalchemy.orm import Session

from src.auth.jwt_handler import create_access_token, get_password_hash, verify_password
from src.entities.usuario import Usuario
from src.schemas.auth import LoginRequest, UserCreate, UserResponse


"""
 CREAR USUARIO
"""


def create_user(db: Session, user: UserCreate) -> Usuario:
    """
    Crea un nuevo usuario en la base de datos.
    """

    """ Verificar si el nombre ya existe"""
    existing_user = get_user_by_username(db, user.Username)
    if existing_user:
        raise ValueError("El nombre de usuario ya existe")

    """ Verificar si el correo ya existe"""
    existing_email = get_user_by_email(db, user.Correo)
    if existing_email:
        raise ValueError("El correo electrónico ya está registrado")

    """Crear nuevo usuario"""
    Hashed_Password = get_password_hash(user.password)
    db_user = Usuario(
        Id_Usuario=uuid4(),
        Username=user.Username,
        Correo=user.Correo,
        Telefono=user.Telefono,
        Nombre=user.Nombre,
        Password_Hash=Hashed_Password,
        Rol=user.Rol,
        Activo=True,
        Fecha_creacion=datetime.utcnow(),
    )

    db.add(db_user)
    db.commit()
    db.refresh(db_user)
    return db_user


"""
 FUNCIONES AUXILIARES
"""


def get_user_by_username(db: Session, username: str) -> Optional[Usuario]:
    return db.query(Usuario).filter(Usuario.Username == username).first()


def get_user_by_email(db: Session, correo: str) -> Optional[Usuario]:
    return db.query(Usuario).filter(Usuario.Correo == correo).first()


def get_user_by_id(db: Session, user_id: UUID) -> Optional[Usuario]:
    return db.query(Usuario).filter(Usuario.Id_Usuario == user_id).first()


def authenticate_user(db: Session, username: str, password: str) -> Optional[Usuario]:
    user = get_user_by_username(db, username)
    if not user:
        return None
    if not verify_password(password, user.Password_Hash):
        return None
    if not user.Activo:
        return None
    return user


def create_user_token(user: Usuario) -> str:
    token_data = {
        "sub": user.Username,
        "user_id": str(user.Id_Usuario),
        "rol": user.Rol,
    }
    return create_access_token(data=token_data)
