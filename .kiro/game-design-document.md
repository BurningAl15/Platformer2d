# Fox's Tale — Game Design Document v0.1

> Documento vivo. Actualizar cada vez que se tomen decisiones de diseño.
> Consultar antes de implementar cualquier feature nuevo.
>
> **Última actualización:** 25/10/2026

---

## 1. Visión general

Plataformero 2D de aventura por mundos. El loop central es exploración + habilidad de plataformas. La dificultad actúa como filtro natural — no hay tiempo límite forzado, el jugador avanza a su ritmo pero la curva de habilidad lo retiene. El juego premia la exploración completa con una capa de economía de suerte (dados y tréboles) que extiende la sesión sin frustrar.

**MVP actual:** 1 mundo, ~10 niveles.

---

## 2. Loop de juego

```
Mapa de selección
    ↓
Jugar nivel
    ↓ morir
Respawn desde checkpoint (consume 1 vida)
    ↓ sin vidas
Pantalla Game Over
    ├── Ver ad → ganar 1-2 vidas → continuar desde donde murió
    ├── Gastar 1 Trébol → lanzar dado mejorado → ganar vidas → continuar desde donde murió
    └── Volver al mapa → reset a 3 vidas → debe reiniciar nivel desde 0
    ↓ completar nivel
Pantalla de resultados (estrellas)
    ↓ si gemas = total del nivel (10/10)
Pantalla del dado de vidas
    ↓
Volver al mapa
```

---

## 3. Sistema de vidas

**Vidas por defecto al iniciar / tras Game Over + volver al mapa:** 3
**Sin límite máximo de vidas acumuladas.**

### Comportamiento al morir
- `currentHealth = 0` → consume 1 vida → respawn desde último checkpoint → salud se resetea al máximo
- Vidas > 0 → respawn normal
- Vidas = 0 → **Game Over**

### Game Over — opciones

| Opción | Condición | Resultado |
|--------|-----------|-----------|
| Ver ad *(Fase 4)* | Siempre disponible | +1 a +2 vidas → continuar desde donde murió |
| Gastar 1 Trébol | Tener al menos 1 Trébol | Lanzar dado mejorado (3-5 ó 2-6, aleatorio) → vidas ganadas se suman → continuar desde donde murió |
| Volver al mapa | Siempre disponible | Reset a 3 vidas → debe reiniciar el nivel desde el principio |

### Persistencia de vidas
- Las vidas persisten entre niveles durante la sesión
- Game Over + volver al mapa → reset a 3
- Nueva partida → reset a 3
- Las vidas adicionales obtenidas por dado se acumulan sin límite máximo

### Score futuro *(pendiente de definir)*
- Registrar con cuántas vidas terminó el jugador un mundo
- Score por nivel — diseño TBD
- Posible pantalla de resumen al terminar un mundo

---

## 4. Coleccionables por nivel

Cada nivel tiene 3 tipos de coleccionables:

| Coleccionable | Cantidad por nivel | Propósito |
|---|---|---|
| **Gemas** 💎 | Variable (ej. 10) | Métrica de rendimiento — badges, estrellas, activa el dado de vidas |
| **Trébol** 🍀 | 1 por nivel | Currency de suerte — mejora el dado de vidas |
| **Fragmento de Dado Secreto** 🎲 | 0 ó 1 (solo en zonas secretas) | Al reunir todos los fragmentos del mundo → Dado Secreto |

---

## 5. Gemas 💎

- Contador local por nivel: `X / Total`
- Se guarda el récord histórico por nivel (mejor cantidad obtenida)
- **10/10 gemas** (o el total del nivel) → activa la pantalla del dado al terminar
- Las gemas **NO son currency** — no se acumulan globalmente, no se gastan
- Sirven para: rating de estrellas + activar el dado de vidas

---

## 6. Trébol 🍀

- Exactamente **1 por nivel**, en un lugar que requiere exploración o habilidad extra
- Al recolectarlo: guardado permanentemente (`TrebolLevel_N = true`)
- **No se pierde** si mueres, haces Game Over o repites el nivel
- Si ya tienes el trébol del nivel, no vuelve a aparecer (o aparece con estado visual "ya obtenido")
- **Uso:** gastar 1 trébol antes de lanzar el dado → cambia al dado mejorado
  - El tipo de dado mejorado recibido es **aleatorio** (3-5 ó 2-6) — parte de la suerte
  - 1 trébol por lanzamiento de dado
  - Disponible en: fin de nivel (antes de lanzar el dado) y en Game Over
- Sin otro uso definido actualmente
- Los tréboles se acumulan indefinidamente en el inventario del jugador

### Visibilidad del trébol
- **HUD durante el nivel:** ícono 🍀 + contador total del inventario
- **MapPoint en el mapa:** badge si ya obtuviste el trébol de ese nivel
- **Pantalla de resultados y pantalla del dado:** contador total de tréboles disponibles

---

## 7. Fragmento de Dado Secreto 🎲

### Distribución
- **~3 fragmentos por mundo** (basado en ~10 niveles por mundo)
- Ubicados en **zonas secretas** — paredes falsas u otras rutas ocultas que hacen pensar "¿por qué está eso ahí?"
- Máximo 1 fragmento por nivel — no todos los niveles tienen uno
- Solo en niveles difíciles o con rutas de exploración

### Ensamblado
- Al recolectar **todos los fragmentos del mundo** → se ensambla el **Dado Secreto**
- El ensamblado tiene animación de celebración
- El dado secreto queda en el inventario hasta que se use

### Efecto al lanzar el Dado Secreto
- **Vidas:** rango premium *(TBD — mayor que el dado mejorado)*
- **Corazón extra:** +1 corazón de salud máximo (temporal, hasta fin de sesión o hasta Game Over)
- El corazón extra es acumulable si se tienen múltiples dados secretos

### Diseño de zonas secretas *(pendiente)*
- Paredes falsas que se puedan atravesar corriendo o con un salto especial
- Visuales que generen curiosidad sin ser obvios
- Los secretos no están implementados en los niveles actuales del MVP

### Pendiente de definir
- Rango exacto de vidas del Dado Secreto
- ¿Se puede usar en Game Over o solo en fin de nivel?
- Mecánica exacta de las paredes falsas (¿atravesables siempre, o con input específico?)
- ¿El corazón extra desaparece solo al recibir daño o al fin de sesión completa?

---

## 8. Sistema del dado de vidas

### 8.1 Cuándo aparece
- **Fin de nivel con 10/10 gemas:** después de la pantalla de resultados (estrellas), antes de volver al mapa
- **Game Over:** como opción si el jugador tiene tréboles disponibles (usa dado mejorado)

### 8.2 Flujo — fin de nivel

```
[Pantalla de resultados — estrellas]
    ↓ (si gemas del nivel = total)
[Pantalla del dado]
    Muestra: tipo de dado activo → Dado Normal (1-3)
    Muestra: cuántos tréboles tienes disponibles

    Pregunta: "¿Usar 1 Trébol para mejorar el dado?"
        → SÍ: animación de cambio de dado
              revelación del nuevo tipo (3-5 ó 2-6, determinado aleatoriamente)
              contador de tréboles -1
        → NO: se usa el dado normal (1-3)

    Botón: "Lanzar"
        → Dado comienza a mostrar números cambiando rápidamente
        → Botón: "Parar" (o para solo tras N segundos si el jugador no interactúa)
        → Resultado: número de vidas ganadas
        → Animación de celebración + vidas sumadas al contador del jugador

    Botón: "Continuar" → volver al mapa
```

### 8.3 Tipos de dado

| Tipo | Rango de vidas | Cómo se obtiene | Notas |
|---|---|---|---|
| **Dado Normal** | 1 – 3 | Por defecto al tener 10/10 gemas | — |
| **Dado Mejorado A** | 3 – 5 | Gastar 1 Trébol (aleatorio A o B) | Mayor probabilidad esperada de buenos resultados |
| **Dado Mejorado B** | 2 – 6 | Gastar 1 Trébol (aleatorio A o B) | Rango mayor pero más varianza |
| **Dado Secreto** | TBD (premium) | Ensamblar todos los fragmentos del mundo | También da corazón extra |

**Nota de diseño:** El Mejorado A (3-5) y el B (2-6) tienen tradeoffs intencionales de suerte. El jugador no elige cuál recibe al gastar el trébol — eso también es parte de la mecánica de suerte.

**Nota técnica:** Los dados no son cubos de 6 caras. Son rangos discretos configurables. La animación de lanzamiento muestra números aleatorios del rango hasta que el jugador para o expira el tiempo.

### 8.4 Mecánica de parada
- El dado muestra números del rango cambiando rápidamente
- El jugador presiona un botón para detenerlo (suerte / timing)
- El número visible al parar es el resultado final
- Si el jugador no interactúa: para solo después de N segundos

---

## 9. Sistema de estrellas por nivel

| Estrellas | Condición |
|---|---|
| ⭐ | Completar el nivel |
| ⭐⭐ | Completar + todas las gemas del nivel |
| ⭐⭐⭐ | Completar + todas las gemas + dentro del tiempo objetivo |

- Guardadas como récord histórico por nivel
- Visibles en el MapPoint del mapa de selección
- Visibles en la pantalla de resultados al terminar el nivel

---

## 10. HUD durante gameplay

| Elemento | Estado de implementación |
|---|---|
| ❤️ Corazones de salud | ✅ Existe |
| 💎 Gemas del nivel `X / Total` | ⚠️ Existe (falta mostrar el `/Total`) |
| 🍀 Contador de tréboles totales | 🔲 Pendiente (campos en UIController listos, falta asignar en Inspector + assets) |
| 🔴 Vidas restantes (`x3`) | 🔲 Pendiente (campo en UIController listo, falta asignar en Inspector + assets) |
| 🎲 Indicador de trébol del nivel (ya obtenido) | 🔲 Baja prioridad — pendiente |

---

## 11. Mapa de selección — información por MapPoint

| Elemento | Estado de implementación |
|---|---|
| Nombre del nivel | ✅ Existe |
| Estrellas (0-3) | ⚠️ Lógica lista, falta visual |
| Gemas `X / Total` | ✅ Existe (parcial) |
| Badge: todas las gemas obtenidas | ✅ Existe |
| Badge: 🍀 trébol del nivel obtenido | 🔲 Pendiente |
| Tiempo récord vs objetivo | ✅ Existe |
| Estado: bloqueado / desbloqueado | ✅ Existe |

---

## 12. Assets visuales pendientes (para Antigravity)

Estos elementos necesitan diseño y assets antes de poder implementarse visualmente:

- [ ] Sprite del Trébol 🍀: estado normal, ya recogido, efecto al recoger (diferente al de la gema)
- [ ] Sprite del Fragmento de Dado Secreto 🎲 (en el nivel y en el inventario/HUD)
- [ ] Animación de ensamblado del Dado Secreto al reunir todos los fragmentos
- [ ] Pantalla del dado completa: fondo, animación de números girando, pantalla de resultado, celebración de vidas ganadas
- [ ] Sprites de los 3 tipos de dado (Normal, Mejorado, Secreto) con distinción visual clara
- [ ] Animación de cambio de dado al gastar un trébol
- [ ] Pantalla de Game Over: layout, 3 botones (Ad / Trébol-Dado / Volver al mapa)
- [ ] HUD actualizado: contador de vidas + contador de tréboles con iconografía
- [ ] Badge de trébol en MapPoint del mapa
- [ ] Animación de transición entre pantalla de resultados → pantalla del dado
- [ ] Efecto de recolección del trébol en nivel

---

## 13. Decisiones de diseño pendientes

- [ ] Rango exacto de vidas del Dado Secreto
- [ ] ¿El Dado Secreto se puede usar en Game Over o solo en fin de nivel?
- [ ] ¿Cómo funcionan exactamente las paredes falsas? (¿atravesables siempre, con input, o con objeto específico?)
- [ ] ¿El corazón extra del Dado Secreto dura toda la sesión o desaparece al recibir daño?
- [ ] ¿Qué pasa si tienes múltiples Dados Secretos acumulados?
- [ ] Score por nivel / vidas con las que terminaste un mundo — diseño TBD
- [ ] ¿Hay pantalla de resumen al terminar un mundo? (suma de estrellas, tréboles, tiempo total)
- [ ] Otros usos futuros posibles para los tréboles

---

## 14. Referencia técnica — sistemas implementados en código

| Sistema | Archivo | Estado |
|---|---|---|
| Salud del jugador | `PlayerHealth.cs` | ✅ |
| Vidas globales | `LivesManager.cs` | ✅ Lógica lista |
| Pantalla Game Over (sin trébol aún) | `GameOverController.cs` | ⚠️ Ad simulado, opción trébol pendiente |
| Gemas por nivel | `LevelManager.cs` | ✅ |
| Rating de estrellas | `LevelRatingSystem.cs` | ✅ Lógica lista, falta UI visual |
| Persistencia general | `SaveManager.cs` | ✅ |
| Trébol — coleccionable | — | 🔲 Pendiente |
| Trébol — persistencia en SaveManager | `SaveManager.cs` | 🔲 Pendiente (agregar `HasTrebol`, `SetTrebolCollected`, `LoadTotalTrebols`) |
| Fragmento Dado Secreto | — | 🔲 Pendiente (después de diseño de secretos en niveles) |
| Pantalla del dado | — | 🔲 Pendiente (bloqueado hasta assets de Antigravity) |
| Dado Normal / Mejorado / Secreto — lógica | — | 🔲 Pendiente |
| HUD de vidas y tréboles | `UIController.cs` | ⚠️ Campos listos, falta asignar en Inspector |
| MapPoint badge de trébol | `MapPoint.cs` | 🔲 Pendiente |
