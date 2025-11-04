from fastapi import APIRouter, Depends, HTTPException, status, Response
from typing import List, Optional
from sqlalchemy.orm import Session

# Ajusta el import según cómo tengas get_db (connection o session)
from database.connection import get_db

from services.proveedores_services import ProveedorService
from repositories.proveedores_repo import ProveedorRepository
from database import models

# Schemas: short para búsquedas, v2 para create/update/out
from schemas.proveedor_short import ProveedorShortOut
from schemas.proveedor_v2 import ProveedorCreate, ProveedorOut, ProveedorUpdate

# (Opcional) auth dependency si quieres control de acceso igual que medicamentos
try:
    from auth.security import get_current_user, require_admin
except Exception:
    # Si aún no tienes auth, definimos stubs para evitar errores; en tu proyecto remuévelos.
    def get_current_user():
        return None
    def require_admin():
        return None

router = APIRouter(prefix="/proveedores", tags=["Proveedores"])


def get_proveedor_service(db: Session = Depends(get_db)) -> ProveedorService:
    return ProveedorService(db)


@router.post("/", response_model=ProveedorOut, status_code=status.HTTP_201_CREATED)
def crear_proveedor(payload: ProveedorCreate, response: Response,
                    service: ProveedorService = Depends(get_proveedor_service),
                    db: Session = Depends(get_db),
                    user: dict = Depends(get_current_user)):
    """
    Crear proveedor.
    """
    # Preparar payload (si tu modelo espera keys concretas)
    payload_dict = payload.model_dump()
    try:
        p = service.create_proveedor(payload_dict, user_id=(user.get('sub') if user else None))
    except Exception as e:
        # Si hay un error de integridad (ej: unique constraint) puedes capturarlo aquí
        raise HTTPException(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR, detail=str(e))
    return p


@router.get("/", response_model=List[ProveedorOut])
def listar_proveedores(db: Session = Depends(get_db),
                       service: ProveedorService = Depends(get_proveedor_service),
                       q: Optional[str] = None,
                       limit: int = 100,
                       user: dict = Depends(get_current_user)):
    """
    Listar proveedores.
    - Si se pasa `q`, se hace una búsqueda simple por nombre o producto_ofrecido y se devuelve la lista completa (v2).
    - `limit` limita la cantidad de resultados.
    """
    items = service.list() or []
    if q:
        qlow = q.lower()
        filtered = [
            p for p in items
            if (getattr(p, 'nombre', '') and qlow in getattr(p, 'nombre', '').lower())
            or (getattr(p, 'producto_ofrecido', '') and qlow in getattr(p, 'producto_ofrecido', '').lower())
        ]
        return filtered[:limit]
    return items[:limit]


@router.get("/search", response_model=List[ProveedorShortOut])
def search_proveedores(query: Optional[str] = None,
                       limit: int = 8,
                       service: ProveedorService = Depends(get_proveedor_service),
                       user: dict = Depends(get_current_user)):
    """
    Búsqueda rápida (autocomplete) — devuelve el esquema 'short'.
    Busca en `nombre` y `producto_ofrecido`.
    """
    if not query:
        return []
    items = service.list() or []
    qlow = query.lower()
    results = []
    for p in items:
        name = getattr(p, 'nombre', '') or ''
        prod = getattr(p, 'producto_ofrecido', '') or ''
        if qlow in name.lower() or qlow in prod.lower():
            results.append(p)
            if len(results) >= limit:
                break
    return results


@router.get("/{proveedor_id}", response_model=ProveedorOut)
def detalle_proveedor(proveedor_id: int,
                      service: ProveedorService = Depends(get_proveedor_service),
                      db: Session = Depends(get_db),
                      user: dict = Depends(get_current_user)):
    p = service.get(proveedor_id)
    if not p:
        raise HTTPException(status_code=404, detail="Proveedor no encontrado")
    return p


@router.put("/{proveedor_id}", response_model=ProveedorOut)
def actualizar_proveedor(proveedor_id: int,
                         payload: ProveedorUpdate,
                         response: Response,
                         service: ProveedorService = Depends(get_proveedor_service),
                         db: Session = Depends(get_db),
                         user: dict = Depends(get_current_user)):
    data = payload.model_dump(exclude_unset=True)
    updated = service.update_proveedor(proveedor_id, data, user_id=(user.get('sub') if user else None))
    if not updated:
        raise HTTPException(status_code=404, detail="Proveedor no encontrado")
    return updated


@router.delete("/{proveedor_id}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_proveedor(proveedor_id: int,
                       service: ProveedorService = Depends(get_proveedor_service),
                       db: Session = Depends(get_db),
                       user: dict = Depends(get_current_user)):
    res = service.delete_proveedor(proveedor_id)
    if res is None:
        raise HTTPException(status_code=404, detail="Proveedor no encontrado")
    # retorna 204 si fue eliminado correctamente
    return None
