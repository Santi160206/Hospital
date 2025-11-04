"""Interfaces para repositories (Dependency Inversion Principle - SOLID)"""

from .medicamento_repository import IMedicamentoRepository
from .movimiento_repository import IMovimientoRepository
from .proveedor_repository import IProveedorRepository

__all__ = [
    'IMedicamentoRepository',
    'IMovimientoRepository',
    "IProveedorRepository"
]
