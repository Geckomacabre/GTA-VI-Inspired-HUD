# Cómo instalar: GTA V **Legacy**

*Esta guía es una traducción. La versión en inglés, [INSTALL-LEGACY.md](INSTALL-LEGACY.md), es la más actualizada.*

Esta guía es para la **versión original de PC** de GTA V.

¿Tu carpeta del juego tiene un archivo llamado **`GTA5_Enhanced.exe`**? Entonces tienes Enhanced.
Usa la **[guía de Enhanced](INSTALL-ENHANCED.es.md)** en su lugar.

**¿Nunca has instalado mods en GTA?** No pasa nada. Sigue los pasos en orden. No te saltes ninguno. Te llevará unos 20 minutos.

> [!IMPORTANT]
> ### 💬 ¿Necesitas ayuda? Únete a nuestro Discord de soporte
> Esta versión es un **fork del VI HUD de FBICat** con funciones extra. Por eso tiene su propio soporte:
> - **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**
> - o un [issue en GitHub](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues)
>
> El Discord de FBICat es para su mod original. Por favor, trae tus preguntas sobre esta versión al nuestro. ¡Gracias!

> ⚠️ **Los mods son solo para Story Mode (Modo Historia).** Nunca entres en GTA Online con mods instalados. Te pueden banear.
> (Script Hook V, una de las herramientas de abajo, cierra el juego si lo intentas.)

---

## Antes de empezar: dos ajustes rápidos de Windows

### 1. Mostrar las extensiones de archivo

Algunos archivos de esta guía terminan en `.asi` o `.dll`. Windows oculta esas terminaciones por defecto.

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

## Paso 1: Encuentra tu carpeta de GTA V

Es la carpeta que tiene **`GTA5.exe`** dentro. La llamaremos tu **carpeta del juego**.

Cómo abrirla:

| Tu launcher | Qué hacer |
| --- | --- |
| **Rockstar Games Launcher** | 1. Abre **Ajustes (Settings)**.<br>2. Haz clic en **Grand Theft Auto V** a la izquierda.<br>3. Haz clic en **Abrir carpeta (Open Folder)** (o **Ver carpeta de instalación (View Installation Folder)**). |
| **Steam** | 1. Ve a tu **Biblioteca (Library)**.<br>2. Haz clic derecho en **Grand Theft Auto V**.<br>3. Haz clic en **Administrar (Manage)** → **Ver archivos locales (Browse local files)**. |
| **Epic Games** | 1. Ve a tu **Biblioteca (Library)**.<br>2. Haz clic en los **⋯** debajo de GTA V.<br>3. Haz clic en **Administrar (Manage)**.<br>4. Haz clic en el **icono de carpeta** junto a "Instalación" ("Installation"). |

✅ Deja esta carpeta abierta. Vas a copiar archivos en ella varias veces.

---

## Paso 2: Instala Script Hook V (permite que funcionen los mods)

1. Ve a la [página de Script Hook V](http://www.dev-c.com/gtav/scripthookv/).
2. Haz clic en **Download**.
3. Descomprímelo.
4. Abre la carpeta **`bin`** que hay dentro.
5. Copia estos archivos en tu **carpeta del juego**:
   - **`ScriptHookV.dll`**
   - **`dinput8.dll`** (carga los mods que terminan en `.asi`)
   - **`args.txt`** (desactiva BattlEye, el antitrampas de Rockstar, que bloquea los mods)
   - **`xinput1_4.dll`**, si está

⚠️ **No** copies `NativeTrainer.asi`. Es un menú de trucos que no necesitas.

✅ Tu carpeta del juego ahora tiene `ScriptHookV.dll` y `dinput8.dll` junto a `GTA5.exe`.

---

## Paso 3: Instala Script Hook V .NET (permite que funcione este HUD)

El HUD necesita la **3.7.0 nightly 193 o más reciente**. La versión de **arriba del todo** en la página de nightlies siempre lo es.
Con una más antigua, los scripts del HUD no arrancan, o uno se detiene con un error (consulta "¿Algo salió mal?").

1. Ve a la [página de nightlies de Script Hook V .NET](https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases).
2. Busca la versión de **arriba del todo**.
3. Abre **Assets**.
4. Descarga el archivo **`.zip`**.
5. Descomprímelo.
6. Copia estos archivos en tu **carpeta del juego**:
   - **`ScriptHookVDotNet.asi`**
   - **`ScriptHookVDotNet2.dll`**
   - **`ScriptHookVDotNet3.dll`**
   - **`ScriptHookVDotNet.ini`**

✅ Esos 4 archivos ya están junto a `GTA5.exe`.

---

## Paso 4: Instala OpenIV (instala el mod de forma segura)

OpenIV pone los mods en una **carpeta `mods`** aparte. Los archivos originales de tu juego nunca se modifican.

1. Descarga OpenIV desde [openiv.com](https://openiv.com/).
2. Instálalo.
3. Abre OpenIV.
4. Elige **GTA V** → **Windows**.
5. Si te lo pide, indícale tu **carpeta del juego**.
6. Arriba, haz clic en **Tools** → **ASI Manager**.
7. Haz clic en **Install** junto a **ASI Loader**.
8. Haz clic en **Install** junto a **OpenIV.asi**.
9. Cierra la ventana.

✅ OpenIV está listo.

---

## Paso 5: Descarga el mod

1. Ve a la [**página de Releases**](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest).
2. En **Assets**, descarga **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`** (el mod, unos 100 MB).
   También activa el mod en el juego. No hace falta descargar ningún parche aparte.

❌ No descargues `GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`. Ese es para la otra versión del juego.

**¿Quieres jugar como Lucia o Jason?** Sus modelos no vienen incluidos.

| Personaje | Dónde conseguir el modelo |
| --- | --- |
| **Lucia** | Su modelo es un trabajo de pago de otra persona. Consíguela de su creador:<br>**[Lucia Caminos en Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)** o **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** en gta5-mods.com.<br>O usa otro modelo de Lucia.<br>Para añadirla al juego, consulta "Opcional: Lucia" más abajo.<br>El HUD funciona sin ella. |
| **Jason** | **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** del mismo creador, o **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** en mod-hub.net.<br>Para añadirlo al juego, consulta "Opcional: Jason" más abajo. |

💡 El número del nombre del archivo es la versión (aquí, 2.7.0). Así siempre sabes cuál instalaste.
Menciónalo cuando pidas ayuda.

---

## Paso 6: Instala el mod con OpenIV

⚠️ **¿El juego está en Program Files?** ¿La ruta de tu carpeta del juego empieza con `C:\Program Files`?
(El Rockstar Games Launcher, Epic Games y Steam lo instalan ahí.)
Entonces haz clic derecho en **OpenIV** → **Ejecutar como administrador (Run as administrator)** y haz clic en **Sí (Yes)**.

💡 Windows solo deja que los administradores cambien archivos ahí. Sin esto, la instalación puede dejar los archivos antiguos sin reemplazar.

1. En OpenIV, haz clic en **Tools** → **Package Installer**.
2. Elige **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`**.
3. Haz clic en **Install**.
4. Cuando pregunte dónde instalarlo, elige **mods folder** (la carpeta mods).
5. Si ofrece crear la carpeta, di que sí.
6. Espera hasta que diga que ha terminado.
7. Haz clic en **Close**.

✅ Instalar el mod otra vez, o encima de una versión anterior, no da problemas. Nunca pone un pack dos veces en la lista.

---

## Paso 7: Revisa un archivo

Busca **`ViMinimap.asi`** en tu **carpeta del juego**. Debería estar justo al lado de `GTA5.exe`.

| ¿Está ahí? | Qué hacer |
| --- | --- |
| **Sí** | Perfecto, ve al Paso 8. |
| **No** | 1. Abre la carpeta **`mods`** de tu carpeta del juego.<br>2. Busca **`ViMinimap.asi`**.<br>3. Muévelo junto a **`GTA5.exe`**. |

💡 Sin él, el minimapa no aparece.

---

## Paso 8: ¡A jugar! 🎉

1. Inicia GTA V.
2. Elige **Story Mode**.

✅ El nuevo HUD aparece solo.

| Para... | Haz esto |
| --- | --- |
| **Cambiar de personaje** | Mantén **Left Alt** (mando: mantén **D-pad down**).<br>Elige con **Q / E** o **Left / Right** (mando: **LB / RB**).<br>Luego suelta.<br>Jason y Lucia necesitan sus modelos (no incluidos, consulta el Paso 5). |
| **Ver el minimapa** | Súbete a un coche. Solo aparece en vehículos, o cuando te busca la policía. |
| **Abrir los ajustes del HUD** | Mantén **V**. |
| **Poner tu propia música** | Pon archivos `.mp3` en **`scripts\Hud\Radio\CustomMusic`** dentro de tu carpeta del juego.<br>En un coche, mantén el botón de la radio (**Q**) y pulsa **Down** (mando: stick derecho hacia abajo) para **On Demand**.<br>Cada carpeta dentro de `CustomMusic` es una emisora propia.<br>**Left / Right** (stick derecho) cambian de emisora.<br>La **rueda del ratón** (D-pad Right / Up) cambia la canción. |

---

## Opcional: pantallas de carga de VI

**Qué obtienes:** la pantalla de inicio y las pantallas de carga de GTA VI de FBICat, con las imágenes de Vice City de Rockstar.
**Qué necesitas:** **`GTA-VI-Inspired-HUD-<version>-LoadingScreens-Legacy.oiv`** de la misma versión que el HUD.
El HUD funciona sin él: instálalo solo si las quieres.

En OpenIV: **Tools** → **Package Installer**, elígelo, **Install** y escoge **mods folder**, igual que con el HUD.

### ¿Ya las tienes y quieres recuperar las del juego?

Hasta la 2.6.4, las pantallas de carga venían dentro del paquete del HUD. Para quitarlas:

1. Abre **OpenIV** y haz clic en **Edit mode** arriba.
2. Abre el **`update`** del propio juego → **`update.rpf`** → `x64` → `data` → `cdimages` (no el de `mods`).
3. Haz clic derecho en **`scaleform_frontend.rpf`** → **Extract** (en CodeWalker: **Extract Raw...**) a tu Escritorio (Desktop).
4. Abre **`mods`** → `update` → **`update.rpf`** → `x64` → `data` → `cdimages`.
5. Arrastra el **`scaleform_frontend.rpf`** de tu Escritorio a esa ventana y reemplaza el que hay ahí.

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
   ¿En su lugar hay una carpeta **`legacy`** y otra **`enhanced`** (como en Jason Duval Extended Look)? Abre **`legacy`**.
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

### Parte 3: Añádelo al juego

1. Descarga **[GTA-VI-Inspired-HUD-Jason-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Jason-Kit.zip)** y descomprímelo.
2. Copia la carpeta **`jasonped`** del kit en **`mods\update\x64\dlcpacks`** dentro de tu carpeta del juego.
3. Abre **OpenIV** y haz clic en **Edit mode** arriba, para activarlo.
4. En OpenIV, abre uno tras otro: `mods` → `update` → `x64` → `dlcpacks` → `jasonped` → **`dlc.rpf`** → `x64`
   → `models` → `cdimages` → **`jasonped.rpf`**.
5. Arrastra tus 4 archivos **`jdfinal`** a esa ventana.
6. En OpenIV, abre: `mods` → `update` → **`update.rpf`** → `common` → `data`. Haz clic derecho en **`dlclist.xml`** → **Edit**.
7. Desplázate **hasta el final del todo**. Pon el cursor al principio de la línea que dice **`</Paths>`**, pulsa **Enter**
   y pega esto en la nueva línea vacía que queda encima:
   ```xml
   <Item>dlcpacks:/jasonped/</Item>
   ```
8. Pulsa **Ctrl + S** para guardar y cierra la ventana.
9. Inicia el juego y mantén **Left Alt** (mando: mantén **D-pad Down**). La tarjeta de Jason va después de la de Trevor.

### ¿No funciona?

| Lo que ves | Qué hacer |
| --- | --- |
| Jason no está en la rueda | Abre **`scripts\ViSwitch.log`** de tu carpeta del juego con el Bloc de notas (Notepad). Si dice **"no Jason model in the game"**, algo de esto está mal: tus 4 archivos no se llaman `jdfinal` (Parte 2), no están dentro de **`jasonped.rpf`**, o a `dlclist.xml` le falta la línea **`<Item>dlcpacks:/jasonped/</Item>`**. Corrígelo e inicia el juego otra vez. |

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

### Parte 3: Añádela al juego

1. Descarga **[GTA-VI-Inspired-HUD-Lucia-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Lucia-Kit.zip)** y descomprímelo.
2. Copia la carpeta **`luciaped`** de tu pack (**`A - 4 files`** o **`B - with a folder`**) en
   **`mods\update\x64\dlcpacks`** dentro de tu carpeta del juego.
3. Abre **OpenIV** y haz clic en **Edit mode** arriba, para activarlo.
4. En OpenIV, abre uno tras otro: `mods` → `update` → `x64` → `dlcpacks` → `luciaped` → **`dlc.rpf`** → `x64`
   → `models` → `cdimages` → **su archivo RPF**.
5. Arrastra sus archivos **`lucia_s`** a esa ventana (pack B: también la carpeta **`lucia_s`**, y **`lucia_s_p`** si
   la tienes).
6. En OpenIV, abre: `mods` → `update` → **`update.rpf`** → `common` → `data`. Haz clic derecho en **`dlclist.xml`** → **Edit**.
7. Desplázate **hasta el final del todo**. Pon el cursor al principio de la línea que dice **`</Paths>`**, pulsa **Enter**
   y pega esto en la nueva línea vacía que queda encima:
   ```xml
   <Item>dlcpacks:/luciaped/</Item>
   ```
8. Pulsa **Ctrl + S** para guardar y cierra la ventana.
9. Inicia el juego y mantén **Left Alt** (mando: mantén **D-pad Down**). La tarjeta de Lucia es la última.

### ¿No funciona?

| Lo que ves | Qué hacer |
| --- | --- |
| Un mensaje dice **Lucia is not registered** | A `dlclist.xml` le falta la línea **`<Item>dlcpacks:/luciaped/</Item>`**. Añádela e inicia el juego otra vez. |
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

## Opcional: Forests of San Andreas y GTA V Remastered

**Qué obtienes:** decenas de miles de árboles, arbustos y objetos extra en el mapa.
**Qué necesitas:** **Forests of San Andreas**, **GTA V Remastered** o los dos, de **Larcius** (basados en los mapas de JRod y le__AK). Funcionan con este HUD.
También vienen como OIV, así que se instalan igual que el HUD.

1. Instala primero el HUD (Pasos 1 a 7).
2. En el zip de cada mod, abre la carpeta **`GTA V Legacy`**.
3. En OpenIV, haz clic en **Tools** → **Package Installer**.
4. Elige uno que quieras:
   - **`Forests of San Andreas (North) Ultimate v….oiv`**
   - **`Forests of San Andreas (South) Ultimate v….oiv`**
   - **`GTA V Remastered Ultimate v….oiv`**
5. Elige **mods folder**, como en el Paso 6.
6. Repite los pasos 3 a 5 para cada uno que quieras.

   💡 Cada uno añade su propio pack a la lista de packs del juego, junto al del HUD. Así que no hay nada que editar.
7. ¿Usas también **Liberty City V Remix**? Instala también los OIV **`Compatibility Liberty City V Remix`** de los mods.
8. ⚠️ **Deja `gameconfig.xml` como está.** Los mapas no necesitan otro distinto.
   Instalar otro `gameconfig.xml` reemplazaría el que preparó el HUD, y el HUD lo necesita.
   ¿El juego se cierra de golpe al cargar con los mapas puestos? Consulta "¿Algo salió mal?" más abajo.

**Para quitarlos otra vez:** instala los OIV **`Uninstaller`** de los mods de la misma forma, en la mods folder.

---

## Opcional: Rebalanced Dispatch Enhanced

**Qué obtienes:** **Rebalanced Dispatch Enhanced** (RDE), del **RDE Team**. Reemplaza la respuesta policial de GTA:
nuevas agencias, controles de carretera, helicópteros y hasta **10 estrellas**. El HUD se aparta para no estorbar.
**Qué necesitas:** el HUD instalado primero y **`RDE 4.1.5.oiv`** de la descarga de RDE.

> [!NOTE]
> Nuevo en la 2.6.3, y solo se ha probado un poco en el juego.
> ¿Algo va mal con RDE y el HUD juntos? Cuéntanoslo en Discord o en un issue de GitHub.

### Parte 1: Instala RDE

1. Instala primero el HUD (Pasos 1 a 7).

   💡 El Script Hook V .NET del Paso 3 es la nightly que pide RDE.
2. En OpenIV, haz clic en **Tools** → **Package Installer**.
3. Elige **`RDE 4.1.5.oiv`** de la descarga de RDE.
4. Elige **mods folder**, como en el Paso 6.

⚠️ Sáltate los archivos **`Compatibility - Community HUD`** de RDE. Son para otro mod de HUD.

💡 RDE trae su propio `gameconfig.xml`, con límites mucho más altos de los que necesita el HUD. No pasa nada.
Instalar o actualizar el HUD después solo sube un límite que se quede corto. Así que nunca deshace los de RDE.

### Parte 2: Qué cambia mientras RDE está activo

- La policía de RDE toma el control.
- Esto queda desactivado:
  - el cerebro policial de GTA6Hud
  - las seis estrellas propias de GTA6Hud (y los militares que llaman)
  - los arrestos con pistola eléctrica del HUD
- Las estrellas del HUD muestran el nivel de RDE hasta **seis**. Los niveles 6 a 10 encienden todos la sexta estrella.
- Para dejar el HUD tal cual, pon `[Compat] Rde = False` en `ViExtras.ini`.

### Parte 3: Quitar RDE otra vez

1. Instala el **`Uninstaller.oiv`** de RDE de la misma forma.
2. Vuelve a instalar el HUD (Paso 6), para que vuelvan sus límites y sus packs.

---

## Opcional: Los Santos RED

**Qué obtienes:** **Los Santos RED** (LSR), de **Greskrendtregk**. Convierte el juego en un sandbox criminal.
Tiene su propia policía, crímenes, nivel de búsqueda y mucho más. El HUD se aparta para no estorbar.
**Qué necesitas:** **RAGE Plugin Hook** y **RAGENativeUI**. LSR funciona en RAGE Plugin Hook, no en Script Hook V .NET.

> [!NOTE]
> Nuevo en la 2.6.3, y solo se ha probado un poco en el juego.
> ¿Algo va mal con LSR y el HUD juntos? Cuéntanoslo en Discord o en un issue de GitHub.

⚠️ Usa LSR solo. No junto con LSPDFR ni con Rebalanced Dispatch Enhanced: ellos también controlan la policía.

### Parte 1: Instala LSR

1. Instala primero el HUD (Pasos 1 a 7).
2. Instala **[RAGE Plugin Hook](https://ragepluginhook.net/)**.
3. Instala **[RAGENativeUI](https://github.com/alexguirre/RAGENativeUI/releases)**. Los menús de LSR lo necesitan.
4. Instala LSR, como dice la propia página de LSR.
5. Inicia el juego con **`RAGEPluginHook.exe`**.

✅ LSR se carga en cuanto termina de cargar Story Mode.

### Parte 2: Qué cambia mientras LSR está activo

LSR se encarga de la policía, los crímenes y el nivel de búsqueda. Así que el HUD no añade nada propio:

- sin testigos
- sin cerebro policial ni memoria policial
- sin calor (heat)
- sin seis estrellas
- sin arrestos con pistola eléctrica

Para dejar el HUD tal cual, pon `[Compat] Lsr = False` en `ViExtras.ini`.

### Parte 3: Estrellas

LSR mantiene el nivel de búsqueda propio de GTA en cero y dibuja sus propias estrellas.
Por eso las estrellas y el recuadro de búsqueda del HUD no aparecen con LSR.

¿Quieres estrellas de GTA VI en lugar de las de LSR?

1. Busca las 4 imágenes en la carpeta [`extras/lsr-stars/wantedlevel`](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/tree/main/extras/lsr-stars/wantedlevel) del HUD.
2. Cópialas en **`Plugins\LosSantosRED\images\wantedlevel`**.
3. Reemplaza las imágenes de LSR con ellas.

### Parte 4: Los ajustes de LSR

LSR crea `Plugins\LosSantosRED\Settings.xml` la primera vez que se ejecuta.

1. Cierra el juego.
2. Abre **`Plugins\LosSantosRED\Settings.xml`** en el Bloc de notas.
3. Cambia estos valores:

| Ajuste | Ponlo en | Por qué |
| --- | --- | --- |
| `AlwaysShowRadar` | `false` | El minimapa 3D del HUD reemplaza el radar de GTA. Si LSR activa el radar, este vuelve a aparecer debajo del mapa. |
| `NeverShowRadar` | `true` | Lo mismo. |
| `TopDisplayShowWeapon` | `false` | El HUD ya muestra tu arma arriba a la derecha. |
| `ShowStreetDisplay` | `false` | El HUD ya muestra la calle debajo del minimapa. |

### Parte 5: Teclas

LSR usa algunas de las teclas del HUD. Cambia un lado de cada pareja que uses:

| Tecla | HUD | LSR |
| --- | --- | --- |
| **Left Ctrl** | Agacharse (`ViCrouch.ini` `[Crouch] Key`) | Agacharse (`CrouchKey` en los ajustes de LSR) |
| **K** | Panel de habilidades (`ViExtras.ini` `[Skills] Key`) | Decir "No" en conversaciones |
| **O** | Silenciar la radio (`ViExtras.ini` `[Radio] MuteKey`) | Iniciar una interacción |
| **Shift + N** | Abdominales (`ViExtras.ini` `[Gym] SitUpsKey`) | Modo grupo |

---

<a id="optional-lspdfr"></a>
## Opcional: LSPDFR

**Qué obtienes:** **LSPDFR** (LSPD First Response), de **G17 Media**. Juegas como agente de policía.
Entra de servicio, atiende llamadas (callouts) y detén a conductores. El HUD sigue funcionando y sabe cuándo estás de servicio.
**Qué necesitas:** el HUD instalado primero y LSPDFR de **[lcpdfr.com](https://www.lcpdfr.com/)**.
LSPDFR funciona en RAGE Plugin Hook, no en Script Hook V .NET.

> [!NOTE]
> Probado solo en **Legacy**.

⚠️ No uses LSPDFR con **Los Santos RED**. Los dos controlan la policía.

💡 **Rebalanced Dispatch Enhanced** trae sus propios archivos para LSPDFR, así que esos dos sí pueden ir juntos.
No hemos probado el HUD con los dos a la vez.

### Parte 1: Instala LSPDFR

1. Instala primero el HUD (Pasos 1 a 7).
2. Instala LSPDFR, como dice la propia página de LSPDFR.
3. Inicia el juego con **`RAGEPluginHook.exe`**.

✅ LSPDFR se carga en cuanto termina de cargar Story Mode.

💡 ¿Juegas sin LSPDFR? Inicia el juego de la forma normal. LSPDFR solo se carga a través de `RAGEPluginHook.exe`.

### Parte 2: Qué hace el HUD cuando estás de servicio

**No tienes que cambiar nada.** El HUD detecta LSPDFR solo. Mientras estás de servicio:

- **No se puede cambiar de personaje.** La rueda de personajes no se abre y las teclas de cambio no hacen nada.
  Cambiar de personaje le quitaría a LSPDFR su agente.
- **Sin testigos.** Nadie sale corriendo ni llama para denunciarte. Tú eres la policía.
- **Sin nivel de búsqueda.** Si aparece uno de todas formas, se borra al instante.

Cuando terminas el servicio, todo vuelve a funcionar como antes.

💡 ¿Quieres cambiar de personaje? Termina el servicio primero.

### Parte 3: Teclas

Tres teclas hacen dos cosas a la vez. Cambia un lado de cada pareja que uses:

| Tecla | Lado del HUD | Lado de LSPDFR |
| --- | --- | --- |
| **Y** | Saludar a alguien (`ViExtras.ini` `[Memory] GreetKey`) | Aceptar una llamada (`ACCEPT_CALLOUT_Key` en `lspdfr\keys.ini`) |
| **N** | Flexiones (`ViExtras.ini` `[Gym] PushUpsKey`) | Menú de persecución (`PURSUIT_MENU_Key` en `lspdfr\keys.ini`) |
| **F4** | La consola de Script Hook V .NET (`ConsoleKeyBinding` en `ScriptHookVDotNet.ini`) | La consola de RAGE Plugin Hook (`ConsoleKey` en `RagePluginHook.ini`) |

Para cambiar una tecla:

1. Cierra el juego.
2. Abre el archivo en el Bloc de notas.
   - `ViExtras.ini` está en tu carpeta **`scripts`**.
   - Los otros archivos están en tu **carpeta del juego**.
3. Cambia la letra que va después del `=`. Elige una tecla que no use nada más.
4. Guarda el archivo.

---

## ¿Algo salió mal?

**¿Sigues atascado?** Pregunta en **[nuestro Discord de soporte](https://discord.gg/C7gE5SHEzH)**, el de esta versión.
O [abre un issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).

| Qué pasa | Qué hacer |
| --- | --- |
| Un mensaje de Script Hook V dice **"unknown game version"** | GTA V se acaba de actualizar. Script Hook V todavía no se ha puesto al día.<br>1. Espera a que salga un Script Hook V nuevo.<br>2. Repite el Paso 2. |
| El juego arranca pero **el nuevo HUD no aparece** | Tu Script Hook V .NET es demasiado antiguo.<br>Repite el Paso 3 con la versión más reciente. |
| Un mensaje dice **Unhandled exception in script** (por ejemplo **"MiniMap"**) con una **FileNotFoundException** | Tu Script Hook V .NET es más antiguo de lo que necesita el HUD (3.7.0 nightly 193).<br>1. Repite el Paso 3 con la versión de **arriba del todo** de la página de nightlies.<br>2. Cierra el juego.<br>3. Pulsa **Win + R**.<br>4. Escribe `%LOCALAPPDATA%\assembly` y pulsa **Enter**.<br>5. Borra la carpeta **`dl3`** (solo es una caché).<br>💡 Si es esto, `ScriptHookVDotNet.log` en tu carpeta del juego dice **"Unable to resolve API version"**. |
| **Actualizaste** el mod, pero el juego sigue comportándose como la **versión antigua** | Script Hook V .NET puede seguir ejecutando su copia guardada de los scripts antiguos.<br>1. Cierra el juego.<br>2. Pulsa **Win + R**.<br>3. Escribe `%LOCALAPPDATA%\assembly` y pulsa **Enter**.<br>4. Borra la carpeta **`dl3`** (solo es una caché).<br>¿Sigue la versión antigua? Instálalo otra vez con OpenIV ejecutado como administrador (Paso 6).<br>O copia tú mismo los scripts: consulta "Actualizar a mano" más abajo. |
| **No hay minimapa** en un coche | `ViMinimap.asi` no está junto a `GTA5.exe`.<br>Repite el Paso 7. |
| Tu carpeta del juego tiene **`MiniMap.asi`** o **`MiniMapHudPresent.asi`** (de otro mod de minimapa 3D o de una instalación anterior) | 1. Bórralo.<br>2. Borra también **`scripts\MiniMap.dll`**, si está. (El del HUD es `scripts\ViMinimap.dll`.)<br>3. Deja **`ViMinimap.asi`** junto a `GTA5.exe`.<br>💡 Es una copia antigua del plugin del minimapa. El HUD trae su propia copia como `ViMinimap.asi`. Dos copias cargadas a la vez se estorban entre sí. |
| El juego **se cierra de golpe al cargar** | 1. Vuelve a instalar el mod (Paso 6). Sube los límites del juego que necesita el HUD.<br>2. ¿Tienes otros mods grandes (personajes extra, mapas)? Prueba a desactivarlos. |
| El juego **se cierra de golpe al cargar** desde que añadiste **Forests of San Andreas** o **GTA V Remastered** | Los muchísimos árboles de los mapas pueden llenar tres de los pools del juego.<br>1. En OpenIV, activa **Edit mode**.<br>2. Abre: `mods` → `update` → `update.rpf` → `common` → `data` → **`gameconfig.xml`**.<br>3. Pon el `value` del `PoolSize` de **`Building`** en `70000`.<br>4. Pon **`phInstGta`** en `20000`.<br>5. Pon **`StaticBounds`** en `20000`.<br>Deja como está cualquier valor que ya sea más alto.<br>⚠️ No instales otro `gameconfig.xml` en su lugar. Reemplazaría el que preparó el HUD. (Si lo hiciste, vuelve a instalar el mod, como en el Paso 6.)<br>¿Sigue cerrándose? Añade el [Heap Adjuster](https://www.gta5-mods.com/tools/heapadjuster), que también recomienda la propia guía de los mapas. |
| Un mensaje dice **Lucia is not registered** | Lucia ya no viene incluida con el HUD (consulta el Paso 5).<br>**¿No la tienes?** Pon `Enabled = False` en `[Lucia]` dentro de `scripts\GTA6Hud.ini`. El mensaje desaparece.<br>**¿Tienes un modelo de Lucia?** Falta su pack en `dlclist.xml`. Añade su línea.<br>💡 Si el mensaje menciona un `VIHudSetup.exe`, ignora esa parte. Este mod no lo usa. |
| **Cambiar de personaje cierra el juego de golpe**, y tu juego no está en inglés | Los jugadores dicen que esto solo pasa cuando el juego está en otro idioma.<br>Hasta que se arregle, pon el idioma de GTA V en **inglés** (English) en los ajustes de GTA V de tu launcher. |
| El HUD dice que tu juego **no puede guardar en su carpeta**, y un **aviso de Windows** pide permiso para que **icacls** haga cambios | Tu juego está en **Program Files**. Windows solo deja que los administradores cambien archivos ahí. Así que el HUD no puede guardar sus ajustes (y ReShade tampoco los suyos).<br>1. Haz clic en **Sí**. Esto deja que tu usuario de Windows cambie la carpeta del juego.<br>(Es lo mismo que hacerlo a mano en **Propiedades (Properties)** → **Seguridad (Security)** de la carpeta.)<br>2. Reinicia el juego una vez.<br>**¿Hiciste clic en No?** Se te volverá a preguntar la próxima vez que inicies el juego.<br>Para que deje de preguntar, pon `[GameFolder] FixAccess = False` en `ViExtras.ini`. |
| **Otra cosa** | Pregunta en el **[Discord de soporte](https://discord.gg/C7gE5SHEzH)**.<br>O [abre un issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues) y adjunta estos archivos de tu carpeta del juego:<br>`ScriptHookVDotNet.log`<br>`ViMinimap.log`<br>`scripts\GTA6Hud.perf.log`<br>`scripts\GTA6Hud.error.log` |

---

## Actualizar a mano (juego en Program Files)

¿Tu juego está en **Program Files**? (El Rockstar Games Launcher y Steam lo instalan ahí.)
Windows protege esa carpeta. Una actualización con OpenIV no siempre reemplaza ahí los archivos antiguos.
Ejecuta OpenIV como administrador (Paso 6) o copia tú mismo los scripts nuevos.

💡 Desde la 2.6, el HUD se ofrece a arreglar la carpeta por ti la primera vez que se ejecuta: consulta la fila del aviso de Windows de arriba.
Después de eso, las actualizaciones se instalan con normalidad.

Un `.oiv` no es más que un zip:

1. Cierra el juego.
2. Copia **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`** a tu Escritorio.
3. Cambia la terminación de la copia a **`.zip`**.

   💡 ¿No ves la parte `.oiv` del nombre? En el Explorador de archivos, activa **Ver** → **Mostrar** → **Extensiones de nombre de archivo**.
4. Abre el zip → **`content`** → **`scripts`**.
5. Selecciona todo lo que hay ahí y cópialo.
6. Pégalo en la carpeta **`scripts`** de tu carpeta del juego.
   Por ejemplo: `C:\Program Files\Rockstar Games\Grand Theft Auto V\scripts`.
7. Elige **Reemplazar los archivos en el destino (Replace the files in the destination)**.
8. Haz clic en **Continuar (Continue)** cuando Windows pida permiso de administrador.
9. Borra la caché de Script Hook V .NET, para que cargue los archivos nuevos:
   - Pulsa **Win + R**.
   - Escribe `%LOCALAPPDATA%\assembly` y pulsa Enter.
   - Borra la carpeta **`dl3`**.

   💡 Solo es una caché. Se vuelve a crear la próxima vez que inicies el juego.
10. **¿Vienes de la 2.5.4 o anterior?** La 2.5.5 añade un pack. Haz también esto:
    - Copia la carpeta **`content`** → **`dlcpacks`** → **`vihud`** del zip en **`mods\update\x64\dlcpacks`** dentro de tu carpeta del juego.
    - Instala el mod una vez con OpenIV, como en el Paso 6. Esto activa el pack nuevo.
11. Inicia el juego.

✅ `scripts\ViExtras.log` ahora empieza con una línea `==== ViExtras …`. Muestra la versión que descargaste.

---

## Cómo quitar el mod

1. En tu **carpeta del juego**, borra **`ViMinimap.asi`**.
2. Abre la carpeta **`scripts`**.
3. Borra estos:
   - **`GTA6Hud.dll`**
   - **`GTA6Hud.ini`**
   - **`ViMinimap.dll`**
   - **`LemonUI.SHVDN3.dll`**
   - los archivos **`MiniMap.*`**
   - la carpeta **`Hud`**
4. Borra la carpeta **`mods`**.

   ⚠️ Esto también quita cualquier otro mod que hayas instalado con OpenIV.

✅ Tu juego vuelve a estar como siempre. Script Hook V y Script Hook V .NET se pueden quedar. No cambian nada
por sí solos.
