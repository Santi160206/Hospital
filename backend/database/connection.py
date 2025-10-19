from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker, declarative_base
import os
from dotenv import load_dotenv
from urllib.parse import quote_plus

load_dotenv()

# Connection details for SQL Server with Windows Authentication
SERVER = os.getenv('DB_SERVER', 'SANTIAGO\\SQLEXPRESS')
DATABASE = os.getenv('DB_NAME', 'ProyectoInvMedicamentos')

# Build ODBC connection string and quote it for SQLAlchemy + pyodbc
odbc_str = (
    f"DRIVER={{ODBC Driver 17 for SQL Server}};SERVER={SERVER};DATABASE={DATABASE};Trusted_Connection=yes"
)
connection_string = "mssql+pyodbc:///?odbc_connect=" + quote_plus(odbc_str)

# Create engine
engine = create_engine(connection_string, echo=False, fast_executemany=True)
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)
Base = declarative_base()


def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()
