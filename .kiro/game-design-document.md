# Fox's Tale — Game Design Document v0.4

> Documento vivo. Actualizar cada vez que se tomen decisiones de diseño.
> Consultar antes de implementar cualquier feature nuevo.
>
> **Última actualización:** 11/11/2026 — progresión de salud, fragmentos de corazón, daño por caídas y HUD aclarados

---

## 1. Visión general

Plataformero 2D de aventura por mundos. El loop central es exploración + habilidad de plataformas. La dificultad actúa como filtro natural — no hay tiempo límite forzado, el jugador avanza a su ritmo pero la curva de habilidad lo retiene. El juego premia la exploración completa con una capa de economía de suerte (dados y tréboles) que extiende la sesión sin frustrar.

**MVP actual:** 1 mundo. El selector visible actualmente presenta 8 niveles; hay escenas adicionales en el repositorio cuyo alcance para la campaña todavía debe confirmarse antes de exponerlas.
**Plataformas objetivo:** Android, iOS.
**Audiencia objetivo para tiendas:** adolescentes y adultos; el producto no se dirige a niños. La preferencia actual es no añadir una pantalla de edad adicional. Si una exigencia concreta de la tienda hiciera necesaria una pantalla adicional, el dueño prefiere cambiar el target a 18+; esa alternativa todavía no está seleccionada. Alinear contenido, ficha y material promocional con la audiencia declarada. Ver `.kiro/handoff.md` para los pendientes de monetización, privacidad y configuración externa.

---

## 2. Loop de juego

```
Mapa de selección
    ↓
Jugar nivel
    ↓ recibir daño (enemigos, pinchos u otros peligros)
Pierde salud → breve invulnerabilidad
    ↓ salud agotada
Consume 1 vida global → respawn en waypoint con salud llena
    ↓ sin vidas globales
Pantalla Game Over
    ├── Ver ad (rewarded) → +1-2 vidas → continuar desde donde murió
    ├── Gastar 1 Trébol → lanzar dado mejorado (3-5 ó 2-6) → continuar
    └── Volver al mapa → reset a 3 vidas → reiniciar nivel desde 0
    ↓ completar nivel
Pantalla de resultados (estrellas ⭐⭐⭐)
    ↓ si gemas = total del nivel (ej. 10/10)
Pantalla del dado de vidas
    ↓
Volver al mapa
```

---

## 3. Salud y vidas globales

**Vidas por defecto al iniciar / tras Game Over + volver al mapa:** 3
**Sin límite máximo de vidas acumuladas.**

La salud del HUD y las vidas globales son dos recursos distintos:

- **Salud:** corazones mostrados en el HUD. La partida comienza con **3 corazones** (6 unidades, cada unidad representa medio corazón).
- **Vidas globales:** contador de reintentos persistentes, mostrado bajo `LivesContainer`. Se consume cuando la salud llega a cero; llegar a cero vidas globales abre Game Over.
- Cada impacto válido quita **medio corazón**. Después, el jugador recibe una breve invulnerabilidad para no recibir varios impactos del mismo contacto continuo.
- Caer al vacío u otra `DeathZone` también quita medio corazón y devuelve al jugador al último waypoint. Si esa caída agota la salud, cuenta como muerte y consume una vida global.
- Las caídas no restauran la salud al respawnear. Una vida global sí restaura la salud al máximo.
- El tutorial mantiene su excepción actual de reintentos ilimitados; las caídas siguen causando daño.

### Fragmentos de corazón permanente

- La primera campaña tiene 8 niveles visibles (sin contar `Level_0`, el tutorial). Se coloca **un fragmento en cada uno de los niveles 1–6**; no se colocan fragmentos en los niveles 7 y 8.
- Cada dos fragmentos recolectados aumentan permanentemente la salud máxima en **un corazón completo**. Los fragmentos y los corazones ganados se conservan al cambiar de nivel y al reiniciar la aplicación; `ResetProgress` los borra al iniciar una partida nueva.
- Cada nivel concede su fragmento una sola vez. El objeto desaparece en visitas posteriores.
- Los fragmentos de corazón son una progresión distinta de los fragmentos del Dado Secreto.
- El HUD reserva seis slots de corazones permanentes y un slot adicional temporal para el corazón del Dado Secreto. El máximo visible de la campaña puede subir de 3 a 6 corazones permanentes, además del bonus temporal del dado.

### Comportamiento al morir
- `currentHealth = 0` → consume 1 vida global → respawn desde último waypoint → salud se resetea al máximo
- Vidas > 0 → respawn normal
- Vidas = 0 → **Game Over**

### Game Over — opciones

| Opción | Condición | Resultado |
|--------|-----------|-----------|
| Ver ad (rewarded) | Siempre disponible | +1 a +2 vidas → continuar desde donde murió |
| Gastar 1 Trébol 🍀 | Tener al menos 1 Trébol | Lanzar dado mejorado (3-5 ó 2-6, aleatorio) → vidas se suman → continuar |
| Volver al mapa | Siempre disponible | Reset a 3 vidas → reiniciar nivel desde el principio |

### Persistencia de vidas
- Persisten entre niveles durante la sesión
- Game Over + volver al mapa → reset a 3
- Nueva partida → reset a 3
- Vidas ganadas por dado se acumulan sin límite

### Score futuro *(pendiente de definir)*
- Registrar vidas con las que el jugador terminó un mundo
- Score por nivel — diseño TBD
- Pantalla de resumen al terminar un mundo (ver sección 15)

---

## 4. Coleccionables por nivel

| Coleccionable | Cantidad por nivel | Propósito |
|---|---|---|
| **Gemas** 💎 | Variable (ej. 10) | Métrica de rendimiento — badges, estrellas, activa el dado de vidas |
| **Trébol** 🍀 | 1 por nivel | Currency de suerte — mejora el dado de vidas |
| **Fragmento de Dado Secreto** 🎲 | 0 ó 1 (zonas secretas) | Al reunir todos los del mundo → Dado Secreto |

---

## 5. Gemas 💎

- Contador local por nivel: `X / Total` en el HUD
- Se guarda el récord histórico por nivel
- **10/10 gemas** → activa la pantalla del dado al terminar el nivel
- Las gemas **NO son currency** — no se acumulan globalmente, no se gastan
- Sirven para: rating de estrellas + activar el dado de vidas

---

## 6. Trébol 🍀

- Exactamente **1 por nivel**, en un lugar que requiere exploración
- Al recolectarlo: guardado permanentemente (`TrebolLevel_N = true`)
- **No se pierde** nunca — ni al morir, ni al Game Over, ni al repetir el nivel
- Si ya tienes el trébol del nivel, aparece translúcido (ya obtenido)
- **Uso:** gastar 1 trébol antes de lanzar el dado → cambia al dado mejorado
  - El tipo recibido es **aleatorio** (3-5 ó 2-6) — parte de la suerte
  - 1 trébol por lanzamiento
  - Disponible en: fin de nivel y en Game Over
- Los tréboles se acumulan sin límite

### Visibilidad
- **HUD:** ícono 🍀 + contador total
- **MapPoint en el mapa:** badge 🍀 si ya obtuviste el del nivel
- **Pantalla del dado:** contador de tréboles disponibles

---

## 7. Fragmento de Dado Secreto 🎲

### Distribución
- **3 fragmentos por mundo** (~10 niveles por mundo)
- Zonas secretas: paredes falsas u otras rutas ocultas
- Máximo 1 fragmento por nivel — no todos los niveles tienen uno

### Ensamblado
- Al recolectar todos los fragmentos del mundo → se ensambla el **Dado Secreto**
- Animación de celebración al ensamblar
- El dado secreto queda en inventario hasta que se use

### Efecto al lanzar el Dado Secreto
- **Vidas:** rango premium (4-6) — mayor que el dado mejorado
- **Corazón extra:** +1 corazón de salud máximo temporal (hasta volver al mapa o Game Over definitivo)
- El corazón extra es acumulable con múltiples dados secretos
- Implementado en `PlayerHealth.AddBonusHeart()` / `ResetBonusHearts()`

### Diseño de zonas secretas *(pendiente — no implementado en niveles)*
- Paredes falsas atravesables
- Visuales que generen curiosidad sin ser obvios

### Pendiente de definir
- ¿El Dado Secreto se usa en Game Over también o solo en fin de nivel?
- ¿Cuándo desaparece el corazón extra (daño, sesión, Game Over)?
- ¿Qué pasa con múltiples Dados Secretos acumulados?

---

## 8. Sistema del dado de vidas

### 8.1 Cuándo aparece
- **Fin de nivel con 10/10 gemas:** después de la pantalla de estrellas, antes de volver al mapa
- **Game Over:** si el jugador gasta un Trébol

### 8.2 Flujo completo — fin de nivel

```
[Pantalla de resultados — estrellas]
    ↓ (si gemas = total del nivel)
[Pantalla del dado — DiceRollController]
    Muestra: Dado Normal (1-3) + contador de tréboles

    "¿Mejorar con Trébol?" (botón activo solo si tienes tréboles)
        → SÍ: animación de cambio, dado mejorado aleatorio (3-5 ó 2-6), tréboles -1
        → NO: usar dado normal

    "Lanzar" → números cambian rápidamente (WaitForSecondsRealtime)
    "Parar"  → fija el resultado
    (o para solo al agotar rollDuration = 2s)

    Resultado animado → vidas sumadas al contador
    "Continuar" → SceneUtils.ToSelectionScene()
```

### 8.3 Flujo desde Game Over

```
[Game Over]
    "Gastar Trébol" → DiceRollController.ShowFromGameOver(tipo_aleatorio, callback)
    → Misma pantalla del dado
    → Al cerrar: callback respawnea al jugador (no va al mapa)
```

### 8.4 Tipos de dado

| Tipo (DiceType) | Rango de vidas | Cómo se obtiene |
|---|---|---|
| `Normal` | 1 – 3 | Por defecto al tener 10/10 gemas |
| `Upgraded_3_5` | 3 – 5 | Gastar 1 Trébol (50% probabilidad) |
| `Upgraded_2_6` | 2 – 6 | Gastar 1 Trébol (50% probabilidad) |
| `Secret` | 4 – 6 + corazón extra | Ensamblar todos los fragmentos del mundo |

---

## 9. Sistema de estrellas por nivel

| Estrellas | Condición |
|---|---|
| ⭐ | Completar el nivel |
| ⭐⭐ | Completar + todas las gemas del nivel |
| ⭐⭐⭐ | Completar + todas las gemas + dentro del tiempo objetivo |

- Guardadas como récord histórico por nivel (`SaveManager.SaveRatingIfBetter`)
- Visibles en el MapPoint del mapa de selección
- Visibles en la pantalla de resultados al terminar

---

## 10. Monetización — Ads e IAP

**SDK Ads:** Unity LevelPlay (antes ironSource).
**SDK IAP:** RevenueCat.

### Ads

| Tipo | Dónde | Condición |
|------|-------|-----------|
| Rewarded Video | Game Over → "Ver Ad" | Siempre disponible como opción |
| Interstitial | Game Over → "Volver al mapa" | Solo si han pasado >120s de sesión y >60s desde el anterior |

**Reglas fijas:**
- Nunca durante gameplay
- Rewarded siempre opcional
- Cooldown de inicio de sesión: 120 segundos
- Si `NoAds` está comprado: `AdsManager` no muestra ads automáticamente

### IAP — Sin Anuncios

| Producto | SKU | Precio |
|----------|-----|--------|
| Sin Anuncios (NoAds) | `com.fatcatgamestudio.foxstale.noads` | A definir |

- Compra única (one-time purchase)
- Al comprar: desactiva permanentemente todos los ads
- Se preserva al resetear el progreso del juego
- Implementado en `RevenueCatManager.cs` + `NoAdsPurchaseButton.cs`

---

## 11. Múltiples mundos

**Arquitectura:** `WorldData` ScriptableObject por mundo.

```
Assets → Create → Fox's Tale → World Data
```

Cada `WorldData` define:
- Nombre del mundo
- Lista de escenas (`Level_1`, `Level_2`...)
- Estrellas del mundo anterior necesarias para desbloquear
- Total de gemas, tréboles y fragmentos de dado

`WorldProgressManager` calcula estrellas totales, estado de desbloqueo y progreso del mundo.

`MapPointsHolder` soporta **dos modos**:
- `worldData = null` → modo legacy (MVP, índices secuenciales)
- `worldData` asignado → modo multi-mundo (lee desde ScriptableObject)

El MVP actual usa el modo legacy. Para el Mundo 2: crear `WorldData_World2`, asignarlo en el Inspector.

---

## 12. HUD durante gameplay

| Elemento | Código | Estado |
|---|---|---|
| ❤️ Corazones de salud | `PlayerHealth`, `UIController.hearts` | ✅ Tres iniciales; hasta seis permanentes y un bonus temporal |
| 💎 Gemas `X / Total` | `UIController.gemsLevelText` | ✅ Funcional |
| 🍀 Contador de tréboles | `CloverContainer/Clover_Txt`, `UIController.trebolCountText` | ✅ Referencia serializada al texto dentro de su contenedor |
| ❤️ Vidas globales | `LivesContainer/Lives_Txt`, `UIController.livesText` | ✅ Referencia serializada al texto dentro de su contenedor |
| 🧩 Fragmentos de corazón | `HeartFragmentPickup`, `SaveManager` | ✅ Guardado único por nivel; pareja = un corazón permanente. Un contador parcial del HUD queda por diseñar. |

### Estado de integración y QA — 11/11/2026

- `UI-Canvas.prefab` conserva los contenedores separados del rediseño: `CloverContainer` contiene icono y contador; `LivesContainer` contiene icono y vidas globales; `LifesContainer` contiene corazones de salud.
- Los slots de salud se leen desde el prefab serializado; no se crean dinámicamente. El primer slot temporal corresponde al Dado Secreto.
- Hay un prefab `HeartFragmentPickup` y una instancia prevista en cada uno de los niveles 1–6. El sistema persiste la primera recolección por nivel.
- `DeathZone` ahora aplica daño de caída antes del respawn de waypoint. Un respawn por caída conserva la salud restante; una muerte por salud agotada consume vida global y repone salud.
- Este cambio aún requiere validación en Unity Play Mode y en dispositivo: verificar ubicación física de los fragmentos, impactos con collider hijo, ventana de invulnerabilidad, daño de caída, checkpoints y navegación del HUD. El estado estático del repositorio no equivale a prueba de ejecución.

---

## 13. Mapa de selección — MapPoint

| Elemento | Estado |
|---|---|
| Nombre del nivel | ✅ |
| Gemas `X / Total` + badge "todas" | ✅ |
| Tiempo récord vs objetivo | ✅ |
| Estado bloqueado / desbloqueado | ✅ |
| Badge 🍀 trébol obtenido | ⚠️ Campo listo — falta sprite en Inspector |
| Estrellas (0-3) | ⚠️ Lógica lista — falta sprites en Inspector |

---

## 14. Assets visuales pendientes (para Antigravity)

El código está listo y esperando. Solo faltan los assets:

| Asset | Usado en |
|---|---|
| `Clover_Collectible.png` (normal + variante atenuada por tinte) | ✅ Pickup y tarjeta del selector; revisar badge del mapa |
| Efecto de recolección del trébol (diferente a la gema) | Nivel — pendiente |
| Sprite del Fragmento de Dado 🎲 | Nivel, HUD |
| Sprite dedicado del Fragmento de Corazón | Pendiente de arte: el pickup nuevo usa de momento el sprite existente de medio corazón |
| Arte dedicado del fragmento de corazón | Mejora visual del `HeartFragmentPickup`; hoy reutiliza el sprite existente de medio corazón |
| Animación de ensamblado del Dado Secreto | Overlay/notificación |
| Sprites de los 3 dados (Normal, Mejorado, Secreto) | Pantalla del dado |
| Animación de cambio de dado (al gastar trébol) | Pantalla del dado |
| Layout visual completo de la pantalla del dado | `DiceRollController` |
| Layout visual del panel de Game Over | `GameOverController` |
| Sprites de estrella llena / vacía | `UIController.starFull/starEmpty` — referencias ya serializadas; revisar presentación en resultados |
| Badge de trébol en MapPoint | `MapPoint.trebolBadge` |
| Ajustes finales de layout del HUD para seis corazones permanentes + bonus del dado | `UI-Canvas.prefab`; requiere revisión en resoluciones móviles |

---

## 15. Controles, plataformas y game feel

### Comportamiento deseado

- En móvil, mostrar los controles táctiles durante gameplay cuando no haya un mando físico conectado. Ocultarlos al detectar un gamepad y volver a mostrarlos cuando se desconecte.
- Mantener gameplay en landscape con autorrotación hacia ambos lados.
- Poder navegar menús y selector con toque, teclado y gamepad; el foco no debe activar ni seleccionar otro botón por accidente.
- Las plataformas móviles horizontales y verticales deben llevar al jugador mientras se mantiene encima, sin vibración; el salto debe permitir bajar de ellas con control.
- Las caídas y peligros quitan salud, respetan la breve invulnerabilidad y devuelven al último waypoint cuando se trata de una DeathZone.

### Game feel — pendientes de iteración

- Jefe final: añadir impacto visual/sonoro y una resolución más épica al pisotón final.
- Enemigos voladores: ajustar ventana de golpe, colisión y respuesta para que sea más legible y satisfactorio acertarles.
- Revisar coyote time y la tolerancia del salto junto a colliders móviles.
- Añadir feedback claro a daño, recolección de tréboles/fragmentos y pérdida de una vida global.

### QA aún abierto

Los cambios de input/UI y de transporte de plataformas se han integrado varias veces y han tenido regresiones reportadas en dispositivos. Las reglas anteriores son el objetivo de diseño; no se consideran verificadas hasta probar la escena/build actual en teléfono con y sin mando físico.

---

## 16. Decisiones de diseño pendientes

- [ ] **Dado Secreto en Game Over:** ¿se puede usar o solo en fin de nivel?
- [ ] **Corazón extra:** ¿desaparece al recibir daño, al morir, o al Game Over?
- [ ] **Múltiples Dados Secretos acumulados:** ¿se pueden guardar varios y usar cuando quieras?
- [ ] **Paredes falsas:** mecánica exacta de activación (¿siempre atravesables, con botón, con objeto clave?)
- [ ] **Score por nivel:** diseño del sistema de puntuación y leaderboards
- [ ] **Pantalla de resumen de mundo:** al completar todos los niveles (estrellas, tréboles, tiempo total)
- [ ] **Desbloqueo de Mundo 2:** cuántas estrellas del Mundo 1 se necesitan

---

## 17. Referencia técnica — sistemas implementados

| Sistema | Archivo(s) | Estado |
|---|---|---|
| Salud | `PlayerHealth.cs` | ✅ |
| Vidas globales | `LivesManager.cs` | ✅ |
| Respawn | `LevelManager.cs` | ✅ |
| Fragmentos de corazón permanente | `HeartFragmentPickup.cs`, `SaveManager.cs` | ✅ Integrados en niveles 1–6; pendiente QA de colocación y ejecución |
| Game Over | `GameOverController.cs` | ✅ (ad real en Fase 4) |
| Ads (Unity LevelPlay) | `AdsManager.cs` | ⚠️ SDK declarado; falta resolverlo, consentimiento, configuración iOS, escenas y pruebas |
| IAP NoAds (RevenueCat) | `RevenueCatManager.cs`, `NoAdsPurchaseButton.cs` | ⚠️ SDK declarado; falta app/productos/entitlement, keys, UI en escena y pruebas |
| Gemas por nivel | `LevelManager.cs` | ✅ |
| Rating de estrellas | `LevelRatingSystem.cs` | ✅ |
| Persistencia | `SaveManager.cs` | ✅ |
| Trébol — coleccionable | `TrebolPickup.cs` | ✅ |
| Trébol — persistencia | `SaveManager.HasTrebol()` etc. | ✅ |
| Fragmento de Dado | `DiceFragmentPickup.cs` | ✅ |
| Dado de vidas — UI | `DiceRollController.cs` | ✅ (falta assets Antigravity) |
| Corazón extra (Dado Secreto) | `PlayerHealth.AddBonusHeart()` | ✅ |
| HUD | `UIController.cs`, `UI-Canvas.prefab` | ✅ Contenedores y slots serializados; pendiente QA visual en móvil |
| MapPoint badge trébol | `MapPoint.trebolBadge` | ⚠️ Falta asignar sprite en Inspector |
| Múltiples mundos | `WorldData.cs`, `WorldProgressManager.cs` | ✅ |
| Builds automáticos | `BuildAutomation.cs` | ✅ |
| Setup automático de escenas | `SceneElementSetup.cs` | ✅ |
