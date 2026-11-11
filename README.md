# Laberinto Neón

Juego arcade de laberinto en 2D hecho en **Unity 6**. Guía a **Bit**, un pequeño robot, para
comer todos los puntos del laberinto mientras esquivas a cuatro **Glitches**. Come un **núcleo**
y por unos segundos podrás borrarlos tú a ellos.

Inspirado en los arcades de laberinto clásicos, pero con personajes, laberinto, pixel art,
sonidos y fuente **propios**, generados por script.

![Menú principal](Docs/Capturas/menu.png)
![Partida](Docs/Capturas/juego.png)

## Cómo jugar

| Tecla | Acción |
|---|---|
| WASD / flechas | Mover a Bit (la dirección se guarda y se aplica en la siguiente vuelta) |
| Esc | Pausa |
| Enter / mouse | Elegir en los menús |

- Come todos los puntos para pasar de nivel. Cada nivel es más rápido.
- Los túneles de los lados te llevan al otro extremo del laberinto.
- Los núcleos asustan a los Glitches: cómelos seguidos para ganar 200, 400, 800 y 1600 puntos.
- El cristal que aparece debajo de la casa da puntos extra.
- Empiezas con 3 vidas y ganas una a los 10,000 puntos.

### Los Glitches

| | Glitch | Cómo persigue |
|---|---|---|
| 🔴 | **Rastreador** | Va directo a donde está Bit |
| 🩷 | **Emboscador** | Apunta 4 celdas delante de Bit |
| 🟣 | **Errático** | A veces persigue, a veces se va al azar |
| 🟠 | **Tímido** | Persigue de lejos, pero huye a su esquina si se acerca |

## Requisitos

- Unity **6000.6.0f1** (con URP 2D).
- Git LFS (los PNG, WAV y la fuente se guardan con LFS).

## Abrir y jugar

1. Clona el repositorio con LFS:
   ```bash
   git lfs install
   git clone https://github.com/LeobardoVillalobos88/MCP_Juego.git
   ```
2. Ábrelo con Unity Hub (Unity 6000.6.0f1).
3. Abre `Assets/Scenes/MainMenu.unity` y presiona **Play**.

Para una build de Windows: *File > Build Profiles*, las escenas ya están en orden
(MainMenu y Game).

## Cómo está hecho

- **Movimiento por celdas** sin física: `MovimientoLaberinto` lleva a Bit y a los Glitches de centro
  a centro de celda; al llegar, cada uno decide su siguiente dirección.
- **IA de los Glitches**: en cada celda eligen, sin dar media vuelta, la dirección que más los
  acerca a su objetivo. Cada tipo calcula un objetivo distinto (ver tabla de arriba).
- **El laberinto** vive en un archivo de texto (`Assets/Niveles/Laberinto.txt`). El juego lo lee
  para saber dónde hay paredes y puntos, y el generador de sprites lo usa para dibujar las paredes.
- **Construcción repetible**: `Herramientas/Constructor.cs` arma las escenas, prefabs, UI y la
  fuente desde el Editor, ejecutado con el paquete **Pipeline** de Unity:
  ```bash
  unity command script run --file Herramientas/Constructor.cs --entry Constructor.Todo --timeout 120
  ```
- **Recursos propios** generados con Python:
  ```bash
  python Herramientas/generar_sprites.py
  python Herramientas/generar_sonidos.py
  python Herramientas/generar_fuente.py
  ```

### Estructura

```
Assets/Scripts/Core/      GameManager, AudioManager, Laberinto, MovimientoLaberinto, Parpadeo, Efimero
Assets/Scripts/Jugador/   Jugador
Assets/Scripts/Enemigos/  Enemigo
Assets/Scripts/Pickups/   Bonus
Assets/Scripts/UI/        HUD, MenuPrincipal, MenuPausa
Herramientas/             Constructor y generadores de arte, sonido y fuente
Docs/                     GDD, PRD, plan y capturas
```

## Documentación

- [GDD — Documento de diseño](Docs/GDD_Laberinto_Neon.md)
- [PRD — Requisitos del producto](Docs/PRD_Laberinto_Neon.md)
- [Plan de implementación](Docs/Planes/2026-11-10-juego-completo.md)
- [AGENTS.md — Guía para agentes de IA](AGENTS.md)

## Hecho con IA

El juego se diseñó y construyó con **Claude Code**, controlando el Editor de Unity con el paquete
Pipeline (compilar, construir escenas, entrar a Play, leer el estado del juego y tomar capturas).
Cada fase la revisó **Codex** en modo de solo lectura, y antes de la entrega se hizo una revisión
adversarial. El historial de commits sigue Conventional Commits, una fase por commit.
