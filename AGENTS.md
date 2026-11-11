# AGENTS.md — Laberinto Neón

Guía para agentes de IA (Claude Code, Codex u otros) que trabajen en este repositorio.
El diseño del juego está en [Docs/GDD_Laberinto_Neon.md](Docs/GDD_Laberinto_Neon.md) y los
requisitos en [Docs/PRD_Laberinto_Neon.md](Docs/PRD_Laberinto_Neon.md).

## Proyecto

- Unity **6000.6.0f1**, URP 2D, Input System (`activeInputHandler: 1`), uGUI + TextMeshPro.
- Paquete `com.unity.pipeline` para controlar el Editor desde la terminal (`unity command ...`).
- Escenas: `Assets/Scenes/MainMenu.unity` (primera del build) y `Assets/Scenes/Game.unity`.

## Estructura

```
Assets/Art/Propios/     sprites generados (no editar a mano)
Assets/Audio/SFX/       sonidos generados (no editar a mano)
Assets/Fonts/           PixelNeon.ttf (generada) y PixelNeon SDF.asset (la crea el Constructor)
Assets/Niveles/         Laberinto.txt: el mapa
Assets/Prefabs/         Pickups/ y Enemigos/ (los crea el Constructor)
Assets/Scripts/         Core/, Jugador/, Enemigos/, Pickups/, UI/
Assets/Settings/        Neon.asset (Bloom) y ajustes de URP
Herramientas/           Constructor.cs y los generadores de Python
Docs/                   GDD, PRD y planes
```

`Assets/Welcome/` y `Assets/TextMesh Pro/` vienen de la plantilla y del paquete; no se modifican.

## Cómo escribir código

- Nombres de clases, métodos, variables y archivos **en español** (`Jugador`, `MovimientoLaberinto`, `puntosRestantes`).
- Código simple, al nivel de un proyecto de clase: campos públicos, `FindAnyObjectByType`,
  corrutinas y `Keyboard.current`. Sin eventos, interfaces, singletons, pooling ni namespaces.
- Un script, una responsabilidad. El movimiento es por celdas y sin física; las colisiones se
  revisan por distancia.
- Comentarios cortos y neutros que explican qué hace el código o por qué. Nada de historia,
  fases ni notas de versiones.

## Cómo se construye

Las escenas, prefabs, UI, la fuente de TMP y los ajustes de importación **no se editan a mano**:
los arma `Herramientas/Constructor.cs`, que borra y vuelve a crear sus objetos en cada escena.
Si cambias la escena, cambia el Constructor y vuelve a ejecutarlo.

```bash
unity command editor set_auto_tick --enabled true
unity command script run --file Herramientas/Constructor.cs --entry Constructor.Todo --timeout 120
```

Las celdas especiales (inicio de Bit, salida y centro de la casa, bonus, inicio y esquina de cada
Glitch) están al inicio del Constructor y en `CrearEscenaJuego`. Coordenadas: `x` desde la
izquierda, `y` desde abajo; la primera fila de `Laberinto.txt` es la de arriba.

### Arte, sonido y fuente

```bash
python Herramientas/generar_sprites.py   # sprites y Paredes.png a partir de Laberinto.txt
python Herramientas/generar_sonidos.py   # efectos WAV
python Herramientas/generar_fuente.py    # fuente PixelNeon.ttf (requiere fontTools)
```

Si cambias `Laberinto.txt`, vuelve a generar los sprites (las paredes se dibujan desde el mapa)
y revisa las celdas especiales del Constructor.

## Cómo se verifica

No hay pruebas unitarias: cada cambio se verifica en el Editor real.

```bash
unity command script recompile
unity command script recompile_status          # repetir hasta completed / up_to_date
unity command console status                   # consoleErrors debe ser 0
unity command editor.playmode start
unity command script eval --file <prueba.cs>   # leer o forzar el estado del juego
unity command capture game_view --source camera --camera "Main Camera" --overlay true --output-path Temp/captura.png
unity command editor.playmode stop
```

- Con Unity en segundo plano, `--source screen` puede devolver un cuadro viejo; usa
  `--source camera` para una captura del momento.
- Los scripts de prueba van fuera de `Assets/` (por ejemplo en una carpeta temporal) para no
  disparar una importación.
- Si una prueba guarda un récord, bórralo al terminar: `PlayerPrefs.DeleteKey("Record")`.

## Commits

- **Conventional Commits** en español y en minúsculas: `feat: ...`, `fix: ...`, `docs: ...`,
  `refactor: ...`, `chore: ...`.
- Un commit por fase o cambio con sentido propio; push a `main` cuando el usuario lo pida.
- Solo a nombre del usuario: sin `Co-Authored-By`.
- PNG, WAV y TTF se suben con Git LFS (ya configurado en `.gitattributes`).
- No incluir `Assets/Welcome/2d-template.png` ni la carpeta `.claude/`.
