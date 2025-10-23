from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi import FastAPI
from fastapi.openapi.utils import get_openapi
import os
import uvicorn
from database.connection import engine, Base, get_db

from routes import medicamentos, auth, users  # alertas se dejará para después
from fastapi.staticfiles import StaticFiles
from fastapi.responses import FileResponse
from pathlib import Path


from contextlib import asynccontextmanager


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
# app.include_router(alertas.router, prefix="/api/alertas", tags=["alertas"])  # Por implementar después


# NOTE: Removed OpenAPI Bearer/Authorize button per request (no security scheme added)

@app.get("/")
async def root():
    return {"message": "API ProyectoInvMedicamentos"}


@asynccontextmanager
async def lifespan(app: FastAPI):
    # create tables at startup
    Base.metadata.create_all(bind=engine)
    # ensure at least one admin exists
    try:
        from services.user_service import UserService
        from auth.passwords import hash_password
        import os

        db = next(get_db())
        usvc = UserService(db)
        admin_count = usvc.count_admins()
        if admin_count == 0:
            # attempt to create admin from environment variables
            admin_user = os.getenv('ADMIN_USERNAME')
            admin_pass = os.getenv('ADMIN_PASSWORD')
            admin_email = os.getenv('ADMIN_EMAIL')
            if admin_user and admin_pass and admin_email:
                payload = {
                    'username': admin_user,
                    'full_name': 'Administrator',
                    'email': admin_email,
                    'hashed_password': hash_password(admin_pass),
                    'role': 'admin'
                }
                try:
                    usvc.create_admin(payload)
                    print('Admin user created from environment variables')
                except Exception as e:
                    print('Failed to create admin user at startup:', e)
            else:
                print('WARNING: No admin user exists and ADMIN_* env vars are not set. Please create an admin via the protected /api/users/create_admin endpoint or set ADMIN_USERNAME, ADMIN_PASSWORD, ADMIN_EMAIL.')
    except Exception as _:
        # do not fail startup if admin creation check error occurs; log and continue
        pass
    yield


app.router.lifespan_context = lifespan

# Serve frontend static files when they are published to backend/static
STATIC_DIR = Path(__file__).parent.joinpath('static')
if STATIC_DIR.exists():
    app.mount('/', StaticFiles(directory=str(STATIC_DIR), html=True), name='static')

    
@app.on_event("shutdown")
def shutdown_event():
    print("Eliminando tablas antes de cerrar...")
    Base.metadata.drop_all(bind=engine)

if __name__ == '__main__':
    # Allow running directly with: python main.py
    host = os.getenv('HOST', '127.0.0.1')
    port = int(os.getenv('PORT', 8000))
    reload = os.getenv('RELOAD', 'True').lower() in ('1','true','yes')
    print(f"Starting uvicorn on {host}:{port} (reload={reload})")
    uvicorn.run('main:app', host=host, port=port, reload=reload)
