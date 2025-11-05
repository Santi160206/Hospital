from sqlalchemy.orm import Session
from database import models
from typing import List, Optional


class LoteRepository:
    """Repositorio para manejar operaciones CRUD de lotes."""

    def __init__(self, db: Session):
        self.db = db

    def create(self, lote: models.Lote) -> models.Lote:
        """Crea un nuevo lote en la base de datos."""
        self.db.add(lote)
        self.db.commit()
        self.db.refresh(lote)
        return lote

    def get(self, lote_id: str) -> Optional[models.Lote]:
        """Obtiene un lote por su ID."""
        return (
            self.db.query(models.Lote)
            .filter(models.Lote.id_lote == lote_id)
            .first()
        )

    def list(self) -> List[models.Lote]:
        """Obtiene todos los lotes registrados."""
        return self.db.query(models.Lote).all()

    def update(self, lote: models.Lote) -> models.Lote:
        """Actualiza los datos de un lote existente."""
        self.db.commit()
        self.db.refresh(lote)
        return lote

    def delete(self, lote: models.Lote) -> None:
        """Elimina un lote de la base de datos."""
        self.db.delete(lote)
        self.db.commit()
