"""
Interfaces de repositories (compatibilidad)
Este archivo mantiene compatibilidad con imports antiguos.
Las interfaces ahora están segregadas en: repositories/interfaces/

SOLID: Interface Segregation Principle aplicado.
"""
from .interfaces.medicamento_repository import IMedicamentoRepository
from .interfaces.movimiento_repository import IMovimientoRepository

__all__ = [
    'IMedicamentoRepository',
    'IMovimientoRepository',
]
