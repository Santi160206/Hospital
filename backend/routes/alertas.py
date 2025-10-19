"""
Endpoints para sistema de alertas automatizado
Según RF-02: Sistema de alertas de stock automatizado
"""
from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
from sqlalchemy import and_, or_
from database.connection import get_db
from database import models
from auth.security import get_current_user
from typing import List, Optional
from datetime import date, timedelta
from pydantic import BaseModel
from uuid import UUID
from decimal import Decimal

router = APIRouter()


class AlertaStockBajo(BaseModel):
    """Alerta de stock bajo o agotado"""
    id: UUID
    nombre: str
    presentacion: str
    stock: int
    minimo_stock: Optional[int]
    nivel_critico: str  # "AGOTADO", "CRITICO", "BAJO"
    mensaje: str
    
    model_config = {"from_attributes": True}


class AlertaVencimiento(BaseModel):
    """Alerta de medicamento próximo a vencer"""
    id: UUID
    nombre: str
    presentacion: str
    lote: str
    fecha_vencimiento: date
    dias_restantes: int
    nivel_urgencia: str  # "VENCIDO", "INMEDIATO", "PROXIMO"
    mensaje: str
    
    model_config = {"from_attributes": True}


class DashboardStats(BaseModel):
    """Estadísticas para el dashboard principal"""
    total_medicamentos_activos: int
    medicamentos_stock_bajo: int
    medicamentos_agotados: int
    medicamentos_proximos_vencer_30_dias: int
    medicamentos_vencidos: int
    valor_total_inventario: Decimal
    medicamentos_criticos: int  # stock_bajo + próximos a vencer


@router.get("/stock-bajo", response_model=List[AlertaStockBajo])
def alertas_stock_bajo(
    db: Session = Depends(get_db),
    user: dict = Depends(get_current_user),
    nivel: Optional[str] = None  # AGOTADO, CRITICO, BAJO
):
    """
    Obtiene alertas de medicamentos con stock bajo.
    
    RF-02.1: Monitorear automáticamente niveles de stock
    
    Niveles:
    - AGOTADO: stock = 0
    - CRITICO: 0 < stock <= minimo_stock / 2
    - BAJO: minimo_stock / 2 < stock <= minimo_stock
    
    Solo muestra medicamentos ACTIVOS y no eliminados.
    """
    q = db.query(models.Medicamento).filter(
        and_(
            models.Medicamento.is_deleted == False,
            models.Medicamento.estado == models.EstadoEnum.ACTIVO
        )
    )
    
    # Filtrar solo los que tienen stock <= minimo_stock
    q = q.filter(models.Medicamento.stock <= models.Medicamento.minimo_stock)
    
    medicamentos = q.all()
    
    alertas = []
    for med in medicamentos:
        # Determinar nivel crítico
        if med.stock == 0:
            nivel_critico = "AGOTADO"
            mensaje = f"Stock agotado: {med.nombre} ({med.presentacion})"
        elif med.minimo_stock and med.stock <= med.minimo_stock / 2:
            nivel_critico = "CRITICO"
            mensaje = f"Stock crítico: {med.nombre} tiene {med.stock} unidades (mínimo: {med.minimo_stock})"
        else:
            nivel_critico = "BAJO"
            mensaje = f"Stock bajo: {med.nombre} tiene {med.stock} unidades (mínimo: {med.minimo_stock})"
        
        # Filtrar por nivel si se especificó
        if nivel and nivel_critico != nivel.upper():
            continue
        
        alertas.append(AlertaStockBajo(
            id=med.id,
            nombre=med.nombre,
            presentacion=med.presentacion,
            stock=med.stock,
            minimo_stock=med.minimo_stock,
            nivel_critico=nivel_critico,
            mensaje=mensaje
        ))
    
    # Ordenar por criticidad: AGOTADO > CRITICO > BAJO
    orden = {"AGOTADO": 0, "CRITICO": 1, "BAJO": 2}
    alertas.sort(key=lambda x: orden.get(x.nivel_critico, 3))
    
    return alertas


@router.get("/vencimientos", response_model=List[AlertaVencimiento])
def alertas_vencimiento(
    db: Session = Depends(get_db),
    user: dict = Depends(get_current_user),
    dias: int = 30  # Días de anticipación
):
    """
    Obtiene alertas de medicamentos próximos a vencer.
    
    RF-02.3: Detectar medicamentos próximos a vencer
    
    Niveles de urgencia:
    - VENCIDO: fecha_vencimiento < hoy
    - INMEDIATO: 0 <= días_restantes <= 7
    - PROXIMO: 7 < días_restantes <= dias (default 30)
    
    Solo muestra medicamentos ACTIVOS y no eliminados.
    """
    hoy = date.today()
    fecha_limite = hoy + timedelta(days=dias)
    
    q = db.query(models.Medicamento).filter(
        and_(
            models.Medicamento.is_deleted == False,
            models.Medicamento.estado == models.EstadoEnum.ACTIVO,
            models.Medicamento.fecha_vencimiento <= fecha_limite
        )
    )
    
    medicamentos = q.all()
    
    alertas = []
    for med in medicamentos:
        dias_restantes = (med.fecha_vencimiento - hoy).days
        
        # Determinar nivel de urgencia
        if dias_restantes < 0:
            nivel_urgencia = "VENCIDO"
            mensaje = f"VENCIDO: {med.nombre} (lote {med.lote}) venció hace {abs(dias_restantes)} días"
        elif dias_restantes <= 7:
            nivel_urgencia = "INMEDIATO"
            mensaje = f"Vence en {dias_restantes} días: {med.nombre} (lote {med.lote})"
        else:
            nivel_urgencia = "PROXIMO"
            mensaje = f"Vence en {dias_restantes} días: {med.nombre} (lote {med.lote})"
        
        alertas.append(AlertaVencimiento(
            id=med.id,
            nombre=med.nombre,
            presentacion=med.presentacion,
            lote=med.lote,
            fecha_vencimiento=med.fecha_vencimiento,
            dias_restantes=dias_restantes,
            nivel_urgencia=nivel_urgencia,
            mensaje=mensaje
        ))
    
    # Ordenar por urgencia: VENCIDO > INMEDIATO > PROXIMO
    orden = {"VENCIDO": 0, "INMEDIATO": 1, "PROXIMO": 2}
    alertas.sort(key=lambda x: (orden.get(x.nivel_urgencia, 3), x.dias_restantes))
    
    return alertas


@router.get("/dashboard", response_model=DashboardStats)
def dashboard_estadisticas(
    db: Session = Depends(get_db),
    user: dict = Depends(get_current_user)
):
    """
    Obtiene estadísticas para el dashboard principal.
    
    RF-02: Sistema de alertas automatizado
    RNF-04.2: Mostrar alertas visuales en el dashboard principal
    
    Retorna:
    - Total de medicamentos activos
    - Medicamentos con stock bajo
    - Medicamentos agotados
    - Medicamentos próximos a vencer (30 días)
    - Medicamentos vencidos
    - Valor total del inventario
    - Medicamentos críticos (stock bajo + vencimiento próximo)
    """
    hoy = date.today()
    fecha_30_dias = hoy + timedelta(days=30)
    
    # Total activos
    total_activos = db.query(models.Medicamento).filter(
        and_(
            models.Medicamento.is_deleted == False,
            models.Medicamento.estado == models.EstadoEnum.ACTIVO
        )
    ).count()
    
    # Stock bajo (stock <= minimo_stock)
    stock_bajo = db.query(models.Medicamento).filter(
        and_(
            models.Medicamento.is_deleted == False,
            models.Medicamento.estado == models.EstadoEnum.ACTIVO,
            models.Medicamento.stock <= models.Medicamento.minimo_stock,
            models.Medicamento.stock > 0
        )
    ).count()
    
    # Agotados (stock = 0)
    agotados = db.query(models.Medicamento).filter(
        and_(
            models.Medicamento.is_deleted == False,
            models.Medicamento.estado == models.EstadoEnum.ACTIVO,
            models.Medicamento.stock == 0
        )
    ).count()
    
    # Próximos a vencer (30 días)
    proximos_vencer = db.query(models.Medicamento).filter(
        and_(
            models.Medicamento.is_deleted == False,
            models.Medicamento.estado == models.EstadoEnum.ACTIVO,
            models.Medicamento.fecha_vencimiento <= fecha_30_dias,
            models.Medicamento.fecha_vencimiento >= hoy
        )
    ).count()
    
    # Vencidos
    vencidos = db.query(models.Medicamento).filter(
        and_(
            models.Medicamento.is_deleted == False,
            models.Medicamento.estado == models.EstadoEnum.ACTIVO,
            models.Medicamento.fecha_vencimiento < hoy
        )
    ).count()
    
    # Valor total del inventario
    from sqlalchemy import func
    valor_total_result = db.query(
        func.sum(models.Medicamento.stock * models.Medicamento.precio)
    ).filter(
        and_(
            models.Medicamento.is_deleted == False,
            models.Medicamento.estado == models.EstadoEnum.ACTIVO
        )
    ).scalar()
    
    valor_total = Decimal(str(valor_total_result)) if valor_total_result else Decimal('0')
    
    # Medicamentos críticos (suma de problemas únicos)
    criticos = stock_bajo + agotados + proximos_vencer + vencidos
    
    return DashboardStats(
        total_medicamentos_activos=total_activos,
        medicamentos_stock_bajo=stock_bajo,
        medicamentos_agotados=agotados,
        medicamentos_proximos_vencer_30_dias=proximos_vencer,
        medicamentos_vencidos=vencidos,
        valor_total_inventario=valor_total,
        medicamentos_criticos=criticos
    )
