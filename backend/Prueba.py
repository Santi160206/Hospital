import pyodbc

try:
    conn = pyodbc.connect(
        "DRIVER={ODBC Driver 17 for SQL Server};"
        "SERVER=127.0.0.1,1433;"
        "DATABASE=master;"
        "UID=juan;"
        "PWD=TuClave123;"
        "Encrypt=no;"
        "TrustServerCertificate=yes;",
        timeout=5
    )
    cur = conn.cursor()
    cur.execute("SELECT @@VERSION;")
    row = cur.fetchone()
    if row is not None:
        print("✅ Conectado correctamente:\n", row[0])
    else:
        print("⚠️ Consulta no devolvió resultados")
    cur.close()
    conn.close()
except Exception as e:
    print("❌ Error de conexión:", e)
