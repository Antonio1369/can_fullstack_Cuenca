import requests
from bs4 import BeautifulSoup
from database import SessionLocal
from models import Noticias
from settings import OLLAMA_EMBEDDING_URL
import json

def scrape_url(url: str) -> dict:
    try:
        response = requests.get(url, timeout=10)
        response.raise_for_status()

        soup = BeautifulSoup(response.text, 'html.parser')
        title = soup.title.string.strip() if soup.title else "Sin título"
        content = " ".join([p.get_text() for p in soup.find_all('p')])

        # Llamada al modelo de embeddings de Ollama
        embedding = generate_embedding(content)

        # Devolver el resultado con el embedding
        return {
            "url": url,
            "title": title,
            "content": content,
            "embedding": embedding
        }
    except Exception as e:
        return {
            "url": url,
            "title": "Error",
            "content": str(e),
            "embedding": None
        }

def generate_embedding(text: str) -> list:
    """Genera el embedding utilizando el modelo de Ollama"""
    headers = {
        "Content-Type": "application/json",
    }
    data = {
        "model": "nomic-embed-text:latest", 
        "prompt": text
    }

    response = requests.post(f"{OLLAMA_EMBEDDING_URL}/api/embeddings", headers=headers, json=data)
    
    if response.status_code == 200:
        json_response = response.json()
        return json_response.get("embedding", [])
    else:
        return []

def save_scraped_data(data: dict):
    db = SessionLocal()
    try:
        # Guardar los datos y el embedding
        noticias = Noticias(
            url=data["url"],
            title=data["title"],
            content=data["content"],
            embedding=json.dumps(data["embedding"])  # Guardamos el embedding como un JSON
        )
        db.add(noticias)
        db.commit()
    except Exception as e:
        db.rollback()
        print(f"Error saving data: {str(e)}")
    finally:
        db.close()
