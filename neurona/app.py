import os
import pickle

from flask import Flask, request, jsonify
import tensorflow as tf
import numpy as np

from entrenar_modelo import MODEL_PATH, VECTORIZER_PATH, train_and_save_model

app = Flask(__name__)

if not os.path.exists(MODEL_PATH) or not os.path.exists(VECTORIZER_PATH):
    train_and_save_model()

modelo = tf.keras.models.load_model(MODEL_PATH)
with open(VECTORIZER_PATH, "rb") as _f:
    vectorizer = pickle.load(_f)


def _predict_batch(items: list[dict]) -> list[dict]:
    texts = [str(item.get("comentario", "")) for item in items]
    puntuaciones = np.array([[float(item.get("puntuacion", 3))] for item in items], dtype=np.float32)
    X_text = vectorizer.transform(texts).toarray().astype(np.float32)

    pred = modelo.predict({"texto_features": X_text, "puntuacion": puntuaciones}, verbose=0)
    clases = np.argmax(pred, axis=1)
    confidences = np.max(pred, axis=1)

    return [
        {"clase": int(clases[i]), "confidence": float(confidences[i])}
        for i in range(len(items))
    ]

@app.route('/predict', methods=['POST'])
def predict():
    data = request.json
    if not isinstance(data, dict):
        return jsonify({"error": "Payload invalido"}), 400

    result = _predict_batch([data])[0]
    return jsonify(result)

@app.route('/score-batch', methods=['POST'])
def score_batch():
    items = request.json  # lista de [{puntuacion, comentario}, ...]
    if not isinstance(items, list):
        return jsonify({"error": "Payload invalido, se esperaba lista"}), 400

    if len(items) == 0:
        return jsonify([])

    results = _predict_batch(items)
    return jsonify(results)

@app.route('/health', methods=['GET'])
def health():
    return jsonify({"status": "ok"})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
