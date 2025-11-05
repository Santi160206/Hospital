"""Interfaces para repositories (Dependency Inversion Principle - SOLID)"""

from .medicamento_repository import IMedicamentoRepository
from .movimiento_repository import IMovimientoRepository
from .lote_repository import ILoteRepository

__all__ = [
    'IMedicamentoRepository',
    'IMovimientoRepository',
    'ILoteRepository',
]
