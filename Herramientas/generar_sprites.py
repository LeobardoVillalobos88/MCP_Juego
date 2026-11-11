# Genera el pixel art propio en Assets/Art/Propios (16 px por celda)
import math, os, random, struct, zlib

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SALIDA = os.path.join(RAIZ, "Assets", "Art", "Propios")
MAPA = os.path.join(RAIZ, "Assets", "Niveles", "Laberinto.txt")
T = 16

def guardar_png(nombre, pixeles):
    alto, ancho = len(pixeles), len(pixeles[0])
    crudo = b"".join(b"\x00" + bytes(c for px in fila for c in px) for fila in pixeles)
    def bloque(tipo, datos):
        return struct.pack(">I", len(datos)) + tipo + datos + struct.pack(">I", zlib.crc32(tipo + datos) & 0xFFFFFFFF)
    png = (b"\x89PNG\r\n\x1a\n"
           + bloque(b"IHDR", struct.pack(">IIBBBBB", ancho, alto, 8, 6, 0, 0, 0))
           + bloque(b"IDAT", zlib.compress(crudo, 9))
           + bloque(b"IEND", b""))
    with open(os.path.join(SALIDA, nombre + ".png"), "wb") as f:
        f.write(png)

def lienzo(ancho=T, alto=T):
    return [[(0, 0, 0, 0) for _ in range(ancho)] for _ in range(alto)]

def poner(img, x, y, color):
    if 0 <= y < len(img) and 0 <= x < len(img[0]):
        img[y][x] = color

def vacio(px):
    return px[3] == 0

# Contorno de 1 px alrededor de lo dibujado
def contorno(img, color):
    salida = [fila[:] for fila in img]
    for y in range(len(img)):
        for x in range(len(img[0])):
            if not vacio(img[y][x]):
                continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = x + dx, y + dy
                if 0 <= ny < len(img) and 0 <= nx < len(img[0]) and not vacio(img[ny][nx]):
                    salida[y][x] = color
                    break
    return salida

def desplazar(img, dx, dy):
    salida = lienzo(len(img[0]), len(img))
    for y in range(len(img)):
        for x in range(len(img[0])):
            if not vacio(img[y][x]):
                poner(salida, x + dx, y + dy, img[y][x])
    return salida

def espejo(img):
    return [fila[::-1] for fila in img]

K = (12, 16, 40, 255)

# ---------- Bit (jugador) ----------

CIAN = (90, 230, 255, 255)
CIAN_OSCURO = (40, 150, 215, 255)
BRILLO = (230, 255, 255, 255)
VISOR = (14, 28, 64, 255)
OJO = (170, 255, 250, 255)
LUZ = (255, 70, 200, 255)
LUZ_APAGADA = (130, 40, 110, 255)

def bit(direccion, cuadro):
    img = lienzo()
    # Cuerpo redondo
    for y in range(T):
        for x in range(T):
            if (x - 7.5) ** 2 + (y - 9) ** 2 <= 5.7 ** 2:
                img[y][x] = CIAN_OSCURO if y >= 12 else CIAN
    poner(img, 4, 6, BRILLO); poner(img, 5, 5, BRILLO); poner(img, 4, 7, BRILLO)

    # Visor y ojos según la dirección
    if direccion in ("Derecha", "Izquierda"):
        filas, columnas, ojos = range(7, 11), range(5, 14), (8, 11)
    elif direccion == "Arriba":
        filas, columnas, ojos = range(5, 9), range(3, 13), (5, 9)
    else:
        filas, columnas, ojos = range(8, 12), range(3, 13), (5, 9)
    for y in filas:
        for x in columnas:
            if not vacio(img[y][x]):
                img[y][x] = VISOR
    y0 = filas[0] + 1
    for ox in ojos:
        for y in (y0, y0 + 1):
            poner(img, ox, y, OJO); poner(img, ox + 1, y, OJO)

    # Antena con luz que parpadea
    poner(img, 7, 3, CIAN_OSCURO); poner(img, 8, 3, CIAN_OSCURO)
    luz = LUZ if cuadro == 0 else LUZ_APAGADA
    for x, y in ((7, 1), (8, 1), (7, 2), (8, 2)):
        poner(img, x, y, luz)

    img = contorno(img, K)
    if direccion == "Izquierda":
        img = espejo(img)
    # Pequeño salto al caminar
    return desplazar(img, 0, -1) if cuadro == 1 else img

def bit_muerte(base, cuadro, total):
    img = lienzo()
    azar = random.Random(7)
    for y in range(T):
        for x in range(T):
            px = base[y][x]
            umbral = azar.random()
            if vacio(px) or umbral < cuadro / total:
                continue
            escala = 1 + cuadro * 0.18
            nx = round(7.5 + (x - 7.5) * escala)
            ny = round(8.5 + (y - 8.5) * escala)
            poner(img, nx, ny, px)
    return img

# ---------- Glitch (enemigo) ----------

BLANCO = (255, 255, 255, 255)
GRIS_CLARO = (205, 205, 205, 255)
GRIS = (140, 140, 140, 255)
AZUL_SUSTO = (40, 70, 235, 255)
AZUL_SUSTO_OSCURO = (25, 40, 170, 255)
CARA_SUSTO = (255, 205, 175, 255)
BLANCO_SUSTO = (235, 235, 255, 255)
GRIS_SUSTO = (180, 180, 210, 255)
CARA_ROJA = (230, 40, 70, 255)

def forma_glitch(cuadro):
    img = lienzo()
    for y in range(T):
        for x in range(T):
            cabeza = (x - 7.5) ** 2 + (y - 7.5) ** 2 <= 6.3 ** 2 and y <= 8
            cuerpo = 2 <= x <= 13 and 7 <= y <= 13
            # Picos de abajo que se alternan
            pico = 2 <= x <= 13 and y == 14 and (x // 2 + cuadro) % 2 == 0
            if cabeza or cuerpo or pico:
                img[y][x] = GRIS_CLARO
    # Cuernitos
    for x, y in ((3, 2), (2, 1), (12, 2), (13, 1)):
        poner(img, x, y, GRIS_CLARO)
    # Franja de falla: una fila recorrida un pixel
    if cuadro == 0:
        img[10] = img[10][1:] + [(0, 0, 0, 0)]
    else:
        img[11] = [(0, 0, 0, 0)] + img[11][:-1]
    return img

def colorear(img, claro, oscuro, brillo):
    salida = [fila[:] for fila in img]
    for y in range(T):
        for x in range(T):
            if vacio(img[y][x]):
                continue
            if y >= 12:
                salida[y][x] = oscuro
            elif x <= 5 and y <= 5:
                salida[y][x] = brillo
            else:
                salida[y][x] = claro
    return contorno(salida, K)

def cara_susto(img, color):
    for x in (5, 6, 9, 10):
        poner(img, x, 6, color); poner(img, x, 7, color)
    # Boca en zigzag
    for x in range(3, 13):
        poner(img, x, 10 if x % 2 == 0 else 11, color)
    return img

PUPILA = (30, 50, 170, 255)

def ojos(direccion):
    img = lienzo()
    # Desplazamiento del ojo y posición de la pupila dentro del ojo
    dx, dy = {"Derecha": (1, 0), "Izquierda": (-1, 0), "Arriba": (0, -1), "Abajo": (0, 1)}[direccion]
    pupila_x, pupila_y = {"Derecha": (2, 2), "Izquierda": (0, 2), "Arriba": (1, 0), "Abajo": (1, 3)}[direccion]
    for ox in (3, 9):
        for y in range(5, 10):
            for x in range(ox, ox + 4):
                esquina = (x in (ox, ox + 3)) and (y in (5, 9))
                if not esquina:
                    poner(img, x + dx, y + dy, BLANCO)
        for y in range(2):
            for x in range(2):
                poner(img, ox + pupila_x + x + dx, 5 + pupila_y + y + dy, PUPILA)
    return img

# ---------- Objetos ----------

def punto():
    img = lienzo()
    for x, y in ((7, 7), (8, 7), (7, 8), (8, 8)):
        img[y][x] = (255, 225, 170, 255)
    return img

def nucleo():
    img = lienzo()
    for y in range(T):
        for x in range(T):
            d = math.hypot(x - 7.5, y - 7.5)
            if d <= 2.2:
                img[y][x] = (255, 255, 230, 255)
            elif d <= 3.8:
                img[y][x] = (255, 225, 120, 255)
            elif d <= 5.2:
                img[y][x] = (255, 200, 80, 70)
    return img

def cristal():
    img = lienzo()
    for y in range(2, 14):
        ancho = 6 - abs(y - 6) * 6 // 4 if y <= 6 else 6 - (y - 6) * 6 // 7
        for x in range(8 - ancho, 8 + ancho):
            if x < 8:
                color = (255, 120, 230, 255) if y <= 6 else (190, 60, 200, 255)
            else:
                color = (120, 240, 255, 255) if y <= 6 else (60, 160, 220, 255)
            img[y][x] = color
    poner(img, 6, 4, BLANCO); poner(img, 5, 5, BLANCO)
    return contorno(img, K)

# ---------- Paredes (una sola imagen del laberinto) ----------

LINEA = (255, 255, 255, 255)
RELLENO = (40, 40, 60, 255)
PUERTA = (255, 150, 220, 255)

def paredes():
    with open(MAPA, encoding="utf-8") as f:
        filas = [fila.rstrip("\n") for fila in f if fila.strip()]
    ancho = max(len(fila) for fila in filas)
    filas = [fila.ljust(ancho) for fila in filas]
    alto = len(filas)
    img = lienzo(ancho * T, alto * T)

    def es_pared(cx, cy):
        if cx < 0 or cx >= ancho or cy < 0 or cy >= alto:
            return True
        return filas[cy][cx] == "#"

    for py in range(alto * T):
        for px in range(ancho * T):
            cx, cy = px // T, py // T
            if not es_pared(cx, cy):
                continue
            # Distancia al pasillo más cercano (para redondear las esquinas)
            distancia = 99
            for vy in (-1, 0, 1):
                for vx in (-1, 0, 1):
                    if es_pared(cx + vx, cy + vy):
                        continue
                    x0, y0 = (cx + vx) * T, (cy + vy) * T
                    dx = max(x0 - (px + 0.5), 0, (px + 0.5) - (x0 + T))
                    dy = max(y0 - (py + 0.5), 0, (py + 0.5) - (y0 + T))
                    distancia = min(distancia, math.hypot(dx, dy))
            if 3 <= distancia < 5:
                img[py][px] = LINEA
            elif 1.5 <= distancia < 3:
                img[py][px] = (255, 255, 255, 60)
            elif distancia >= 5:
                img[py][px] = RELLENO

    # Puerta de la casa
    for cy, fila in enumerate(filas):
        for cx, c in enumerate(fila):
            if c == "-":
                for px in range(cx * T, cx * T + T):
                    for py in range(cy * T + 6, cy * T + 9):
                        img[py][px] = PUERTA
    return img

def main():
    os.makedirs(SALIDA, exist_ok=True)

    for direccion in ("Derecha", "Izquierda", "Arriba", "Abajo"):
        for cuadro in (0, 1):
            guardar_png("Bit" + direccion + str(cuadro), bit(direccion, cuadro))
        guardar_png("Ojos" + direccion, ojos(direccion))

    base = bit("Derecha", 0)
    for cuadro in range(8):
        guardar_png("BitMuerte" + str(cuadro), bit_muerte(base, cuadro, 8))

    for cuadro in (0, 1):
        forma = forma_glitch(cuadro)
        guardar_png("Glitch" + str(cuadro), colorear(forma, GRIS_CLARO, GRIS, BLANCO))
        guardar_png("GlitchAsustado" + str(cuadro),
                    cara_susto(colorear(forma, AZUL_SUSTO, AZUL_SUSTO_OSCURO, AZUL_SUSTO), CARA_SUSTO))
        guardar_png("GlitchAsustadoBlanco" + str(cuadro),
                    cara_susto(colorear(forma, BLANCO_SUSTO, GRIS_SUSTO, BLANCO_SUSTO), CARA_ROJA))

    guardar_png("Punto", punto())
    guardar_png("Nucleo", nucleo())
    guardar_png("Cristal", cristal())
    guardar_png("Paredes", paredes())
    print("Sprites generados en", SALIDA)

main()
