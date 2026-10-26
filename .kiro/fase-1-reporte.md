# Fase 1 — Controles: Reporte de cambios

**Fecha:** 25/10/2026  
**Depende de:** Fase 0 completada.

---

## Resumen ejecutivo

Se implementó el soporte completo de controles en tres ejes:
1. **Mobile (on-screen):** los botones virtuales ahora tienen una arquitectura real usando `OnScreenStick` / `OnScreenButton` nativos del Input System.
2. **Gamepad físico:** `GamepadDetector` detecta conexión/desconexión en tiempo real (USB-C y Bluetooth).
3. **Auto-ocultamiento:** al conectar un gamepad físico, los controles on-screen se ocultan automáticamente; al desconectarlo, vuelven si es plataforma mobile.

Los bindings de Gamepad ya existían en `PlayerInputSystem.inputactions` desde antes (Left Stick → Move, buttonSouth → Jump, start → Pause, etc.). No fue necesario modificar el `.inputactions`.

---

## Archivos creados

### `Assets/Scripts/Mobile/GamepadDetector.cs` (NUEVO)
**Qué hace:** Se suscribe a `InputSystem.onDeviceChange`. Cuando detecta un Gamepad conectado, llama `PlatformDetector.ForceSetMobileUIActive(false, false)` para ocultar controles on-screen. Cuando se desconecta, llama `PlatformDetector.ForceReconfigure()` para restaurar el estado correcto según la plataforma.

**⚠️ Acción requerida en Unity:** Agregar este componente al mismo GameObject de `AppBootstrap` en la escena `MainMenu`. El script `AppBootstrap` ahora tiene `[RequireComponent(typeof(GamepadDetector))]` que lo fuerza automáticamente.

---

### `Assets/Scripts/Mobile/OnScreenControlsSetup.cs` (NUEVO)
**Qué hace:** Script de documentación + validación. Se agrega al GameObject raíz de la UI mobile de gameplay. En el Inspector muestra las referencias a `OnScreenStick` y `OnScreenButton` y loguea warnings si no están asignados.

**Contiene las instrucciones exactas** de cómo configurar el joystick y botones on-screen en Unity Editor (ver sección de acciones requeridas abajo).

---

## Archivos modificados

### `Assets/Scripts/Mobile/MobileInputProvider.cs`
**Cambios:**
- Documentado correctamente como puente opcional.
- Agregados métodos `SetMoveInput(Vector2)` y `SetJumpPressed(bool)` para casos donde se usen botones on-screen custom (no `OnScreenStick`/`OnScreenButton`).
- El archivo ya no es un stub — ahora explica su rol en la arquitectura.

---

### `Assets/Scripts/Utils/InputManager.cs`
**Cambios:**
- Eliminada la referencia serialized a `MobileInputProvider` (nunca se usaba).
- `MoveInput` ahora retorna `Vector2.zero` de forma segura si `inputActions` es null.
- Nueva propiedad `NavigationInput` (antes era solo `GetNavigationInput()`).
- `GetNavigationInput()` se mantiene como alias para retrocompatibilidad.
- Documentación completa de todos los controles soportados.
- Lambdas de suscripción limpiadas (eliminados los `ctx` sin usar).

---

### `Assets/Scripts/Mobile/PlatformDetector.cs`
**Cambios:**
- Nuevo método `ForceSetMobileUIActive(bool gameplay, bool levelSelector)`:
  - A diferencia de `SetMobileUIActive`, **no** verifica `IsMobile()`.
  - Usado por `GamepadDetector` para ocultar controles en mobile cuando hay gamepad.

---

### `Assets/Scripts/Core/AppBootstrap.cs`
**Cambios:**
- Agregado `[RequireComponent(typeof(GamepadDetector))]`.
- Al agregar `AppBootstrap` en Unity Editor, `GamepadDetector` se agrega automáticamente al mismo GameObject.

---

## Acciones requeridas en el Editor de Unity

### Paso 1: GamepadDetector en AppBootstrap
- `AppBootstrap` ahora tiene `[RequireComponent(typeof(GamepadDetector))]`.
- Al abrir la escena `MainMenu`, Unity puede pedir agregar `GamepadDetector` al GameObject de `AppBootstrap` automáticamente. Aceptar.

### Paso 2: Configurar joystick on-screen
1. En la escena de **gameplay**, localizar el GameObject del joystick virtual de la UI mobile.
2. `Add Component → OnScreenStick` (está en `Unity.InputSystem`).
3. En el campo **Control Path**, escribir: `<Gamepad>/leftStick`
4. Ajustar **Movement Range** al radio visual del joystick (ej. 50).

### Paso 3: Configurar botón de salto on-screen
1. Localizar el GameObject del botón de salto de la UI mobile.
2. `Add Component → OnScreenButton`.
3. **Control Path**: `<Gamepad>/buttonSouth`

### Paso 4: (Opcional) Botón de pausa on-screen
1. Si hay un botón de pausa en la UI mobile:
2. `Add Component → OnScreenButton`.
3. **Control Path**: `<Gamepad>/start`

### Paso 5: Agregar OnScreenControlsSetup al GameObject raíz de la UI mobile
1. Seleccionar el GameObject padre de todos los controles on-screen.
2. `Add Component → OnScreenControlsSetup`.
3. Asignar en el Inspector las referencias a joystick y botón de salto.
4. Al ejecutar en el Editor, si algo no está asignado aparecerá un warning claro.

---

## Bindings de Gamepad confirmados (ya existían en .inputactions)

| Acción | Binding |
|--------|---------|
| Move | `<Gamepad>/leftStick` |
| Jump | `<Gamepad>/buttonSouth` (A/Cross) |
| Pause (Player map) | `<Gamepad>/start` |
| Pause (UI map) | — (heredado del Player map) |
| Navigate (UI) | `<Gamepad>/leftStick` + `<Gamepad>/dpad` |
| Submit (UI) | `<Gamepad>/buttonSouth` |
| Cancel (UI) | `*/{Cancel}` (automático por plataforma) |

---

## Cómo probar

### Gamepad físico (GameSir X5s, Xbox, PS):
1. Conectar el gamepad por USB-C o Bluetooth **antes** de iniciar el juego.
2. En la consola de Unity: `[GamepadDetector] Gamepad conectado: <nombre>`.
3. Los controles on-screen deben ocultarse automáticamente.
4. Movimiento, salto y pausa deben funcionar con el mando.

### Gamepad conectado en caliente (runtime):
1. Iniciar el juego sin gamepad conectado → controles on-screen visibles (si es mobile).
2. Conectar el gamepad → controles on-screen se ocultan automáticamente.
3. Desconectar el gamepad → controles on-screen vuelven (si es mobile).

### On-screen controls (mobile):
1. Build en Android/iOS o usar el simulador de Unity.
2. El joystick debe mover al personaje.
3. El botón de salto debe activar el salto (simple y doble).

---

## Cómo revertir

```bash
git diff HEAD --name-only   # ver archivos cambiados
git checkout HEAD -- Assets/Scripts/Utils/InputManager.cs
git checkout HEAD -- Assets/Scripts/Mobile/PlatformDetector.cs
git checkout HEAD -- Assets/Scripts/Core/AppBootstrap.cs
# GamepadDetector y OnScreenControlsSetup son nuevos, eliminar si se revierte:
# rm Assets/Scripts/Mobile/GamepadDetector.cs
# rm Assets/Scripts/Mobile/OnScreenControlsSetup.cs
```
