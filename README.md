ProyectoInvMedicamentos - API FastAPI

Estructura propuesta:

- main.py -> punto de entrada FastAPI
- database/ -> conexión y modelos SQLAlchemy
- schemas/ -> Pydantic models
- auth/ -> JWT helpers y dependencias
- routes/ -> Routers REST (medicamentos)

Setup rápido (Windows PowerShell):

1. Crear virtualenv

```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

2. Variables de entorno (usar `.env` en root). Ver `.env.example`

3. Ejecutar servidor

```powershell
uvicorn main:app --reload
```

ProyectoInvMedicamentos - API FastAPI

Estructura propuesta:

- `main.py` -> punto de entrada FastAPI
- `database/` -> conexión y modelos SQLAlchemy
- `schemas/` -> Pydantic models
- `auth/` -> JWT helpers y dependencias
- `routes/` -> Routers REST (medicamentos)

Setup rápido (Windows PowerShell):

1. Crear virtualenv

```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

2. Variables de entorno (usar `.env` en root). Ver `.env.example`

3. Crear tablas y usuario admin (solo la primera vez)

```powershell
python scripts/create_admin.py
```

4. Ejecutar servidor (usar el wrapper para cargar .env y arrancar uvicorn)

```powershell
./run.ps1
```

Alternativa: arrancar uvicorn desde Python (programmaticamente)

```powershell
python run_server.py
```

5. Probar autenticación y endpoints

- Obtener token: POST http://127.0.0.1:8000/api/auth/token (form data username/password)
- Usar token: añadir header `Authorization: Bearer <token>` para llamar a rutas protegidas.

Notas:
- Configurar `DB_SERVER` y `DB_NAME` en `.env` si difieren.
- JWT_SECRET debe ser cambiado en producción.
