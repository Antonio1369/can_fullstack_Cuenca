from django.contrib import admin
from api_rag.models import Noticias, UserQuery, User, UserFile
from django.contrib.auth.admin import UserAdmin as BaseUserAdmin


class NoticiasAdmin(admin.ModelAdmin):
    fields = ['url', 'title', 'content', 'embedding']
    list_display = ['id', 'url', 'title', 'content', 'embedding']
    search_fields = ['title', 'content']
    list_filter = ['title']


class UserQueryAdmin(admin.ModelAdmin):
    fields = ['query_text', 'query_embedding', 'created_at']
    list_display = ['id', 'query_text', 'created_at']
    search_fields = ['query_text']
    list_filter = ['created_at']
    
class UserAdmin(BaseUserAdmin):
    fieldsets = (
        (None, {'fields': ('username', 'password')}),
        ('Información Personal', {'fields': ('email',)}),
        ('Permisos', {
            'fields': ('is_active', 'is_staff', 'is_superuser', 'groups', 'user_permissions'),
        }),
        ('Fechas importantes', {'fields': ('last_login', 'date_joined')}),
    )
    add_fieldsets = (
        (None, {
            'classes': ('wide',),
            'fields': ('username', 'email', 'password1', 'password2'),
        }),
    )
    list_display = ('id','username', 'email', 'is_staff', 'is_active')
    search_fields = ('username', 'email')
    list_filter = ('is_staff', 'is_superuser', 'is_active', 'groups')
    ordering = ('username',)
    filter_horizontal = ('groups', 'user_permissions',)
    


class UserFileAdmin(admin.ModelAdmin):
    list_display = ('id', 'user', 'filename', 'task_id', 'uploaded_at', 'processed')
    list_filter = ('processed', 'uploaded_at', 'user')
    search_fields = ('filename', 'task_id', 'user__username')
    ordering = ('-uploaded_at',)
    
admin.site.register(User, UserAdmin)
admin.site.register(Noticias, NoticiasAdmin)
admin.site.register(UserFile,UserFileAdmin)
admin.site.register(UserQuery, UserQueryAdmin)
