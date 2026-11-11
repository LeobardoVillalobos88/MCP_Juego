# Genera los efectos de sonido de 8 bits en Assets/Audio/SFX
import math, os, random, struct, wave

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SALIDA = os.path.join(RAIZ, "Assets", "Audio", "SFX")
MUESTREO = 22050

def guardar_wav(nombre, muestras):
    with wave.open(os.path.join(SALIDA, nombre + ".wav"), "wb") as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(MUESTREO)
        f.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, m)) * 32000)) for m in muestras))

def nota(nombre):
    # Frecuencia de una nota tipo "C5" o "F#4"
    orden = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"]
    tono, octava = nombre[:-1], int(nombre[-1])
    return 440 * 2 ** ((orden.index(tono) - 9) / 12 + (octava - 4))

def onda(tipo, fase):
    fase %= 1
    if tipo == "cuadrada":
        return 1 if fase < 0.5 else -1
    if tipo == "triangulo":
        return 4 * abs(fase - 0.5) - 1
    return math.sin(fase * 2 * math.pi)

# Tono con barrido de frecuencia y caída de volumen
def tono(duracion, f_inicio, f_fin=None, tipo="cuadrada", volumen=0.35, caida=True):
    f_fin = f_fin or f_inicio
    total = int(duracion * MUESTREO)
    fase, muestras = 0, []
    for i in range(total):
        t = i / total
        fase += (f_inicio + (f_fin - f_inicio) * t) / MUESTREO
        envolvente = (1 - t) if caida else min(1, (1 - t) * 8)
        muestras.append(onda(tipo, fase) * volumen * envolvente)
    return muestras

def melodia(notas, tempo, tipo="cuadrada", volumen=0.3):
    muestras = []
    for nombre, tiempos in notas:
        duracion = tiempos * tempo
        if nombre == "-":
            muestras += [0] * int(duracion * MUESTREO)
        else:
            muestras += tono(duracion, nota(nombre), tipo=tipo, volumen=volumen, caida=False)
    return muestras

def mezclar(a, b):
    largo = max(len(a), len(b))
    a, b = a + [0] * (largo - len(a)), b + [0] * (largo - len(b))
    return [x + y for x, y in zip(a, b)]

def ruido(duracion, volumen=0.3):
    azar = random.Random(3)
    total = int(duracion * MUESTREO)
    return [azar.uniform(-1, 1) * volumen * (1 - i / total) for i in range(total)]

def main():
    os.makedirs(SALIDA, exist_ok=True)

    guardar_wav("Punto0", tono(0.07, 600, 900, "triangulo", 0.5))
    guardar_wav("Punto1", tono(0.07, 900, 600, "triangulo", 0.5))
    guardar_wav("Nucleo", tono(0.12, 300, 500) + tono(0.12, 450, 750) + tono(0.18, 600, 1100))
    guardar_wav("ComerGlitch", tono(0.12, 1200, 300) + tono(0.25, 300, 1600, "triangulo", 0.4))
    guardar_wav("Muerte", tono(0.35, 900, 500, "cuadrada", 0.3, False) + tono(0.35, 700, 300, "cuadrada", 0.3, False)
                + tono(0.5, 500, 80) + ruido(0.25, 0.2))
    guardar_wav("Bonus", tono(0.08, 1000) + tono(0.08, 1300) + tono(0.08, 1600) + tono(0.2, 2000))
    guardar_wav("VidaExtra", tono(0.1, 1400) + [0] * 1500 + tono(0.1, 1400) + [0] * 1500 + tono(0.25, 1800))
    guardar_wav("Clic", tono(0.04, 1500, 1200, "cuadrada", 0.25))

    # Melodía de inicio (propia)
    voz = melodia([("E5", 1), ("G5", 1), ("B5", 1), ("G5", 1), ("A5", 1), ("C6", 1), ("E6", 2),
                   ("D6", 1), ("B5", 1), ("G5", 1), ("B5", 1), ("C6", 2), ("-", 1), ("E6", 3)], 0.13)
    bajo = melodia([("C3", 4), ("A2", 4), ("G2", 4), ("C3", 4)], 0.13, "triangulo", 0.35)
    guardar_wav("Inicio", mezclar(voz, bajo))

    guardar_wav("NivelCompleto", melodia([("C5", 1), ("E5", 1), ("G5", 1), ("C6", 1), ("-", 1), ("G5", 1), ("C6", 3)], 0.11))

    # Fondos en bucle
    fase, muestras = 0, []
    for i in range(int(0.8 * MUESTREO)):
        t = i / MUESTREO
        frecuencia = 420 + 120 * math.sin(t / 0.8 * 2 * math.pi)
        fase += frecuencia / MUESTREO
        muestras.append(onda("triangulo", fase) * 0.18)
    guardar_wav("Sirena", muestras)

    fase, muestras = 0, []
    for i in range(int(0.4 * MUESTREO)):
        t = i / MUESTREO
        frecuencia = 250 + 900 * ((t * 5) % 1)
        fase += frecuencia / MUESTREO
        muestras.append(onda("cuadrada", fase) * 0.1)
    guardar_wav("Alarma", muestras)

    print("Sonidos generados en", SALIDA)

main()
