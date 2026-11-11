# Laberinto Neón — Documento de Requisitos del Producto (PRD)

> Unity 6 (6000.6.0f1) · URP 2D · Fecha: 10 de noviembre de 2026
> Diseño detallado del juego: [GDD_Laberinto_Neon.md](GDD_Laberinto_Neon.md)

---

## 1. Resumen

*Laberinto Neón* es un juego arcade de laberinto en 2D: el jugador guía a **Bit**, un pequeño robot,
para comer todos los puntos del laberinto mientras esquiva a cuatro **Glitches** con
comportamientos distintos. Toma la mecánica del género (*maze chase*), pero con personajes,
laberinto, arte, sonido y fuente propios.

## 2. Objetivo

Entregar, como actividad de clase, un juego completo y jugable hecho con apoyo de IA, que demuestre:

- Un flujo de trabajo ordenado: diseño → plan → construcción por fases → pruebas → revisión.
- Control del editor de Unity desde la IA con el paquete **Pipeline** (`com.unity.pipeline`).
- Código simple, en español y con la misma organización que los demás proyectos del autor.
- Historial en GitHub con **Conventional Commits**, un commit por fase.

## 3. Usuarios

| Usuario | Necesidad |
|---|---|
| Jugador | Partidas cortas, controles simples con teclado, dificultad que sube por nivel |
| Profesor / evaluador | Abrir el proyecto, jugarlo y entender cómo se construyó y con qué herramientas |
| Desarrollador (o agente de IA) | Modificar el juego sin romperlo: estructura clara, documentación y una forma repetible de reconstruir escenas |

## 4. Alcance

### Requisitos funcionales

| ID | Requisito |
|---|---|
| RF-01 | Menú principal con título, récord, personajes, botones *Jugar* y *Salir*, y controles. |
| RF-02 | Laberinto de 27 × 24 celdas con puntos, 4 núcleos, casa de los Glitches y túnel lateral. |
| RF-03 | Bit se mueve por celdas con WASD o flechas, guarda la dirección presionada y puede dar media vuelta. |
| RF-04 | Comer un punto da 10, un núcleo 50; al comer todos se completa el nivel. |
| RF-05 | Cuatro Glitches con objetivos distintos: Rastreador, Emboscador, Errático y Tímido. |
| RF-06 | Los Glitches alternan dispersión y persecución, salen de la casa por turnos y vuelven a ella como ojos al ser comidos. |
| RF-07 | Un núcleo asusta a los Glitches; comerlos da 200, 400, 800 y 1600 en la misma racha. |
| RF-08 | Bit empieza con 3 vidas, gana una a los 10,000 puntos y pierde una al tocar un Glitch. |
| RF-09 | Cada nivel aumenta la velocidad y reduce el tiempo de susto. |
| RF-10 | Un cristal bonus aparece a los 70 y 170 puntos comidos y desaparece a los 9 s. |
| RF-11 | HUD con puntaje, récord, nivel, vidas y el mensaje *¡LISTO!*. |
| RF-12 | Pausa con Esc (*Continuar*, *Menú*) solo con el juego en marcha. |
| RF-13 | Pantalla de fin de partida con puntaje, récord, *Reintentar* y *Menú*. |
| RF-14 | Récord guardado entre partidas (`PlayerPrefs`). |
| RF-15 | Efectos de sonido para cada acción y fondos en bucle (sirena y alarma). |
| RF-16 | Menús usables con teclado (flechas + Enter) y con mouse. |

### Requisitos no funcionales

| ID | Requisito |
|---|---|
| RNF-01 | Todo el arte, sonido y la fuente son propios y se regeneran con scripts de Python en `Herramientas/`. |
| RNF-02 | Las escenas, prefabs y UI se construyen con `Herramientas/Constructor.cs` mediante el paquete Pipeline, de forma repetible. |
| RNF-03 | Scripts en español, cortos, con una responsabilidad; campos públicos, `FindAnyObjectByType`, corrutinas y `Keyboard.current`; sin eventos, interfaces ni singletons. |
| RNF-04 | Movimiento por celdas sin física; colisiones por distancia. |
| RNF-05 | 0 errores y 0 advertencias en la consola de Unity. |
| RNF-06 | Se ve bien en 16:9 (referencia 1920 × 1080); el pixel art usa filtro `Point`. |
| RNF-07 | Commits con Conventional Commits en español y push a `main` por fase. |

## 5. Fuera de alcance

- Varios laberintos o un editor de niveles.
- Controles táctiles o con mando, y builds para otras plataformas que no sean PC.
- Tabla de puntajes en línea o varios jugadores.
- Música de fondo (solo efectos y fondos cortos en bucle).

## 6. Criterios de aceptación

- [x] Desde *MainMenu*, *Jugar* (o Enter) abre el juego y muestra *¡LISTO!*.
- [x] Bit come puntos, cruza el túnel y no atraviesa paredes ni la puerta de la casa.
- [x] Los cuatro Glitches salen de la casa, persiguen con estilos distintos y matan a Bit al tocarlo.
- [x] Con un núcleo se asustan; al comer uno suma puntos, sus ojos vuelven a la casa y sale de nuevo.
- [x] Al comer todos los puntos las paredes parpadean y empieza el nivel 2, más rápido.
- [x] El bonus aparece al comer 70 puntos.
- [x] Esc pausa y reanuda; no pausa durante una muerte.
- [x] Sin vidas aparece el fin de partida y el récord se guarda.
- [x] La consola queda sin errores.

Cada criterio se comprobó en el Editor con el paquete Pipeline (`script eval` en Play Mode y
capturas de pantalla).

## 7. Riesgos y decisiones

| Riesgo | Decisión |
|---|---|
| Parecerse demasiado a un juego existente | Personajes, nombres, laberinto, arte, sonido y fuente propios; solo se toma la mecánica del género. |
| Paredes con esquinas mal unidas usando tiles | El laberinto completo se dibuja como una sola imagen a partir de `Laberinto.txt`. |
| Escenas difíciles de rehacer a mano | Un solo `Constructor` que borra y vuelve a crear los objetos de cada escena. |
| Revisión del código | Cada fase la revisó Codex en solo lectura; antes de entregar, una revisión adversarial. |
