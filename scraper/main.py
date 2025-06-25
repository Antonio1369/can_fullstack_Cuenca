from fastapi import FastAPI, UploadFile, HTTPException, Form
from tasks import process_links_task, process_userfile_data
from celery.result import AsyncResult
from celeryconfig import celery_app 
app = FastAPI()

@app.post("/process")
async def process_file(
    file: UploadFile,
    userId: int = Form(...)
):
    try:
        content = await file.read()
        urls = content.decode().splitlines()
        if not urls:
            raise HTTPException(status_code=400, detail="No URLs found in the file")
        
        user_id = userId
        task = process_links_task.delay(urls, user_id)
        
        userfile_task = process_userfile_data.delay(userId,file.filename,task.id,False)
        print(f"userfile_task {userfile_task}", userId,file.filename,task.id,False)
        
        return {
            "message": f"{len(urls)} links enviados a la cola.",
            "task_id": task.id,
            "userId": userId
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.get("/task-status/{task_id}")
async def task_status(task_id: str):
    try:
        task_result = AsyncResult(task_id, app=celery_app)
        
        if task_result.state == "PENDING":
            return {
                "status": "pending",
                "task_id": task_id
            }
        elif task_result.state == "SUCCESS":
            return {
                "status": "completed",
                "result": task_result.result,
                "task_id": task_id
            }
        elif task_result.state == "FAILURE":
            return {
                "status": "failed",
                "error": str(task_result.result),
                "task_id": task_id
            }
        else:
            return {
                "status": task_result.state,
                "task_id": task_id,
                "progress": task_result.info.get("progress", 0) if hasattr(task_result, "info") else None
            }
            
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))