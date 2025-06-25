#!/bin/bash

echo "Ejecutando las migraciones  rag"
docker compose exec -T rag python manage.py makemigrations api_rag
docker compose exec -T rag python manage.py migrate

#echo "Iniciando Celery..."
#docker compose exec -T scraper celery -A celeryconfig.app worker --loglevel=debug

