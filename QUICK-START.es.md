# ⚡ Inicio rápido: la versión más sencilla

*Esta guía es una traducción. La versión en inglés, [QUICK-START.md](QUICK-START.md), es la más actualizada.*

*El GTA VI-Inspired HUD está hecho por **FBICat**, con funciones de la versión para FiveM. Los créditos están en el [README](README.md#-credits).*

¿Nunca has instalado mods en GTA V? Haz estos 6 pasos en orden. Cada uno enlaza a la guía completa por si quieres más detalles:
**[Guía de Legacy](INSTALL-LEGACY.es.md)** · **[Guía de Enhanced](INSTALL-ENHANCED.es.md)**.

> [!IMPORTANT]
> ### 💬 ¿Necesitas ayuda? Únete a nuestro Discord de soporte
> Esta versión es un **fork del VI HUD de FBICat** con funciones extra, así que tiene su propio soporte:
> **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**, o un [issue en GitHub](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).
> El Discord de FBICat es para su mod original, así que por favor trae tus preguntas sobre esta versión al nuestro. ¡Gracias!

> ⚠️ Solo Story Mode (Modo Historia). Nunca lleves los mods a GTA Online.

---

## 1. ¿Qué GTA V tienes?

Abre tu carpeta de GTA V (el botón "abrir carpeta" / "ver archivos locales" del launcher).

- Ves **`GTA5.exe`** → tienes **Legacy**.
- Ves **`GTA5_Enhanced.exe`** → tienes **Enhanced**.

Todo lo de abajo indica para cuál es cada paso. No los confundas: los archivos equivocados hacen que el juego se cierre de golpe.

## 2. Copia 3 herramientas en esa carpeta

| Herramienta | Legacy | Enhanced |
| :--- | :--- | :--- |
| **[Script Hook V](http://www.dev-c.com/gtav/scripthookv/)** | de su carpeta `bin`: `ScriptHookV.dll`, `dinput8.dll`, `args.txt` | lo mismo |
| **Script Hook V .NET** | el zip **[nightly](https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases)**: sus 4 archivos `ScriptHookVDotNet` | **[SHVDNE](https://github.com/Chiheb-Bacha/ScriptHookVDotNetEnhanced/releases/latest)** (¡el de Enhanced!): sus 5 archivos |
| **Cargador de mods** | **[OpenIV](https://openiv.com/)**: instálalo y luego **Tools → ASI Manager** → instala **ASI Loader** y **OpenIV.asi** | **[OpenRPF](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced)**: copia `OpenRPF.asi` y crea una carpeta llamada `mods` |

¿Usas **NaturalVision Enhanced**, **Lively World Expansion**, **Central Real-Life Traffic / Environment** o **AuraV-Lite** en Enhanced? Sigue sus secciones "Opcional" de la **[guía de Enhanced](INSTALL-ENHANCED.es.md)** (con NVE, sáltate OpenRPF; con LWE, no copies su `dlclist.xml`; con Central, vuelve a añadir las líneas del HUD a su `dlclist.xml`).

## 3. Descarga el mod

En **[Releases](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest)**, descarga:

- **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`** *o* **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`** (solo el tuyo)
- *(opcional)* Lucia no viene incluida: su modelo es un trabajo de pago de otra persona. Consíguela de su creador,
  **[Lucia Caminos en Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)** o **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** en gta5-mods.com (para Jason, **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** del mismo creador, o **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** en mod-hub.net).

## 4. Instala el mod

- **Legacy:** OpenIV → **Tools → Package Installer** → elige el archivo → instálalo en la **mods folder** (la carpeta mods).
- **Enhanced:** el **[OIV Package Installer](https://www.gta5-mods.com/tools/oiv-package-installer)**
  (`CodeWalker.OIVInstaller.exe`) → arrastra el archivo dentro → elige tu carpeta de GTA V → **Install**.
- **¿El juego está en Program Files?** (Su ruta empieza con `C:\Program Files`.) Haz clic derecho en OpenIV o en
  `CodeWalker.OIVInstaller.exe` → **Ejecutar como administrador (Run as administrator)** primero, o la instalación puede dejar archivos antiguos sin reemplazar.
- Ya no hay RegisterPatch: el propio archivo del mod activa sus packs.

## 5. Una última comprobación

- **Legacy:** asegúrate de que **`ViMinimap.asi`** está junto a `GTA5.exe`. Si está en `mods`, sácalo de ahí.
- **Enhanced:** en el juego, **Ajustes (Settings) → Gráficos (Graphics) → Tipo de pantalla (Screen Type) → Ventana sin bordes (Windowed Borderless)** (si no, el minimapa no aparece).

## 6. A jugar 🎉

Inicia **Story Mode**. El HUD aparece solo.

- **Mantén V:** ajustes del HUD
- **Mantén Q en un coche:** radio. Pulsa **Down** (o el stick derecho hacia abajo) para **On Demand**: tu propia música. Antes, pon
  archivos `.mp3` en `scripts\Hud\Radio\CustomMusic`.
- **Mantén Left Alt** (mando: **D-pad Down**): personajes, la fila de tarjetas de GTA VI (también Jason y Lucia, si tienes sus modelos). **Q / E** (mando: **LB / RB**) para elegir; al soltar, cambias de personaje

Todos los controles están en el **[README](README.md#-controls)**.
