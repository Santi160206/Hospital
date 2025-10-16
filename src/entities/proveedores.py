import uuid
from sqlalchemy import Column, DateTime, ForeignKey, String
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.orm import relationship
from Database.conexion import Base


class Proveedor(Base):
    """
    Modelo de Proveedor
    """

    __tablename__ = "Proveedores"

    Id_Proveedor = Column(
        UUID(as_uuid=True), primary_key=True, default=uuid.uuid4, index=True
    )
    Nombre = Column(String(100), nullable=False, index=True)
    Direccion = Column(String(200))
    Producto_Ofrecido = Column(String(150))

    # 🔹 Clave foránea llamada 'Laboratorio' que referencia Medicamentos
    Laboratorio = Column(
        UUID(as_uuid=True),
        ForeignKey("Medicamentos.Id_Medicamento"),
        nullable=True,
        index=True,
    )

    """Campos de auditoría"""
    Id_usuario_creacion = Column(
        UUID(as_uuid=True), ForeignKey("Usuarios.Id_Usuario"), index=True
    )
    Id_usuario_actualizacion = Column(
        UUID(as_uuid=True), ForeignKey("Usuarios.Id_Usuario"), index=True
    )
    Fecha_creacion = Column(DateTime, index=True)
    Fecha_actualizacion = Column(DateTime, index=True)

    # Relación con Medicamentos
    medicamento = relationship("Medicamento", back_populates="proveedores")

