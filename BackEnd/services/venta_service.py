"""
Service para lógica de negocio de ventas
"""
from typing import List, Dict, Any, Optional
from sqlalchemy.orm import Session
from sqlalchemy import func, desc
from datetime import datetime, date, timedelta
from decimal import Decimal

from database import models
from repositories.interfaces import IVentaRepository
from repositories.venta_repo import VentaRepository


class VentaService:
    def __init__(
        self, 
        db: Session, 
        venta_repo: Optional[IVentaRepository] = None
    ):
        """Constructor con inyección de dependencias (DIP)."""
        self.db = db
        
        if venta_repo is None:
            self.venta_repo: IVentaRepository = VentaRepository(db)
        else:
            self.venta_repo = venta_repo

    def crear_venta(self, venta_data: Dict[str, Any], usuario_id: str) -> Dict[str, Any]:
        """Crea una nueva venta con sus detalles y actualiza stock"""
        try:
            # Calcular totales y validar stock
            detalles_data = venta_data.get('detalles', [])
            total_venta = Decimal('0')
            detalles_a_crear = []
            
            # Validar stock y preparar detalles
            for detalle in detalles_data:
                medicamento = self.db.query(models.Medicamento).filter(
                    models.Medicamento.id == detalle['medicamento_id'],
                    models.Medicamento.estado == models.EstadoEnum.ACTIVO,
                    models.Medicamento.is_deleted == False
                ).first()
                
                if not medicamento:
                    return {'ok': False, 'reason': 'medicamento_no_encontrado', 'medicamento_id': detalle['medicamento_id']}
                
                if medicamento.stock < detalle['cantidad']:
                    return {
                        'ok': False, 
                        'reason': 'stock_insuficiente', 
                        'medicamento_id': detalle['medicamento_id'],
                        'medicamento_nombre': medicamento.nombre,
                        'stock_disponible': medicamento.stock,
                        'cantidad_solicitada': detalle['cantidad']
                    }
                
                # Usar precio del medicamento si no se especifica
                precio_unitario = detalle.get('precio_unitario')
                if precio_unitario is None:
                    precio_unitario = float(medicamento.precio)
                
                subtotal = Decimal(str(precio_unitario)) * detalle['cantidad']
                total_venta += subtotal
                
                detalles_a_crear.append({
                    'medicamento_id': detalle['medicamento_id'],
                    'cantidad': detalle['cantidad'],
                    'precio_unitario': Decimal(str(precio_unitario)),
                    'subtotal': subtotal,
                    'medicamento_nombre': medicamento.nombre
                })
            
            # Crear la venta
            nueva_venta = models.Venta(
                usuario_id=usuario_id,
                total=total_venta,
                cliente=venta_data.get('cliente'),
                notas=venta_data.get('notas')
            )
            
            # Usar el repository para crear la venta
            nueva_venta = self.venta_repo.create(nueva_venta)
            
            # Crear detalles y actualizar stock
            for detalle in detalles_a_crear:
                # Crear detalle de venta
                detalle_venta = models.DetalleVenta(
                    venta_id=nueva_venta.id,
                    medicamento_id=detalle['medicamento_id'],
                    cantidad=detalle['cantidad'],
                    precio_unitario=detalle['precio_unitario'],
                    subtotal=detalle['subtotal']
                )
                self.db.add(detalle_venta)
                
                # Actualizar stock del medicamento
                medicamento = self.db.query(models.Medicamento).filter(
                    models.Medicamento.id == detalle['medicamento_id']
                ).first()
                medicamento.stock -= detalle['cantidad']
                
                # Registrar movimiento de salida
                movimiento = models.Movimiento(
                    medicamento_id=detalle['medicamento_id'],
                    tipo=models.MovimientoTipoEnum.SALIDA,
                    cantidad=detalle['cantidad'],
                    usuario_id=usuario_id,
                    motivo=f"Venta #{nueva_venta.id}"
                )
                self.db.add(movimiento)
            
            # Commit de toda la transacción
            self.db.commit()
            self.db.refresh(nueva_venta)
            
            return {
                'ok': True, 
                'venta': nueva_venta,
                'detalles': detalles_a_crear
            }
            
        except Exception as e:
            self.db.rollback()
            return {'ok': False, 'reason': 'error_transaccion', 'error': str(e)}

    def obtener_venta_por_id(self, venta_id: str) -> Optional[models.Venta]:
        """Obtiene una venta por su ID con sus detalles"""
        return self.venta_repo.get(venta_id)

    def listar_ventas(
        self, 
        fecha_inicio: Optional[date] = None,
        fecha_fin: Optional[date] = None,
        usuario_id: Optional[str] = None,
        limit: int = 100
    ) -> List[models.Venta]:
        """Lista ventas con filtros opcionales"""
        # Convertir dates a strings para el repository
        fecha_inicio_str = fecha_inicio.isoformat() if fecha_inicio else None
        fecha_fin_str = fecha_fin.isoformat() if fecha_fin else None
        
        return self.venta_repo.list(
            skip=0,
            limit=limit,
            fecha_inicio=fecha_inicio_str,
            fecha_fin=fecha_fin_str,
            usuario_id=usuario_id
        )

    def generar_reporte_ventas(
        self, 
        fecha_inicio: date, 
        fecha_fin: date
    ) -> Dict[str, Any]:
        """Genera reporte completo de ventas para un período"""
        # Ajustar fechas para incluir todo el día
        fecha_inicio_dt = datetime.combine(fecha_inicio, datetime.min.time())
        fecha_fin_dt = datetime.combine(fecha_fin, datetime.max.time())
        
        # Convertir a strings para el repository
        fecha_inicio_str = fecha_inicio_dt.isoformat()
        fecha_fin_str = fecha_fin_dt.isoformat()
        
        # Usar el repository para obtener datos
        ventas_periodo = self.venta_repo.get_ventas_por_periodo(fecha_inicio_str, fecha_fin_str)
        total_ventas = self.venta_repo.get_total_ventas_por_periodo(fecha_inicio_str, fecha_fin_str)
        cantidad_ventas = self.venta_repo.get_cantidad_ventas_por_periodo(fecha_inicio_str, fecha_fin_str)
        productos_mas_vendidos = self.venta_repo.get_productos_mas_vendidos(fecha_inicio_str, fecha_fin_str)
        
        # Ventas por día (usamos query directo por complejidad)
        ventas_por_dia = self.db.query(
            func.date(models.Venta.fecha).label('fecha'),
            func.sum(models.Venta.total).label('total_dia'),
            func.count(models.Venta.id).label('cantidad_ventas')
        ).filter(
            models.Venta.fecha.between(fecha_inicio_dt, fecha_fin_dt)
        ).group_by(
            func.date(models.Venta.fecha)
        ).order_by(
            func.date(models.Venta.fecha)
        ).all()
        
        return {
            'periodo': f"{fecha_inicio} a {fecha_fin}",
            'total_ventas': total_ventas,
            'cantidad_ventas': cantidad_ventas,
            'ventas_promedio': total_ventas / cantidad_ventas if cantidad_ventas > 0 else 0,
            'productos_mas_vendidos': [
                {
                    'medicamento_id': item.id,
                    'medicamento_nombre': item.nombre,
                    'cantidad_vendida': item.cantidad_vendida,
                    'total_ventas': float(item.total_ventas)
                }
                for item in productos_mas_vendidos
            ],
            'ventas_por_dia': [
                {
                    'fecha': item.fecha.strftime('%Y-%m-%d'),
                    'total_ventas': float(item.total_dia),
                    'cantidad_ventas': item.cantidad_ventas
                }
                for item in ventas_por_dia
            ]
        }

    def obtener_proyeccion_demanda(self, dias_proyeccion: int = 30) -> Dict[str, Any]:
        """Genera proyección de demanda basada en ventas históricas"""
        fecha_fin = date.today()
        fecha_inicio = fecha_fin - timedelta(days=90)  # Últimos 90 días para análisis
        
        fecha_inicio_dt = datetime.combine(fecha_inicio, datetime.min.time())
        fecha_fin_dt = datetime.combine(fecha_fin, datetime.max.time())
        
        fecha_inicio_str = fecha_inicio_dt.isoformat()
        fecha_fin_str = fecha_fin_dt.isoformat()
        
        # Ventas históricas por producto
        ventas_historicas = self.venta_repo.get_productos_mas_vendidos(
            fecha_inicio_str, fecha_fin_str, limit=1000
        )
        
        dias_analizados = (fecha_fin - fecha_inicio).days
        
        proyecciones = []
        for item in ventas_historicas:
            cantidad_vendida = item.cantidad_vendida or 0
            promedio_diario = cantidad_vendida / dias_analizados if dias_analizados > 0 else 0
            demanda_proyectada = promedio_diario * dias_proyeccion
            
            proyecciones.append({
                'medicamento_id': item.id,
                'medicamento_nombre': item.nombre,
                'ventas_ultimos_90_dias': cantidad_vendida,
                'promedio_diario': round(promedio_diario, 2),
                'demanda_proyectada_30_dias': round(demanda_proyectada),
                'nivel_reorden_recomendado': round(demanda_proyectada * 1.2)  # 20% extra
            })
        
        return {
            'periodo_analizado': f"{fecha_inicio} a {fecha_fin}",
            'dias_proyeccion': dias_proyeccion,
            'proyecciones': proyecciones
        }