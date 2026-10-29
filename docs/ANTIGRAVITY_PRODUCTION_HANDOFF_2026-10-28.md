# Fox’s Tale — handoff para cerrar monetización antes de publicar

**Fecha:** 2026-10-28 (Lima)  
**Proyecto:** `/Users/aldhairvera/server/projects/UnityProjects/FatcatGameStudio/Fox_s Tale`  
**Rama actual:** `refactor/trying-refactor`, con cambios locales del propietario/agentes: preservarlos. No crear copias del proyecto ni sobrescribirlos.  
**Estado:** `NO_GO_PRODUCTION`. El objetivo es Android **y** iOS monetizados y probados. Detener builds y subidas nuevos hasta completar P0 de Unity y configuración de tienda. El archive iOS build 2 es solo diagnóstico y no contiene el futuro trabajo de UI/consentimiento.

## Actualización verificada en consolas — 28 de octubre de 2026 (Lima)

- **Google Play:** ya se creó y **activó** el producto único `com.fatcatgamestudio.foxstale.noads`, opción de compra `noads`, compra permanente, compatible con la API antigua, disponible en 174 países/regiones. Precio base EE. UU. **US$2.99** y precio mostrado para Perú **S/9.99**. La consola en español interpretó inicialmente `2.99` como `299`; se corrigió a `2,99` y se verificó la fila `2,99 USD` **antes** de activar. El producto activo está en `https://play.google.com/console/u/0/developers/7720975764288323280/app/4976400942511877165/one-time-products/sku/com.fatcatgamestudio.foxstale.noads`. Ya no es una tarea pendiente crear este SKU. No se verificaron aún compra real ni disponibilidad desde cliente.
- **RevenueCat:** proyecto `0b0ff296`, app Play `app53b6dcbbc8`, app Apple `appd0ba507214`. Ambos productos con el SKU exacto están en el catálogo y cada uno muestra `1 Entitlement`. **Verificado en la ficha `default`**: el único paquete `$rc_lifetime` enlaza el producto Play y el producto Apple correctos. El producto Play muestra `Could not check`: faltan credenciales de cuenta de servicio en la configuración de la app Play. El producto Apple muestra `Missing Metadata`. Se solicitó autorización específica para cargar `fatcat-google-play.json` a este proyecto; **no cargar una clave privada sin esa autorización**.
- **App Store Connect:** IAP Apple `6827746088` sigue en `Prepare for Submission`, sin captura de revisión. La política pública `https://burningal15.github.io/Platformer2d/` se guardó como Privacy Policy URL. Se publicaron el 2026-10-29 cinco categorías de App Privacy: Device ID (publicidad de terceros, vinculado, tracking), Purchase History (analytics y funcionalidad, no vinculado, sin tracking), Product Interaction, Advertising Data y Performance Data (publicidad de terceros, vinculados, sin tracking). App Store Connect mostró «Published a few seconds ago by Aldhair Vera Camacho». Se retiró Coarse Location: [Unity LevelPlay](https://docs.unity.com/en-us/grow/levelplay/platform/legal-resources/apple-privacy-questionnaire) declara que no la recoge y el export iOS actual solo contiene el pod `IronSourceSDK`, sin adaptador Unity Ads observado. [RevenueCat](https://www.revenuecat.com/docs/platform-resources/apple-platform-resources/apple-app-privacy) respalda Purchase History. Si Antigravity añade redes de mediación u otros SDK al build final, volver a auditar las categorías antes de enviar.
- **Código:** comprobado otra vez que `NoAdsPurchaseButton` no está cableado en escena/prefab y que `SetPrivacyEligibilityResolved` no tiene caller; siguen siendo P0. El build 2 no sirve para validar esas funciones. No se han subido builds nuevos en esta actualización.
- **Play Publishing Overview:** la app aparece como `Borrador de aplicación`, producción `Inactivo`. `Enviar aplicación a revisión` está deshabilitado y la página indica completar pasos del panel. Hay **cambios sin enviar**: prueba cerrada Alpha (revisar/confirmar versión y enviarla), ficha en inglés, clasificación de contenido, audiencia 18+, URL de privacidad, declaración de anuncios, Data Safety, declaración de apps de salud, categoría Arcade y Play Games on PC. El histórico de `docs/MONETIZATION_RELEASE.md` dice que Data Safety se guardó y el panel la marcó completa; por tanto su presencia en el resumen prueba que **no se envió a revisión**, no que el cuestionario esté necesariamente vacío. Verificar la declaración de apps de salud y el estado actual de cada tarea en el panel antes de afirmar que faltan respuestas. No hay aprobación/release de producción. Estas tareas no se resuelven por activar el IAP.
- **Videos reales inspeccionados tras corrección del propietario:** `Recordings/movie_009.mp4` (4,87 s), `movie_010.mp4` (23,17 s) y `movie_011.mp4` (13,83 s), todos 2560×1440. Se inspeccionaron fotogramas cada 2 s el 2026-10-28: muestran únicamente gameplay del zorro, plataformas, enemigos y gemas. Se extrajeron **sin edición** `store-assets/source-video-frames/foxstale-gameplay-from-movie_010-7s.png` y `foxstale-gameplay-from-movie_010-17s.png` como fuentes reales para la ficha. **Ningún video muestra el panel Remove Ads, botón Comprar/Restaurar ni precio**; no usar un fotograma de ellos como captura de revisión del IAP. Al terminar el prefab de tienda, grabar/capturar esa pantalla real desde el juego para el IAP Apple.

## 1. Verdad actual que debe respetarse

### Verificado en el checkout el 28 de octubre

- Unity `6000.6.0f1`; bundle/package `com.fatcatgamestudio.foxstale`; Android mínimo API 26, target API 36 (plataforma estable reconocida por el SDK local; target 37 falló por paquete SDK mal registrado); iOS mínimo 15.0; versión fuente `1.0.1`.
- `Assets/Scenes/MainMenu.unity` contiene `AppBootstrap`, `RevenueCatManager` y `AdsManager`. `AppBootstrap` exige esos managers. `Assets/Scripts/IAP/RevenueCatManager.cs` implementa obtención del precio, compra, restauración y entitlement `remove_ads` para `com.fatcatgamestudio.foxstale.noads`.
- **P0 UI:** `Assets/Scripts/IAP/NoAdsPurchaseButton.cs` existe, pero no está agregado a `MainMenu.unity` ni al prefab `Assets/Prefabs/ReUsable/UI-Canvas.prefab`; no hay entrada visible de compra/restauración. No solucionar esto creando un árbol de UI con `new GameObject` en runtime. Autorarlo en Edit Mode como prefab y conectar sus referencias reales.
- **P0 anuncios:** `AdsManager.SetPrivacyEligibilityResolved(bool)` no tiene caller en `Assets`; hasta resolver elegibilidad/consentimiento el SDK permanece cerrado. No llamar `SetPrivacyEligibilityResolved(true)` de forma incondicional. El juego declara audiencia de 18+ en su política; revisar la declaración real de cada tienda antes de cambiarla.
- El icono iOS `Assets/FoxsTaleAppIcon_1024.png` se añadió a los slots iPhone/iPad de `ProjectSettings.asset`; revisar visualmente el icono generado antes de producción.
- El pipeline `scripts/release-test.sh` exportó iOS build 2. `fastlane/Fastfile` instaló Pods y Xcode terminó `Archive Succeeded` con dSYM. **No hay IPA ni TestFlight:** la exportación anterior falló por falta de perfil App Store. El 2026-10-28, con autorización explícita del propietario, se generó e instaló `Foxs Tale App Store Distribution 2026` (UUID `6bb363be-859b-4fd9-92c2-d54d0a60f8e3`, Apple ID `RR97C6L96R`) para `4UBGTX27U5.com.fatcatgamestudio.foxstale`. Vence 2027-04-14 Lima. Su certificado SHA-1 `FEA19474F498843D23BF5B3333DEF027A3057E97` coincide con la identidad Apple Distribution local. Archivo instalado en `~/Library/Developer/Xcode/UserData/Provisioning Profiles/6bb363be-859b-4fd9-92c2-d54d0a60f8e3.mobileprovision`. `fastlane/ExportOptions-FoxsTale.plist` ya usa ese nombre. **Aún no se repitió exportación ni se subió build**; hacerlo tras cerrar P0 de monetización. No rotar certificados.
- `scripts/release-test.sh` admite `--skip-build` para reutilizar export iOS, pero **no usar el export build 2 como release de producción**: precede las tareas P0 de monetización.

### Verificado previamente en consolas, según `docs/MONETIZATION_RELEASE.md`

- Apple app ID `6827745041`. IAP no consumible ID `6827746088`, SKU `com.fatcatgamestudio.foxstale.noads`, base EE. UU. US$2.99, estado `Prepare for Submission`; falta captura de revisión y envío con versión de app. Reconfirmar en consola antes de enviar.
- RevenueCat proyecto `0b0ff296`: apps Play/Apple, entitlement `remove_ads`, offering `default`, paquete `$rc_lifetime`; claves Apple de compras/ASC figuraban válidas. La app Play aún carecía de credenciales de servicio y producto validado. Las public SDK keys locales están en un asset ignorado por Git; no copiar valores al handoff.
- LevelPlay: Android app key `290dc33f5`, rewarded `d9opyhliijhr3fb4`, interstitial `7xsq048b2uuvo8vj`; iOS app key `290e4f7ad`, rewarded `4sm02ryaqjxmk9pd`, interstitial `6wzvpnyl3oqj65qm`. Revisar las unidades/estado en dashboard antes de usar en producción; la existencia de IDs no prueba fill.
- Play Internal recibió AAB `v1`; Play Publisher API para esta app devolvió 403 a la cuenta de servicio, aunque la consola del propietario permitió la carga. La ficha Play tenía 10/11 tareas completas; quedaba la ficha de tienda y cambios pendientes de revisión. Reconfirmar estado actual.

## 2. Trabajo de Antigravity — terminar antes de pedir un nuevo build

### P0-A. Store UI de Remove Ads (Unity Edit Mode)

1. Abrir el proyecto canónico y revisar `MainMenu.unity`, `UI-Canvas.prefab`, `NoAdsPurchaseButton.cs` y `RevenueCatManager.cs`. Elegir una entrada visible y coherente en menú o ajustes; crear un **prefab** de panel de tienda, no UI flotante generada por código.
2. Mostrar producto Remove Ads como compra permanente, precio **localizado que devuelve RevenueCat**, botón Comprar, botón Restaurar compras, estado activo, loading y feedback de error/cancelación. No fijar `$2.99` en el botón para todos los territorios. Si el producto no carga, desactivar compra y mostrar por qué.
3. Cablear `NoAdsPurchaseButton` a los `Button`, `TextMeshProUGUI`, indicador y overlay serializados; entrada para abrir y cerrar el panel. Verificar que la compra desactiva intersticiales inmediatamente y que Restore permanece disponible. No conceder No Ads por un simple reset local ni por callback de error.
4. Guardar escena y prefab **fuera de Play Mode**. Capturar la pantalla real de compra en iPhone para la ficha IAP Apple; no usar mockup.
5. Criterio de cierre: se abre desde el menú, muestra producto/moneda correctos, compra sandbox y restore reflejan entitlement tras relanzar. Registrar capturas y versión instalada.

### P0-B. Ads, privacidad y experiencia (Unity + dashboards)

1. Revisar `Assets/Scripts/Managers/AdsManager.cs`, los callers de `ShowRewarded` y `ShowInterstitial`, y los IDs serializados en `MainMenu.unity`. Confirmar que rewarded solo premia tras callback de éxito una vez, evita doble toque y deja juego utilizable en no-fill. Confirmar que No Ads suprime los formatos prometidos; el código actual también bloquea rewarded según GDD.
2. Implementar la elegibilidad/consentimiento real para audiencia y región. Usar un flujo CMP/SDK compatible con las reglas actuales de LevelPlay, Apple y Play; vincular el resultado a `SetPrivacyEligibilityResolved` y conservar fail-closed si no hay resolución. No inventar consentimiento ni forzar `true` por el hecho de que la app sea 18+.
3. Confirmar declaraciones de ads, privacidad y permisos en tiendas frente al build real y la política pública `https://burningal15.github.io/Platformer2d/`. Incluir la ruta de reporte de anuncios inapropiados requerida para iOS si aplica al formato mostrado; documentar la conclusión con fuente oficial y fecha.
4. Criterio de cierre: interstitial en pausa natural, rewarded voluntario con un solo premio, consentimiento denegado sin ads, comprado No Ads sin ads, y resultados observados en Android/iPhone. Los logs de inicialización sin impresión no bastan.

### P0-C. Preparar build candidato **después** de A y B

1. Cerrar Unity GUI tras guardar. Revisar `git diff` y version numbers superiores a los más altos recibidos por ambas tiendas; no reutilizar build 2 como candidato final.
2. Ejecutar `scripts/release-test.sh` para exportar AAB/IPA usando números confirmados y secretos **locales**, nunca en chat/Git. Validar icono 1024, bundle, API 36, permisos Billing/AD_ID según declaración, firma y contenido de app. El script solo apunta a Play Internal/TestFlight.
3. Antes de subir, comprobar en la exportación que Xcode utiliza el perfil App Store ya instalado para este bundle y el certificado local coincidente; `ExportOptions-FoxsTale.plist` contiene su nombre exacto.
4. No enviar a producción/App Review todavía. Entregar artefactos, logs, números, SHA público de firma y estado de subida sin exponer secretos.

## 3. Trabajo de consolas coordinado con Codex/propietario

1. **Google Play:** producto único `com.fatcatgamestudio.foxstale.noads` **ya activo** a US$2.99 base. Confirmar el estado de la ficha y de la declaración AD_ID/Data Safety contra el build final. Dar acceso de publicación interna a la cuenta de servicio si se quiere Fastlane; si no, cargar por consola. No rotar keystore.
2. **RevenueCat:** los productos Play y Apple **ya están** enlazados al entitlement `remove_ads` y a `default/$rc_lifetime`; añadir credenciales Play de la app solo con autorización específica para compartir el JSON privado y verificar importación/estado `Could not check`. `Missing Metadata` Apple puede persistir hasta completar Apple. No usar Test Store como sustituto.
3. **Apple:** el perfil App Store para `com.fatcatgamestudio.foxstale` ya está generado/instalado y App Privacy tiene URL y respuestas publicadas. Completar IAP con captura real; revisar precios, disponibilidad y versión. TestFlight primero, App Review cuando QA esté cerrado.

## 4. QA de salida y entrega esperada

| Área | Prueba mínima en **ambas** plataformas |
| --- | --- |
| Store | Precio localizado, compra sandbox real, cancelación, error, restore, persistencia tras reinicio y modo avión. |
| Ads | Consentimiento aceptado/denegado, fill y no-fill, rewarded una vez, interstitial en pausa, Remove Ads suprime anuncios sin afectar jugabilidad. |
| Juego | Splash, menú, niveles, Game Over, controles táctiles/gamepad, progreso guardado, icono correcto. |
| Tiendas | Número/build instalable desde Play Internal/TestFlight; IAP disponible para sandbox; ausencia de rechazos/advertencias bloqueantes. |

Antigravity debe añadir al final de este documento un veredicto fechado `GO_TEST_BUILD` o `NO_GO`, con commit/snapshot, archivos modificados, compilación, capturas y pendientes exactos. `GO_TEST_BUILD` autoriza **solo** distribuir para QA; `GO_PRODUCTION` requiere evidencias en dispositivos y consolas. Ante cualquier conflicto con `docs/MONETIZATION_RELEASE.md` o `.kiro/handoff.md`, registrar la evidencia nueva y fecha, sin borrar el histórico.
