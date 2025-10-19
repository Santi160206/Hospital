from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi import FastAPI
from fastapi.openapi.utils import get_openapi
import os
import uvicorn
from database.connection import engine, Base

from routes import medicamentos, auth, users


app = FastAPI(title="ProyectoInvMedicamentos API")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(auth.router, prefix="/api/auth", tags=["auth"])
app.include_router(medicamentos.router, prefix="/api/medicamentos", tags=["medicamentos"])
app.include_router(users.router, prefix="/api/users", tags=["users"])


# NOTE: Removed OpenAPI Bearer/Authorize button per request (no security scheme added)

@app.get("/")
async def root():
    return {"message": "API ProyectoInvMedicamentos"}


@app.on_event('startup')
def ensure_tables():
    # Create database tables at startup if they don't exist
    Base.metadata.create_all(bind=engine)


if __name__ == '__main__':
    # Allow running directly with: python main.py
    host = os.getenv('HOST', '127.0.0.1')
    port = int(os.getenv('PORT', 8000))
    reload = os.getenv('RELOAD', 'True').lower() in ('1','true','yes')
    print(f"Starting uvicorn on {host}:{port} (reload={reload})")
    uvicorn.run('main:app', host=host, port=port, reload=reload)
