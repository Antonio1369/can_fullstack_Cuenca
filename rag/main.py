# from fastapi import FastAPI, Depends, HTTPException, status
# from fastapi.security import OAuth2PasswordBearer
# from pydantic import BaseModel
# #import jwt
# from datetime import datetime, timedelta
# from rag import RAG  

# SECRET_KEY = "your_secret_key"
# ALGORITHM = "HS256"
# ACCESS_TOKEN_EXPIRE_MINUTES = 30
# oauth2_scheme = OAuth2PasswordBearer(tokenUrl="login")

# app = FastAPI()

# # Instancia del sistema RAG
# rag_system = RAG()

# class QueryRequest(BaseModel):
#     query: str

# # def verify_token(token: str):
# #     try:
# #         payload = jwt.decode(token, SECRET_KEY, algorithms=[ALGORITHM])
# #         return payload
# #     except jwt.ExpiredSignatureError:
# #         raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Token expired")
# #     except jwt.PyJWTError:
# #         raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Token invalid")


# # def get_current_user(token: str = Depends(oauth2_scheme)):
# #     return verify_token(token)

# @app.post("/query")
# #async def query_rag(request: QueryRequest, current_user: dict = Depends(get_current_user)):
# async def query_rag(request: QueryRequest):
#     response = rag_system.query(request.query)
#     return {"response": response}
