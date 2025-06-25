import requests
from bs4 import BeautifulSoup
from settings import OLLAMA_EMBEDDING_URL

def scrape_url(url: str, userId: int) -> dict:
    try:
        response = requests.get(url, timeout=10)
        response.raise_for_status()

        soup = BeautifulSoup(response.text, 'html.parser')
        title = soup.title.string.strip() if soup.title else "Sin título"
        content = " ".join([p.get_text() for p in soup.find_all('p')])

        embedding = generate_embedding(content)

        return {
            "url": url,
            "title": title,
            "content": content,
            "embedding": embedding,
            "userId": userId
        }
    except Exception as e:
        return {
            "url": url,
            "title": "Error",
            "content": str(e),
            "embedding": None,
            "userId": userId
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

