"""
Service para la lógica de negocio de proveedores.
SRP: responsabilidad única — lógica de negocio de proveedores.
DIP: depende de abstracciones (repositorio), no de concreciones.
"""

from repositories.proveedores_repo import ProveedorRepository
from repositories.interfaces import IProveedorRepository
from database import models
from sqlalchemy.orm import Session
from typing import Optional, Dict, Any, List


class ProveedorService:
    """Service para manejar lógica de negocio de proveedores."""

    def __init__(self, db: Session, proveedor_repo: Optional[IProveedorRepository] = None):
        """Constructor con inyección de dependencias."""
        self.db = db
        self.repo = proveedor_repo or ProveedorRepository(db)

    def create_proveedor(self, payload: Dict[str, Any], user_id: Optional[str] = None) -> models.Proveedor:
        proveedor = models.Proveedor(**payload)
        try:
            self.repo.create(proveedor)
            return proveedor
        except Exception:
            self.db.rollback()
            raise

    def get(self, proveedor_id: int) -> Optional[models.Proveedor]:
        return self.repo.get(proveedor_id)

    def list(self) -> List[models.Proveedor]:
        return self.repo.list()

    def update_proveedor(self, proveedor_id: int, changes: Dict[str, Any], user_id: Optional[str] = None):
        proveedor = self.repo.get(proveedor_id)
        if not proveedor:
            return None

        for field, new_value in changes.items():
            setattr(proveedor, field, new_value)

        try:
            self.repo.update(proveedor)
            return proveedor
        except Exception:
            self.db.rollback()
            raise

    def delete_proveedor(self, proveedor_id: int):
        proveedor = self.repo.get(proveedor_id)
        if not proveedor:
            return None

        try:
            self.repo.delete(proveedor)
            return True
        except Exception:
            self.db.rollback()
            raise
