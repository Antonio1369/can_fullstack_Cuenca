import requests
import json
from django.conf import settings
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework import status
from .models import Noticias
import numpy as np
from drf_yasg.utils import swagger_auto_schema
from sklearn.metrics.pairwise import cosine_similarity


class QueryRAGView(APIView):
    @swagger_auto_schema(operation_description="Consulta de RAG con parámetros de búsqueda")
    def post(self, request, *args, **kwargs):
        print("Recibiendo consulta:", request.data)
        print("Headers:", request.headers)
    

        query = request.data.get("query", "")

        if not query:
            return Response({"error": "Query is required"}, status=status.HTTP_400_BAD_REQUEST)

        try:
            print("Haciendo solicitud para obtener embeddings...")
            response = requests.post(
                f'{settings.OLLMAMA_URL}/api/embeddings',
                json={"model": settings.EMBEDDING_MODEL, "prompt": query}
            )

            if response.status_code != 200:
                print("Error generando embeddings para la consulta.")
                return Response({"error": "Error generating embedding for query"}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)

            query_embedding = response.json().get("embedding")

            query_embedding = np.array(query_embedding).reshape(1, -1)
            noticias = Noticias.objects.all()
            similarities = []

            print("Comparando la consulta con las noticias...")
            for noticia in noticias:
                noticia_embedding = json.loads(noticia.embedding)
                noticia_embedding = np.array(noticia_embedding).reshape(1, -1)
                similarity = cosine_similarity(query_embedding, noticia_embedding)[0][0]
                similarities.append((noticia, similarity))

            similarities.sort(key=lambda x: x[1], reverse=True)
            top_noticias = similarities[:5]
            news_content = "\n".join([news[0].content for news in top_noticias])

            print("Consultando LLM para generar la respuesta...")
            llm_response = requests.post(
                f'{settings.OLLMAMA_URL}/api/chat',
                json={
                    "model": settings.LLM_MODEL,
                    "messages": [{"role": "user", "content": f"Basado en las siguientes noticias, ¿puedes generar una respuesta a la consulta '{query}'?\n{news_content}"}]
                },
                stream=True
            )

            if llm_response.status_code == 200:
                print("Recibiendo respuesta del LLM...")
                answer_parts = []
                for line in llm_response.iter_lines():
                    if line:
                        try:
                            data = json.loads(line.decode('utf-8'))
                            if data.get('done', False):
                                break
                            answer_parts.append(data.get('message', {}).get('content', ''))
                        except json.JSONDecodeError:
                            continue

                full_answer = ''.join(answer_parts)

                print("Respuesta generada por LLM:", full_answer)

                return Response({"answer": full_answer})
            else:
                print("Error generando la respuesta de LLM.")
                return Response({"error": "Error generating LLM response"}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)

        except requests.exceptions.RequestException as e:
            print(f"Error en la solicitud LLM: {str(e)}")
            return Response({"error": f"Error: {str(e)}"}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)
