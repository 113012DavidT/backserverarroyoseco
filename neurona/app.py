from flask import Flask, request, jsonify
import tensorflow as tf
import numpy as np

app = Flask(__name__)
modelo = tf.keras.models.load_model("modelo_reseñas.keras")

@app.route('/predict', methods=['POST'])
def predict():
    data = request.json
    puntuacion = float(data['puntuacion'])
    comentario = data.get('comentario', '')
    test_vec = np.array([[puntuacion, len(comentario)]])
    pred = modelo.predict(test_vec, verbose=0)
    clase = int(np.argmax(pred))
    return jsonify({"clase": clase})

@app.route('/score-batch', methods=['POST'])
def score_batch():
    items = request.json  # lista de [{puntuacion, comentario}, ...]
    results = []
    for item in items:
        puntuacion = float(item['puntuacion'])
        comentario = item.get('comentario', '')
        test_vec = np.array([[puntuacion, len(comentario)]])
        pred = modelo.predict(test_vec, verbose=0)
        clase = int(np.argmax(pred))
        confidence = float(np.max(pred))
        results.append({"clase": clase, "confidence": confidence})
    return jsonify(results)

@app.route('/health', methods=['GET'])
def health():
    return jsonify({"status": "ok"})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
