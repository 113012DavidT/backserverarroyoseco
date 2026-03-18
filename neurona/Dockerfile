FROM python:3.11-slim

WORKDIR /app

# Instalar dependencias del sistema necesarias para TensorFlow
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgomp1 \
    && rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

# Si el modelo no existe, entrenarlo al construir la imagen
RUN python entrenar_modelo.py

EXPOSE 5000

CMD ["gunicorn", "--bind", "0.0.0.0:5000", "--workers", "2", "--timeout", "120", "app:app"]
