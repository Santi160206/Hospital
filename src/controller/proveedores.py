from sqlalchemy.orm import Session
from src.entities.Proveedores import Proveedor
from src.schemas.proveedor import ProveedorCreate
from uuid import UUID

def create_proveedor(db: Session, proveedor: ProveedorCreate):
    nuevo_proveedor = Proveedor(
        Nombre=proveedor.Nombre,
        Direccion=proveedor.Direccion,
        Producto_Ofrecido=proveedor.Producto_Ofrecido,
        Laboratorio=proveedor.Laboratorio,
    )
    db.add(nuevo_proveedor)
    db.commit()
    db.refresh(nuevo_proveedor)
    return nuevo_proveedor

def get_proveedores(db: Session):
    return db.query(Proveedor).all()

def get_proveedor(db: Session, proveedor_id: UUID):
    return db.query(Proveedor).filter(Proveedor.Id_Proveedor == proveedor_id).first()

def update_proveedor(db: Session, proveedor_id: UUID, proveedor: ProveedorCreate):
    proveedor_db = db.query(Proveedor).filter(Proveedor.Id_Proveedor == proveedor_id).first()
    if not proveedor_db:
        return None
    for key, value in proveedor.dict().items():
        setattr(proveedor_db, key, value)
    db.commit()
    db.refresh(proveedor_db)
    return proveedor_db

def delete_proveedor(db: Session, proveedor_id: UUID):
    proveedor_db = db.query(Proveedor).filter(Proveedor.Id_Proveedor == proveedor_id).first()
    if not proveedor_db:
        return None
    db.delete(proveedor_db)
    db.commit()
    return proveedor_db
