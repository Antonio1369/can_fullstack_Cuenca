FROM python:3.11-slim

WORKDIR /app

COPY . .

RUN pip install -r requirements.txt

RUN python manage.py makemigrations api_rag
RUN python manage.py migrate

RUN python manage.py collectstatic --noinput

EXPOSE 8001

CMD ["python", "manage.py", "runserver", "0.0.0.0:8001", "--insecure"]
