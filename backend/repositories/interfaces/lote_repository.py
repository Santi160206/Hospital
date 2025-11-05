"""
Interface para LoteRepository
SOLID: Interface Segregation Principle - Solo operaciones de Lotes
"""
from typing import Protocol, Optional, List
from database import models


class ILoteRepository(Protocol):
    """Interfaz para operaciones CRUD de lotes.
    
    ISP: Esta interfaz contiene SOLO operaciones relacionadas con lotes,
    no con medicamentos, alertas ni compras.
    """

    def get(self, lote_id: str) -> Optional[models.Lote]:
        """Obtiene un lote por ID."""
        ...

    def list(self) -> List[models.Lote]:
        """Lista todos los lotes registrados."""
        ...

    def find_by_fecha_vencimiento(
        self,
        fecha_vencimiento,
        include_inactivos: bool = False
    ) -> List[models.Lote]:
        """Busca lotes por fecha de vencimiento, con opción de incluir inactivos."""
        ...

    def find_by_estado(
        self,
        estado,
        include_deleted: bool = False
    ) -> List[models.Lote]:
        """Busca lotes por estado (activo, inactivo, vencido, etc.)."""
        ...

    def create(self, lote: models.Lote) -> models.Lote:
        """Crea un nuevo lote (agrega a sesión, no hace commit)."""
        ...

    def update(self, lote: models.Lote) -> models.Lote:
        """Actualiza un lote (agrega a sesión, no hace commit)."""
        ...

    def delete(self, lote: models.Lote) -> None:
        """Elimina un lote de la base de datos."""
        ...
