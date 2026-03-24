FROM python:3.11-slim AS base

# ── Dependencias del sistema (mínimas) ──────────────────────────────────────
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgomp1 \
    && rm -rf /var/lib/apt/lists/*

# ── Crear usuario no-root antes de instalar nada ────────────────────────────
RUN groupadd --gid 1001 appgroup \
    && useradd --uid 1001 --gid appgroup --no-create-home --shell /sbin/nologin appuser

WORKDIR /app

# ── Instalar dependencias Python sin caché ──────────────────────────────────
COPY requirements.txt .
RUN pip install --no-cache-dir --upgrade pip \
    && pip install --no-cache-dir -r requirements.txt

# ── Copiar solo los archivos estrictamente necesarios ───────────────────────
COPY app.py entrenar_modelo.py ./

# ── Pre-entrenar modelo durante el build ────────────────────────────────────
RUN python entrenar_modelo.py

# ── Ajustar permisos: el proceso solo necesita leer /app ────────────────────
RUN chown -R appuser:appgroup /app

USER appuser

EXPOSE 5000

# ── Gunicorn con configuración de producción ────────────────────────────────
CMD ["gunicorn", \
     "--bind", "0.0.0.0:5000", \
     "--workers", "2", \
     "--threads", "2", \
     "--timeout", "120", \
     "--log-level", "warning", \
     "--access-logfile", "-", \
     "--error-logfile", "-", \
     "--forwarded-allow-ips", "*", \
     "app:app"]
