import uuid
from sqlalchemy import Column, String, Integer, Date, DateTime, Boolean, Enum, ForeignKey, func, JSON, Numeric
from sqlalchemy.orm import relationship
from .connection import Base
import enum
from sqlalchemy import UniqueConstraint
from sqlalchemy.types import TypeDecorator, CHAR
from sqlalchemy.dialects.mssql import UNIQUEIDENTIFIER


class GUID(TypeDecorator):
    impl = CHAR
    cache_ok = True

    def load_dialect_impl(self, dialect):
        if dialect.name == 'mssql':
            return dialect.type_descriptor(UNIQUEIDENTIFIER())
        else:
            return dialect.type_descriptor(CHAR(36))

    def process_bind_param(self, value, dialect):
        if value is None:
            return value
        if isinstance(value, bytes):
            try:
                value = value.decode()
            except Exception:
                value = str(value)
        if not isinstance(value, str):
            return str(value)
        return value

    def process_result_value(self, value, dialect):
        if value is None:
            return None
        try:
            return str(value)
        except Exception:
            return value


class EstadoEnum(enum.Enum):
    ACTIVO = "ACTIVO"
    INACTIVO = "INACTIVO"
    PENDIENTE_SYNC = "PENDIENTE_SYNC"


class Medicamento(Base):
    __tablename__ = 'medicamentos'

    id = Column(GUID(), primary_key=True, default=lambda: str(uuid.uuid4()))
    nombre = Column(String(200), nullable=False)
    fabricante = Column(String(200), nullable=False)
    presentacion = Column(String(200), nullable=False)
    lote = Column(String(100), nullable=False)
    fecha_vencimiento = Column(Date, nullable=False)
    stock = Column(Integer, nullable=False, default=0)
    minimo_stock = Column(Integer, nullable=True)
    precio = Column(Numeric(12, 2), nullable=False, server_default="0")
    principio_activo = Column(String(300), nullable=True)
    principio_activo_search = Column(String(300), nullable=True, index=True)
    estado = Column(Enum(EstadoEnum), default=EstadoEnum.ACTIVO, nullable=False)
    is_deleted = Column(Boolean, default=False)
    created_by = Column(String(100), nullable=True)
    created_at = Column(DateTime, server_default=func.now())
    updated_by = Column(String(100), nullable=True)
    updated_at = Column(DateTime, onupdate=func.now(), server_default=func.now())
    deleted_at = Column(DateTime, nullable=True)
    deleted_by = Column(String(100), nullable=True)
    search_key = Column(String(400), nullable=False, unique=True)

    movimientos = relationship('Movimiento', back_populates='medicamento')


class MovimientoTipoEnum(enum.Enum):
    ENTRADA = 'ENTRADA'
    SALIDA = 'SALIDA'


class Movimiento(Base):
    __tablename__ = 'movimientos'

    id = Column(GUID(), primary_key=True, default=lambda: str(uuid.uuid4()))
    medicamento_id = Column(GUID(), ForeignKey('medicamentos.id'))
    tipo = Column(Enum(MovimientoTipoEnum), nullable=False)
    cantidad = Column(Integer, nullable=False)
    usuario_id = Column(String(100), nullable=True)
    motivo = Column(String(200), nullable=True)
    fecha = Column(DateTime, server_default=func.now())

    medicamento = relationship('Medicamento', back_populates='movimientos')


class AuditLog(Base):
    __tablename__ = 'audit_logs'
    
    id = Column(GUID(), primary_key=True, default=lambda: str(uuid.uuid4()))
    entidad = Column(String(100), nullable=False)
    entidad_id = Column(GUID(), nullable=False)
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

    id = Column(GUID(), primary_key=True, default=lambda: str(uuid.uuid4()))
    username = Column(String(100), nullable=False)
    full_name = Column(String(200), nullable=True)
    email = Column(String(200), nullable=False)
    hashed_password = Column(String(256), nullable=False)
    role = Column(Enum(UserRoleEnum), default=UserRoleEnum.FARMACEUTICO, nullable=False)
    is_active = Column(Boolean, default=True)
    created_at = Column(DateTime, server_default=func.now())


class TipoAlertaEnum(enum.Enum):
    STOCK_MINIMO = 'STOCK_MINIMO'
    STOCK_CRITICO = 'STOCK_CRITICO'
    STOCK_AGOTADO = 'STOCK_AGOTADO'
    VENCIMIENTO_PROXIMO = 'VENCIMIENTO_PROXIMO'
    VENCIMIENTO_INMEDIATO = 'VENCIMIENTO_INMEDIATO'
    VENCIDO = 'VENCIDO'


class EstadoAlertaEnum(enum.Enum):
    ACTIVA = 'ACTIVA'
    PENDIENTE_REPOSICION = 'PENDIENTE_REPOSICION'
    RESUELTA = 'RESUELTA'


class PrioridadAlertaEnum(enum.Enum):
    BAJA = 'BAJA'
    MEDIA = 'MEDIA'
    ALTA = 'ALTA'
    CRITICA = 'CRITICA'


class Alerta(Base):
    """
    Modelo para sistema de alertas automatizado.
    HU-2.01: Alertas de stock bajo
    HU-2.02: Alertas de vencimiento
    
    Incluye:
    - Persistencia de alertas generadas
    - Historial completo con estados
    - Priorización automática
    - Trazabilidad de acciones
    """
    __tablename__ = 'alertas'
    
    id = Column(GUID(), primary_key=True, default=lambda: str(uuid.uuid4()))
    medicamento_id = Column(GUID(), ForeignKey('medicamentos.id'), nullable=False)
    tipo = Column(Enum(TipoAlertaEnum), nullable=False)
    estado = Column(Enum(EstadoAlertaEnum), default=EstadoAlertaEnum.ACTIVA, nullable=False)
    prioridad = Column(Enum(PrioridadAlertaEnum), nullable=False)
    mensaje = Column(String(500), nullable=False)
    
    # Datos específicos para alertas de stock
    stock_actual = Column(Integer, nullable=True)
    stock_minimo = Column(Integer, nullable=True)
    
    # Datos específicos para alertas de vencimiento
    fecha_vencimiento = Column(Date, nullable=True)
    dias_restantes = Column(Integer, nullable=True)
    lote = Column(String(100), nullable=True)
    
    # Metadatos adicionales
    metadatos = Column(JSON, nullable=True)
    
    # Auditoría y trazabilidad
    created_at = Column(DateTime, server_default=func.now())
    updated_at = Column(DateTime, onupdate=func.now(), server_default=func.now())
    resuelta_at = Column(DateTime, nullable=True)
    resuelta_by = Column(String(100), nullable=True)
    notificada = Column(Boolean, default=False)
    notificada_at = Column(DateTime, nullable=True)
    
    # Relación con medicamento
    medicamento = relationship('Medicamento', backref='alertas')

