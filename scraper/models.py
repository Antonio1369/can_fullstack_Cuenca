from sqlalchemy import Column, Integer, String, Text
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import Session
from database import engine, SessionLocal

Base = declarative_base()

class Noticias(Base):
    __tablename__ = "noticias"
    id = Column(Integer, primary_key=True, index=True)
    url = Column(String, unique=True)
    title = Column(String)
    content = Column(Text)
    embedding = Column(Text) 

Base.metadata.create_all(bind=engine)

def save_scraped_data(data: dict):
    db = SessionLocal()
    try:
        noticias = Noticias(**data)
        db.add(noticias)
        db.commit()
    except Exception as e:
        db.rollback()
        raise e
    finally:
        db.close()
