# 🏥 Proyecto: Sistema de Gestión de Inventario de Medicamentos

API y Frontend para la gestión integral de inventario de medicamentos, con enfoque en trazabilidad, alertas de stock/vencimiento y arquitectura limpia.

---

## 🚀 Quickstart: Ejecuta el Proyecto

### Requisitos Previos

* **Backend (Python):** Python 3.10+
* **Frontend (.NET):** .NET 7/8/9 SDK
* **Base de Datos:** SQL Server o SQLite (opcional, para desarrollo).

### ⚙️ 1. Configuración del Backend (FastAPI)

1.  **Crear Entorno Virtual e Instalar Dependencias:**
    ```powershell
    python -m venv .venv
    .\.venv\Scripts\Activate.ps1
    pip install -r backend/requirements.txt
    ```

2.  **Variables de Entorno:**
    * Copia [.env.example](.env.example) a un nuevo archivo llamado `.env`.
    * Ajusta las variables de conexión a la base de datos (`DB_SERVER`, `DB_NAME`, etc.) y el secreto JWT (`JWT_SECRET`).

3.  **Inicializar Base de Datos:**
    ```powershell
    # Crea tablas y el usuario administrador inicial
    python backend/scripts/create_admin.py
    ```

4.  **Ejecutar el Servidor:**
    ```powershell
    uvicorn backend.main:app --reload --host 127.0.0.1 --port 8000
    ```
    *(El servidor backend estará disponible en `http://127.0.0.1:8000`)*

### 🖥️ 2. Ejecución del Frontend (Blazor Server)

1.  **Navegar e Iniciar:**
    ```bash
    cd frontend
    dotnet restore
    dotnet run --project FrontEndBlazor.csproj
    ```
2.  **Acceso:** Abre tu navegador y navega a la dirección indicada (típicamente `http://localhost:port`).

---

## 🌟 Arquitectura y Patrones Implementados

El proyecto sigue una arquitectura modular y limpia, basada en Python/FastAPI para el backend.

| Capa | Propósito | Puntos Clave |
| :--- | :--- | :--- |
| **Routes/Controllers** | Manejo de peticiones HTTP. | Rutas **delgadas** que delegan la lógica al Service Layer. |
| **Service Layer** | **Reglas de Negocio** y **Transacciones**. | Lógica de inventario, validaciones, y manejo del `commit` a la BD. |
| **Repositories** | Abstracción del Acceso a Datos. | Implementa el **Repository Pattern**. No hace `commit`. |
| **Database** | Modelos de datos. | Definidos con **SQLAlchemy**. Incluye modelos para Auditoría. |

* **Patrones clave:** Repository Pattern, Service Layer, Dependency Injection. (Ver más detalles en [PATRONES_IMPLEMENTADOS.txt](PATRONES_IMPLEMENTADOS.txt)).

---

## 📁 Estructura Detallada del Repositorio

| Carpeta / Archivo | Componente | Descripción de Contenido |
| :--- | :--- | :--- |
| **`backend/`** | **FastAPI** | Código del API, lógica de negocio y acceso a datos. |
| ├── `main.py` | Configuración | Punto de entrada y montaje de *routers*. |
| ├── `database/` | Modelos BD | **Modelos SQLAlchemy** ([`models.py`](backend/database/models.py)). |
| ├── `schemas/` | DTOs/Esquemas | Modelos Pydantic para I/O (ej: `MedicamentoCreate`). |
| ├── `routes/` | API Endpoints | Controladores para `medicamentos`, `auth`, `alertas`, `users`. |
| ├── `services/` | Lógica Negocio | Implementación de la **Service Layer** (ej: `MedicamentoService`). |
| ├── `repositories/` | Acceso a Datos | Implementación de Repositorios (Interfaces e Implementaciones). |
| **`frontend/`** | **Blazor Server** | Interfaz de usuario que consume la API REST. |
| ├── `Program.cs` | DI / Setup | Configuración de Inyección de Dependencias, `HttpClients` y Auth. |
| ├── `Services/` | Clientes API | Lógica de comunicación con el Backend (ej: `MedicamentoService.cs`). |
| └── `Components/Pages` | UI | Páginas y componentes Razor (ej: `Medicamentos.razor`). |

---

## 🔗 Endpoints Principales de la API

| Funcionalidad | Método | Ruta (Ejemplo) | Descripción | Archivo Fuente |
| :--- | :--- | :--- | :--- | :--- |
| **Crear Medicamento** | `POST` | `/api/medicamentos/` | Registra nuevo medicamento. | [`backend/routes/medicamentos.py`](backend/routes/medicamentos.py) |
| **Listar Medicamentos** | `GET` | `/api/medicamentos/` | Lista todos o con filtros. | [`backend/routes/medicamentos.py`](backend/routes/medicamentos.py) |
| **Movimiento Stock** | `POST` | `/api/medicamentos/{id}/movimientos` | Registra entrada o salida de stock. | [`backend/routes/medicamentos.py`](backend/routes/medicamentos.py) |
| **Auditoría** | `GET` | `/api/medicamentos/{id}/audit` | Historial de auditoría. | [`backend/routes/medicamentos.py`](backend/routes/medicamentos.py) |
| **Alertas Stock** | `GET` | `/api/alertas/stock-bajo` | Lista medicamentos con stock bajo. | [`backend/routes/alertas.py`](backend/routes/alertas.py) |
| **Login** | `POST` | `/api/auth/login` | Obtiene token JWT. | [`backend/routes/auth.py`](backend/routes/auth.py) |

---

## 🛠️ Utilidades y Scripts

Los scripts son herramientas clave para el mantenimiento y desarrollo:

| Script | Propósito | Comando de Ejemplo |
| :--- | :--- | :--- |
| [`create_admin.py`](backend/scripts/create_admin.py) | **Setup inicial:** Crea la estructura de BD y el usuario `admin`. | `python backend/scripts/create_admin.py` |
| [`crear_base_datos.py`](backend/crear_base_datos.py) | Crea la base de datos física (útil para SQL Server). | `python backend/crear_base_datos.py` |
| [`check_inactivos.py`](backend/scripts/check_inactivos.py) | Revisa medicamentos con movimientos que fueron marcados como inactivos. | `python backend/scripts/check_inactivos.py` |

---

## ✅ Pruebas Unitarias

El backend utiliza `pytest` para la ejecución de pruebas.

```bash
cd backend
pip install -r requirements.txt # Asegurar dependencias de test
pytest -q