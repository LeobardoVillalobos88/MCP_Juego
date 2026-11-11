# Laberinto Neón — Plan de implementación

**Meta:** Un juego completo de laberinto al estilo arcade: menú, laberinto con puntos y núcleos,
4 Glitches con comportamientos distintos, vidas, niveles, bonus, pausa, fin de partida y sonido.

**Arquitectura:** Scripts cortos, en español y con una sola responsabilidad, al nivel de
`Juego Halloween` y `Asteroids`. Se encuentran entre sí con `FindAnyObjectByType` o con referencias
del Inspector. Toda la escena, prefabs, UI e importación la arma `Herramientas/Constructor.cs`,
que se ejecuta con el paquete Pipeline (`unity command script run`), así la construcción es repetible.

**Tecnología:** Unity 6000.6.0f1 · URP 2D · Input System (`Keyboard.current`) · uGUI + TextMeshPro ·
paquete `com.unity.pipeline` · Python 3 para el arte y el sonido.

## Reglas generales

- Diseño de referencia: `Docs/GDD_Laberinto_Neon.md`.
- Nombres de scripts, clases, variables y métodos en español.
- Comentarios cortos y neutros.
- Campos públicos, corrutinas y `Keyboard.current`; sin eventos, interfaces, singletons ni pooling.
- Movimiento por celdas, sin física: las colisiones se revisan por distancia.
- Sprites a 16 px por unidad (1 celda = 1 unidad), filtro `Point`, sin compresión.
- Commits con Conventional Commits en español y `git push` a `main` al terminar cada fase.

## Cómo se verifica

Cada fase se revisa en el Editor real con el paquete Pipeline: compilar sin errores, construir con el
`Constructor`, entrar a Play, leer el estado con `script eval` y tomar capturas del Game view.

```bash
unity command script recompile
unity command script recompile_status
unity command script run --file Herramientas/Constructor.cs --entry Constructor.Todo
unity command console status
unity command editor.playmode start
unity command capture game_view --source screen --save_path Screenshots/juego.png
```

## Fases

1. **Configuración y documentos** — paquete Pipeline, GDD y este plan.
2. **Arte y sonido** — `generar_sprites.py` (Bit, Glitches, ojos, asustado, puntos, núcleo, cristal,
   16 paredes según sus vecinos, puerta, vida) y `generar_sonidos.py` (efectos en WAV).
3. **Laberinto y jugador** — `Laberinto` lee el mapa, pinta el Tilemap y coloca los puntos;
   `MovimientoLaberinto` mueve de celda en celda con el túnel; `Jugador` lee el teclado, come y anima.
4. **Glitches** — `Enemigo` con estados, objetivos por tipo, susto y regreso a casa.
5. **Partida** — `GameManager` (puntaje, vidas, niveles, modos, bonus, récord), `HUD`,
   `AudioManager`, menú principal, pausa y fin de partida.
6. **Pulido** — brillo de neón (Bloom), ajustes de balance, prueba completa y revisión final.
