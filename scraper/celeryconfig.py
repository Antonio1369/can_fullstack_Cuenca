from celery import Celery
from settings import REDIS_URL

app = Celery('scraper', broker=REDIS_URL, backend=REDIS_URL)

# Otras configuraciones de Celery
app.conf.update(
    result_backend=REDIS_URL,
    task_serializer='json',
    accept_content=['json'],
    result_serializer='json',
    timezone='UTC',
    enable_utc=True,
)
