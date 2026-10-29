# Fox's Tale: monetización y builds de prueba

Actualizado: 2026-10-28. Esta página registra identificadores y estado; no guarda claves privadas ni contraseñas. Verificación posterior en Play Console: el canal de prueba interna está activo con la versión 1 (1.0.1), disponible para testers desde el 28-oct a las 03:44; la consola aún muestra 0,00 % de instalaciones. Producción sigue inactiva. Se exportó Xcode desde Unity el 27-oct y se compiló localmente una app iOS sin firma el 28-oct.

## Estado operativo verificado — 2026-10-28

- La versión 1 (1.0.1, código 1) aparece activa y disponible para testers internos en Play Console; el paquete es `com.fatcatgamestudio.foxstale`. El canal incluye las listas `ExternalTesters` y `GlowWords Internal`, con 1 usuario cada una, y el enlace de participación web del canal está activo. No se verificaron direcciones de correo de testers. El JSON local de cuenta de servicio existe y tiene formato válido, pero la API de Play responde HTTP 403 `The caller does not have permission` para Fox's Tale. Fastlane no puede subir hasta que esa cuenta reciba acceso a esta app; el AAB se subió desde la sesión del propietario en Play Console.
- El certificado de subida Android creado es exclusivo de Fox's Tale. Keystore local: `~/.fatcat-build/keystores/FoxsTale-upload-2026.p12`, alias `foxstaleupload`, SHA-256 público `991804251f6f96f5ab4412ff5e9f124611c556893a663195fe0f7b51927e8708`. Contraseñas solo en `~/.fatcat-build/secrets.env` (modo 600); no copiar al repositorio ni compartir. La cuenta Play aún debe aceptar/registrar este certificado en el primer AAB y habilitar Play App Signing cuando el flujo de Play lo solicite.
- El `.p8` local `~/.appstoreconnect/private_keys/AuthKey_R6DA76UX3G.p8` está presente y la API R6DA valida lectura de la app `6827745041` y del bundle `com.fatcatgamestudio.foxstale`. App Store Connect informa cero builds. Esta clave es distinta del ID `6TGLSXPKYY` que RevenueCat tiene configurado; no se debe declarar que son el mismo archivo. Hay un certificado iOS de distribución en el equipo que vence el 2027-02-22, pero el llavero local no tiene una identidad de firma válida; falta completar firma/provisioning.
- Las claves públicas del SDK de RevenueCat están presentes en el JSON local ignorado por Git. No se exponen aquí.
- `activeInputHandler` está guardado como `1` (Input System Package) en `ProjectSettings/ProjectSettings.asset`. Unity requiere reiniciar el Editor para recargar ese backend; el Editor GUI quedó abierto y no respondió a la automatización en esta revisión. La compilación iniciada desde menú falló durante recompilación de scripts; no hay archivo en `Builds/Android/`.
- Unity 6000.6 requiere Android API mínima 26; `BuildAutomation` y `ProjectSettings` apuntan a API 26 mínima y Target API 36 para coincidir con la plataforma instalada en el SDK. El AAB v1 incluye `com.android.vending.BILLING`, pero se creó antes de añadir el permiso AD_ID descrito abajo y habrá que generar otra versión antes del rollout.
- `NoAdsPurchaseButton.cs` implementa compra/restauración, pero no está añadido/cableado en una pantalla o prefab. No se validó compra ni restauración en dispositivo.
- `AdsManager.SetPrivacyEligibilityResolved` sigue sin caller confirmado. LevelPlay continúa fail-closed: no se inicializan anuncios ni se simula consentimiento.
- El export de Unity tenía un `Podfile` vacío y el build de Xcode fallaba porque faltaban los headers de LevelPlay. Se reparó el export local con `IronSourceSDK` CocoaPod 9.5.0.0 y se alineó el target mínimo de los pods a iOS 15, que es el mínimo del proyecto. `xcodebuild` terminó correctamente y produjo `/private/tmp/foxstale-derived/Build/Products/Release-iphoneos/FoxsTale.app` (126 MB), bundle correcto y MinimumOSVersion 15.0. Está sin firmar (CODE_SIGNING_ALLOWED=NO); no es una IPA y no se instala en dispositivo ni se subió a TestFlight.
- La lane de TestFlight en `fastlane/Fastfile` ahora añade el pod LevelPlay y ejecuta `pod install` sobre cada export nuevo antes de compilar; eleva targets de pods antiguos a iOS 15. La sintaxis Ruby quedó validada. La ejecución completa de esa lane aún espera una identidad/provisioning de distribución.

### Declaraciones de Google Play — 2026-10-28

- **Inicio de sesión:** declarado que la app no requiere cuentas ni credenciales. **Anuncios:** declarado que contiene anuncios por su integración LevelPlay. **Audiencia:** 18+ solamente, siguiendo la preferencia condicional del propietario; la encuesta IARC independiente dio PEGI 7, ESRB 10+, Brasil 14+ y clasificaciones regionales indicadas en Play Console.
- **Clasificación de contenido:** cuestionario IARC completado y guardado con `aldhairvera.gamedev@gmail.com`, con autorización expresa del propietario tanto para compartir ese correo con IARC/autoridades como para aceptar sus Términos de Uso.
- **ID de publicidad:** declarado que sí se usa, para publicidad/marketing, porque LevelPlay admite y utiliza identificadores publicitarios. Para alinear el siguiente AAB con la declaración, se activó el manifiesto Android personalizado y se añadió `com.google.android.gms.permission.AD_ID` en `Assets/Plugins/Android/AndroidManifest.xml`. El AAB v1 ya cargado no contiene este permiso.
- **Seguridad de los datos:** declaración completa guardada en Play Console el 2026-10-28; el panel muestra la tarea completada. Incluye historial de compras (RevenueCat), ubicación aproximada, otras acciones/interacciones, diagnósticos e IDs de dispositivo (LevelPlay); recoge y comparte ubicación aproximada, otras acciones e IDs según los propósitos declarados; declara cifrado en tránsito y correo/enlace para solicitar eliminación.
- **Política de privacidad:** publicada y verificada en GitHub Pages, en español e inglés: https://burningal15.github.io/Platformer2d/. En Play Console se guardó esa URL el 2026-10-28; el panel muestra la tarea completada. La política detalla RevenueCat, LevelPlay, almacenamiento local y solicitudes de eliminación.
- **Contacto público de soporte:** `aldhairvera.gamedev@gmail.com`, autorizado por el propietario y publicado en Play Console; también recibe solicitudes de privacidad/eliminación. El panel muestra completada la tarea de categoría/contacto; categoría configurada como Juego > Arcade.
- **Estado de configuración inicial en Play Console:** 10 de 11 tareas completas. La única pendiente es la ficha de Play Store; el propietario estaba completándola. Los cambios de privacidad/categoría guardados quedan en Resumen de publicación hasta que se envíen a revisión.
- **Estado de ads:** aunque Play declara que el juego contiene anuncios, el SDK permanece fail-closed porque `AdsManager.SetPrivacyEligibilityResolved()` no tiene caller y falta el flujo de consentimiento/elegibilidad. La inclusión de LevelPlay y la declaración de ads no significan que haya anuncios activos en la build actual.

El script `scripts/release-test.sh` ahora detecta el `Library/EditorInstance.json` activo y aborta con una indicación clara si el Editor ya tiene abierto este proyecto; Unity no admite un segundo proceso batch sobre el mismo checkout.

## Identidad y productos

| Servicio | Configuración verificada | Estado |
| --- | --- | --- |
| Bundle/package | `com.fatcatgamestudio.foxstale` | Apple Developer, App Store Connect, Play Console y Unity usan esta identidad. |
| Apple Remove Ads | `com.fatcatgamestudio.foxstale.noads`, no consumible, US$2.99 en EE. UU. | Prepare for Submission; falta captura de revisión y una versión nueva de la app. |
| Google Remove Ads | Mismo SKU, compra única/no consumible prevista | Play exige primero un APK/AAB con permiso `BILLING`; aún no existe el producto real. |
| RevenueCat | [Proyecto Fox's Tale](https://app.revenuecat.com/projects/0b0ff296/overview); entitlement `remove_ads`; offering `default`, paquete `$rc_lifetime` | Apps Android e iOS creadas, ambos productos asociados al entitlement y al mismo paquete. Android no se puede validar contra Play hasta crear producto y conectar credenciales. Apple figura `Missing Metadata` en RevenueCat. |
| LevelPlay Android | App key `290dc33f5`; rewarded `d9opyhliijhr3fb4`; interstitial `7xsq048b2uuvo8vj` | Registrados para Fox's Tale. |
| LevelPlay iOS | App key `290e4f7ad`; rewarded `4sm02ryaqjxmk9pd`; interstitial `6wzvpnyl3oqj65qm` | Registrados para Fox's Tale. |

## Claves Apple reutilizadas en RevenueCat

El propietario autorizó el 2026-10-27 reutilizar las claves disponibles para integrar Fox's Tale. RevenueCat muestra ambas como **Valid credentials** en [Fox's Tale (App Store)](https://app.revenuecat.com/projects/0b0ff296/apps/appd0ba507214):

| Uso | Key ID | Alcance en Fox's Tale |
| --- | --- | --- |
| In-app purchase key | `Z5V4FL5RJD` | Validación de transacciones StoreKit 2. |
| App Store Connect API key | `6TGLSXPKYY` | Consulta/importación de productos y precios. |

Ambas claves ya se usan en otras apps del mismo estudio. El ID de la clave permite identificarla; el archivo privado `.p8` y su contenido **no** van en Git, documentación, logs ni capturas. Si se rota alguna clave, actualizar la configuración en RevenueCat y la variable de Fastlane que corresponda. La clave de compras en RevenueCat y la API de Fastlane cumplen funciones distintas; Fastlane necesita un archivo `.p8` local al subir a TestFlight. El ID `6TGLSXPKYY` está configurado en RevenueCat, pero el archivo privado correspondiente no está presente en la ruta local comprobada y debe localizarse o generarse una clave de distribución propia.

La app Android de RevenueCat es [Fox's Tale (Play Store)](https://app.revenuecat.com/projects/0b0ff296/apps/app53b6dcbbc8). Sus credenciales de servicio de Google Play aún faltan. Las claves SDK públicas de Fox's Tale se guardan localmente en `Assets/Resources/RevenueCatPublicKeys.local.json`, archivo ignorado por Git; `RevenueCatManager` las lee al iniciar y el build las incluye dentro de la app como configuración pública del SDK. No copiar la clave Test Store ni claves de otra app a ese archivo.

## Comando de build y subida

`scripts/release-test.sh` llama Unity 6000.6.0f1 en batch mode. `fastlane/Fastfile` solo tiene dos lanes de subida: `android internal` y `ios upload_testflight`. Ninguna lane envía a producción o App Review.

```sh
# Solo artefactos locales:
scripts/release-test.sh android --android-code 2
scripts/release-test.sh ios --ios-build 2
scripts/release-test.sh both --android-code 2 --ios-build 2

# Build y subida a canales de prueba:
scripts/release-test.sh both --android-code 2 --ios-build 2 --upload-test
```

Los números de ejemplo deben sustituirse por números **mayores que el último recibido por cada tienda**. El script exige números explícitos y rechaza sobrescribir artefactos con el mismo número. Android produce `Builds/CI/Android/FoxsTale_<code>.aab`; iOS exporta `Builds/CI/iOS/XcodeProject_<build>` y Fastlane genera la IPA. Los artefactos y logs están ignorados por Git.

Variables requeridas para Android: `FOXSTALE_KEYSTORE_PATH`, `FOXSTALE_KEYSTORE_PASSWORD`, `FOXSTALE_KEYALIAS_NAME`, `FOXSTALE_KEYALIAS_PASSWORD`. La ruta del keystore debe ser absoluta y apuntar a un archivo existente. Para subir: `PLAY_STORE_JSON_KEY_PATH`, que debe apuntar al JSON de una cuenta de servicio autorizada en Play Console. No reutilizar un keystore de otra app sin verificar su certificado de subida con la ficha de Google Play de Fox's Tale.

Variables requeridas para subir iOS: `ASC_KEY_ID`, `ASC_ISSUER_ID`, `ASC_KEY_FILEPATH`, `IOS_DEVELOPMENT_TEAM`. `ASC_KEY_FILEPATH` es la ruta absoluta al archivo privado de la API key de App Store Connect autorizada para esta app. Xcode también necesita certificados y aprovisionamiento de distribución válidos para el equipo. El script no los crea ni almacena.

**Estado tras revisión 2026-10-27:** UPM resolvió LevelPlay 9.5.1, RevenueCat 9.15.0, EDM 2.1.1 e Input System 1.20.1; se exportó un proyecto Xcode local con Pods mediante Unity 6000.6.0f1. Se añadió el define de LevelPlay para Android/iOS y se arregló un error de compilación en `FadeEffect.cs`. BuildAutomation quedó ajustado a Android API 36 para usar el SDK estable instalado y reconocido por Gradle; API 37 falló porque Unity contiene el paquete `android-37.0` mal registrado y Gradle no encuentra `android-37`. No es todavía un build firmado ni subido.

**Antes de una prueba real:** añadir/cablear los managers y el botón de compra/restaurar en escenas/prefabs existentes; implementar y verificar consentimiento/elegibilidad, manteniendo ads cerrados hasta entonces; obtener keystore Android y valores privados locales, cuenta de servicio Play, archivo `.p8` y Team/certificados Xcode; confirmar números de versión en ambas consolas; generar y subir AAB/IPA solo a Internal/TestFlight; crear el producto Play tras subir AAB con Billing; enlazarlo en RevenueCat y probar compra/restauración/anuncios en dispositivos. No promover a producción ni enviar IAP a revisión.

El Target API 36 coincide con la plataforma instalada en el SDK y supera el mínimo vigente consultado de API 36 para apps nuevas/actualizaciones desde el 31-ago-2026. Fuente: [requisito oficial de Target API](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en-EN).

Referencias oficiales consultadas el 2026-10-27: [Fastlane supply](https://docs.fastlane.tools/actions/supply/), [Fastlane pilot](https://docs.fastlane.tools/actions/pilot/), [Fastlane build_app](https://docs.fastlane.tools/actions/build_app/), [API key de App Store Connect en Fastlane](https://docs.fastlane.tools/app-store-connect-api/).
# Actualización de consolas — 2026-10-28 (Lima)

El estado más reciente y los bloqueos de salida están en [ANTIGRAVITY_PRODUCTION_HANDOFF_2026-10-28.md](ANTIGRAVITY_PRODUCTION_HANDOFF_2026-10-28.md). Google Play **ya tiene activo** el producto único `com.fatcatgamestudio.foxstale.noads` a US$2.99 base (S/9.99 Perú); cualquier instrucción antigua de crearlo queda superada. RevenueCat `default/$rc_lifetime` ya enlaza los productos Play y Apple, pero la app Play aún carece de credenciales de servicio para validación y Apple IAP aún muestra `Missing Metadata`. App Privacy Apple tiene URL y cinco categorías publicadas el 2026-10-29. Play todavía muestra la app como borrador y enumera Data Safety y apps de salud entre los **cambios no enviados**; Data Safety ya se guardó según la verificación anterior y no debe confundirse con un cuestionario vacío. Store UI/consentimiento de Unity y QA en dispositivos siguen bloqueando un build de lanzamiento.

