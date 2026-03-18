import tensorflow as tf
import numpy as np

# Datos de entrenamiento
puntuaciones = np.array([1, 2, 3, 4, 5], dtype=float)
comentarios = ["horrible", "regular", "aceptable", "muy bueno", "excelente"]
comentario_vec = np.array([len(c) for c in comentarios], dtype=float)

X = np.column_stack((puntuaciones, comentario_vec))
y = np.array([0, 0, 1, 2, 2], dtype=int)

modelo = tf.keras.Sequential([
    tf.keras.layers.Dense(8, activation='relu', input_shape=[2]),
    tf.keras.layers.Dense(3, activation='softmax')
])

modelo.compile(optimizer='adam', loss='sparse_categorical_crossentropy', metrics=['accuracy'])
modelo.fit(X, y, epochs=1000, verbose=False)

modelo.save("modelo_reseñas.keras")
print("Modelo guardado en 'modelo_reseñas.keras'")
