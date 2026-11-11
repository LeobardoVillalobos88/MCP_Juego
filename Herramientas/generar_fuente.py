# Genera la fuente pixel propia Assets/Fonts/PixelNeon.ttf (letras de 5 x 7 pixeles)
import os
from fontTools.fontBuilder import FontBuilder
from fontTools.pens.ttGlyphPen import TTGlyphPen

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SALIDA = os.path.join(RAIZ, "Assets", "Fonts", "PixelNeon.ttf")
PIXEL = 100     # unidades por pixel
ESPACIO = 4     # ancho del espacio en pixeles

LETRAS = {
    "A": [".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#"],
    "B": ["####.", "#...#", "#...#", "####.", "#...#", "#...#", "####."],
    "C": [".###.", "#...#", "#....", "#....", "#....", "#...#", ".###."],
    "D": ["####.", "#...#", "#...#", "#...#", "#...#", "#...#", "####."],
    "E": ["#####", "#....", "#....", "####.", "#....", "#....", "#####"],
    "F": ["#####", "#....", "#....", "####.", "#....", "#....", "#...."],
    "G": [".###.", "#...#", "#....", "#.###", "#...#", "#...#", ".####"],
    "H": ["#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#"],
    "I": [".###.", "..#..", "..#..", "..#..", "..#..", "..#..", ".###."],
    "J": ["..###", "...#.", "...#.", "...#.", "...#.", "#..#.", ".##.."],
    "K": ["#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#"],
    "L": ["#....", "#....", "#....", "#....", "#....", "#....", "#####"],
    "M": ["#...#", "##.##", "#.#.#", "#.#.#", "#...#", "#...#", "#...#"],
    "N": ["#...#", "##..#", "#.#.#", "#..##", "#...#", "#...#", "#...#"],
    "O": [".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###."],
    "P": ["####.", "#...#", "#...#", "####.", "#....", "#....", "#...."],
    "Q": [".###.", "#...#", "#...#", "#...#", "#.#.#", "#..#.", ".##.#"],
    "R": ["####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#"],
    "S": [".####", "#....", "#....", ".###.", "....#", "....#", "####."],
    "T": ["#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.."],
    "U": ["#...#", "#...#", "#...#", "#...#", "#...#", "#...#", ".###."],
    "V": ["#...#", "#...#", "#...#", "#...#", "#...#", ".#.#.", "..#.."],
    "W": ["#...#", "#...#", "#...#", "#.#.#", "#.#.#", "#.#.#", ".#.#."],
    "X": ["#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#"],
    "Y": ["#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#.."],
    "Z": ["#####", "....#", "...#.", "..#..", ".#...", "#....", "#####"],
    "0": [".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###."],
    "1": ["..#..", ".##..", "..#..", "..#..", "..#..", "..#..", ".###."],
    "2": [".###.", "#...#", "....#", "...#.", "..#..", ".#...", "#####"],
    "3": ["#####", "...#.", "..#..", "...#.", "....#", "#...#", ".###."],
    "4": ["...#.", "..##.", ".#.#.", "#..#.", "#####", "...#.", "...#."],
    "5": ["#####", "#....", "####.", "....#", "....#", "#...#", ".###."],
    "6": ["..##.", ".#...", "#....", "####.", "#...#", "#...#", ".###."],
    "7": ["#####", "....#", "...#.", "..#..", ".#...", ".#...", ".#..."],
    "8": [".###.", "#...#", "#...#", ".###.", "#...#", "#...#", ".###."],
    "9": [".###.", "#...#", "#...#", ".####", "....#", "...#.", ".##.."],
    "!": ["..#..", "..#..", "..#..", "..#..", "..#..", ".....", "..#.."],
    "¡": ["..#..", ".....", "..#..", "..#..", "..#..", "..#..", "..#.."],
    "?": [".###.", "#...#", "....#", "...#.", "..#..", ".....", "..#.."],
    ".": [".....", ".....", ".....", ".....", ".....", ".....", "..#.."],
    ",": [".....", ".....", ".....", ".....", ".....", "..#..", ".#..."],
    ":": [".....", "..#..", ".....", ".....", ".....", "..#..", "....."],
    "-": [".....", ".....", ".....", ".###.", ".....", ".....", "....."],
    "/": ["....#", "....#", "...#.", "..#..", ".#...", "#....", "#...."],
    "+": [".....", "..#..", "..#..", "#####", "..#..", "..#..", "....."],
    " ": [".....", ".....", ".....", ".....", ".....", ".....", "....."],
}

# Marcas que van en las dos filas de arriba de la letra
ACENTO = ["...#.", "..#.."]
TILDE = [".##.#", "#.##."]
CON_MARCA = {"Á": ("A", ACENTO), "É": ("E", ACENTO), "Í": ("I", ACENTO),
             "Ó": ("O", ACENTO), "Ú": ("U", ACENTO), "Ñ": ("N", TILDE)}

# Columnas que ocupa la letra (para que cada una mida solo lo que dibuja)
def columnas(filas):
    usadas = [j for fila in filas for j, c in enumerate(fila) if c == "#"]
    return (min(usadas), max(usadas)) if usadas else (0, ESPACIO - 2)

def dibujar(filas):
    # La fila de abajo queda sobre la línea base
    inicio, _ = columnas(filas)
    pluma = TTGlyphPen(None)
    for i, fila in enumerate(filas):
        y = (len(filas) - 1 - i) * PIXEL
        for j, c in enumerate(fila):
            if c != "#":
                continue
            x = (j - inicio) * PIXEL
            pluma.moveTo((x, y))
            pluma.lineTo((x, y + PIXEL))
            pluma.lineTo((x + PIXEL, y + PIXEL))
            pluma.lineTo((x + PIXEL, y))
            pluma.closePath()
    return pluma.glyph()

def main():
    figuras = dict(LETRAS)
    for letra, (base, marca) in CON_MARCA.items():
        figuras[letra] = marca + LETRAS[base]

    nombres = {c: "g%04X" % ord(c) for c in figuras}
    orden = [".notdef"] + list(nombres.values())

    # Las minúsculas usan las mismas figuras que las mayúsculas
    mapa = {}
    for c, nombre in nombres.items():
        mapa[ord(c)] = nombre
        if c.lower() != c:
            mapa[ord(c.lower())] = nombre

    fuente = FontBuilder(1000, isTTF=True)
    fuente.setupGlyphOrder(orden)
    fuente.setupCharacterMap(mapa)
    glifos = {".notdef": TTGlyphPen(None).glyph()}
    glifos.update({nombres[c]: dibujar(filas) for c, filas in figuras.items()})
    fuente.setupGlyf(glifos)
    # Ancho de la letra más un pixel de separación
    metricas = {".notdef": (ESPACIO * PIXEL, 0)}
    for c, filas in figuras.items():
        inicio, fin = columnas(filas)
        metricas[nombres[c]] = ((fin - inicio + 2) * PIXEL, 0)
    fuente.setupHorizontalMetrics(metricas)
    fuente.setupHorizontalHeader(ascent=900, descent=-200)
    fuente.setupNameTable({"familyName": "PixelNeon", "styleName": "Regular"})
    fuente.setupOS2(sTypoAscender=900, sTypoDescender=-200, usWinAscent=900, usWinDescent=200)
    fuente.setupPost()

    os.makedirs(os.path.dirname(SALIDA), exist_ok=True)
    fuente.save(SALIDA)
    print("Fuente generada en", SALIDA)

main()
