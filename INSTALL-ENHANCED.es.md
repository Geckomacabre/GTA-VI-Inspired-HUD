# Cómo instalar: GTA V **Enhanced**

*Esta guía es una traducción. La versión en inglés, [INSTALL-ENHANCED.md](INSTALL-ENHANCED.md), es la más actualizada.*

Esta guía es para la **versión de PC de 2025** de GTA V. Mira qué archivo tiene tu carpeta del juego:

| Tu carpeta del juego tiene | Tienes | Usa |
| --- | --- | --- |
| **`GTA5_Enhanced.exe`** | Enhanced | **Esta guía** |
| **`GTA5.exe`** | Legacy | La **[guía de Legacy](INSTALL-LEGACY.es.md)** |

¿Nunca has instalado mods en GTA? No pasa nada. Sigue los pasos en orden. No te saltes ninguno. Te llevará unos 20 minutos.

> [!IMPORTANT]
> ### 💬 ¿Necesitas ayuda? Únete a nuestro Discord de soporte
> Esta versión es un **fork del VI HUD de FBICat** con funciones extra. Por eso tiene su propio soporte:
> **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**, o un [issue en GitHub](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).
> El Discord de FBICat es para su mod original. Por favor, trae tus preguntas sobre esta versión al nuestro. ¡Gracias!

> ⚠️ **Los mods son solo para Story Mode (Modo Historia).** Nunca entres en GTA Online con mods instalados. Te pueden banear.
> (Script Hook V, una de las herramientas de abajo, cierra el juego si lo intentas.)

> ⚠️ **Enhanced usa herramientas distintas a las de Legacy.** Usa solo las descargas enlazadas en esta guía.
> Las herramientas y los mods de Legacy hacen que Enhanced se cierre de golpe.

---

## Antes de empezar: dos ajustes rápidos de Windows

### 1. Mostrar las extensiones de archivo

Algunos archivos de esta guía terminan en `.asi` o `.dll`. Windows oculta esas terminaciones hasta que activas esto.

1. Abre cualquier carpeta.
2. Haz clic en **Ver (View)** arriba.
3. Haz clic en **Mostrar (Show)** → **Extensiones de nombre de archivo (File name extensions)**.

### 2. Saber descomprimir

La mayoría de las descargas son archivos `.zip` (una carpeta comprimida en un solo archivo). Para abrir uno:

1. Haz clic derecho sobre él.
2. Elige **Extraer todo… (Extract All…)**.
3. Haz clic en **Extraer (Extract)**.

✅ Aparece una carpeta normal a su lado.

---

## Paso 1: Encuentra tu carpeta de GTA V Enhanced

Es la carpeta que tiene **`GTA5_Enhanced.exe`** dentro. La llamaremos tu **carpeta del juego**.

Cómo abrirla:

| Tu launcher | Haz esto |
| --- | --- |
| **Rockstar Games Launcher** | Abre **Ajustes (Settings)**.<br>Haz clic en **Grand Theft Auto V Enhanced** a la izquierda.<br>Haz clic en **Abrir carpeta (Open Folder)** (o **Ver carpeta de instalación (View Installation Folder)**). |
| **Steam** | Abre tu **Biblioteca (Library)**.<br>Haz clic derecho en **Grand Theft Auto V Enhanced**.<br>Haz clic en **Administrar (Manage)** → **Ver archivos locales (Browse local files)**. |
| **Epic Games** | Abre tu **Biblioteca (Library)**.<br>Haz clic en los **⋯** debajo de GTA V Enhanced.<br>Haz clic en **Administrar (Manage)**.<br>Haz clic en el **icono de carpeta** junto a "Instalación" ("Installation"). |

✅ Deja esta carpeta abierta. Vas a copiar archivos en ella varias veces.

---

## Paso 2: Instala Script Hook V (permite que funcionen los mods)

1. Ve a la [página de Script Hook V](http://www.dev-c.com/gtav/scripthookv/).
2. Haz clic en **Download**. La misma descarga sirve para Legacy y Enhanced.
3. Descomprímelo.
4. Abre la carpeta **`bin`** que hay dentro.
5. Copia estos archivos en tu **carpeta del juego**:
   - **`ScriptHookV.dll`**
   - **`xinput1_4.dll`** (carga los mods que terminan en `.asi` en Enhanced)
   - **`args.txt`** (desactiva BattlEye, el antitrampas de Rockstar, que bloquea los mods)
   - **`dinput8.dll`** (el cargador de mods de Legacy. Enhanced no lo usa, así que copiarlo no hace ningún daño.)
6. ⚠️ **No** copies `NativeTrainer.asi`. Es un menú de trucos que no necesitas.

✅ Tu carpeta del juego ahora tiene `ScriptHookV.dll` y `xinput1_4.dll` junto a `GTA5_Enhanced.exe`.

---

## Paso 3: Instala Script Hook V .NET **Enhanced** (permite que funcione este HUD)

⚠️ Este **no** es el Script Hook V .NET normal. El normal hace que Enhanced se cierre solo,
unos 20 segundos después de cargar la partida. Necesitas la edición **Enhanced**, llamada **SHVDNE**.

1. Ve a la [página de releases de SHVDNE](https://github.com/Chiheb-Bacha/ScriptHookVDotNetEnhanced/releases/latest).
2. En **Assets**, descarga el archivo **`.zip`**.
3. Descomprímelo.
4. Copia los **5 archivos** de su carpeta principal en tu **carpeta del juego**:
   - **`ScriptHookVDotNet.asi`**
   - **`ScriptHookVDotNet.ini`**
   - **`ScriptHookVDotNet2.dll`**
   - **`ScriptHookVDotNet3.dll`**
   - **`MinHook.x64.dll`**
5. Sáltate las carpetas `Debug`, `Docs` y `Licenses` y el `README.txt`. No los necesitas.

¿Windows pregunta si quieres reemplazar archivos? Haz clic en **Reemplazar (Replace)**. (Significa que antes tenías la versión normal.)

✅ Esos 5 archivos ya están junto a `GTA5_Enhanced.exe`.

---

## Paso 4: Instala OpenRPF (mantiene el mod en una carpeta `mods` segura)

OpenRPF permite que el juego cargue mods desde una **carpeta `mods`** aparte. Los archivos originales de tu juego nunca se modifican.

1. Ve a la [página de OpenRPF](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced).
2. Descárgalo.
3. Descomprímelo.
4. Copia **`OpenRPF.asi`** en tu **carpeta del juego**.
5. ⚠️ ¿La descarga trae también un `dsound.dll`? **No lo copies.** Script Hook V ya te dio un cargador
   de mods: `xinput1_4.dll`, del Paso 2. Dos cargadores a la vez provocan cierres de golpe.
6. En tu carpeta del juego, crea una carpeta nueva llamada **`mods`**, si todavía no hay una.
   (Clic derecho → **Nuevo (New)** → **Carpeta (Folder)**.)

✅ `OpenRPF.asi` está junto a `GTA5_Enhanced.exe`, y hay una carpeta `mods`.

---

## Paso 5: Descarga el mod y el instalador

### Parte 1: Consigue el mod

1. Ve a la [**página de Releases**](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest).
2. En **Assets**, descarga **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`**. Es el mod, unos 100 MB.
   También activa el mod en el juego, así que no hace falta descargar ningún parche aparte.

❌ No descargues `GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`. Es para la otra versión y hará que Enhanced se cierre de golpe.

💡 El número del nombre del archivo es la versión (aquí, 2.7.0). Así siempre sabes cuál instalaste.
Menciónalo cuando pidas ayuda.

### ¿Quieres jugar como Lucia?

No viene incluida: su modelo es un trabajo de pago de otra persona. El HUD funciona sin ella.

Consíguela de su creador:
- **[Lucia Caminos en Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)**
- **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** en gta5-mods.com

O usa otro modelo de Lucia. Luego consulta "Opcional: Lucia" más abajo para añadirla al juego.

**Para Jason**, consigue uno de estos:
- **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** del mismo creador
- **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** en mod-hub.net

Luego consulta "Opcional: Jason" más abajo para añadirlo al juego.

### Parte 2: Consigue el instalador

OpenIV no instala mods en Enhanced. Así que usarás el **OIV Package Installer** en su lugar.

1. Ve a [su página de gta5-mods](https://www.gta5-mods.com/tools/oiv-package-installer).
2. Descárgalo.
3. Descomprímelo donde quieras, por ejemplo en tu Escritorio (Desktop).

---

## Paso 6: Instala el mod

Primero, mira la ruta de tu carpeta del juego. De ella depende cómo abres el instalador:

| La ruta de tu carpeta del juego empieza con | Cómo abrir el instalador |
| --- | --- |
| `C:\Program Files` (el Rockstar Games Launcher, Epic Games y Steam lo instalan ahí) | Haz clic derecho en **`CodeWalker.OIVInstaller.exe`** → **Ejecutar como administrador (Run as administrator)**.<br>Haz clic en **Sí (Yes)**. |
| Cualquier otra cosa | Ejecuta **`CodeWalker.OIVInstaller.exe`**. |

⚠️ **¿El juego está en Program Files?** Windows solo deja que los administradores cambien archivos ahí. Sin **Ejecutar como administrador**,
la instalación puede dejar los archivos antiguos sin reemplazar.

1. Abre la carpeta del OIV Package Installer.
2. Ejecuta **`CodeWalker.OIVInstaller.exe`** como dice la tabla.
3. Arrastra **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`** a su ventana. (O haz clic en **Browse** y elígelo.)
4. Te pide tu carpeta del juego. Elige la carpeta que tiene **`GTA5_Enhanced.exe`**.
5. Haz clic en **Install**.
6. Espera hasta que termine.

✅ Instalar el mod otra vez, o encima de una versión anterior, no da problemas. Nunca pone un pack dos veces en la lista.

---

## Paso 7: Cambia un ajuste del juego

1. Inicia el juego.
2. Ve a **Ajustes (Settings)** → **Gráficos (Graphics)**.
3. Pon **Tipo de pantalla (Screen Type)** en **Ventana sin bordes (Windowed Borderless)**.

💡 En Enhanced, el minimapa se dibuja en una pequeña ventana encima del juego. Esa ventana solo se ve en
el modo sin bordes (Borderless).

---

## Paso 8: ¡A jugar! 🎉

Carga **Story Mode**. El nuevo HUD aparece solo.

| Para... | Haz esto |
| --- | --- |
| **Cambiar de personaje** | Mantén **Left Alt** (mando: mantén **D-pad down**).<br>Elige con **Q / E** o **Left / Right** (mando: **LB / RB**).<br>Suelta.<br>Jason y Lucia necesitan sus modelos (no incluidos, consulta el Paso 5). |
| **Ver el minimapa** | Súbete a un coche.<br>Solo aparece en vehículos, o cuando te busca la policía. |
| **Abrir los ajustes del HUD** | Mantén **V**. |
| **Poner tu propia música** | Pon archivos `.mp3` en **`scripts\Hud\Radio\CustomMusic`** dentro de tu carpeta del juego.<br>En un coche, mantén el botón de la radio (**Q**).<br>Pulsa **Down** (mando: stick derecho hacia abajo) para **On Demand**.<br>Cada carpeta dentro de `CustomMusic` es una emisora propia.<br>**Left / Right** (stick derecho) cambian de emisora.<br>La **rueda del ratón** (D-pad Right / Up) cambia la canción. |

---

## Opcional: pantallas de carga de VI

**Qué obtienes:** las pantallas de carga de GTA VI de FBICat, con las imágenes de Vice City de Rockstar.
**Qué necesitas:** **`GTA-VI-Inspired-HUD-<version>-LoadingScreens-Enhanced.oiv`** de la misma versión que el HUD.
El HUD funciona sin él: instálalo solo si las quieres.

Arrástralo al **OIV Package Installer** (`CodeWalker.OIVInstaller.exe`), igual que con el HUD.

### ¿Ya las tienes y quieres recuperar las del juego?

Hasta la 2.6.4, las pantallas de carga venían dentro del paquete del HUD. Para quitarlas:

1. Abre **CodeWalker RPF Explorer** y haz clic en **Edit** → **Edit mode**.
2. Abre el **`update`** del propio juego → **`update.rpf`** → `x64` → `data` → `cdimages` (no el de `mods`).
3. Haz clic derecho en **`scaleform_frontend.rpf`** → **Extract** (en CodeWalker: **Extract Raw...**) a tu Escritorio.
4. Abre **`mods`** → `update` → **`update.rpf`** → `x64` → `data` → `cdimages`.
5. Arrastra el **`scaleform_frontend.rpf`** de tu Escritorio a esa ventana y reemplaza el que hay ahí.

**Con NaturalVision Enhanced**, borra también **`onigiri\platform\data\cdimages\scaleform_frontend.rpf`** si lo copiaste ahí.

---

## Opcional: Jason

**Qué obtienes:** Jason en la rueda de personajes, después de Trevor.
**Qué necesitas:** su modelo (no viene incluido) y el **kit de Jason** gratuito. Sigue las partes en orden.

### Parte 1: Consigue a Jason

1. Descarga a Jason de **uno** de estos:
   - **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** del creador de Lucia
   - **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** en mod-hub.net
2. Descomprímelo.
3. Abre la carpeta **`No Cap Version`** (o **`Cap Version`** si lo quieres con gorra).
   ¿En su lugar hay una carpeta **`legacy`** y otra **`enhanced`** (como en Jason Duval Extended Look)? Abre **`enhanced`**.
4. Dentro hay **4 archivos**. Sus nombres terminan en **`.ydd`**, **`.yft`**, **`.ymt`** y **`.ytd`**.

> ¿No ves las terminaciones (`.ydd`, etc.)? En el Explorador de archivos (File Explorer), haz clic en **Ver** → **Mostrar** → **Extensiones de nombre de archivo**.

### Parte 2: Revisa los nombres de los 4 archivos

Fíjate en la parte de cada nombre que va **antes del punto**:

| Tus 4 archivos se llaman | Qué hacer |
| --- | --- |
| `jdfinal.ydd`, `jdfinal.yft`, `jdfinal.ymt`, `jdfinal.ytd` | **Nada.** Ve a la Parte 3. |
| Cualquier otra cosa, por ejemplo `Jason.ydd`, `Jason.yft`, `Jason.ymt`, `Jason.ytd` | **Renombra cada archivo a `jdfinal`**, manteniendo la terminación: `Jason.ydd` pasa a ser **`jdfinal.ydd`**, `Jason.yft` pasa a ser **`jdfinal.yft`**, `Jason.ymt` pasa a ser **`jdfinal.ymt`**, `Jason.ytd` pasa a ser **`jdfinal.ytd`**. No cambies nada más. |

Y listo: el HUD busca **`jdfinal`**, así que en cuanto tus archivos se llamen así, lo encuentra. No hace falta
cambiar ningún ajuste.

### Parte 3: Haz que sus archivos funcionen en Enhanced

Enhanced no puede usar los archivos tal como vienen, así que conviértelos primero.
¿Los sacaste de una carpeta **`enhanced`** (Parte 1)? Ya están convertidos: salta a la Parte 4.

1. Consigue **CodeWalker RPF Explorer** (versión **30_dev48** o más reciente) en el canal de releases del
   [Discord de dexyfex](https://discord.gg/BxfKHkk).
2. Crea una carpeta nueva y vacía en tu Escritorio llamada **`Jason converted`**.
3. En CodeWalker RPF Explorer, haz clic en **Tools** → **Asset Converter**.
4. Como carpeta de **entrada** (input), elige la carpeta con tus 4 archivos **`jdfinal`**. Como carpeta de **salida** (output), elige
   **`Jason converted`**. Haz clic en **Process**.
5. Abre **`Jason converted`**. Si **`jdfinal.ymt`** no está dentro, cópialo tú mismo desde la carpeta de entrada.

### Parte 4: Añádelo al juego

1. Descarga **[GTA-VI-Inspired-HUD-Jason-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Jason-Kit.zip)** y descomprímelo.
2. Copia la carpeta **`jasonped`** del kit en tu carpeta del juego:
   - **Con NaturalVision Enhanced:** en **`onigiri\dlcpacks`**
   - **Sin él:** en **`mods\update\x64\dlcpacks`**
3. En CodeWalker RPF Explorer, haz clic en **Edit** → **Edit mode**, para activarlo.
4. Abre la carpeta **`jasonped`** que acabas de copiar y luego: **`dlc.rpf`** → `x64` → `models` → `cdimages` →
   **`jasonped.rpf`**.
5. Arrastra los 4 archivos de **`Jason converted`** a esa ventana.
6. Abre tu **`dlclist.xml`**:
   - **Con NaturalVision Enhanced:** abre **`onigiri\common\data\dlclist.xml`** con el **Bloc de notas (Notepad)**.
   - **Sin él:** en CodeWalker RPF Explorer, abre `mods` → `update` → **`update.rpf`** → `common` → `data` →
     **`dlclist.xml`**.
7. Desplázate **hasta el final del todo**. Pon el cursor al principio de la línea que dice **`</Paths>`**, pulsa **Enter**
   y pega esto en la nueva línea vacía que queda encima:
   ```xml
   <Item>dlcpacks:/jasonped/</Item>
   ```
8. Pulsa **Ctrl + S** para guardar y ciérralo.
9. Inicia el juego y mantén **Left Alt** (mando: mantén **D-pad Down**). La tarjeta de Jason va después de la de Trevor.

### ¿No funciona?

| Lo que ves | Qué hacer |
| --- | --- |
| Jason no está en la rueda | Abre **`scripts\ViSwitch.log`** de tu carpeta del juego con el Bloc de notas. Si dice **"no Jason model in the game"**, algo de esto está mal: tus 4 archivos no se llaman `jdfinal` (Parte 2), no están dentro de **`jasonped.rpf`**, o a `dlclist.xml` le falta la línea **`<Item>dlcpacks:/jasonped/</Item>`**. Corrígelo e inicia el juego otra vez. |

### Otros modelos de Jason (avanzado)

Solo necesitas esto si tu descarga de Jason no son los 4 archivos de arriba.

- **Muchos archivos** con un `^` en el nombre (como `Jason^head_000_r.ydd`, `Jason^uppr_000_u.ydd`): cambia la parte
  **antes del `^`** por `jdfinal` en **todos** los archivos (`Jason^head_000_r.ydd` pasa a ser `jdfinal^head_000_r.ydd`), y
  también en los que no tienen `^` (`Jason.ymt` pasa a ser `jdfinal.ymt`). Ponlos todos en `jasonped.rpf`.
- **Viene como un pack propio** (tiene su propio `dlc.rpf`): no necesitas el kit. Instala ese pack como diga su
  readme. Luego abre **`scripts\GTA6Hud.ini`** con el Bloc de notas, busca **`[Jason]`** y cambia **`Model = jdfinal`**
  por el nombre de su ped (el nombre de su archivo `.ydd`, antes del punto).

---

## Opcional: Lucia

**Qué obtienes:** Lucia en la rueda de personajes, en la última tarjeta.
**Qué necesitas:** su modelo (no viene incluido) y el **kit de Lucia** gratuito. Sigue las partes en orden.

💡 ¿Ya tienes una carpeta **`luciaped`** en tu carpeta dlcpacks? Entonces ya hay una Lucia instalada: sáltate esta parte.

### Parte 1: Consigue a Lucia

1. Descarga a Lucia de **uno** de estos, o usa otro modelo de Lucia:
   - **[Lucia Caminos en Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)** de su creador
   - **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** en gta5-mods.com
2. Descomprímelo y busca la carpeta que tiene sus archivos. Sus nombres terminan en `.yft`, `.ymt`, etc.

> ¿No ves las terminaciones (`.yft`, etc.)? En el Explorador de archivos, haz clic en **Ver** → **Mostrar** → **Extensiones de nombre de archivo**.

### Parte 2: Qué pack usar y los nombres de sus archivos

El kit tiene dos packs, **A** y **B**. Mira qué hay en su carpeta:

| Su carpeta tiene | Pack | Qué hacer |
| --- | --- | --- |
| **4 archivos**, que terminan en `.ydd`, `.yft`, `.ymt` y `.ytd` | **A** (`A - 4 files`) | ¿Ya se llaman `lucia_s`? **Nada.** ¿Se llaman de otra forma, por ejemplo `Lucia.ydd`? **Renombra cada archivo a `lucia_s`**, manteniendo la terminación: `Lucia.ydd` pasa a ser **`lucia_s.ydd`**, y lo mismo con `.yft`, `.ymt` y `.ytd`. No cambies nada más. |
| Un **`.yft`**, un **`.ymt`** y una **carpeta** con el mismo nombre (llena de archivos como `head_000_r.ydd`) | **B** (`B - with a folder`) | ¿Ya se llaman `lucia_s`? **Nada.** ¿Se llaman de otra forma, por ejemplo `Lucia`? **Renombra los tres a `lucia_s`**: `Lucia.yft` pasa a ser **`lucia_s.yft`**, `Lucia.ymt` pasa a ser **`lucia_s.ymt`** y la carpeta `Lucia` pasa a ser **`lucia_s`**. No renombres los archivos de dentro de la carpeta. ¿Hay una segunda carpeta que termina en `_p` (sus sombreros y gafas)? Renómbrala a **`lucia_s_p`**. |

Y listo: el HUD busca **`lucia_s`**, así que en cuanto sus archivos se llamen así, la encuentra. No hace falta
cambiar ningún ajuste.

Más abajo, **su archivo RPF** es **`luciaped.rpf`** para el pack A y **`luciaped_streamed.rpf`** para el pack B.

### Parte 3: Haz que sus archivos funcionen en Enhanced

Enhanced no puede usar los archivos tal como vienen, así que conviértelos primero.
¿Su descarga tiene una carpeta **`enhanced`**? Esos archivos ya están convertidos: úsalos y salta a la Parte 4.

1. Consigue **CodeWalker RPF Explorer** (versión **30_dev48** o más reciente) en el canal de releases del
   [Discord de dexyfex](https://discord.gg/BxfKHkk).
2. Crea una carpeta nueva y vacía en tu Escritorio llamada **`Lucia converted`**.
3. En CodeWalker RPF Explorer, haz clic en **Tools** → **Asset Converter**.
4. Como carpeta de **entrada** (input), elige la carpeta con sus archivos **`lucia_s`**. Como carpeta de **salida** (output), elige
   **`Lucia converted`**. Haz clic en **Process**.
5. **Solo pack B:** crea una carpeta vacía llamada **`lucia_s`** dentro de **`Lucia converted`**. Vuelve a usar el Asset Converter,
   con su **carpeta `lucia_s`** como entrada y esa nueva carpeta **`lucia_s`** como salida. ¿Tienes una
   carpeta **`lucia_s_p`**? Haz lo mismo con ella.
6. Abre **`Lucia converted`**. Si **`lucia_s.ymt`** no está dentro, cópialo tú mismo desde la carpeta de entrada.

### Parte 4: Añádela al juego

1. Descarga **[GTA-VI-Inspired-HUD-Lucia-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Lucia-Kit.zip)** y descomprímelo.
2. Copia la carpeta **`luciaped`** de tu pack (**`A - 4 files`** o **`B - with a folder`**) en tu carpeta del juego:
   - **Con NaturalVision Enhanced:** en **`onigiri\dlcpacks`**
   - **Sin él:** en **`mods\update\x64\dlcpacks`**
3. En CodeWalker RPF Explorer, haz clic en **Edit** → **Edit mode**, para activarlo.
4. Abre la carpeta **`luciaped`** que acabas de copiar y luego: **`dlc.rpf`** → `x64` → `models` → `cdimages` →
   **su archivo RPF**.
5. Arrastra todo lo que hay en **`Lucia converted`** a esa ventana.
6. Abre tu **`dlclist.xml`**:
   - **Con NaturalVision Enhanced:** abre **`onigiri\common\data\dlclist.xml`** con el **Bloc de notas**.
   - **Sin él:** en CodeWalker RPF Explorer, abre `mods` → `update` → **`update.rpf`** → `common` → `data` →
     **`dlclist.xml`**.
7. Desplázate **hasta el final del todo**. Pon el cursor al principio de la línea que dice **`</Paths>`**, pulsa **Enter**
   y pega esto en la nueva línea vacía que queda encima:
   ```xml
   <Item>dlcpacks:/luciaped/</Item>
   ```
8. Pulsa **Ctrl + S** para guardar y ciérralo.
9. Inicia el juego y mantén **Left Alt** (mando: mantén **D-pad Down**). La tarjeta de Lucia es la última.

### ¿No funciona?

| Lo que ves | Qué hacer |
| --- | --- |
| Un mensaje dice **Lucia is not registered** | A `dlclist.xml` (con **NaturalVision Enhanced**, el `onigiri\common\data\dlclist.xml` de NVE) le falta la línea **`<Item>dlcpacks:/luciaped/</Item>`**. Añádela e inicia el juego otra vez. |
| Lucia no está en la rueda | Abre **`scripts\GTA6Hud.perf.log`** de tu carpeta del juego con el Bloc de notas. Si dice **"lucia: no Lucia model in the game"**, algo de esto está mal: sus archivos no se llaman `lucia_s` (Parte 2), no están dentro de **su archivo RPF**, usaste el otro pack (4 archivos necesitan el **A**, archivos con una carpeta necesitan el **B**), o a `dlclist.xml` le falta la línea **`<Item>dlcpacks:/luciaped/</Item>`**. Corrígelo e inicia el juego otra vez. |
| Lucia es invisible, o solo aparece en parte | Es el pack equivocado para sus archivos (Parte 2). Borra la carpeta `luciaped` que copiaste y vuelve a empezar con el otro. |

### Otros modelos de Lucia (avanzado)

Solo necesitas esto si tu descarga de Lucia no es de ninguno de los dos tipos de la Parte 2.

- **Muchos archivos** con un `^` en el nombre (como `Lucia^head_000_r.ydd`): son las partes de una Lucia de pack **B**,
  organizadas para FiveM. Crea una carpeta llamada **`lucia_s`**, mueve dentro todos los archivos con `^` y renombra cada uno a la
  parte que va **después** del `^` (`Lucia^head_000_r.ydd` pasa a ser `head_000_r.ydd`). Renombra los archivos sin `^` a
  `lucia_s` (`Lucia.yft` pasa a ser `lucia_s.yft`) y luego usa el pack B. Los archivos con `_p^` (sus sombreros y gafas) van de la misma
  forma a una carpeta llamada **`lucia_s_p`**.
- **Viene como un pack propio** (tiene su propio `dlc.rpf`): no necesitas el kit. Instala ese pack como diga su
  readme. Luego abre **`scripts\GTA6Hud.ini`** con el Bloc de notas, busca **`[Lucia]`** y cambia **`Model = lucia_s`**
  por el nombre de su ped (el nombre de su archivo `.yft`, antes del punto).
- **Conservar su propio nombre** en lugar de renombrar sus archivos: el `README.txt` del kit explica cómo, con el `peds.meta` del pack.

---

## Opcional: NaturalVision Enhanced

**Qué obtienes:** **NaturalVision Enhanced** (NVE), de **Razed**, funcionando con este HUD.
**Qué necesitas:** el zip de NVE. NVE carga los mods con su propio cargador (onigiri), así que algunos pasos cambian.

1. Haz los Pasos 1 a 3 de esta guía (Script Hook V y SHVDNE).
   - ⚠️ **Sáltate el Paso 4: no instales OpenRPF.** NVE muestra un aviso sobre `OpenRPF.asi` cuando arranca el juego.
   - ¿Ya copiaste `OpenRPF.asi`? Bórralo de tu carpeta del juego.
2. Instala NVE:
   1. Abre su zip.
   2. Entra en la carpeta **`Main Package`**.
   3. Copia todo lo que hay dentro en tu **carpeta del juego**. (El `README Install Guide.html` de NVE tiene los detalles.)
   4. ¿Windows pregunta si quieres reemplazar **`xinput1_4.dll`**? Haz clic en **Reemplazar**. Es el mismo archivo que usa Script Hook V.
3. Instala el HUD con los Pasos 5 y 6.
4. Copia los packs del HUD en **`onigiri\dlcpacks`**:
   1. En tu carpeta del juego, abre **`mods\update\x64\dlcpacks`**. El instalador pone aquí los packs del HUD.
      El cargador de NVE no lee esta carpeta.
   2. Copia las carpetas **`vihud`**, **`vihudfonts`** y **`vihudanims`**.
   3. Pégalas en **`onigiri\dlcpacks`**.
5. NVE trae su propio **`dlclist.xml`**, y el juego usa ese. Añádele los packs del HUD:
   1. Abre **`onigiri\common\data\dlclist.xml`** (el Bloc de notas sirve).
   2. Desplázate hasta el **final**.
   3. Añade estas líneas justo encima del **`</Paths>`** de cierre, cada una en su propia línea:
   ```xml
   <Item>dlcpacks:/vihud/</Item>
   <Item>dlcpacks:/vihudfonts/</Item>
   <Item>dlcpacks:/vihudanims/</Item>
   ```
   - ⚠️ No debajo de `<Paths>` arriba del todo: el juego se salta las líneas que pongas ahí.
   - ¿Tienes un modelo de Lucia? Añade también las líneas de su pack.
   - Deja el `gameconfig.xml` de NVE como está.
6. **Las pantallas de carga.** Puedes saltarte esto: tendrás las pantallas de carga propias del juego. El instalador puso las
   pantallas de carga del HUD en **`mods\update\update.rpf`**. El cargador de NVE tampoco lee eso, así que cópialas fuera:
   1. Consigue **CodeWalker RPF Explorer** (versión **30_dev48** o más reciente) en el canal de releases del
      [Discord de dexyfex](https://discord.gg/BxfKHkk).
   2. En tu carpeta del juego, abre **`onigiri\platform\data`** y crea ahí una carpeta nueva llamada **`cdimages`**.
   3. Abre **CodeWalker RPF Explorer** y entra en: `mods` → `update` → **`update.rpf`** → `x64` → `data` →
      `cdimages`.
   4. Haz clic derecho en **`scaleform_frontend.rpf`** → **Extract Raw...**. Elige la carpeta **`onigiri\platform\data\cdimages`**
      que acabas de crear y haz clic en **Guardar (Save)**.

   💡 Ese archivo es una copia de las pantallas de menú del juego con las pantallas de carga del HUD dentro.
   **Después de una actualización del juego,** bórralo, vuelve a instalar el HUD y repite este paso.
   Así tendrás las pantallas de menú del juego actualizado.
7. Haz el Paso 7 (Ventana sin bordes) y a jugar.

### Quitar NVE (y conservar el HUD)

**Qué obtienes:** el juego sin NVE, y el HUD sigue funcionando.

⚠️ **Lively World Expansion y Central Real-Life Traffic / Environment ("for NVE")** se cargan a través de NVE, así que también
desaparecen.

Vas a mover los archivos de NVE a una carpeta en lugar de borrarlos. Así podrás volver a poner NVE más adelante.

1. Cierra el juego.
2. Crea una carpeta nueva en tu Escritorio llamada **`NVE removed`**.
3. Desde tu **carpeta del juego**, mueve esto a **`NVE removed`**:
   - **`NVE.asi`**, **`NVE.bin`** y **`NVE.ini`**
   - **`onigiri.asi`**, **`onigiri.ini`** y la carpeta **`onigiri`**
     (también contiene LWE, Central y tus copias de los packs del HUD. El HUD sigue teniendo los suyos en `mods`.)
   - **`ReShade.asi`**, **`ReShade.ini`**, **`ReShade2.ini`** y la carpeta **`reshade-shaders`**
   - cualquier archivo **`renodx-*.addon64`** (¿los sacaste para AuraV? Muévelos también a `NVE removed`)
   - **¿Tenías Central?** También **`CentralLimits.asi`**, **`CentralLimits.ini`** y **`CentralDlcPatch.asi`**, y
     **`ResourceExtensions.asi`** y **`ResourceExtensions.ini`** si solo los conseguiste para Central.
4. ⚠️ **Conserva `xinput1_4.dll`.** NVE lo reemplazó, pero es el mismo archivo que usa Script Hook V para cargar los mods.
5. Haz el **Paso 4** (instalar OpenRPF). NVE necesitaba que no estuviera. Sin NVE, el HUD se carga desde la carpeta `mods` a través de
   OpenRPF.
6. Comprueba que el HUD sigue en la carpeta `mods`: abre **`mods\update\x64\dlcpacks`**. Deberías ver **`vihud`**,
   **`vihudfonts`** y **`vihudanims`**.
   - ¿No están? Haz otra vez el **Paso 6** (instalar el HUD).
7. **¿Tenías a Lucia o a Jason?** Sus packs estaban en `onigiri\dlcpacks`, que acabas de mover.
   1. Copia **`luciaped`** / **`jasonped`** de **`NVE removed\onigiri\dlcpacks`** en **`mods\update\x64\dlcpacks`**.
   2. Añade sus líneas al `dlclist.xml` de `mods` → `update` → **`update.rpf`** → `common` → `data`
      (Parte 4 de "Opcional: Jason" / "Opcional: Lucia", la forma **Sin él**).
8. Inicia el juego.

✅ El HUD aparece, con el aspecto original del juego y las pantallas de carga propias del HUD.

| Lo que ves | Qué hacer |
| --- | --- |
| Un mensaje dice **VI UI pack (vihud) is not registered** | Los packs del HUD no están en la lista de la carpeta `mods`. Haz otra vez el **Paso 6** (instalar el HUD). |
| El juego **se cierra solo** o se cierra de golpe al cargar | Comprueba que conservaste **`xinput1_4.dll`** y que hiciste el **Paso 4** (OpenRPF). Luego mira `asiloader.log` en tu carpeta del juego: ya no debería mencionar `NVE.asi` ni `onigiri.asi`. |

**Para volver a poner NVE:** mueve todo lo de **`NVE removed`** de vuelta a tu carpeta del juego y borra **`OpenRPF.asi`**.

---

## Opcional: Lively World Expansion (con NaturalVision Enhanced)

**Qué obtienes:** **Lively World Expansion** (LWE). Llena el mapa de más plantas y objetos.
**Qué necesitas:** NVE y el HUD preparados primero. El paquete de LWE para NVE también se carga a través del onigiri de NVE.

El zip de LWE trae su propio **`dlclist.xml`**. Ese archivo **reemplaza** el que ya tienes.
La copia de LWE deja fuera:
- los packs del HUD
- tres de los packs más nuevos de NVE (`NVE_lights`, `NVE_vegetation`, `NVE_vehicles_addon`)
- la actualización de 2026 de GTA

Así que, en lugar de eso, añade los packs de LWE a tu lista:

1. Prepara primero NVE y el HUD ("Opcional: NaturalVision Enhanced" más arriba).
2. Copia los packs de LWE:
   1. Abre el zip de LWE.
   2. Elige una carpeta de densidad: **`LWE Enhanced - High Density`**, **`Medium Density`** o **`Low Density`**.
   3. Entra en su carpeta **`onigiri\dlcpacks`**.
   4. Copia las **5 carpetas** que hay ahí en **`onigiri\dlcpacks`** dentro de tu carpeta del juego.
3. ⚠️ **No copies el `onigiri\common\data\dlclist.xml` de LWE.** Usa tu propia lista:
   1. Abre tu propio **`onigiri\common\data\dlclist.xml`**.
   2. Desplázate hasta el final.
   3. Añade estas líneas justo encima del **`</Paths>`** de cierre (no debajo de `<Paths>` arriba del todo), cada una en su propia línea.
      Estas son para densidad alta (High):
   ```xml
   <Item>dlcpacks:/lwecityhigh/</Item>
   <Item>dlcpacks:/lwedeserthigh/</Item>
   <Item>dlcpacks:/lweforesthigh/</Item>
   <Item>dlcpacks:/lwemountainshigh/</Item>
   <Item>dlcpacks:/lweprops/</Item>
   ```
   Para Medium o Low, cambia `high` por `medium` o `low` en las 4 primeras líneas. `lweprops` se queda igual.
4. **¿Ya copiaste el `dlclist.xml` de LWE encima del tuyo?** Vuelve a poner el de NVE:
   1. Del zip de NVE, copia **`Main Package\onigiri\common\data\dlclist.xml`** en `onigiri\common\data` dentro de tu
      carpeta del juego.
   2. Añade las líneas del HUD (paso 5 de la sección de NVE).
   3. Añade las líneas de LWE (paso 3 de aquí).
5. Deja el `gameconfig.xml` de NVE como está, y a jugar.

La [página de instalación](https://livelyworldexpansion.com/installation/) de LWE tiene sus propios consejos de gráficos.

---

## Opcional: Central Real-Life Traffic y Environment (con NaturalVision Enhanced)

**Qué obtienes:** **Central Real-Life Traffic** y **Central Real-Life Environment**, de **CentralMods**.
Llenan Los Santos de coches reales y lugares reales.
**Qué necesitas:** NVE y el HUD preparados primero, y las versiones **"Enhanced (for NVE)"** de Central.
Se cargan a través del onigiri de NVE.

Funcionan con este HUD. Pero los **`Main Files`** de cada uno traen su propio **`dlclist.xml`**. **Reemplaza**
el tuyo, y esa lista deja fuera los packs del HUD. Así que vuelve a añadirlos cuando Central esté instalado:

1. Prepara primero NVE y el HUD ("Opcional: NaturalVision Enhanced" más arriba).
2. **Para Traffic:** instala [ResourceExtensions](https://www.gta5-mods.com/scripts/resource-extensions), de **Koukol**:
   - Copia **`ResourceExtensions.asi`** y **`ResourceExtensions.ini`** en tu carpeta del juego.
   - ⚠️ Central Traffic lo necesita. Sin él, el juego se cierra de golpe justo después de cargar.
3. Instala Central como dice su ReadMe:
   1. Copia todo lo que hay en su carpeta **`Main Files`** en tu carpeta del juego.
   2. Haz clic en **Reemplazar**.
   3. ¿Usas los dos? Haz lo mismo con el otro mod de Central.
4. **¿Usas también Lively World Expansion?**
   1. Abre la carpeta **`Optional`** de Central.
   2. Abre **`LWE Compatibility`**. Es `2. LWE Compatibility` en Traffic y `1. LWE Compatibility` en
      Environment. Son iguales.
   3. Abre la carpeta de tu densidad de LWE: **`High density`**, **`Medium density`** o **`Low density`**.
   4. Copia la carpeta `onigiri` de ahí en tu carpeta del juego.
   5. Haz clic en **Reemplazar**.
5. Vuelve a añadir las líneas del HUD:
   1. Abre **`onigiri\common\data\dlclist.xml`**.
   2. Desplázate hasta el final.
   3. Añade estas líneas justo encima del **`</Paths>`** de cierre (no debajo de `<Paths>` arriba del todo), cada una en su propia línea:
   ```xml
   <Item>dlcpacks:/vihud/</Item>
   <Item>dlcpacks:/vihudfonts/</Item>
   <Item>dlcpacks:/vihudanims/</Item>
   ```
   ¿Tenías otros packs en tu lista, como Forests of San Andreas, GTA V Remastered o un modelo de Lucia? Vuelve a añadir también sus líneas.
6. **Conserva el `gameconfig.xml` que Central Traffic puso en `onigiri\common\data`.**
   - Es el original del juego. En su lugar, ResourceExtensions sube los límites.
   - No vuelvas a poner el de NVE.
   - No subas ningún tamaño de pool en él.

Ten en cuenta:
- **Probado** con Central Real-Life Traffic 2.9.1 y Environment 0.2 (los dos "Enhanced (for NVE)"), NVE, Lively World
  Expansion High y el HUD. Story Mode carga con todos los scripts del HUD funcionando.
- **¿Instalas los dos?** Traen el mismo `dlclist.xml`, así que el orden da igual. Añade las líneas del HUD una sola vez,
  después del segundo.
- **¿Solo usas Environment?** Su lista también deja fuera los packs de coches de NVE, porque Central Traffic reemplaza esos coches.
  Para conservar los coches de NVE, vuelve a añadir también `<Item>dlcpacks:/NVE_vehicles_addon/</Item>` y `<Item>dlcpacks:/NVE_vehicles/</Item>`
  a la lista.
- **Para quitarlos**, sigue el ReadMe de Central. Si vuelves a poner el `dlclist.xml` de NVE, añádele otra vez las líneas del HUD.

---

## Opcional: Forests of San Andreas y GTA V Remastered

**Qué obtienes:** **Forests of San Andreas** y **GTA V Remastered**, de **Larcius** (basados en los mapas de JRod y le__AK).
Llenan el mapa de decenas de miles de árboles, arbustos y objetos extra. Funcionan con este HUD.
**Qué necesitas:** el HUD instalado primero.

El zip de cada mod tiene una carpeta **`GTA V Enhanced`** con sus packs. Cada pack es una carpeta con un `dlc.rpf` dentro:

| Carpeta del pack | De |
| --- | --- |
| **`forest_n`** | Forests (norte) |
| **`forest_s`** | Forests (sur) |
| **`vremastered`** | Remastered |

Instala primero el HUD. Luego elige tu configuración abajo.

### Con NaturalVision Enhanced (sin OpenRPF, sin herramientas extra)

1. Copia las carpetas de pack que quieras (`forest_n`, `forest_s`, `vremastered`) en **`onigiri\dlcpacks`**.
2. Abre **`onigiri\common\data\dlclist.xml`**.
3. Añade una línea para cada una justo encima de `</Paths>`:
   ```xml
   <Item>dlcpacks:/forest_n/</Item>
   <Item>dlcpacks:/forest_s/</Item>
   <Item>dlcpacks:/vremastered/</Item>
   ```
4. Deja el `gameconfig.xml` de NVE como está.

### Sin NVE (con OpenRPF, Paso 4)

1. Copia las carpetas de pack que quieras en **`mods\update\x64\dlcpacks`** dentro de tu carpeta del juego, junto a las del HUD.
2. Consigue **CodeWalker RPF Explorer** (versión 30_dev48 o más reciente, del canal de releases del
   [Discord de dexyfex](https://discord.gg/BxfKHkk)).
   Lo necesitas porque sus líneas van en `mods\update\update.rpf\common\data\dlclist.xml`, que está dentro de un archivo RPF.
3. En CodeWalker RPF Explorer, haz clic en **File** → **Configure Game...**.
4. Elige tu carpeta del juego.
5. Abre `mods\update\update.rpf` → `common` → `data` → **`dlclist.xml`**.
6. Añade las mismas líneas de arriba, justo encima de `</Paths>`.
7. Guarda.

Ten en cuenta:
- **Elige estos o Lively World Expansion, no los dos.** Plantan árboles en el mismo campo, así que tendrías árboles
  dentro de árboles. Forests y Remastered están hechos para ir juntos.
- **Reflejos:** en Enhanced, los mods no pueden traer su shader de reflejos. Así que la pintura de los coches, los charcos y las carreteras mojadas pueden
  mostrar recuadros alrededor de árboles y arbustos. Eso no pasa si **Reflejos con trazado de rayos (Ray Traced Reflections)** está en **Muy alto (Very High)** o **Ultra**.
- **Para quitarlos:** saca sus líneas de `dlclist.xml` y borra las carpetas de los packs.

---

## Opcional: AuraV-Lite (path tracing)

**Qué obtienes:** path tracing en GTA V Enhanced, con [AuraV-Lite](https://kyposmods.com), de **Kypos**.
Funciona junto a este HUD.
**Qué necesitas:** AuraV-Lite. Descárgalo de la web de Kypos.

⚠️ **¿Usas NaturalVision Enhanced?** En su lugar, sigue "Opcional: AuraV-Lite con NaturalVision Enhanced" más abajo.

1. Copia **`AuraV.asi`**, **`msdmo.dll`** y **`nvngx_dlssd.dll`** en tu **carpeta del juego**.
2. Copia también la carpeta **`AuraV`** en tu carpeta del juego.
3. En el juego, ve a **Ajustes** → **Gráficos**.
4. Activa **Trazado de rayos (Ray Tracing)** y **NVIDIA DLSS**. El path tracing necesita los dos.
5. **¿Usas RenoDX** (complementos de ReShade llamados `renodx-*.addon64`)? Abrir el menú de AuraV hace que el juego se cierre de golpe mientras
   estén en tu carpeta del juego. Saca los archivos `renodx-*.addon64` de la carpeta del juego mientras uses AuraV.
6. **Quita la tecla del menú de AuraV de F10.** F10 también es la tecla del HUD para recargar sus ajustes.
   1. Abre el menú de AuraV una vez con **F10**.
   2. Cambia **"The key that opens and closes this menu"** (la tecla que abre y cierra este menú) a otra tecla, como **F11**.

Ten en cuenta:
- No dejes que **DLSS Swapper** reemplace los archivos DLSS de GTA. El path tracing se puede congelar.
- Con una tarjeta gráfica menos potente, no te pases con los ajustes más exigentes.
- Si algo sale mal, `AuraV\AuraV.log` dice por qué.

**Para quitarlo:** borra esos 3 archivos y la carpeta `AuraV`.

---

## Opcional: AuraV-Lite con NaturalVision Enhanced

**Qué obtienes:** AuraV-Lite y **NaturalVision Enhanced** funcionando juntos con este HUD.
**Qué necesitas:** NVE y el HUD preparados, y la descarga de AuraV.

Hay dos cosas que estorban:
- Los atajos de arranque de AuraV hacen que el cargador de NVE se cierre de golpe. (NVE ya se salta por sí mismo las pantallas de arranque del juego.)
- NVE trae los complementos de ReShade de RenoDX, que hacen que el menú de AuraV se cierre de golpe.

Prepáralo en este orden:

1. Prepara primero NVE y el HUD ("Opcional: NaturalVision Enhanced" más arriba).
2. Inicia el juego una vez para comprobar que aparece el HUD.
3. Copia **`AuraV.asi`**, **`msdmo.dll`** y **`nvngx_dlssd.dll`** de la descarga de AuraV en tu **carpeta del juego**,
   junto a `NVE.asi` y `onigiri.asi`.
4. Copia también la carpeta **`AuraV`** en tu carpeta del juego. ⚠️ Nada de AuraV va en `onigiri`.
5. **Antes de iniciar el juego**, crea un archivo de texto llamado **`AuraV.ini`** en la carpeta **`AuraV`**, solo con
   estas líneas:
   ```ini
   [boot]
   skip_legal=0
   cut_logos=0
   skip_menu=0
   story=0

   [AuraV]
   Key=F11
   ```
   - Las líneas de `[boot]` desactivan los atajos de arranque de AuraV, para que el cargador de NVE no se cierre de golpe.
   - `Key=F11` quita el menú de AuraV de **F10**, la tecla del HUD para recargar sus ajustes.
   - AuraV conserva estas líneas. Añade el resto de sus ajustes al archivo la primera vez que se ejecuta.
   - ⚠️ Asegúrate de que el archivo se llame `AuraV.ini`, no `AuraV.ini.txt`
     (Explorador de archivos: **Ver** → **Mostrar** → **Extensiones de nombre de archivo**).
   - **¿Ya iniciaste el juego con AuraV?** Entonces `AuraV\AuraV.ini` ya existe. Ábrelo, pon las líneas de `[boot]`
     en `0` y `Key` (en `[AuraV]`) en `F11`, y guarda.
6. **Saca los complementos de RenoDX.** NVE pone archivos `renodx-*.addon64` en tu carpeta del juego. El menú de AuraV hace que el
   juego se cierre de golpe mientras están ahí.
   1. Muévelos a una carpeta fuera de la carpeta del juego.
   2. Déjalos ahí, por si quitas AuraV más adelante.
   3. Deja el propio ReShade de NVE (`ReShade.asi`, con el aspecto de NVE en `NVE.ini`) donde está. Funciona con AuraV.
7. En el juego, ve a **Ajustes** → **Gráficos**.
8. Activa **Trazado de rayos** y **NVIDIA DLSS**. El path tracing necesita los dos.
9. Mantén **Ventana sin bordes** (Paso 7).
10. A jugar. **F11** abre el menú de AuraV.

Ten en cuenta:
- **Probado** con AuraV-Lite 0.2.7, NVE y el HUD. Story Mode carga con el path tracing activado y el HUD visible.
- **¿AuraV está funcionando?** `asiloader.log` en tu carpeta del juego menciona `AuraV.asi`. `AuraV\AuraV.log` dice "in the
  world" cuando Story Mode ha cargado. Si el juego se cierra de golpe, ese log dice por qué.
- Los consejos de "Opcional: AuraV-Lite" de más arriba (DLSS Swapper, tarjetas gráficas menos potentes) también sirven aquí.
- **Para quitarlo:** borra los 3 archivos de AuraV y la carpeta `AuraV`. Vuelve a poner los complementos de RenoDX en la carpeta del juego.

---

## Opcional: LSPDFR

**LSPDFR** (para jugar como agente de policía) está hecho para **Legacy**.
LSPDFR para Enhanced todavía es una vista previa temprana. No hemos probado el HUD con él.

¿Quieres probarlo de todas formas? La **[parte de LSPDFR de la guía de Legacy](INSTALL-LEGACY.es.md#optional-lspdfr)** explica:

- qué hace el HUD cuando estás de servicio (no tienes que cambiar nada)
- qué teclas hacen dos cosas a la vez, y cómo cambiarlas

---

## ¿Algo salió mal?

Busca lo que te pasa en la columna de la izquierda. Haz lo que dice la columna de la derecha.

**¿Sigues atascado?** Pregunta en **[nuestro Discord de soporte](https://discord.gg/C7gE5SHEzH)**, el de esta versión. O
[abre un issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).

| Qué pasa | Qué hacer |
| --- | --- |
| El juego **se cierra solo unos 20 segundos después de cargar** | Tienes el Script Hook V .NET normal.<br>1. Repite el Paso 3 con **SHVDNE**.<br>2. Borra la carpeta **`dl3`** de `%LOCALAPPDATA%\assembly` (una caché del antiguo).<br>**Para comprobar cuál tienes:** en tu carpeta del juego, haz clic derecho en **`ScriptHookVDotNet.asi`** → **Propiedades (Properties)** → **Detalles (Details)**. SHVDNE pone **"Script Hook V .Net Enhanced"** ahí. El normal no (su versión es 3.6.0.0).<br>**¿Sigue pasando?** Puede que un complemento de ReShade (un archivo que termina en `.addon64` en tu carpeta del juego) esté chocando. Mueve esos archivos a otro sitio y vuelve a probar. |
| Tu carpeta del juego tiene **`MiniMap.asi`** o **`MiniMapHudPresent.asi`** (de otro mod de minimapa 3D o de una instalación anterior) | 1. Bórralo.<br>2. Borra también `scripts\MiniMap.dll`, si está. (El del HUD es `scripts\ViMinimap.dll`.)<br>3. Deja `ViMinimap.asi` junto a `GTA5_Enhanced.exe`.<br>Es una copia antigua del plugin del minimapa que el HUD ya trae como `ViMinimap.asi`. Dos copias cargadas a la vez se estorban entre sí. |
| El juego **se cierra de golpe al abrir el menú de AuraV-Lite** | Tienes los complementos de ReShade de RenoDX (`renodx-*.addon64`) en tu carpeta del juego. NVE los trae.<br>Sácalos mientras uses AuraV.<br>Consulta "Opcional: AuraV-Lite" más arriba, o "Opcional: AuraV-Lite con NaturalVision Enhanced". |
| Un cuadro **"Debug Assertion Failed!"** de Microsoft Visual C++ menciona **`renodx-dlssfix.addon64`** | Es el complemento de RenoDX que arregla el DLSS, no el HUD. Es una versión de depuración (debug).<br>¿No usas DLSS 5? Entonces no lo necesitas: borra `renodx-dlssfix.addon64` de tu carpeta del juego.<br>`renodx-gtav-enhanced.addon64` se puede quedar.<br>No hace falta reinstalar Visual C++ por esto. |
| Usas **AuraV-Lite** con **NaturalVision Enhanced** y el juego **se cierra de golpe al arrancar** | 1. En `AuraV\AuraV.ini`, en `[boot]`, pon `skip_legal`, `cut_logos`, `skip_menu` y `story` en `0`.<br>2. Inicia el juego otra vez.<br>Consulta "Opcional: AuraV-Lite con NaturalVision Enhanced" más arriba. |
| Usas **NaturalVision Enhanced** y el juego **se cierra de golpe al cargar Story Mode** o **el HUD no aparece** | Sigue "Opcional: NaturalVision Enhanced" más arriba:<br>• sin OpenRPF<br>• los packs del HUD copiados en `onigiri\dlcpacks`<br>• el `dlclist.xml` de NVE con ellos en la lista<br>NVE trae su propio `dlclist.xml` y su propio `gameconfig.xml` (en `onigiri\common\data`). El juego usa esos en lugar de los que preparó el paquete del HUD.<br>Abre el `dlclist.xml` de NVE. Añade estas líneas justo encima de `</Paths>`, cada una en su propia línea:<br>`<Item>dlcpacks:/vihud/</Item>`<br>`<Item>dlcpacks:/vihudfonts/</Item>`<br>`<Item>dlcpacks:/vihudanims/</Item>`<br>⚠️ Deja el `gameconfig.xml` de NVE tal como viene. Subir aún más sus tamaños de pool hace que el juego se quede sin memoria al cargar. |
| Usas **NaturalVision Enhanced** y las **pantallas de carga son las del juego**, no las de GTA VI del HUD | Haz el paso 6 de "Opcional: NaturalVision Enhanced" más arriba.<br>El cargador de NVE no lee la carpeta `mods`. Así que las pantallas de carga tienen que copiarse en `onigiri\platform\data\cdimages`. |
| Usas **NaturalVision Enhanced** y, **desde una actualización del juego**, los menús o las pantallas de carga se ven mal | 1. Borra `onigiri\platform\data\cdimages\scaleform_frontend.rpf`. Es una copia de las pantallas de menú del juego de antes de la actualización.<br>2. Vuelve a instalar el HUD.<br>3. Haz otra vez el paso 6 de "Opcional: NaturalVision Enhanced". |
| Instalaste **Lively World Expansion** (LWE) y ahora **faltan partes del HUD** | El `dlclist.xml` de LWE reemplazó el de NVE. No incluye los packs del HUD (ni algunos de los de NVE).<br>1. Vuelve a poner el `dlclist.xml` de NVE.<br>2. Añádele las líneas de LWE y las del HUD.<br>Consulta "Opcional: Lively World Expansion" más arriba. |
| Instalaste **Central Real-Life Traffic** o **Environment** y ahora **faltan partes del HUD** | El `dlclist.xml` de Central reemplazó el tuyo. No incluye los packs del HUD.<br>Vuelve a añadirle las líneas del HUD: consulta "Opcional: Central Real-Life Traffic y Environment" más arriba. |
| El juego **se cierra de golpe justo después de la pantalla de carga** desde que añadiste **Central Real-Life Traffic** | 1. Instala **ResourceExtensions** (Central lo necesita).<br>2. Conserva el `gameconfig.xml` original que Central puso en `onigiri\common\data`.<br>ResourceExtensions no es compatible con uno modificado, incluido el de NVE. |
| Añadiste **Forests of San Andreas** o **GTA V Remastered** y **no aparecen árboles nuevos** | Sus packs tienen que estar donde tu juego carga los packs:<br>• **Con NaturalVision Enhanced:** en `onigiri\dlcpacks`, con sus líneas en `onigiri\common\data\dlclist.xml`.<br>• **Sin él:** en `mods\update\x64\dlcpacks`, con sus líneas en el `dlclist.xml` de `mods\update\update.rpf`.<br>Consulta "Opcional: Forests of San Andreas y GTA V Remastered" más arriba. |
| Un mensaje de Script Hook V dice **"unknown game version"** | GTA V se acaba de actualizar y Script Hook V todavía no se ha puesto al día.<br>1. Espera a que salga un Script Hook V nuevo.<br>2. Repite el Paso 2. |
| El juego arranca pero **el nuevo HUD no aparece** | Comprueba que los 5 archivos de SHVDNE del Paso 3 están junto a `GTA5_Enhanced.exe`. |
| **Actualizaste** el mod, pero el juego sigue comportándose como la **versión antigua** | Script Hook V .NET puede seguir ejecutando su copia guardada de los scripts antiguos.<br>1. Cierra el juego.<br>2. Pulsa **Win + R**.<br>3. Escribe `%LOCALAPPDATA%\assembly` y pulsa Enter.<br>4. Borra la carpeta **`dl3`**. Solo es una caché.<br>**¿Sigue la versión antigua?** Instálalo otra vez con el OIV Package Installer ejecutado como administrador (Paso 6). O copia tú mismo los scripts: consulta "Actualizar a mano" más abajo. |
| **No hay minimapa** en un coche | Pon el juego en **Ventana sin bordes** (Paso 7). |
| Un error sobre **.NET** o **Visual C++** | 1. Instala [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net48-web-installer).<br>2. Instala el [Visual C++ Redistributable (x64)](https://aka.ms/vs/17/release/vc_redist.x64.exe).<br>3. Reinicia tu PC. |
| Un mensaje dice **Lucia is not registered** | Lucia ya no viene incluida con el HUD (consulta el Paso 5).<br>**¿No la tienes?** Pon `Enabled = False` en `[Lucia]` dentro de `scripts\GTA6Hud.ini`. El mensaje desaparece.<br>**¿Tienes un modelo de Lucia?** Falta su pack en `dlclist.xml` (con **NaturalVision Enhanced**, en el de NVE). Añade su línea.<br>Si el mensaje menciona un `VIHudSetup.exe`, ignora esa parte: este mod no lo usa. |
| **Cambiar de personaje cierra el juego de golpe**, y tu juego no está en inglés | Los jugadores dicen que esto solo pasa cuando el juego está en otro idioma.<br>Hasta que se arregle: pon el idioma de GTA V en **inglés** (English) en los ajustes de GTA V de tu launcher. |
| El HUD dice que tu juego **no puede guardar en su carpeta**, y un **aviso de Windows** pide permiso para que **icacls** haga cambios | Tu juego está en **Program Files**, donde Windows solo deja que los administradores cambien archivos. Así que el HUD no puede guardar sus ajustes (y ReShade tampoco los suyos).<br>1. Haz clic en **Sí**. Esto deja que tu usuario de Windows cambie la carpeta del juego. Es lo mismo que hacerlo a mano en **Propiedades** → **Seguridad (Security)** de la carpeta.<br>2. Reinicia el juego una vez.<br>**¿Hiciste clic en No?** Se te volverá a preguntar la próxima vez que inicies el juego.<br>`ViExtras.ini` `[GameFolder] FixAccess = False` hace que deje de preguntar. |
| **Otra cosa** | Pregunta en el **[Discord de soporte](https://discord.gg/C7gE5SHEzH)**.<br>O [abre un issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues) y adjunta estos archivos de tu carpeta del juego:<br>• `ScriptHookVDotNet.log`<br>• `scripts\GTA6Hud.perf.log`<br>• `scripts\GTA6Hud.error.log` |

---

## Actualizar a mano (juego en Program Files)

**Cuándo lo necesitas:** tu juego está en **Program Files** (donde lo instalan el Rockstar Games Launcher y Steam).
Windows protege esa carpeta. Si no ejecutas el OIV Package Installer como administrador (Paso 6), una actualización con
él no siempre reemplaza los archivos antiguos.

También puedes copiar tú mismo los scripts nuevos. Un `.oiv` no es más que un zip.

💡 Desde la 2.6, el HUD se ofrece a arreglar la carpeta por ti la primera vez que se ejecuta. Consulta la fila del aviso de Windows de arriba.
Después de eso, las actualizaciones se instalan con normalidad.

1. Cierra el juego.
2. Copia **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`** a tu Escritorio.
3. Cambia la terminación de la copia a **`.zip`**.
   ¿No ves la parte `.oiv` del nombre? En el Explorador de archivos, activa **Ver** → **Mostrar** → **Extensiones de nombre de archivo**.
4. Abre el zip → **`content`** → **`scripts`**.
5. Selecciona todo lo que hay ahí y cópialo.
6. Pégalo en la carpeta **`scripts`** de tu carpeta del juego. Por ejemplo:
   `C:\Program Files\Rockstar Games\Grand Theft Auto V Enhanced\scripts`.
7. Elige **Reemplazar los archivos en el destino (Replace the files in the destination)**.
8. Haz clic en **Continuar (Continue)** cuando Windows pida permiso de administrador.
9. Borra la caché de Script Hook V .NET, para que cargue los archivos nuevos:
   1. Pulsa **Win + R**.
   2. Escribe `%LOCALAPPDATA%\assembly` y pulsa Enter.
   3. Borra la carpeta **`dl3`**. Solo es una caché. Se vuelve a crear la próxima vez que inicies el juego.
10. ¿Vienes de la 2.5.4 o anterior? La 2.5.5 añade un pack:
    1. Copia la carpeta **`content`** → **`dlcpacks`** → **`vihud`** del zip en **`mods\update\x64\dlcpacks`** dentro de
       tu carpeta del juego.
    2. Instala el mod una vez con el OIV Package Installer, como en el Paso 6. Eso activa el pack nuevo.
11. Inicia el juego. `scripts\ViExtras.log` ahora empieza con una línea `==== ViExtras …`.
    Muestra la versión que descargaste.

---

## Cómo quitar el mod

💡 ¿Quieres quitar **NaturalVision Enhanced** y conservar el HUD? Consulta "Quitar NVE (y conservar el HUD)" en la sección
de NVE.

### La forma más fácil

1. Abre el **OIV Package Installer**.
2. Ve a **Manage Mods**.
3. Desinstala los dos paquetes.

### O a mano

1. En tu **carpeta del juego**, borra **`ViMinimap.asi`**.
2. Abre la carpeta **`scripts`**.
3. Borra estos:
   - **`GTA6Hud.dll`**
   - **`GTA6Hud.ini`**
   - **`ViMinimap.dll`**
   - **`LemonUI.SHVDN3.dll`**
   - los archivos **`MiniMap.*`**
   - la carpeta **`Hud`**
4. Borra la carpeta **`mods`**. ⚠️ Esto también quita cualquier otro mod que haya dentro.
5. Con NaturalVision Enhanced, borra también:
   - **`onigiri\platform\data\cdimages\scaleform_frontend.rpf`** (las pantallas de carga del HUD)
   - los packs del HUD en `onigiri\dlcpacks`

✅ Tu juego vuelve a estar como siempre.

Script Hook V, SHVDNE y OpenRPF se pueden quedar. No cambian nada por sí solos.
