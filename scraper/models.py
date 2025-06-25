from sqlalchemy import Column, Integer, String, Boolean, DateTime, Text
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import Session
from sqlalchemy.sql import func
from database import engine, SessionLocal

Base = declarative_base()

class Noticias(Base):
    __tablename__ = "noticias"
    id = Column(Integer, primary_key=True, index=True)
    url = Column(String, unique=True)
    title = Column(String)
    content = Column(Text)
    embedding = Column(Text) 

class UserFile(Base):
    __tablename__ = "user_files"
    id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, nullable=False)  # Relación simple con usuario (int)
    filename = Column(String, nullable=False)
    task_id = Column(String, nullable=True)
    uploaded_at = Column(DateTime(timezone=True), server_default=func.now())
    processed = Column(Boolean, default=False)

Base.metadata.create_all(bind=engine)
