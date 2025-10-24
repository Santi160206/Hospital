from typing import Any, Dict
from datetime import datetime, date


def _as_str(val: Any) -> Any:
    if val is None:
        return None
    # UUIDs and similar objects -> str
    try:
        from uuid import UUID
        if isinstance(val, UUID):
            return str(val)
    except Exception:
        pass

    # datetimes -> isoformat
    if isinstance(val, datetime):
        return val.isoformat()
    if isinstance(val, date):
        return val.isoformat()

    return val



def medicamento_to_dict(m) -> Dict[str, Any]:
    return {
        'id': _as_str(getattr(m, 'id', None)),
        'nombre': getattr(m, 'nombre', None),
        'presentacion': getattr(m, 'presentacion', None),
        'fabricante': getattr(m, 'fabricante', None),
        'lote': getattr(m, 'lote', None),
        'fecha_vencimiento': _as_str(getattr(m, 'fecha_vencimiento', None)),
        'stock': getattr(m, 'stock', None),
        'minimo_stock': getattr(m, 'minimo_stock', None),
    # precio stored as Decimal in DB; serialize as string to avoid float precision loss
    'precio': str(getattr(m, 'precio', None)) if getattr(m, 'precio', None) is not None else None,
        'estado': getattr(m, 'estado', None).name if getattr(m, 'estado', None) is not None else None,
        'is_deleted': getattr(m, 'is_deleted', False),
        'created_by': _as_str(getattr(m, 'created_by', None)),
        'created_at': _as_str(getattr(m, 'created_at', None)),
        'updated_by': _as_str(getattr(m, 'updated_by', None)),
        'updated_at': _as_str(getattr(m, 'updated_at', None)),
        'deleted_by': _as_str(getattr(m, 'deleted_by', None)),
        'deleted_at': _as_str(getattr(m, 'deleted_at', None)),
    }


def movimiento_to_dict(mv) -> Dict[str, Any]:
    return {
        'id': _as_str(getattr(mv, 'id', None)),
        'medicamento_id': _as_str(getattr(mv, 'medicamento_id', None)),
        'tipo': getattr(mv, 'tipo', None).name if getattr(mv, 'tipo', None) is not None else None,
        'cantidad': getattr(mv, 'cantidad', None),
        'usuario_id': _as_str(getattr(mv, 'usuario_id', None)),
        'motivo': getattr(mv, 'motivo', None),
        'fecha': _as_str(getattr(mv, 'fecha', None)),
    }
