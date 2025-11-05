"""
Service para lógica de negocio de Lotes
SOLID: Single Responsibility - Lógica de negocio de lotes
"""

from sqlalchemy.orm import Session
from typing import Optional, Dict, Any, List
from repositories.lote_repo import LoteRepository
from repositories.interfaces import ILoteRepository
from database import models
from utils.text import normalize_text
from datetime import date, datetime


class LoteService:
    """Service para manejar la lógica de negocio de los lotes.
    
    SRP: Se encarga únicamente de la lógica de lotes.
    DIP: Depende de ILoteRepository (abstracción) en lugar de una implementación concreta.
    """

    def __init__(self, db: Session, lote_repo: Optional[ILoteRepository] = None):
        """Constructor con inyección de dependencias (DIP)."""
        self.db = db
        self.lote_repo: ILoteRepository = lote_repo or LoteRepository(db)

    # -------------------------------------------------------------------------
    # Crear Lote
    # -------------------------------------------------------------------------
    def create_lote(self, payload: Dict[str, Any], user_id: Optional[str] = None) -> models.Lote:
        """Crea un nuevo lote con validaciones de negocio básicas."""
        lote = models.Lote(**payload)

        if lote.Fecha_Vencimiento < date.today():
            raise ValueError("La fecha de vencimiento no puede ser anterior a hoy.")

        lote.Estado = models.EstadoEnum.ACTIVO
        try:
            self.lote_repo.create(lote)
            self.db.flush()
            self.db.refresh(lote)
            self.db.commit()
            return lote
        except Exception:
            self.db.rollback()
            raise

    # -------------------------------------------------------------------------
    # Obtener / Listar
    # -------------------------------------------------------------------------
    def get(self, lote_id: str) -> Optional[models.Lote]:
        return self.lote_repo.get(lote_id)

    def list(self) -> List[models.Lote]:
        return self.lote_repo.list()

    # -------------------------------------------------------------------------
    # Actualizar
    # -------------------------------------------------------------------------
    def update_lote(self, lote_id: str, changes: Dict[str, Any], user_id: Optional[str] = None):
     lote = self.lote_repo.get(lote_id)
     if not lote:
        return None

     audit_entries = []
     for field, new_value in changes.items():
        old_value = getattr(lote, field, None)
        if new_value != old_value:
            setattr(lote, field, new_value)
            audit_entries.append((field, old_value, new_value))

     if not audit_entries:
        return None  # ⚠️ No hay cambios → la ruta responderá 404 o similar

     # Validación de fecha vencimiento
     if 'Fecha_Vencimiento' in changes and lote.Fecha_Vencimiento < date.today():
        raise ValueError("invalid_expiration")

     try:
        self.lote_repo.update(lote)

        for field, old, new in audit_entries:
            al = models.AuditLog(
                entidad='lotes',
                entidad_id=lote.id_lote,
                usuario_id=user_id,
                accion='UPDATE',
                campo=field,
                valor_anterior=str(old),
                valor_nuevo=str(new)
            )
            self.db.add(al)

        self.db.flush()
        self.db.refresh(lote)
        self.db.commit()

        return lote  
     except Exception:
        self.db.rollback()
        return None

    # -------------------------------------------------------------------------
    # Eliminar / Desactivar
    # -------------------------------------------------------------------------
    def delete_lote(self, lote_id: str, user_id: Optional[str] = None):
        """Desactiva un lote en lugar de eliminarlo físicamente."""
        lote = self.lote_repo.get(lote_id)
        if not lote:
            return None

        if lote.Cantidad_disponible > 0:
            # No eliminar si hay stock, solo desactivar
            lote.Estado = models.EstadoEnum.INACTIVO
            deleted = False
        else:
            # Si no hay stock, marcar como inactivo definitivo
            lote.Estado = models.EstadoEnum.INACTIVO
            deleted = True

        try:
            self.lote_repo.update(lote)
            al = models.AuditLog(
                entidad='lotes',
                entidad_id=lote.id_lote,
                usuario_id=user_id,
                accion='DEACTIVATE'
            )
            self.db.add(al)
            self.db.flush()
            self.db.commit()
            return {'deleted': deleted}
        except Exception:
            self.db.rollback()
            return {'deleted': False}

    # -------------------------------------------------------------------------
    # Reactivar
    # -------------------------------------------------------------------------
    def reactivar_lote(self, lote_id: str, user_id: Optional[str] = None):
        lote = self.lote_repo.get(lote_id)
        if not lote:
            return None

        if lote.Fecha_Vencimiento < date.today():
            return {'reactivated': False, 'reason': 'expired'}

        lote.Estado = models.EstadoEnum.ACTIVO

        try:
            self.lote_repo.update(lote)
            al = models.AuditLog(
                entidad='lotes',
                entidad_id=lote.id_lote,
                usuario_id=user_id,
                accion='REACTIVATE'
            )
            self.db.add(al)
            self.db.flush()
            self.db.refresh(lote)
            self.db.commit()
            return {'reactivated': True, 'lote': lote}
        except Exception:
            self.db.rollback()
            return {'reactivated': False}

    # -------------------------------------------------------------------------
    # Buscar (por fecha o estado)
    # -------------------------------------------------------------------------
    def search_by_estado(self, estado: models.EstadoEnum, limit: int = 10):
        return (
            self.db.query(models.Lote)
            .filter(models.Lote.Estado == estado)
            .limit(limit)
            .all()
        )

    def search_vencidos(self, fecha_ref: Optional[date] = None):
        """Busca lotes vencidos hasta la fecha indicada."""
        fecha_ref = fecha_ref or date.today()
        return (
            self.db.query(models.Lote)
            .filter(models.Lote.Fecha_Vencimiento < fecha_ref)
            .all()
        )

