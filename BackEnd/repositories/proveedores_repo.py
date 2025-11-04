from sqlalchemy.orm import Session
from database import models
from typing import List, Optional


class ProveedorRepository:
    """Repositorio para manejar operaciones CRUD de proveedores."""

    def __init__(self, db: Session):
        self.db = db

    def create(self, proveedor: models.Proveedor) -> models.Proveedor:
        self.db.add(proveedor)
        self.db.commit()
        self.db.refresh(proveedor)
        return proveedor

    def get(self, proveedor_id: int) -> Optional[models.Proveedor]:
        return self.db.query(models.Proveedor).filter(models.Proveedor.id_Proveedor == proveedor_id).first()

    def list(self) -> List[models.Proveedor]:
        return self.db.query(models.Proveedor).all()

    def update(self, proveedor: models.Proveedor) -> models.Proveedor:
        self.db.commit()
        self.db.refresh(proveedor)
        return proveedor

    def delete(self, proveedor: models.Proveedor) -> None:
        self.db.delete(proveedor)
        self.db.commit()
