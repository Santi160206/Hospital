"""
Script para crear usuarios iniciales del sistema.
"""

from sqlalchemy.orm import Session
from Database.conexion import SessionLocal
from src.controller.auth_controller import create_user
from src.schemas.auth import UserCreate



def create_initial_users():
    db: Session = SessionLocal()

    try:
        users_to_create = [
            UserCreate(
                Username="Santi001",
                Correo="Gonzalez@hospital.com",
                Telefono="3258796345",
                Nombre="Santiago G",
                password="sant123",
                Rol="admin",
            ),
            UserCreate(
                Username="Emmanuel15",
                Correo="Emmanuel@hospital.com",
                Telefono="3008964725",
                Nombre="Emmanuel",
                password="Emma123",
                Rol="admin",
            ),
            UserCreate(
                Username="Santi002",
                Correo="Alvarez@hospital.com",
                Telefono="3008964725",
                Nombre="Santiago A",
                password="sant456",
                Rol="admin",
            ),
            UserCreate(
                Username="JuanM77",
                Correo="Juan@hospital.com",
                Telefono="3008964725",
                Nombre="Juan Manuel",
                password="Juan123",
                Rol="admin",
            )
        ]

        for user in users_to_create:
            try:
                user.password = user.password[:72]
                created_user = create_user(db, user)
                print(
                    f"✅ Usuario creado: {created_user.Username} - Rol: {created_user.Rol}"
                )
            except Exception as e:
                db.rollback()
                print(f"❌ No se pudo crear el usuario {user.Username}: {e}")

    except Exception as e:
        print(f"❌ Error general al crear usuarios iniciales: {e}")

    finally:
        db.close()
