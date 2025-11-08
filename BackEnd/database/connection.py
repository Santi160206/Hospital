from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker, declarative_base
import os
from dotenv import load_dotenv
from urllib.parse import quote_plus

load_dotenv()

SERVER = os.getenv('DB_SERVER', 'localhost\\SQLEXPRESS')
DATABASE = os.getenv('DB_NAME', 'ProyectoInvMedicamentos')

odbc_str = (
    f"DRIVER={{ODBC Driver 17 for SQL Server}};SERVER={SERVER};DATABASE={DATABASE};Trusted_Connection=yes"
)
connection_string = "mssql+pyodbc:///?odbc_connect=" + quote_plus(odbc_str)

engine = create_engine(connection_string, echo=False, fast_executemany=True)
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)
Base = declarative_base()


def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()
