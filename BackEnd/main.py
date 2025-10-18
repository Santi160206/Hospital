import uvicorn
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
import logging
from sqlalchemy import create_engine, text
from Database.conexion import DATABASE_URL
from src.entities import __all__
from Database.conexion import *
from usuarios_iniciales import create_initial_users

from src.routers import (auth, Medicamentos)


app = FastAPI(
    title="API Hospital",
    version="1.0.0",
    description="API REST básica para gestión de un Hospital",
    openapi_tags=[],
)

"""
    Configura la política CORS de la aplicación FastAPI.
    
    - Permite todas las fuentes (orígenes).
    - Permite todas las credenciales.
    - Permite todos los métodos HTTP (GET, POST, PUT, DELETE, etc.).
    - Permite todas las cabeceras.
"""

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(auth.router)
app.include_router(Medicamentos.router)

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

""" Se crean las tablas al ejecutar el servidor"""


@app.on_event("startup")
def startup_event():
    create_tables()
    create_initial_users()
    print("✅ Tablas creadas al iniciar FastAPI")


@app.on_event("shutdown")
def shutdown_event():
    drop_tables()
    print("✅ Tablas Eliminadas al cerrar FastAPI")


def main():
    print("Iniciando servidor FastAPI...")
    uvicorn.run(
        "main:app",
        # host="0.0.0.0",
        port=8000,
        reload=True,
        log_level="info",
    )


if __name__ == "__main__":
    main()


@app.get(
    "/",
    summary="Información de la API",
    description="Endpoint raíz que proporciona información básica sobre la API",
    tags=["General"],
)
def root():
    return {
        "message": "Bienvenido a la API del Hospital",
        "version": "1.0.0",
        "descripcion": "Sistema para gestión de medicamentos en un hospital",
    }
