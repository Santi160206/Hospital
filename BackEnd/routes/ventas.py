from fastapi import APIRouter, Depends, HTTPException, status, Query
from sqlalchemy.orm import Session
from typing import List, Optional
from datetime import date, datetime
from database.connection import get_db
from auth.security import get_current_user, require_farmaceutico, require_admin
from schemas.venta import (
    VentaCreate, VentaOut, ReporteVentaPeriodo, 
    ReporteCompletoOut, VentasPorPeriodoOut, ProductoMasVendidoOut
)
from services.venta_service import VentaService

router = APIRouter()

def get_venta_service(db: Session = Depends(get_db)) -> VentaService:
    return VentaService(db)

@router.post("/", response_model=VentaOut, status_code=status.HTTP_201_CREATED)
def crear_venta(
    payload: VentaCreate,
    service: VentaService = Depends(get_venta_service),
    db: Session = Depends(get_db),
    user: dict = Depends(require_farmaceutico)
):
    """
    Registrar una nueva venta
    
    HU: Como farmacéutico/administrador, quiero registrar automáticamente todas las ventas 
    para tener historial, contabilizar y descontar stock.
    
    - Registra la venta con todos sus detalles
    - Actualiza el stock de los medicamentos automáticamente
    - Crea movimientos de salida para auditoría
    - Valida que haya stock suficiente
    """
    venta_data = payload.model_dump()
    usuario_id = user.get('sub')
    
    resultado = service.crear_venta(venta_data, usuario_id)
    
    if not resultado.get('ok'):
        reason = resultado.get('reason')
        if reason == 'medicamento_no_encontrado':
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Medicamento no encontrado: {resultado.get('medicamento_id')}"
            )
        elif reason == 'stock_insuficiente':
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail={
                    "error": "Stock insuficiente",
                    "medicamento": resultado.get('medicamento_nombre'),
                    "stock_disponible": resultado.get('stock_disponible'),
                    "cantidad_solicitada": resultado.get('cantidad_solicitada')
                }
            )
        elif reason == 'error_transaccion':
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Error al procesar la venta"
            )
        else:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Error al crear la venta"
            )
    
    # Obtener la venta completa con detalles para la respuesta
    venta_completa = service.obtener_venta_por_id(resultado['venta'].id)
    return venta_completa

@router.get("/", response_model=List[VentaOut])
def listar_ventas(
    fecha_inicio: Optional[date] = Query(None, description="Fecha inicio (YYYY-MM-DD)"),
    fecha_fin: Optional[date] = Query(None, description="Fecha fin (YYYY-MM-DD)"),
    usuario_id: Optional[str] = Query(None, description="Filtrar por usuario"),
    limit: int = Query(100, le=500, description="Límite de resultados"),
    service: VentaService = Depends(get_venta_service),
    user: dict = Depends(get_current_user)
):
    """
    Listar ventas con filtros opcionales
    
    - Farmacéuticos ven solo sus ventas (a menos que sean admin)
    - Admins ven todas las ventas
    """
    # Si no es admin y no especificó usuario, filtrar por su propio usuario
    if user.get('role') != 'admin' and usuario_id is None:
        usuario_id = user.get('sub')
    elif user.get('role') != 'admin' and usuario_id != user.get('sub'):
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="No tiene permisos para ver ventas de otros usuarios"
        )
    
    ventas = service.listar_ventas(fecha_inicio, fecha_fin, usuario_id, limit)
    return ventas

@router.get("/{venta_id}", response_model=VentaOut)
def obtener_venta(
    venta_id: str,
    service: VentaService = Depends(get_venta_service),
    user: dict = Depends(get_current_user)
):
    """
    Obtener una venta específica por ID
    """
    venta = service.obtener_venta_por_id(venta_id)
    if not venta:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Venta no encontrada"
        )
    
    # Validar permisos: solo admin o el usuario que creó la venta
    if user.get('role') != 'admin' and str(venta.usuario_id) != user.get('sub'):
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="No tiene permisos para ver esta venta"
        )
    
    return venta

@router.get("/reportes/ventas-periodo", response_model=ReporteCompletoOut)
def generar_reporte_ventas_periodo(
    fecha_inicio: date = Query(..., description="Fecha inicio (YYYY-MM-DD)"),
    fecha_fin: date = Query(..., description="Fecha fin (YYYY-MM-DD)"),
    service: VentaService = Depends(get_venta_service),
    user: dict = Depends(require_admin)
):
    """
    Generar reporte completo de ventas por período
    
    HU: Como administrador, quiero generar reportes de ventas por período 
    y proyecciones de demanda para planificar compras y stock.
    
    - Solo accesible para administradores
    - Incluye: totales, productos más vendidos, ventas por día
    """
    if fecha_inicio > fecha_fin:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="La fecha de inicio no puede ser mayor a la fecha fin"
        )
    
    # Validar que el rango no sea muy grande (máximo 1 año)
    if (fecha_fin - fecha_inicio).days > 365:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="El rango de fechas no puede ser mayor a 1 año"
        )
    
    reporte = service.generar_reporte_ventas(fecha_inicio, fecha_fin)
    return reporte

@router.get("/reportes/proyeccion-demanda")
def generar_proyeccion_demanda(
    dias_proyeccion: int = Query(30, ge=7, le=90, description="Días para proyección (7-90)"),
    service: VentaService = Depends(get_venta_service),
    user: dict = Depends(require_admin)
):
    """
    Generar proyección de demanda para planificación de stock
    
    HU: Como administrador, quiero proyecciones de demanda para planificar compras y stock.
    
    - Basado en ventas históricas de los últimos 90 días
    - Incluye nivel de reorden recomendado
    - Solo accesible para administradores
    """
    proyeccion = service.obtener_proyeccion_demanda(dias_proyeccion)
    return proyeccion

@router.get("/reportes/ventas-diarias")
def obtener_ventas_diarias(
    fecha: date = Query(..., description="Fecha específica (YYYY-MM-DD)"),
    service: VentaService = Depends(get_venta_service),
    user: dict = Depends(get_current_user)
):
    """
    Obtener resumen de ventas para una fecha específica
    
    - Admins ven todas las ventas del día
    - Farmacéuticos ven solo sus ventas
    """
    if user.get('role') == 'admin':
        usuario_id = None
    else:
        usuario_id = user.get('sub')
    
    ventas = service.listar_ventas(fecha, fecha, usuario_id, 1000)
    
    total_dia = sum(venta.total for venta in ventas)
    cantidad_ventas = len(ventas)
    
    return {
        "fecha": fecha,
        "total_ventas": float(total_dia),
        "cantidad_ventas": cantidad_ventas,
        "ventas": ventas
    }