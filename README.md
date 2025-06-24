# Prueba Técnica: Procesamiento de Links con Microservicios y consulta de información usando LLM


![Diagrama de Arquitectura](diagrama-sgcan-f15-2025.jpg)

## Descripción

Se te ha dado la tarea de scrapear las notas de prensa de la página <https://www.comunidadandina.org/notas-de-prensa/>

El objetivo de esta tarea es implementar un sistema basado en microservicios para procesar un archivo .txt que contiene varios links de noticias. El sistema debe realizar las siguientes funciones:

1. **Subida de Archivos**: Una vez cargado el archivo, los links extraídos son procesados de manera asíncrona mediante un sistema de colas. Este enfoque distribuye las tareas en segundo plano para evitar la sobrecarga del servidor. Los archivos de prueba estan ubicados en la carpeta [/files](files)
2. **Scrapeo de Links**: Despues de cargar el archivo, procesar los links del archivo utilizando colas para evitar bloquear el frontend.
3. **Almacenamiento**: Guardar el contenido scrapeado de cada link en una base de datos (Postgres, MySQL, MSSQL).
4. **Implementación de LLM**: Una vez obtenida la información, deberás crear un sistema **RAG** el cual permita consultar la información obtenida de las notas de prensa mediante un agente o chatbot. <br>
***Nota**: En caso de no desarrollar un RAG, puedes optar por desarrollar un sistema de **búsqueda semántica** de las notas de prensa utilizando **embeddings**.
5. **Frontend**: Proveer una interfaz de usuario desarrollada con **React** o **Next.js** (usando TypeScript).
6. **Backend**: Utilizar arquitectura de microservicios con al menos 2 de estas tecnologías: **.NET**, **Python** o **Node**.
7. **Despliegue**: Toda la solución debe ser desplegable mediante Docker, con un único archivo `docker-compose.yml`.

## Evaluación

- Se evaluará el uso de Clean Code, optimización y uso de buenas practicas de código.
- Se evaluará el uso de Redis en la solución.
- Se valorará el uso de pruebas unitarias.
- Se valorará el uso patrones de diseño.

## Requisitos Técnicos

1. **Frontend**: 
   - La plantilla incluida está implementada con **React** (pueden optar por utilizar otra plantilla de esta tecnología).
   - Desarrollado con **React** o **Next.js**.
   - Mostrar un listado de los archivos subidos con su estado de procesamiento, fecha de subida y cantidad de links procesados por archivo.
   - Mostrar un listado de los links scrapeados por archivo, incluyendo el contenido extraído.
   - **LLM**:
      - **RAG**: crear una interfaz tipo chatbot que permita comunicarse con el LLM.
      - En el caso de optar por una **búsqueda semántica**: crear una interfaz que permita realizar una búsqueda semántica y listar los resultados obtenidos.
   - BONUS ADICIONAL:
      - Permitir a los usuarios registrarse e iniciar sesión.
      - Listar archivos subidos por el usuario logueado.

2. **Backend**: Arquitectura de **microservicios**:
    - **Hub API**:
      - Funciona como único punto de entrada para el frontend y se comunica con los demas microservicios.
      - Implementado en **.NET**.
      - BONUS ADICIONAL:
        - Implementar autenticación via JWT.
    - **Servicio de Scrapeo**:
      - Procesa los links de los archivos subidos utilizando colas para evitar bloquear el sistema.
      - Implementado con **Python (FastAPI)**.
      - Utilizar Redis, Celery para las colas.
    - **RAG**:
      - Utilizar un framework como **LlamaIndex** o **Langchain**.
      - Utilizar storage para almacenar los vectores y no generarlos nuevamente por cada consulta.
      - Opción 2 (alternativa a RAG) - **Búsqueda semántica**:
        - Generar los embeddings de las información de las notas de prensa e implementar una solución para realizar una búsqueda.
      - **Nota**: En cualquiera de las opciones anteriores se deberá crear un servicio que exponga un endpoint que permita realizar las consultas. El servicio puede ser implementado con cualquiera de estas 3 tecnologías: Python (Fastapi, Django, etc.), Nodejs (Express, NestJS, etc.) o .NET

3. **Infraestructura**:
   - **Docker**: Toda la solución debe ser levantada con Docker utilizando un único archivo `docker-compose.yml`.
   - **Base de datos**: Utilizar **PostgreSQL** para almacenar los datos.
   - **Base de datos en memoria**: Utilizar **Redis** para almacenar las colas del scrapper.
   - **Base de datos de vectores**: Se puede utilizar cualquier tipo de base de datos de vectores (faiss, postgres Pgvector, chromadb, etc.).
      - **Nota**: En el caso del **RAG** puedes optar por utilizar el almacenamiento de vectores en archivos.

---

## Arquitectura del Proyecto

La arquitectura de carpetas recomendada del proyecto.

```plaintext
solution
├── hub/                        # Servicio Hub (API Gateway con .NET)
│   ├── Controllers/            # Controladores
│   ├── Models/                 # Modelos
│   └── Program.cs              # Configuración principal
│   └── Dockerfile              # Configuración docker del servicio.
├── react/                      # Frontend (React o Next.js) typescript.
│   ├── src/                    # Código fuente
│   ├── public/                 # Archivos estáticos
│   └── package.json            # Dependencias
├── scraper/                    # Servicio de Scrapeo (FastAPI)
│   ├── apps/                   # Lógica del scrapeo
│   ├── models.py               # Modelos de la base de datos
│   ├── tasks.py                # Lógica de colas
│   └── settings.py             # Configuración
│   └── Dockerfile              # Configuración docker del servicio.
├── rag/                        # RAG app
│   ├── rag.py                  # RAG implementado en LlamaIndex
│   └── main.py                 # Servicio que expone el RAG implementado
├── docker-compose.yml          # Orquestación de contenedores
├── .env                        # Variables de entorno
└── README.md                   # Este archivo
```

## Puertos
```plaintext
| Servicio                  | Puerto |
|---------------------------|--------|
| frontend                  | 9999   |
| hub                       | 9010   |
| scraper                   | 9020   |
| RAG o Búsqueda semántica  | 9030   |
| postgres                  | 9040   |
| redis                     | 9050   |
```

## Endpoints Principales

**Hub API**
- `POST /register`: Registro de usuarios.
- `POST /upload`: Subida de archivos CSV/txt.
- `GET /files`: Listado de archivos subidos por el usuario.
- `GET /files/{file_id}/links`: Listado de links procesados.

- Opcional: `POST /login`: Inicio de sesión y obtención de JWT.

**Servicio de Scrapeo**
- `POST /process`: Procesar un archivo.

**Servicio RAG**
- `POST /query`: Ruta del servicio a la cual se consultará la información del RAG.

**Opción alternativa a RAG: Búsqueda semántica**
- `POST /search`: Realizar búsqueda.


## Recursos brindados
Se brinda el acceso a los siguientes recursos para realizar la prueba técnica:
- LLM
  ```
  # Test:
  curl --location 'https://ollama.sgcan.dev/api/chat' \
  --header 'Content-Type: application/json' \
  --data '{
      "model": "phi4:latest",
      "messages": [
          {
              "role": "user",
              "content": "Hola como estas? escribe una historia de 50 palabras"
          }
      ]
  }'
  ```

- Embedding Model
  ```
  # Test:
  curl --location 'https://ollama.sgcan.dev/api/embeddings' \
  --header 'Content-Type: application/json' \
  --data '{
      "model": "nomic-embed-text",
      "prompt": "The sky is blue because of Rayleigh scattering"
  }'
  ```

- Configuración para el RAG
  ```
  URL de Ollama a utilizar: https://ollama.sgcan.dev
  
  Modelos disponibles:
  ## LLM:
  - qwen3:32b
  - phi4:latest
  
  ## Embedding:
  - mxbai-embed-large:latest
  - nomic-embed-text:latest
  ```