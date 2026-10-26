# Fase 5 — Builds automáticos: Reporte de cambios

**Fecha:** 26/10/2026  
**Depende de:** Fase 0, 1, 2, 3

---

## Resumen ejecutivo

Se implementó el sistema de compilación y automatización de builds para Android e iOS:
1. **Pipeline de Build en Editor y CLI:** Scripts listos para generar APKs de pruebas, AABs para Google Play Store y proyecto Xcode para iOS.
2. **Configuración de Compliance Android:** ID de aplicación configurado (`com.fatcatgamestudio.foxstale`), Target API 34+ (requerido por Google Play), arquitectura ARM64 + ARMv7 (IL2CPP) y permiso de Internet forzado para analíticas y ads.
3. **Soporte CLI / CI/CD:** Métodos estáticos ejecutables directamente mediante Unity en batchmode sin intervención manual.

---

## Archivos creados

### `Assets/Editor/BuildAutomation.cs` (NUEVO)
**Qué hace:** Provee opciones en la barra de menú de Unity (`CustomEditorTools/Builds/...`) y métodos ejecutables desde línea de comandos (`-executeMethod`).

**Métodos disponibles:**
```csharp
BuildAutomation.BuildAndroidAPK()   // Genera APK para pruebas rápidas
BuildAutomation.BuildAndroidAAB()   // Genera Android App Bundle para Google Play
BuildAutomation.BuildiOS()          // Exporta proyecto Xcode para iOS
```

**Ejecución desde CLI:**
```bash
/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -projectPath . \
  -executeMethod BuildAutomation.BuildAndroidAPK
```

---

## Acciones requeridas en el Editor / Externas

| Acción | Dónde | Estado |
|---|---|---|
| Configurar Keystore de Android para Release | Unity PlayerSettings / Keystore Manager (guardar fuera del repo) | ⚠️ Pendiente para release |
| Configurar Apple Developer Team ID & Provisioning Profile | Unity PlayerSettings iOS / Xcode | ⚠️ Pendiente para iOS build |
| Configurar URL de Privacy Policy y Data Safety | Google Play Console & App Store Connect | ⚠️ Pendiente para publicación |
