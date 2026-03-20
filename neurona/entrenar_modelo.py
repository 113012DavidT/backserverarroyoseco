import os

import numpy as np
import psycopg2
import tensorflow as tf


MODEL_PATH = "modelo_reseñas.keras"


def _db_config() -> dict:
    return {
        "host": os.getenv("DB_HOST", "db"),
        "port": int(os.getenv("DB_PORT", "5432")),
        "dbname": os.getenv("DB_NAME", "arroyoseco"),
        "user": os.getenv("DB_USER", "postgres"),
        "password": os.getenv("DB_PASSWORD", "postgres"),
    }


def _load_reviews_from_db() -> tuple[np.ndarray, np.ndarray]:
    cfg = _db_config()
    conn = psycopg2.connect(**cfg)
    try:
        with conn.cursor() as cur:
            cur.execute(
                'SELECT "Puntuacion", COALESCE("Comentario", \'\') FROM "Reviews" WHERE "Puntuacion" IS NOT NULL'
            )
            rows = cur.fetchall()
    finally:
        conn.close()

    if not rows:
        return np.array([], dtype=np.float32), np.array([], dtype=object)

    puntuaciones = np.array([float(r[0]) for r in rows], dtype=np.float32)
    comentarios = np.array([str(r[1]) for r in rows], dtype=object)
    return puntuaciones, comentarios


def _fallback_dataset() -> tuple[np.ndarray, np.ndarray]:
    puntuaciones = np.array([1, 2, 3, 4, 5, 1, 5], dtype=np.float32)
    comentarios = np.array(
        [
            "horrible",
            "regular",
            "aceptable",
            "muy bueno",
            "excelente",
            "pesimo servicio",
            "todo perfecto",
        ],
        dtype=object,
    )
    return puntuaciones, comentarios


def _to_class_labels(puntuaciones: np.ndarray) -> np.ndarray:
    # 0: malo (1-2), 1: medio (3), 2: bueno (4-5)
    return np.where(puntuaciones <= 2, 0, np.where(puntuaciones == 3, 1, 2)).astype(np.int32)


def _build_model(comentarios: np.ndarray, puntuaciones: np.ndarray) -> tf.keras.Model:
    text_input = tf.keras.Input(shape=(1,), dtype=tf.string, name="comentario")
    score_input = tf.keras.Input(shape=(1,), dtype=tf.float32, name="puntuacion")

    vectorizer = tf.keras.layers.TextVectorization(
        max_tokens=3000,
        output_mode="tf_idf",
        standardize="lower_and_strip_punctuation",
    )
    vectorizer.adapt(list(comentarios.astype(str)))

    normalizer = tf.keras.layers.Normalization(axis=-1)
    normalizer.adapt(puntuaciones.reshape(-1, 1))

    x_text = vectorizer(text_input)
    x_score = normalizer(score_input)
    x = tf.keras.layers.Concatenate()([x_text, x_score])
    x = tf.keras.layers.Dense(32, activation="relu")(x)
    x = tf.keras.layers.Dropout(0.2)(x)
    x = tf.keras.layers.Dense(16, activation="relu")(x)
    output = tf.keras.layers.Dense(3, activation="softmax", name="clase")(x)

    model = tf.keras.Model(inputs={"comentario": text_input, "puntuacion": score_input}, outputs=output)
    model.compile(
        optimizer=tf.keras.optimizers.Adam(learning_rate=1e-3),
        loss="sparse_categorical_crossentropy",
        metrics=["accuracy"],
    )
    return model


def train_and_save_model() -> None:
    tf.random.set_seed(42)
    np.random.seed(42)

    try:
        puntuaciones, comentarios = _load_reviews_from_db()
        source = "database"
    except Exception as ex:
        print(f"No se pudo leer la base de datos ({ex}). Usando fallback.")
        puntuaciones, comentarios = _fallback_dataset()
        source = "fallback"

    if len(puntuaciones) < 20:
        f_scores, f_comments = _fallback_dataset()
        puntuaciones = np.concatenate([puntuaciones, f_scores])
        comentarios = np.concatenate([comentarios, f_comments])

    labels = _to_class_labels(puntuaciones)
    model = _build_model(comentarios, puntuaciones)

    train_inputs = {
        "comentario": tf.constant([[str(c)] for c in comentarios], dtype=tf.string),
        "puntuacion": puntuaciones.reshape(-1, 1),
    }

    callbacks = [
        tf.keras.callbacks.EarlyStopping(monitor="loss", patience=8, restore_best_weights=True)
    ]

    model.fit(
        train_inputs,
        labels,
        epochs=120,
        batch_size=min(32, max(4, len(labels))),
        verbose=False,
        callbacks=callbacks,
    )

    model.save(MODEL_PATH)
    print(f"Modelo guardado en '{MODEL_PATH}' con {len(labels)} muestras ({source}).")


if __name__ == "__main__":
    train_and_save_model()
