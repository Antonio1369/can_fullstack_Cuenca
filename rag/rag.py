from llama_index.core import (
    ServiceContext,
    VectorStoreIndex,
    SimpleDirectoryReader,
    load_index_from_storage,
    StorageContext,
    Settings
)
from llama_index.llms.openai import OpenAI

import openai
import getpass
import os
from dotenv import load_dotenv

load_dotenv()

api_key = os.getenv("LLAMA_SGCAN_KEY")
Settings.lln =  OpenAI(model="gpt4", temperature=0.1)
openai.api_key = getpass.getpass(api_key)

documents = SimpleDirectoryReader("./storage/").load_data() #lee todos los documentos de la carpeta
print(type(documents[0]))
print(documents[0])

index = VectorStoreIndex.from_documents(documents)
#Almacenamiento persistente
index.storage_context.persist()
storagge_context = StorageContext.from_defaults(persist_di="./storage")
index = load_index_from_storage(storagge_context=storagge_context)

query_engine = index.as_query_engine()
question = "Que es comunidad andina"
response = query_engine.query(question)
print(str(response))

