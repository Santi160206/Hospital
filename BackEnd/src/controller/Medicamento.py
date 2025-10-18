from sqlalchemy.orm import Session

from src.entities.medicamento import Medicamento


def create_medicamento(db: Session, medicamento: Medicamento):
    new_medicamento = Medicamento(
        ##Id_Medicamento=str(medicamento.Id_Medicamento),
        Nombre_Comercial=medicamento.Nombre_Comercial,
        Principio_Activo=medicamento.Principio_Activo,
        Presentacion=medicamento.Presentacion,
        Unidad_Medida=medicamento.Unidad_Medida,
        Precio_Unitario=medicamento.Precio_Unitario,
    )
    db.add(new_medicamento)
    db.commit()
    db.refresh(new_medicamento)
    return new_medicamento


def get_medicamento(db: Session, medicamento_id: int):
    return db.query(Medicamento).filter(Medicamento.Id_Medicamento == medicamento_id).first()


def get_medicamentos(db: Session):
    return db.query(Medicamento).all()


def update_medicamento(db: Session, medicamento_id: int, medicamento: Medicamento):
    db_medicamento = db.query(Medicamento).filter(Medicamento.Id_Medicamento == medicamento_id).first()
    if db_medicamento:
        db_medicamento.Nombre_Comercial=medicamento.Nombre_Comercial
        db_medicamento.Principio_Activo=medicamento.Principio_Activo
        db_medicamento.Presentacion=medicamento.Presentacion
        db_medicamento.Unidad_Medida=medicamento.Unidad_Medida
        db_medicamento.Precio_Unitario=medicamento.Precio_Unitario
        db.commit()
        db.refresh(db_medicamento)
    return db_medicamento


def delete_medicamento(db: Session, medicamento_id: int):
    db_medicamento = db.query(Medicamento).filter(Medicamento.Id_Medicamento == medicamento_id).first()
    if db_medicamento:
        db.delete(db_medicamento)
        db.commit()
    return db_medicamento
