from django.db import models
from django.contrib.auth.models import AbstractUser
from django.core.validators import FileExtensionValidator
import json

class User(AbstractUser):
    email = models.EmailField(unique=True)
    
    class Meta:
        db_table = 'user'

    def __str__(self):
        return self.username
    
class UserQuery(models.Model):
    user = models.ForeignKey(User, related_name="queries", on_delete=models.CASCADE)  
    query_text = models.TextField()  
    query_embedding = models.TextField()
    created_at = models.DateTimeField(auto_now_add=True)  

    def save_embedding(self, embedding):
        self.query_embedding = json.dumps(embedding)
        self.save()
        
    def get_embedding(self):
        return json.loads(self.query_embedding)
    
    class Meta:
        db_table = 'userquery'  

class UserFile(models.Model): 
    user = models.ForeignKey(User, related_name="files", on_delete=models.CASCADE)
    file = models.FileField(upload_to='user_uploads/%Y/%m/%d/', validators=[FileExtensionValidator(allowed_extensions=['pdf', 'docx', 'txt', 'csv'])])
    original_filename = models.CharField(max_length=255)
    file_size = models.PositiveIntegerField()
    task_id = models.CharField(max_length=255, null=True, blank=True)
    uploaded_at = models.DateTimeField(auto_now_add=True)
    processed = models.BooleanField(default=False)
    
    class Meta:
        db_table = 'user_files'
        ordering = ['-uploaded_at']
    
    def save(self, *args, **kwargs):
        if not self.pk:  
            self.original_filename = self.file.name
            self.file_size = self.file.size
        super().save(*args, **kwargs)
    
    @property
    def file_extension(self):
        return self.original_filename.split('.')[-1].lower()

class Noticias(models.Model):
    url = models.CharField(max_length=255, unique=True)
    title = models.CharField(max_length=255)
    content = models.TextField()
    embedding = models.TextField()

    class Meta:
        db_table = 'noticias'  

    def __str__(self):
        return self.title


