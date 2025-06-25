from celery import Celery
from settings import REDIS_URL

def make_celery():
    celery_app = Celery('scraper', broker=REDIS_URL, backend=REDIS_URL)
    celery_app.conf.update(
        task_serializer='json',
        accept_content=['json'],
        result_serializer='json',
        timezone='UTC',
        enable_utc=True,
        task_routes={
            'tasks.process_links_task': {'queue': 'scraper_queue'},
            'tasks.process_userfile_data':  {'queue': 'scraper_queue'},
        },
    )
    return celery_app

celery_app = make_celery()
