from fastapi import FastAPI, UploadFile, HTTPException
from tasks import process_links_task
from celery.result import AsyncResult

app = FastAPI()

@app.post("/process")
async def process_file(file: UploadFile):
    try:
        content = await file.read()
        urls = content.decode().splitlines()

        if not urls:
            raise HTTPException(status_code=400, detail="No URLs found in the file")
        

        task = process_links_task.delay(urls)  
        return {"message": f"{len(urls)} links enviados a la cola.", "task_id": task.id}
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))



@app.get("/task-status/{task_id}")
async def task_status(task_id: str):
    task_result = AsyncResult(task_id)

    if task_result.state == "PENDING":
        return {"status": "pending", "message": "La tarea está pendiente."}
    elif task_result.state == "STARTED":
        return {"status": "started", "message": "La tarea está en proceso."}
    elif task_result.state == "SUCCESS":
        return {"status": "success", "message": "La tarea ha sido procesada correctamente.", "result": task_result.result}
    elif task_result.state == "FAILURE":
        return {"status": "failure", "message": "Hubo un error al procesar la tarea.", "error": str(task_result.result)}
    else:
        return {"status": "unknown", "message": "El estado de la tarea es desconocido."}
