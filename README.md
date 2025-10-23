// ...existing code...
# ProyectoInvMedicamentos

API y frontend para gestión de inventario de medicamentos (FastAPI + SQLAlchemy backend, Blazor Server frontend).

---

## Contenido / Estructura del repositorio

Raíz:
- [.env.example](.env.example)
- [README.md](README.md)
- [Narrativa.txt](Narrativa.txt)
- [PATRONES_IMPLEMENTADOS.txt](PATRONES_IMPLEMENTADOS.txt)
- [Proyecto.sln](Proyecto.sln)

Backend (Python / FastAPI)
- Entrada y configuración: [`main.app`](backend/main.py) — [backend/main.py](backend/main.py)
- Conexión y modelos: [backend/database/models.py](backend/database/models.py)
- Esquemas (Pydantic): [`MedicamentoCreate` / `MedicamentoOut` en `medicamento_v2`](backend/schemas/medicamento_v2.py) — [backend/schemas/medicamento_v2.py](backend/schemas/medicamento_v2.py)
- Esquema auditoría: [`AuditLogOut`](backend/schemas/audit.py) — [backend/schemas/audit.py](backend/schemas/audit.py)
- Respuestas comunes: [backend/schemas/response.py](backend/schemas/response.py)
- Rutas / Controllers:
  - Medicamentos: [`medicamentos.router`](backend/routes/medicamentos.py) — [backend/routes/medicamentos.py](backend/routes/medicamentos.py)
  - Auth: [`auth` routes y `login` endpoints](backend/routes/auth.py) — [backend/routes/auth.py](backend/routes/auth.py)
  - Users: [backend/routes/users.py](backend/routes/users.py)
  - Alertas (stock / vencimientos): [backend/routes/alertas.py](backend/routes/alertas.py)
- Service Layer:
  - Lógica negocio medicamentos: [`MedicamentoService`](backend/services/medicamento_service.py) — [backend/services/medicamento_service.py](backend/services/medicamento_service.py)
- Repositories (Repository Pattern):
  - Interfaz: [`IMedicamentoRepository`](backend/repositories/interfaces/medicamento_repository.py) — [backend/repositories/interfaces/medicamento_repository.py](backend/repositories/interfaces/medicamento_repository.py)
  - Implementación movimientos: [`MovimientoRepository`](backend/repositories/movimiento_repo.py) — [backend/repositories/movimiento_repo.py](backend/repositories/movimiento_repo.py)
- Scripts útiles:
  - Crear BD: [backend/crear_base_datos.py](backend/crear_base_datos.py)
  - Crear tablas y admin: [backend/scripts/create_admin.py](backend/scripts/create_admin.py) — [backend/create_tables.py](backend/create_tables.py)
  - Migraciones / checks: [backend/scripts/ensure_unique_searchkey.py](backend/scripts/ensure_unique_searchkey.py), [backend/scripts/check_inactivos.py](backend/scripts/check_inactivos.py), [backend/scripts/fix_inactivos.py](backend/scripts/fix_inactivos.py)
- Dependencias / requirements: [backend/requirements.txt](backend/requirements.txt)

Frontend (Blazor Server)
- Proyecto: [frontend/FrontEndBlazor.csproj](frontend/FrontEndBlazor.csproj)
- Boot / DI: [frontend/Program.cs](frontend/Program.cs) — [`TokenService` / AuthMessageHandler` registrations](frontend/Program.cs)
- Servicios cliente:
  - Auth client: [frontend/Services/AuthService.cs](frontend/Services/AuthService.cs)
  - Medicamento client: [frontend/Services/MedicamentoService.cs](frontend/Services/MedicamentoService.cs)
- Páginas y componentes:
  - Medicamentos UI: [frontend/Components/Pages/Medicamentos.razor](frontend/Components/Pages/Medicamentos.razor)
  - Home: [frontend/Components/Pages/Home.razor](frontend/Components/Pages/Home.razor)
  - Login / Register: [frontend/Components/Pages/Login.razor](frontend/Components/Pages/Login.razor), [frontend/Components/Pages/Register.razor](frontend/Components/Pages/Register.razor)
- DTOs y modelos: [frontend/Models/MedicamentoDto.cs](frontend/Models/MedicamentoDto.cs), [frontend/Models/AuditLogDto.cs](frontend/Models/AuditLogDto.cs), [frontend/Models/ApiResponses.cs](frontend/Models/ApiResponses.cs)

---

## Arquitectura resumida

1. Rutas (FastAPI) — validación/HTTP → [`backend/routes/*.py`](backend/routes)  
2. Service Layer (Python) — reglas de negocio, transacciones: [`MedicamentoService`](backend/services/medicamento_service.py)  
3. Repositories — acceso a datos (no hacen commit): [`IMedicamentoRepository`](backend/repositories/interfaces/medicamento_repository.py), [`MovimientoRepository`](backend/repositories/movimiento_repo.py)  
4. Base de datos — modelos SQLAlchemy: [backend/database/models.py](backend/database/models.py)  
5. Frontend consume la API mediante HttpClients configurados en [`frontend/Program.cs`](frontend/Program.cs) y servicios en [frontend/Services](frontend/Services).

Ver también la descripción de patrones implementados: [PATRONES_IMPLEMENTADOS.txt](PATRONES_IMPLEMENTADOS.txt)

---

## Instalación y quickstart

Requisitos:
- Python 3.10+
- .NET 7/8/9 SDK (según proyecto)
- SQL Server (o usar SQLite para desarrollo)
- Ver [backend/requirements.txt](backend/requirements.txt) y [frontend/FrontEndBlazor.csproj](frontend/FrontEndBlazor.csproj)

Backend (Windows PowerShell)
1. Crear virtualenv e instalar:
```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r backend/requirements.txt
```
2. Copiar y editar variables de entorno:
- Copiar [.env.example](.env.example) → `.env` y ajustar `DB_SERVER`, `DB_NAME`, `JWT_SECRET`, `ADMIN_*`
3. Crear tablas y admin (primera vez):
```powershell
python backend/scripts/create_admin.py
# o
python backend/create_tables.py
```
4. Ejecutar servidor:
```powershell
uvicorn backend.main:app --reload --host 127.0.0.1 --port 8000
```
- Punto de entrada y routers: [`main.app`](backend/main.py)

Frontend (Blazor Server)
1. Restaurar y ejecutar:
```bash
cd frontend
dotnet restore
dotnet run --project FrontEndBlazor.csproj
```
2. Configurar URL backend si es necesario en `frontend/appsettings.json` o variable `BACKEND_URL` — ver [`frontend/Program.cs`](frontend/Program.cs).

---

## Principales endpoints (ejemplos)

- Auth
  - POST /api/auth/signin (form) — endpoint en [backend/routes/auth.py](backend/routes/auth.py)
  - POST /api/auth/login (JSON) — [backend/routes/auth.py](backend/routes/auth.py)

- Medicamentos
  - POST /api/medicamentos/ — crear → [backend/routes/medicamentos.py::crear_medicamento](backend/routes/medicamentos.py)
  - GET /api/medicamentos/ — listar → [backend/routes/medicamentos.py](backend/routes/medicamentos.py)
  - PUT /api/medicamentos/{med_id} — actualizar → [backend/routes/medicamentos.py](backend/routes/medicamentos.py)
  - DELETE /api/medicamentos/{med_id} — eliminar (soft/delete/inactivar) → [backend/routes/medicamentos.py](backend/routes/medicamentos.py)
  - POST /api/medicamentos/{med_id}/movimientos — registrar movimiento → [backend/routes/medicamentos.py](backend/routes/medicamentos.py)
  - GET /api/medicamentos/{med_id}/audit — historial auditoría → [backend/routes/medicamentos.py::listar_auditoria_medicamento](backend/routes/medicamentos.py)

- Alertas
  - GET /api/alertas/stock-bajo — [backend/routes/alertas.py](backend/routes/alertas.py)
  - GET /api/alertas/vencimientos?dias=30 — [backend/routes/alertas.py](backend/routes/alertas.py)

Frontend pages consume estas rutas usando:
- [`MedicamentoService`](frontend/Services/MedicamentoService.cs)
- [`AuthService`](frontend/Services/AuthService.cs)

---

## Scripts y utilidades

- Crear base de datos SQL Server: [backend/crear_base_datos.py](backend/crear_base_datos.py)
- Crear tablas: [backend/create_tables.py](backend/create_tables.py)
- Crear admin: [backend/scripts/create_admin.py](backend/scripts/create_admin.py)
- Verificar / arreglar inactivos: [backend/scripts/check_inactivos.py](backend/scripts/check_inactivos.py), [backend/scripts/fix_inactivos.py](backend/scripts/fix_inactivos.py)
- Test rápido API: [backend/scripts/test_api.py](backend/scripts/test_api.py)
- Debug registro movimiento: [backend/scripts/debug_registrar_movimiento.py](backend/scripts/debug_registrar_movimiento.py)

---

## Tests

Backend uses pytest. Ejecutar:
```bash
cd backend
pip install -r requirements.txt
pytest -q
```

---

## Buenas prácticas y notas de desarrollo

- Routes delgadas → delegan al Service Layer (ver [PATRONES_IMPLEMENTADOS.txt](PATRONES_IMPLEMENTADOS.txt)).
- Repositorios NO hacen commit; Services manejan transacciones.
- Detección de duplicados: `search_key` normalizado (nombre|presentacion|fabricante) — ver lógica en [backend/routes/medicamentos.py](backend/routes/medicamentos.py) y [backend/services/medicamento_service.py](backend/services/medicamento_service.py).
- Soft-delete vs inactivate: el sistema marca inactivos si existen movimientos. Scripts para revisar/recuperar: [backend/scripts/check_inactivos.py](backend/scripts/check_inactivos.py), [backend/scripts/fix_inactivos.py](backend/scripts/fix_inactivos.py).
- Auditoría: logs guardados → esquema [`AuditLogOut`](backend/schemas/audit.py).

---

## Contribuir

1. Abrir issue / discusión.
2. Crear branch feature/bugfix.
3. Añadir tests para cambios en lógica de negocio.
4. PR con descripción y referencia a issues.

---

Si querés, puedo:
- Añadir ejemplos de request/response (cURL / httpx / Postman).
- Generar diagramas (arquitectura o ER).
- Extender README con políticas de despliegue y variables de entorno detalladas.

Referencias rápidas:
- [`main.app`](backend/main.py) — [backend/main.py](backend/main.py)  
- Rutas medicamentos: [`medicamentos.router`](backend/routes/medicamentos.py) — [backend/routes/medicamentos.py](backend/routes/medicamentos.py)  
- Lógica: [`MedicamentoService`](backend/services/medicamento_service.py) — [backend/services/medicamento_service.py](backend/services/medicamento_service.py)  
- Repositorio: [`IMedicamentoRepository`](backend/repositories/interfaces/medicamento_repository.py) — [backend/repositories/interfaces/medicamento_repository.py](backend/repositories/interfaces/medicamento_repository.py)  
- Modelos BD: [backend/database/models.py](backend/database/models.py)  
- Frontend DI / HttpClients: [frontend/Program.cs](frontend/Program.cs)  
- UI Medicamentos: [frontend/Components/Pages/Medicamentos.razor](frontend/Components/Pages/Medicamentos.razor)
}
// ...existing code...