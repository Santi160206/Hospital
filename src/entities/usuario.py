import uuid
from datetime import datetime

from sqlalchemy import Column, DateTime, ForeignKey, String, Boolean
from sqlalchemy.dialects.postgresql import UUID


from Database.conexion import Base


class Usuario(Base):
    """
    Modelo de usuario
    """

    __tablename__ = "Usuarios"

    Id_Usuario = Column(
        UUID(as_uuid=True), primary_key=True, default=uuid.uuid4, index=True
    )
    Username = Column(String, unique=True, index=True, nullable=False)
    Correo = Column(String, unique=True, index=True, nullable=False)
    Telefono = Column(String, nullable=True)
    Nombre = Column(String, nullable=False)
    Password_Hash = Column(String, nullable=False)
    Rol = Column(String, default="Usuario", nullable=False)
    Activo = Column(Boolean, default=True, nullable=False)

    """Campos de auditoría"""
    Id_usuario_creacion = Column(
        UUID(as_uuid=True), ForeignKey("Usuarios.Id_Usuario"), index=True
    )
    Id_usuario_actualizacion = Column(
        UUID(as_uuid=True), ForeignKey("Usuarios.Id_Usuario"), index=True
    )
    Fecha_creacion = Column(DateTime, default=datetime.utcnow, index=True)
    Fecha_actualizacion = Column(DateTime, index=True, nullable=True)
