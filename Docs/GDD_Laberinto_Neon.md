# Laberinto Neón — Documento de Diseño (GDD)

> Actividad: juego al estilo de los arcades de laberinto, hecho con IA · Unity 6 (6000.6.0f1) · URP 2D
> Fecha del diseño: 10 de noviembre de 2026

---

## 1. Ficha

| | |
|---|---|
| **Género** | Arcade de laberinto (*maze chase*) 2D, vista desde arriba |
| **Temática** | Arcade clásico de neón con personajes propios |
| **Plataforma** | PC, teclado. Se juega desde el Editor o en una build de Windows |
| **Arte** | Pixel art propio generado con `Herramientas/generar_sprites.py` (16 px por celda) |
| **Sonido** | Efectos de 8 bits propios generados con `Herramientas/generar_sonidos.py` |
| **Controles** | WASD/flechas: moverse · Esc: pausa · Mouse: menús |

## 2. Premisa

**Bit** es un pequeño robot que vive dentro de una máquina arcade. Los **Glitches**, errores del
sistema, se escaparon de su celda y quieren borrarlo. Bit debe recoger todos los puntos de datos
del laberinto para limpiar el nivel. Los **núcleos** le dan unos segundos de poder para borrar a
los Glitches.

## 3. Ciclo de juego

```
¡Listo! → recorrer el laberinto comiendo puntos → esquivar a los Glitches
→ comer un núcleo → los Glitches se asustan y se pueden borrar → seguir comiendo
→ sin puntos: nivel completo → siguiente nivel más rápido → …
```

- **Victoria de nivel:** comer todos los puntos y núcleos.
- **Perder una vida:** tocar a un Glitch que no está asustado.
- **Fin de partida:** quedarse sin vidas. Se muestra el puntaje y el récord.

## 4. Personajes

### Bit (jugador)
- Se mueve de celda en celda. La dirección que se presiona se guarda y se aplica en cuanto el
  pasillo lo permite, para que las vueltas se sientan suaves.
- Puede dar media vuelta en cualquier momento.
- Velocidad base: 5 celdas por segundo.
- 3 vidas al empezar; vida extra al llegar a 10,000 puntos.

### Glitches (enemigos)

Todos usan la misma regla para moverse: en cada celda eligen, sin dar media vuelta, la dirección
que los deja más cerca de su **objetivo**. Lo que cambia es el objetivo:

| Glitch | Color | Objetivo al perseguir | Sale de la casa |
|---|---|---|---|
| **Rastreador** | Rojo | La celda de Bit | Empieza afuera |
| **Emboscador** | Rosa | 4 celdas delante de Bit | A los 2 s |
| **Errático** | Morado | En cada cruce: la mitad de las veces persigue a Bit, la otra mitad elige al azar | A los 5 s |
| **Tímido** | Naranja | Persigue a Bit si está lejos (más de 8 celdas); si está cerca, huye a su esquina | A los 8 s |

**Estados de un Glitch**

| Estado | Qué hace | Velocidad |
|---|---|---|
| En casa | Espera su turno para salir | — |
| Saliendo | Cruza la puerta hacia el laberinto | 3 |
| Dispersión | Va a su esquina del laberinto | 4.6 |
| Persecución | Va a su objetivo | 4.6 |
| Asustado | Azul, elige al azar; parpadea en blanco los últimos 2 s | 2.5 |
| Regresando | Solo ojos; vuelve a la casa y sale de nuevo | 10 |

- Dispersión y persecución se alternan para todos a la vez: 7 s de dispersión y 20 s de
  persecución. Al cambiar de modo dan media vuelta.
- Al comer un núcleo, los Glitches que están en el laberinto se asustan y dan media vuelta.

## 5. Puntaje

| Acción | Puntos |
|---|---|
| Punto de datos | 10 |
| Núcleo | 50 |
| Glitch asustado (en la misma racha de un núcleo) | 200, 400, 800, 1600 |
| Bonus (cristal) | 100 en el nivel 1, +200 por nivel, máximo 2000 |

- El **cristal bonus** aparece debajo de la casa al comer 70 y 170 puntos y dura 9 s.
- El **récord** se guarda con `PlayerPrefs`.

## 6. Niveles

- El laberinto es el mismo en todos los niveles.
- Cada nivel sube la velocidad de todos un 5 % (hasta +30 %) y baja el tiempo de susto en 1 s
  (de 7 s hasta un mínimo de 2 s).
- Al completar un nivel, las paredes parpadean y el nivel empieza de nuevo con todos los puntos.

## 7. Laberinto

27 × 24 celdas, simétrico, con un túnel a los lados que conecta izquierda y derecha.

```
###########################
#............#............#
#.####.#####.#.#####.####.#
#o####.#####.#.#####.####o#
#.........................#
#.####.#.#########.#.####.#
#......#.....#.....#......#
######.##### # #####.######
######.#           #.######
######.# ###---### #.######
######.# #CCCCCCC# #.######
      .  #CCCCCCC#  .
######.# #CCCCCCC# #.######
######.# ######### #.######
######.#           #.######
######.# ######### #.######
#............#............#
#.####.#####.#.#####.####.#
#o........... ...........o#
###.##.#.#########.#.##.###
#......#.....#.....#......#
#.##########.#.##########.#
#.........................#
###########################
```

`#` pared · `.` punto · `o` núcleo · `-` puerta de la casa · `C` casa ·
espacio: pasillo sin punto.

## 8. Pantallas

- **Menú principal:** título, récord, botones *Jugar* y *Salir*, y los controles.
- **Juego:** HUD arriba con puntaje, récord y nivel; vidas abajo. Mensaje *¡LISTO!* al empezar
  cada vida.
- **Pausa (Esc):** *Continuar* y *Menú*.
- **Fin de partida:** puntaje final, récord, *Reintentar* y *Menú*.

## 9. Sonidos

Comer punto (dos tonos alternados), núcleo, comer Glitch, muerte, inicio, nivel completo,
vida extra, bonus, clic de menú, sirena (fondo normal) y alarma (fondo con Glitches asustados).

## 10. Estructura del proyecto

```
Assets/
  Art/Propios/        sprites generados
  Art/Tiles/          tiles de las paredes
  Audio/SFX/          efectos generados
  Prefabs/Enemigos/   Glitch
  Prefabs/Pickups/    Punto, Núcleo, Bonus
  Scenes/             MainMenu, Game
  Scripts/Core/       GameManager, AudioManager, Laberinto, MovimientoLaberinto, Parpadeo
  Scripts/Jugador/    Jugador
  Scripts/Enemigos/   Enemigo
  Scripts/Pickups/    Bonus
  Scripts/UI/         HUD, MenuPrincipal, MenuPausa
Herramientas/         Constructor.cs, generar_sprites.py, generar_sonidos.py
Docs/                 GDD y planes
```
