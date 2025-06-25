from sqlalchemy.orm import Session
from database import SessionLocal
from models import Noticias, UserFile
import json
from datetime import datetime

def update_userfile_status(task_id: str, processed: bool = True):
    db: Session = SessionLocal()
    try:
        userfile = db.query(UserFile).filter(UserFile.task_id == task_id).first()
        if not userfile:
            print(f"No se encontró UserFile con task_id: {task_id}")
            return False
        
        userfile.processed = processed
        userfile.updated_at = datetime.utcnow()  
        db.commit()
        return True
    except Exception as e:
        db.rollback()
        print(f"Error actualizando UserFile status: {str(e)}")
        return False
    finally:
        db.close()
    
    
def save_scraped_data(data: dict):
    db = SessionLocal()
    try:
        noticias = Noticias(
            url=data["url"],
            title=data["title"],
            content=data["content"],
            embedding=json.dumps(data["embedding"]),  
        )
        db.add(noticias)
        db.commit()
    except Exception as e:
        db.rollback()
        print(f"Error saving data: {str(e)}")
    finally:
        db.close()

def save_userfile_metadata(user_id: int, filename: str, task_id: str, processed: bool = False):
    db = SessionLocal()
    try:
        print(f"Intentando guardar UserFile - user_id: {user_id}, filename: {filename}, task_id: {task_id}")  # Debug
        
        if not isinstance(user_id, int) or user_id <= 0:
            raise ValueError("user_id debe ser un entero positivo")
        if not filename or not isinstance(filename, str):
            raise ValueError("filename debe ser un string no vacío")
        if not task_id or not isinstance(task_id, str):
            raise ValueError("task_id debe ser un string no vacío")
        
        new_file = UserFile(
            user_id=user_id,
            filename=filename,
            task_id=task_id,
            processed=processed,
            uploaded_at=datetime.utcnow()  
        )
        
        db.add(new_file)
        db.flush()  
        print(f"UserFile antes de commit: ID={new_file.id}")  # Debug
        
        db.commit()
        print(f"UserFile guardado exitosamente con ID: {new_file.id}")
        return new_file.id
        
    except Exception as e:
        db.rollback()
        print(f"Error al guardar UserFile metadata: {type(e).__name__}: {str(e)}")
        import traceback
        traceback.print_exc()
        return None
    finally:
        db.close()