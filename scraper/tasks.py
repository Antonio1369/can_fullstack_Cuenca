from celeryconfig import celery_app
from apps.scraper import scrape_url
from crud import save_userfile_metadata, save_scraped_data, update_userfile_status

@celery_app.task(bind=True)  
def process_links_task(self, links: list[str], userId: int):
    print(f"Procesando links {links} para usuario {userId}")

    for url in links:
        try:
            result = scrape_url(url, userId)
            save_scraped_data(result)
        except Exception as e:
            print(f"Error procesando {url}: {str(e)}")

    update_userfile_status(task_id=self.request.id, processed=True)
    print(f"Tarea {self.request.id} finalizada y userfile actualizado")

@celery_app.task(bind=True)
def process_userfile_data(self, userId: int, filename: str, task_id: str, processed: bool):
    print(f"Registrando metadata: userId={userId}, archivo={filename}")
    try:
        save_userfile_metadata(
            user_id=userId,
            filename=filename,
            task_id=task_id,
            processed=processed
        )
    except Exception as e:
        print(f"Error al guardar metadata: {e}")