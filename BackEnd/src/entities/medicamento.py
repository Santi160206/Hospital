import uuid

from sqlalchemy import Column, DateTime, ForeignKey, String, Float
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.orm import relationship

from Database.conexion import Base


class Medicamento(Base):
    """
    Modelo de Medicamento
    """

    __tablename__ = "Medicamentos"

    Id_Medicamento = Column(
        UUID(as_uuid=True), primary_key=True, default=uuid.uuid4, index=True
    )
    Nombre_Comercial = Column(String, index=True)
    Principio_Activo = Column(String, index=True)
    Presentacion = Column(String, index=True)
    Unidad_Medida = Column(String, index=True)
    Precio_Unitario = Column(Float, index=True)


    """Campos de auditoría"""
    Id_usuario_creacion = Column(
        UUID(as_uuid=True), ForeignKey("Usuarios.Id_Usuario"), index=True
    )
    Id_usuario_actualizacion = Column(
        UUID(as_uuid=True), ForeignKey("Usuarios.Id_Usuario"), index=True
    )
    Fecha_creacion = Column(DateTime, index=True)
    Fecha_actualizacion = Column(DateTime, index=True)

  
