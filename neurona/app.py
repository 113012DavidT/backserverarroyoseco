import os
import pickle
import logging
from functools import lru_cache
from collections import defaultdict
import time

from flask import Flask, request, jsonify, g
import tensorflow as tf
import numpy as np

from entrenar_modelo import MODEL_PATH, VECTORIZER_PATH, train_and_save_model

# ── Logging estructurado ─────────────────────────────────────────────────────
logging.basicConfig(
    level=logging.WARNING,
    format="%(asctime)s %(levelname)s %(name)s %(message)s",
)
logger = logging.getLogger("neurona")

app = Flask(__name__)

# ── Seguridad: deshabilitar header que expone el servidor ────────────────────
app.config["PROPAGATE_EXCEPTIONS"] = False

# ── Rate-limiting simple en memoria ─────────────────────────────────────────
RATE_LIMIT = int(os.environ.get("RATE_LIMIT_PER_MIN", "60"))
_rate_buckets: dict[str, list[float]] = defaultdict(list)

def _check_rate(ip: str) -> bool:
    now = time.monotonic()
    window = 60.0
    bucket = _rate_buckets[ip]
    # Eliminar llamadas fuera de la ventana
    _rate_buckets[ip] = [t for t in bucket if now - t < window]
    if len(_rate_buckets[ip]) >= RATE_LIMIT:
        return False
    _rate_buckets[ip].append(now)
    return True

# ── Validación de inputs ─────────────────────────────────────────────────────
MAX_COMENTARIO_LEN = 2000
VALID_PUNTUACION_RANGE = (1.0, 5.0)

def _validate_item(item: dict) -> str | None:
    """Retorna mensaje de error o None si es válido."""
    if not isinstance(item, dict):
        return "Cada item debe ser un objeto JSON"
    comentario = item.get("comentario", "")
    if not isinstance(comentario, str):
        return "El campo comentario debe ser texto"
    if len(comentario) > MAX_COMENTARIO_LEN:
        return f"El comentario excede {MAX_COMENTARIO_LEN} caracteres"
    puntuacion = item.get("puntuacion", 3)
    try:
        p = float(puntuacion)
    except (TypeError, ValueError):
        return "El campo puntuacion debe ser numerico"
    if not (VALID_PUNTUACION_RANGE[0] <= p <= VALID_PUNTUACION_RANGE[1]):
        return f"puntuacion debe estar entre {VALID_PUNTUACION_RANGE[0]} y {VALID_PUNTUACION_RANGE[1]}"
    return None

# ── Cargar modelo ────────────────────────────────────────────────────────────
if not os.path.exists(MODEL_PATH) or not os.path.exists(VECTORIZER_PATH):
    train_and_save_model()

modelo = tf.keras.models.load_model(MODEL_PATH)
with open(VECTORIZER_PATH, "rb") as _f:
    vectorizer = pickle.load(_f)


def _predict_batch(items: list[dict]) -> list[dict]:
    texts = [str(item.get("comentario", "")) for item in items]
    puntuaciones = np.array(
        [[float(item.get("puntuacion", 3))] for item in items], dtype=np.float32
    )
    X_text = vectorizer.transform(texts).toarray().astype(np.float32)
    pred = modelo.predict(
        {"texto_features": X_text, "puntuacion": puntuaciones}, verbose=0
    )
    clases = np.argmax(pred, axis=1)
    confidences = np.max(pred, axis=1)
    return [
        {"clase": int(clases[i]), "confidence": float(confidences[i])}
        for i in range(len(items))
    ]


# ── Cabeceras de seguridad en todas las respuestas ───────────────────────────
@app.after_request
def _security_headers(response):
    response.headers["X-Content-Type-Options"] = "nosniff"
    response.headers["X-Frame-Options"] = "DENY"
    response.headers["X-XSS-Protection"] = "0"
    response.headers["Cache-Control"] = "no-store"
    response.headers["Content-Security-Policy"] = "default-src 'none'"
    response.headers.pop("Server", None)
    return response


# ── Middleware: rate limiting ─────────────────────────────────────────────────
@app.before_request
def _rate_limit():
    ip = request.headers.get("X-Forwarded-For", request.remote_addr or "").split(",")[0].strip()
    if not _check_rate(ip):
        return jsonify({"error": "Demasiadas solicitudes. Intenta en un minuto."}), 429


# ── Endpoints ────────────────────────────────────────────────────────────────
@app.route("/predict", methods=["POST"])
def predict():
    data = request.get_json(silent=True)
    if not isinstance(data, dict):
        return jsonify({"error": "Payload invalido"}), 400

    err = _validate_item(data)
    if err:
        return jsonify({"error": err}), 422

    result = _predict_batch([data])[0]
    return jsonify(result)


@app.route("/score-batch", methods=["POST"])
def score_batch():
    items = request.get_json(silent=True)
    if not isinstance(items, list):
        return jsonify({"error": "Payload invalido, se esperaba lista"}), 400
    if len(items) == 0:
        return jsonify([])
    if len(items) > 200:
        return jsonify({"error": "Maximo 200 items por lote"}), 422

    for idx, item in enumerate(items):
        err = _validate_item(item)
        if err:
            return jsonify({"error": f"Item {idx}: {err}"}), 422

    results = _predict_batch(items)
    return jsonify(results)


@app.route("/health", methods=["GET"])
def health():
    return jsonify({"status": "ok"})


# ── Manejador de errores: sin tracebacks expuestos ───────────────────────────
@app.errorhandler(Exception)
def _handle_exception(exc):
    logger.exception("Error interno no controlado")
    return jsonify({"error": "Error interno del servidor"}), 500

@app.errorhandler(404)
def _not_found(_):
    return jsonify({"error": "Ruta no encontrada"}), 404

@app.errorhandler(405)
def _method_not_allowed(_):
    return jsonify({"error": "Metodo no permitido"}), 405


if __name__ == "__main__":
    # Solo para desarrollo local; en produccion usa gunicorn via Docker
    app.run(host="127.0.0.1", port=5000, debug=False)

