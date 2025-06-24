from celery import Celery
from apps.scraper import scrape_url
from models import save_scraped_data
from settings import REDIS_URL

celery = Celery("tasks", broker=REDIS_URL)

@celery.task
def process_links_task(links: list[str]):
    for url in links:
        #manejo de try catch para evitar cargar los workers
        try:
            result = scrape_url(url)
            save_scraped_data(result)
        except Exception as e:
            print(f"Error processing {url}: {str(e)}")
