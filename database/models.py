import uuid
from sqlalchemy import Column, String, Integer, Date, DateTime, Boolean, Enum, ForeignKey, func, JSON, Numeric
from sqlalchemy.dialects.mssql import UNIQUEIDENTIFIER
from sqlalchemy.orm import relationship
from .connection import Base
import enum
from sqlalchemy import UniqueConstraint


class EstadoEnum(enum.Enum):
    ACTIVO = "ACTIVO"
    INACTIVO = "INACTIVO"
    PENDIENTE_SYNC = "PENDIENTE_SYNC"


class Medicamento(Base):
    __tablename__ = 'medicamentos'

    id = Column(UNIQUEIDENTIFIER, primary_key=True, default=uuid.uuid4)
    nombre = Column(String(200), nullable=False)
    fabricante = Column(String(200), nullable=False)
    presentacion = Column(String(200), nullable=False)
    lote = Column(String(100), nullable=False)
    fecha_vencimiento = Column(Date, nullable=False)
    stock = Column(Integer, nullable=False, default=0)
    minimo_stock = Column(Integer, nullable=True)
    precio = Column(Numeric(12, 2), nullable=False, server_default="0")
    estado = Column(Enum(EstadoEnum), default=EstadoEnum.ACTIVO, nullable=False)
    is_deleted = Column(Boolean, default=False)
    created_by = Column(String(100), nullable=True)
    created_at = Column(DateTime, server_default=func.now())
    updated_by = Column(String(100), nullable=True)
    updated_at = Column(DateTime, onupdate=func.now(), server_default=func.now())
    deleted_at = Column(DateTime, nullable=True)
    deleted_by = Column(String(100), nullable=True)
    # make search_key unique to enforce DB-level uniqueness across restarts/processes
    search_key = Column(String(400), nullable=False, unique=True)

    movimientos = relationship('Movimiento', back_populates='medicamento')


class MovimientoTipoEnum(enum.Enum):
    ENTRADA = 'ENTRADA'
    SALIDA = 'SALIDA'


class Movimiento(Base):
    __tablename__ = 'movimientos'

    id = Column(UNIQUEIDENTIFIER, primary_key=True, default=uuid.uuid4)
    medicamento_id = Column(UNIQUEIDENTIFIER, ForeignKey('medicamentos.id'))
    tipo = Column(Enum(MovimientoTipoEnum), nullable=False)
    cantidad = Column(Integer, nullable=False)
    usuario_id = Column(String(100), nullable=True)
    motivo = Column(String(200), nullable=True)
    fecha = Column(DateTime, server_default=func.now())

    medicamento = relationship('Medicamento', back_populates='movimientos')


class AuditLog(Base):
    __tablename__ = 'audit_logs'

    id = Column(UNIQUEIDENTIFIER, primary_key=True, default=uuid.uuid4)
    entidad = Column(String(100), nullable=False)
    entidad_id = Column(UNIQUEIDENTIFIER, nullable=False)
    usuario_id = Column(String(100), nullable=True)
    accion = Column(String(50), nullable=False)
    campo = Column(String(200), nullable=True)
    valor_anterior = Column(String(1000), nullable=True)
    valor_nuevo = Column(String(1000), nullable=True)
    metadatos = Column(JSON, nullable=True)
    timestamp = Column(DateTime, server_default=func.now())


class UserRoleEnum(enum.Enum):
    ADMIN = 'admin'
    FARMACEUTICO = 'farmaceutico'
    COMPRAS = 'compras'


class User(Base):
    __tablename__ = 'users'
    __table_args__ = (
        UniqueConstraint('username', name='uq_users_username'),
        UniqueConstraint('email', name='uq_users_email'),
    )

    id = Column(UNIQUEIDENTIFIER, primary_key=True, default=uuid.uuid4)
    username = Column(String(100), nullable=False)
    full_name = Column(String(200), nullable=True)
    email = Column(String(200), nullable=False)
    hashed_password = Column(String(256), nullable=False)
    role = Column(Enum(UserRoleEnum), default=UserRoleEnum.FARMACEUTICO, nullable=False)
    is_active = Column(Boolean, default=True)
    created_at = Column(DateTime, server_default=func.now())

