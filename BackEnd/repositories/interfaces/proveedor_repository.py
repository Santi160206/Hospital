"""
Interface para ProveedorRepository
SOLID: Interface Segregation Principle - Solo operaciones de Proveedores
"""
from typing import Protocol, Optional, List
from database import models


class IProveedorRepository(Protocol):
    """Interfaz para operaciones CRUD de proveedores.
    
    ISP: Esta interfaz contiene SOLO operaciones relacionadas con proveedores,
    no con movimientos (eso está en IProveedorRepository).
    """
    
    def get(self, prov_id: str) -> Optional[models.Proveedor]:
        """Obtiene un proveedor por ID."""
        ...

    def list(self) -> List[models.Proveedor]:
        """Lista todos los proveedores no eliminados."""
        ...

    def find_by_search_key(
        self, 
        search_key: str, 
        exclude_id: Optional[str] = None, 
        include_deleted: bool = False, 
        include_inactive: bool = False
    ) -> Optional[models.Proveedor]:
        """Busca un proveedor por search_key (para detección de duplicados)."""
        ...

    def create(self, m: models.Proveedor) -> models.Proveedor:
        """Crea un nuevo proveedor (agrega a sesión, no hace commit)."""
        ...

    def update(self, m: models.Proveedor) -> models.Proveedor:
        """Actualiza un proveedor (agrega a sesión, no hace commit)."""
        ...
