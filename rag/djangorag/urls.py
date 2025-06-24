from django.contrib import admin
from django.urls import path, include
from drf_yasg.views import get_schema_view
from drf_yasg import openapi
from rest_framework import permissions
from djangorag import settings
from django.conf.urls.static import static

schema_view = get_schema_view(
    openapi.Info(
        title="SGCAN RAG DOCS",
        default_version='v1',
        description="Documentación de la API de Django RAG",
        contact=openapi.Contact(email="mcuencad@uni.pe"),

    ),
    public=True,
    permission_classes=(permissions.AllowAny,),
)

urlpatterns = [
    
    path('api/', include('api_rag.urls')), 
    path('docs/', schema_view.with_ui('swagger', cache_timeout=0), name='schema-swagger-ui'),
    path('admin/', admin.site.urls),
    
]+ static(settings.MEDIA_URL, document_root=settings.MEDIA_ROOT)
