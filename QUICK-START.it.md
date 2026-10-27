# ⚡ Avvio rapido: la versione semplicissima

*Questa è una traduzione. La guida in inglese è sempre la più aggiornata: [QUICK-START.md](QUICK-START.md).*

*Il GTA VI-Inspired HUD è opera di **FBICat**, con funzioni prese dalla versione FiveM. I crediti sono nel [README](README.md#-credits).*

Non hai mai moddato GTA V? Fai questi 6 passi in ordine. Se vuoi più dettagli, ognuno rimanda alla guida completa:
**[Guida Legacy](INSTALL-LEGACY.it.md)** · **[Guida Enhanced](INSTALL-ENHANCED.it.md)**.

> [!IMPORTANT]
> ### 💬 Serve aiuto? Entra nel nostro Discord di supporto
> Questa versione è un **fork del VI HUD di FBICat** con funzioni in più, quindi ha un supporto tutto suo:
> **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**, oppure una [segnalazione su GitHub](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).
> Il Discord di FBICat è dedicato alla sua mod originale, quindi per le domande su questa versione vieni pure nel nostro. Grazie!

> ⚠️ Solo Story Mode (modalità storia). Non portare mai le mod in GTA Online.

---

## 1. Quale GTA V hai?

Apri la cartella di GTA V (il pulsante "apri cartella" / "sfoglia i file locali" del launcher).

- Vedi **`GTA5.exe`** → hai **Legacy**.
- Vedi **`GTA5_Enhanced.exe`** → hai **Enhanced**.

Qui sotto ogni passo dice per quale versione è. Non confonderle: i file sbagliati fanno crashare il gioco.

## 2. Copia 3 strumenti in quella cartella

| Strumento | Legacy | Enhanced |
| :--- | :--- | :--- |
| **[Script Hook V](http://www.dev-c.com/gtav/scripthookv/)** | dalla sua cartella `bin`: `ScriptHookV.dll`, `dinput8.dll`, `args.txt` | gli stessi |
| **Script Hook V .NET** | lo zip **[nightly](https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases)**: i suoi 4 file `ScriptHookVDotNet` | **[SHVDNE](https://github.com/Chiheb-Bacha/ScriptHookVDotNetEnhanced/releases/latest)** (quello per Enhanced!): i suoi 5 file |
| **Loader delle mod** | **[OpenIV](https://openiv.com/)**: installalo, poi **Tools → ASI Manager** → installa **ASI Loader** e **OpenIV.asi** | **[OpenRPF](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced)**: copia `OpenRPF.asi` e crea una cartella chiamata `mods` |

Usi **NaturalVision Enhanced**, **Lively World Expansion**, **Central Real-Life Traffic / Environment** o **AuraV-Lite** su Enhanced? Segui le loro sezioni "Opzionale" nella **[guida Enhanced](INSTALL-ENHANCED.it.md)** (con NVE salta OpenRPF; con LWE non copiare il suo `dlclist.xml`; con Central rimetti le righe dell'HUD nel suo `dlclist.xml`).

## 3. Scarica la mod

Dalla pagina **[Releases](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest)**, prendi:

- **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`** *oppure* **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`** (solo quello della tua versione)
- *(opzionale)* Lucia non è inclusa: il suo modello è un lavoro a pagamento di qualcun altro. Prendila dal suo creatore,
  **[Lucia Caminos su Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)** o **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** su gta5-mods.com (per Jason, **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** dallo stesso creatore, oppure **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** su mod-hub.net).

## 4. Installa la mod

- **Legacy:** OpenIV → **Tools → Package Installer** → scegli il file → installa nella **mods folder** (cartella mods).
- **Enhanced:** l'**[OIV Package Installer](https://www.gta5-mods.com/tools/oiv-package-installer)**
  (`CodeWalker.OIVInstaller.exe`) → trascinaci dentro il file → scegli la cartella di GTA V → **Install**.
- **Gioco in Program Files?** (Il suo percorso inizia con `C:\Program Files`.) Prima fai clic destro su OpenIV o
  `CodeWalker.OIVInstaller.exe` → **Esegui come amministratore (Run as administrator)**, altrimenti l'installazione può lasciare al loro posto i file vecchi.
- Non serve più RegisterPatch: è il file stesso della mod ad attivare i suoi pack.

## 5. Un ultimo controllo

- **Legacy:** assicurati che **`ViMinimap.asi`** sia accanto a `GTA5.exe`. Se è dentro `mods`, spostalo fuori.
- **Enhanced:** nel gioco, **Settings → Graphics → Screen Type → Windowed Borderless** (Impostazioni → Grafica → Tipo di schermo → Finestra senza bordi), altrimenti la minimappa non si vede.

## 6. Gioca 🎉

Avvia la **Story Mode**. L'HUD compare da solo.

- **Tieni premuto V:** impostazioni dell'HUD
- **Tieni premuto Q in auto:** radio. Premi **Down** (freccia giù, o levetta destra in giù) per **On Demand**: la tua musica. Prima metti
  i file `.mp3` in `scripts\Hud\Radio\CustomMusic`.
- **Tieni premuto Left Alt** (Alt sinistro; controller: **D-pad Down**): personaggi, la fila di schede di GTA VI (anche Jason e Lucia, se hai i loro modelli). **Q / E** (controller: **LB / RB**) sceglie, quando lasci il tasto cambi personaggio

Tutti i comandi sono nel **[README](README.md#-controls)**.
