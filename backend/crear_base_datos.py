"""
Script para crear la base de datos en SQL Server
Solo necesario si usas SQL Server (no SQLite)
"""
import pyodbc
import os
from dotenv import load_dotenv

load_dotenv()

SERVER = os.getenv('DB_SERVER', 'SANTIAGO\\SQLEXPRESS')
DATABASE = os.getenv('DB_NAME', 'ProyectoInvMedicamentos')

print("=" * 70)
print("CREAR BASE DE DATOS EN SQL SERVER")
print("=" * 70)
print()

print(f"Servidor: {SERVER}")
print(f"Base de datos a crear: {DATABASE}")
print()

try:
    # Conectar a master (base de datos del sistema)
    print("1. Conectando a SQL Server (master)...")
    conn_str = f"DRIVER={{ODBC Driver 17 for SQL Server}};SERVER={SERVER};DATABASE=master;Trusted_Connection=yes"
    conn = pyodbc.connect(conn_str, timeout=5)
    cursor = conn.cursor()
    print("   ✓ Conexión exitosa")
    print()
    
    # Verificar si la base de datos ya existe
    print(f"2. Verificando si '{DATABASE}' ya existe...")
    cursor.execute(f"SELECT database_id FROM sys.databases WHERE name = '{DATABASE}'")
    exists = cursor.fetchone()
    
    if exists:
        print(f"   ℹ La base de datos '{DATABASE}' ya existe")
        print()
        
        respuesta = input("¿Deseas eliminarla y crearla de nuevo? (s/n): ").lower().strip()
        if respuesta == 's' or respuesta == 'si':
            print(f"   Eliminando base de datos '{DATABASE}'...")
            cursor.execute(f"DROP DATABASE [{DATABASE}]")
            conn.commit()
            print("   ✓ Base de datos eliminada")
        else:
            print("   Operación cancelada")
            conn.close()
            exit(0)
    
    # Crear la base de datos
    print(f"3. Creando base de datos '{DATABASE}'...")
    cursor.execute(f"CREATE DATABASE [{DATABASE}]")
    conn.commit()
    print(f"   ✓ Base de datos '{DATABASE}' creada exitosamente")
    print()
    
    # Verificar que se creó
    cursor.execute(f"SELECT database_id FROM sys.databases WHERE name = '{DATABASE}'")
    if cursor.fetchone():
        print("=" * 70)
        print("¡ÉXITO!")
        print("=" * 70)
        print()
        print(f"La base de datos '{DATABASE}' está lista para usar.")
        print()
        print("Ahora puedes:")
        print("  1. Crear las tablas: python create_tables.py")
        print("  2. Iniciar el backend: python main.py")
        print()
    
    conn.close()

except pyodbc.Error as e:
    print(f"✗ Error: {e}")
    print()
    print("Posibles causas:")
    print("  - SQL Server no está corriendo")
    print("  - No tienes permisos para crear bases de datos")
    print("  - El nombre del servidor es incorrecto")
    print()
    print("Soluciones:")
    print("  1. Verifica que SQL Server esté corriendo (services.msc)")
    print("  2. Ejecuta: python diagnosticar_sql.py")
    print("  3. O usa SQLite: python usar_sqlite.py")
    print()

except Exception as e:
    print(f"✗ Error inesperado: {e}")
    print()
