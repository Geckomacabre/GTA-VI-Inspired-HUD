# Come installare: GTA V **Enhanced**

*Questa è una traduzione. La guida in inglese è sempre la più aggiornata: [INSTALL-ENHANCED.md](INSTALL-ENHANCED.md).*

Questa guida è per la **versione PC del 2025** di GTA V. Controlla quale file c'è nella cartella del gioco:

| Nella cartella del gioco c'è | Hai | Usa |
| --- | --- | --- |
| **`GTA5_Enhanced.exe`** | Enhanced | **Questa guida** |
| **`GTA5.exe`** | Legacy | La **[guida Legacy](INSTALL-LEGACY.it.md)** |

Non hai mai moddato GTA? Nessun problema. Segui i passi in ordine. Non saltarne nessuno. Ci vogliono circa 20 minuti.

> [!IMPORTANT]
> ### 💬 Serve aiuto? Entra nel nostro Discord di supporto
> Questa versione è un **fork del VI HUD di FBICat** con funzioni in più. Per questo ha un supporto tutto suo:
> **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**, oppure una [segnalazione su GitHub](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).
> Il Discord di FBICat è dedicato alla sua mod originale. Per le domande su questa versione, vieni pure nel nostro. Grazie!

> ⚠️ **Le mod sono solo per la Story Mode (modalità storia).** Non entrare mai in GTA Online con le mod installate. Puoi essere bannato.
> (Script Hook V, uno degli strumenti qui sotto, chiude il gioco se ci provi.)

> ⚠️ **Enhanced usa strumenti diversi da Legacy.** Usa solo i download indicati in questa guida.
> Gli strumenti e le mod per Legacy fanno crashare Enhanced.

---

## Prima di iniziare: due rapide impostazioni di Windows

### 1. Mostra le estensioni dei file

Alcuni file in questa guida finiscono in `.asi` o `.dll`. Windows nasconde queste estensioni finché non attivi questa opzione.

1. Apri una cartella qualsiasi.
2. Fai clic su **Visualizza (View)** in alto.
3. Fai clic su **Mostra (Show)** → **Estensioni nomi file (File name extensions)**.

### 2. Sapere come estrarre uno zip

La maggior parte dei download sono file `.zip` (una cartella compressa in un solo file). Per aprirne uno:

1. Fai clic destro sul file.
2. Scegli **Estrai tutto… (Extract All…)**.
3. Fai clic su **Estrai (Extract)**.

✅ Accanto al file compare una cartella normale.

---

## Passo 1: Trova la cartella di GTA V Enhanced

È la cartella che contiene **`GTA5_Enhanced.exe`**. La chiameremo **cartella del gioco**.

Come aprirla:

| Il tuo launcher | Fai così |
| --- | --- |
| **Rockstar Games Launcher** | Apri **Impostazioni (Settings)**.<br>Fai clic su **Grand Theft Auto V Enhanced** a sinistra.<br>Fai clic su **Apri cartella (Open Folder)** (o **Visualizza cartella di installazione (View Installation Folder)**). |
| **Steam** | Apri la tua **Libreria (Library)**.<br>Fai clic destro su **Grand Theft Auto V Enhanced**.<br>Fai clic su **Gestisci (Manage)** → **Sfoglia i file locali (Browse local files)**. |
| **Epic Games** | Apri la tua **Libreria (Library)**.<br>Fai clic sui **⋯** sotto GTA V Enhanced.<br>Fai clic su **Gestisci (Manage)**.<br>Fai clic sull'**icona della cartella** accanto a "Installazione" ("Installation"). |

✅ Tieni aperta questa cartella. Ci copierai dei file più volte.

---

## Passo 2: Installa Script Hook V (permette alle mod di funzionare)

1. Vai alla [pagina di Script Hook V](http://www.dev-c.com/gtav/scripthookv/).
2. Fai clic su **Download**. Lo stesso download funziona per Legacy ed Enhanced.
3. Estrai lo zip.
4. Apri la cartella **`bin`** al suo interno.
5. Copia questi file nella tua **cartella del gioco**:
   - **`ScriptHookV.dll`**
   - **`xinput1_4.dll`** (carica le mod che finiscono in `.asi` su Enhanced)
   - **`args.txt`** (disattiva BattlEye, l'anti-cheat di Rockstar, che blocca le mod)
   - **`dinput8.dll`** (il loader delle mod per Legacy. Enhanced non lo usa, quindi copiarlo non fa danni.)
6. ⚠️ **Non** copiare `NativeTrainer.asi`. È un menu di trucchi che non ti serve.

✅ Ora nella cartella del gioco ci sono `ScriptHookV.dll` e `xinput1_4.dll` accanto a `GTA5_Enhanced.exe`.

---

## Passo 3: Installa Script Hook V .NET **Enhanced** (permette a questo HUD di funzionare)

⚠️ **Non** è il normale Script Hook V .NET. Quello normale fa chiudere Enhanced da solo,
circa 20 secondi dopo che sei entrato nel gioco. Ti serve l'edizione **Enhanced**, chiamata **SHVDNE**.

1. Vai alla [pagina delle release di SHVDNE](https://github.com/Chiheb-Bacha/ScriptHookVDotNetEnhanced/releases/latest).
2. Sotto **Assets**, scarica il file **`.zip`**.
3. Estrailo.
4. Copia i **5 file** della sua cartella principale nella tua **cartella del gioco**:
   - **`ScriptHookVDotNet.asi`**
   - **`ScriptHookVDotNet.ini`**
   - **`ScriptHookVDotNet2.dll`**
   - **`ScriptHookVDotNet3.dll`**
   - **`MinHook.x64.dll`**
5. Salta le cartelle `Debug`, `Docs` e `Licenses` e il file `README.txt`. Non ti servono.

Windows chiede di sostituire dei file? Fai clic su **Sostituisci (Replace)**. (Vuol dire che prima avevi la versione normale.)

✅ Ora quei 5 file sono accanto a `GTA5_Enhanced.exe`.

---

## Passo 4: Installa OpenRPF (tiene la mod in una cartella `mods` sicura)

OpenRPF permette al gioco di caricare le mod da una **cartella `mods`** separata. I file originali del gioco non vengono mai modificati.

1. Vai alla [pagina di OpenRPF](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced).
2. Scaricalo.
3. Estrailo.
4. Copia **`OpenRPF.asi`** nella tua **cartella del gioco**.
5. ⚠️ Nel download c'è anche un `dsound.dll`? **Non copiarlo.** Script Hook V ti ha già dato un loader
   delle mod: `xinput1_4.dll` dal Passo 2. Due loader insieme causano crash.
6. Nella cartella del gioco, crea una nuova cartella chiamata **`mods`**, se non c'è già.
   (Clic destro → **Nuovo (New)** → **Cartella (Folder)**.)

✅ `OpenRPF.asi` è accanto a `GTA5_Enhanced.exe`, e c'è una cartella `mods`.

---

## Passo 5: Scarica la mod e l'installer

### Parte 1: Procurati la mod

1. Vai alla [**pagina Releases**](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest).
2. Sotto **Assets**, scarica **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`**. È la mod, circa 100 MB.
   Attiva anche la mod nel gioco, quindi non c'è nessuna patch separata da scaricare.

❌ Non scaricare `GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`. È per l'altra versione e fa crashare Enhanced.

💡 Il numero nel nome del file è la versione (qui 2.7.0). Così sai sempre quale hai installato.
Indicalo quando chiedi aiuto.

### Vuoi giocare con Lucia?

Non è inclusa: il suo modello è un lavoro a pagamento di qualcun altro. L'HUD funziona anche senza di lei.

Prendila dal suo creatore:
- **[Lucia Caminos su Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)**
- **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** su gta5-mods.com

Oppure usa un altro modello di Lucia. Poi vedi "Opzionale: Lucia" più sotto per metterla nel gioco.

**Per Jason**, prendi uno di questi:
- **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** dallo stesso creatore
- **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** su mod-hub.net

Poi vedi "Opzionale: Jason" più sotto per metterlo nel gioco.

### Parte 2: Procurati l'installer

OpenIV non installa mod su Enhanced. Quindi userai invece l'**OIV Package Installer**.

1. Vai alla [sua pagina su gta5-mods](https://www.gta5-mods.com/tools/oiv-package-installer).
2. Scaricalo.
3. Estrailo dove vuoi, per esempio sul Desktop.

---

## Passo 6: Installa la mod

Prima guarda il percorso della cartella del gioco. Decide come avviare l'installer:

| Il percorso della cartella del gioco inizia con | Come avviare l'installer |
| --- | --- |
| `C:\Program Files` (il Rockstar Games Launcher, Epic Games e Steam lo mettono lì) | Fai clic destro su **`CodeWalker.OIVInstaller.exe`** → **Esegui come amministratore (Run as administrator)**.<br>Fai clic su **Sì (Yes)**. |
| Qualsiasi altro | Avvia **`CodeWalker.OIVInstaller.exe`**. |

⚠️ **Gioco in Program Files?** Windows permette solo agli amministratori di modificare i file lì. Senza **Esegui come amministratore**,
l'installazione può lasciare al loro posto i file vecchi.

1. Apri la cartella dell'OIV Package Installer.
2. Avvia **`CodeWalker.OIVInstaller.exe`** come dice la tabella.
3. Trascina **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`** sulla sua finestra. (Oppure fai clic su **Browse** e sceglilo.)
4. Ti chiede la cartella del gioco. Scegli la cartella con **`GTA5_Enhanced.exe`**.
5. Fai clic su **Install**.
6. Aspetta che finisca.

✅ Reinstallare la mod, o installarla sopra una versione più vecchia, va bene. Non elenca mai un pack due volte.

---

## Passo 7: Cambia un'impostazione del gioco

1. Avvia il gioco.
2. Vai in **Settings** (Impostazioni) → **Graphics** (Grafica).
3. Imposta **Screen Type** (Tipo di schermo) su **Windowed Borderless** (Finestra senza bordi).

💡 Su Enhanced, la minimappa viene disegnata in una piccola finestra sopra il gioco. Quella finestra si vede solo
in modalità Borderless.

---

## Passo 8: Gioca! 🎉

Carica la **Story Mode**. Il nuovo HUD compare da solo.

| Per... | Fai così |
| --- | --- |
| **Cambiare personaggio** | Tieni premuto **Left Alt** (Alt sinistro; controller: tieni premuto **D-pad down**).<br>Scegli con **Q / E** o **Left / Right** (controller: **LB / RB**).<br>Lascia andare.<br>Jason e Lucia richiedono i loro modelli (non inclusi, vedi Passo 5). |
| **Vedere la minimappa** | Sali su un'auto.<br>Si vede solo nei veicoli, o quando sei ricercato. |
| **Aprire le impostazioni dell'HUD** | Tieni premuto **V**. |
| **Ascoltare la tua musica** | Metti dei file `.mp3` in **`scripts\Hud\Radio\CustomMusic`** nella cartella del gioco.<br>In auto, tieni premuto il tasto della radio (**Q**).<br>Premi **Down** (freccia giù; controller: levetta destra in giù) per **On Demand**.<br>Una cartella dentro `CustomMusic` è una stazione a sé.<br>**Left / Right** (levetta destra) cambiano stazione.<br>La **rotellina del mouse** (D-pad Right / Up) cambia canzone. |

---

## Opzionale: schermate di caricamento VI

**Cosa ottieni:** le schermate di caricamento GTA VI di FBICat, con le immagini di Vice City di Rockstar.
**Cosa ti serve:** **`GTA-VI-Inspired-HUD-<version>-LoadingScreens-Enhanced.oiv`** dalla stessa release dell'HUD.
L'HUD funziona anche senza: installalo solo se le vuoi.

Trascinalo sull'**OIV Package Installer** (`CodeWalker.OIVInstaller.exe`), come per l'HUD.

### Le hai già e rivuoi quelle originali del gioco?

Fino alla 2.6.4 le schermate di caricamento erano dentro il pacchetto dell'HUD. Per toglierle:

1. Apri **CodeWalker RPF Explorer** e fai clic su **Edit** → **Edit mode** (modalità modifica).
2. Apri, nei file originali del gioco, **`update`** → **`update.rpf`** → `x64` → `data` → `cdimages` (non quello dentro `mods`).
3. Fai clic destro su **`scaleform_frontend.rpf`** → **Extract** (in CodeWalker: **Extract Raw...**) e salvalo sul Desktop.
4. Apri **`mods`** → `update` → **`update.rpf`** → `x64` → `data` → `cdimages`.
5. Trascina lo **`scaleform_frontend.rpf`** dal Desktop in quella finestra, e sostituisci quello che c'è.

**Con NaturalVision Enhanced**, elimina anche **`onigiri\platform\data\cdimages\scaleform_frontend.rpf`** se l'avevi copiato lì.

---

## Opzionale: Jason

**Cosa ottieni:** Jason sulla ruota dei personaggi, dopo Trevor.
**Cosa ti serve:** il suo modello (non è incluso) e il **kit di Jason** gratuito. Segui le parti in ordine.

### Parte 1: Procurati Jason

1. Scarica Jason da **uno** di questi:
   - **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** dal creatore di Lucia
   - **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** su mod-hub.net
2. Estrai lo zip.
3. Apri la cartella **`No Cap Version`** (o **`Cap Version`** se lo vuoi con il cappellino).
   Ci sono invece una cartella **`legacy`** e una **`enhanced`** (come in Jason Duval Extended Look)? Apri **`enhanced`**.
4. Dentro ci sono **4 file**. I loro nomi finiscono in **`.ydd`**, **`.yft`**, **`.ymt`** e **`.ytd`**.

> Non vedi le estensioni (`.ydd` e così via)? In Esplora file (File Explorer), fai clic su **Visualizza** → **Mostra** → **Estensioni nomi file**.

### Parte 2: Controlla i nomi dei 4 file

Guarda la parte di ogni nome **prima del punto**:

| I tuoi 4 file si chiamano | Cosa fare |
| --- | --- |
| `jdfinal.ydd`, `jdfinal.yft`, `jdfinal.ymt`, `jdfinal.ytd` | **Niente.** Vai alla Parte 3. |
| In qualsiasi altro modo, per esempio `Jason.ydd`, `Jason.yft`, `Jason.ymt`, `Jason.ytd` | **Rinomina ogni file in `jdfinal`**, tenendo l'estensione: `Jason.ydd` diventa **`jdfinal.ydd`**, `Jason.yft` diventa **`jdfinal.yft`**, `Jason.ymt` diventa **`jdfinal.ymt`**, `Jason.ytd` diventa **`jdfinal.ytd`**. Non cambiare nient'altro. |

Fatto: l'HUD cerca **`jdfinal`**, quindi quando i tuoi file si chiamano così, lo trova. Non devi modificare
nessuna impostazione.

### Parte 3: Fai funzionare i suoi file su Enhanced

Enhanced non può usare i file così come sono, quindi prima convertili.
Li hai presi da una cartella **`enhanced`** (Parte 1)? Sono già convertiti: passa alla Parte 4.

1. Procurati **CodeWalker RPF Explorer** (versione **30_dev48** o più recente) dal canale releases del
   [Discord di dexyfex](https://discord.gg/BxfKHkk).
2. Crea sul Desktop una nuova cartella vuota chiamata **`Jason converted`**.
3. In CodeWalker RPF Explorer, fai clic su **Tools** → **Asset Converter**.
4. Come cartella di **input**, scegli la cartella con i tuoi 4 file **`jdfinal`**. Come cartella di **output**, scegli
   **`Jason converted`**. Fai clic su **Process**.
5. Apri **`Jason converted`**. Se **`jdfinal.ymt`** non c'è, copialo tu dalla cartella di input.

### Parte 4: Mettilo nel gioco

1. Scarica **[GTA-VI-Inspired-HUD-Jason-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Jason-Kit.zip)** ed estrailo.
2. Copia la cartella **`jasonped`** del kit nella tua cartella del gioco:
   - **Con NaturalVision Enhanced:** in **`onigiri\dlcpacks`**
   - **Senza NVE:** in **`mods\update\x64\dlcpacks`**
3. In CodeWalker RPF Explorer, fai clic su **Edit** → **Edit mode**, così è attiva.
4. Apri la cartella **`jasonped`** che hai appena copiato, poi: **`dlc.rpf`** → `x64` → `models` → `cdimages` →
   **`jasonped.rpf`**.
5. Trascina i 4 file da **`Jason converted`** in quella finestra.
6. Apri il tuo **`dlclist.xml`**:
   - **Con NaturalVision Enhanced:** apri **`onigiri\common\data\dlclist.xml`** con **Blocco note (Notepad)**.
   - **Senza NVE:** in CodeWalker RPF Explorer, apri `mods` → `update` → **`update.rpf`** → `common` → `data` →
     **`dlclist.xml`**.
7. Scorri fino in **fondo**. Metti il cursore all'inizio della riga che dice **`</Paths>`**, premi **Enter** (Invio),
   e incolla questo nella nuova riga vuota che compare sopra:
   ```xml
   <Item>dlcpacks:/jasonped/</Item>
   ```
8. Premi **Ctrl + S** per salvare, e chiudilo.
9. Avvia il gioco e tieni premuto **Left Alt** (controller: tieni premuto **D-pad Down**). La scheda di Jason è dopo quella di Trevor.

### Non funziona?

| Cosa vedi | Cosa fare |
| --- | --- |
| Jason non è sulla ruota | Apri **`scripts\ViSwitch.log`** nella cartella del gioco con Blocco note. Se dice **"no Jason model in the game"** (nessun modello di Jason nel gioco), c'è uno di questi errori: i tuoi 4 file non si chiamano `jdfinal` (Parte 2), non sono dentro **`jasonped.rpf`**, oppure in `dlclist.xml` manca la riga **`<Item>dlcpacks:/jasonped/</Item>`**. Correggi e riavvia il gioco. |

### Altri modelli di Jason (avanzato)

Ti serve solo se il Jason che hai scaricato non è fatto dei 4 file qui sopra.

- **Tanti file** con un `^` nel nome (come `Jason^head_000_r.ydd`, `Jason^uppr_000_u.ydd`): rinomina la parte
  **prima del `^`** in `jdfinal` in **ogni** file (`Jason^head_000_r.ydd` diventa `jdfinal^head_000_r.ydd`), e
  anche i file senza `^` (`Jason.ymt` diventa `jdfinal.ymt`). Mettili tutti in `jasonped.rpf`.
- **Arriva come pack a sé** (ha un suo `dlc.rpf`): il kit non ti serve. Installa quel pack come dice il suo
  readme. Poi apri **`scripts\GTA6Hud.ini`** con Blocco note, trova **`[Jason]`** e cambia **`Model = jdfinal`**
  con il nome del suo ped (il nome del suo file `.ydd`, prima del punto).

---

## Opzionale: Lucia

**Cosa ottieni:** Lucia sulla ruota dei personaggi, come ultima scheda.
**Cosa ti serve:** il suo modello (non è incluso) e il **kit di Lucia** gratuito. Segui le parti in ordine.

💡 Hai già una cartella **`luciaped`** nella tua cartella dlcpacks? Allora una Lucia è già installata: salta questa parte.

### Parte 1: Procurati Lucia

1. Scarica Lucia da **uno** di questi, oppure usa un altro modello di Lucia:
   - **[Lucia Caminos su Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)** dal suo creatore
   - **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** su gta5-mods.com
2. Estrai lo zip e trova la cartella con i suoi file. I loro nomi finiscono in `.yft`, `.ymt` e così via.

> Non vedi le estensioni (`.yft` e così via)? In Esplora file, fai clic su **Visualizza** → **Mostra** → **Estensioni nomi file**.

### Parte 2: Quale pack, e i nomi dei suoi file

Il kit ha due pack, **A** e **B**. Guarda cosa c'è nella sua cartella:

| La sua cartella contiene | Pack | Cosa fare |
| --- | --- | --- |
| **4 file**, che finiscono in `.ydd`, `.yft`, `.ymt` e `.ytd` | **A** (`A - 4 files`) | Si chiamano già `lucia_s`? **Niente.** Si chiamano in un altro modo, per esempio `Lucia.ydd`? **Rinomina ogni file in `lucia_s`**, tenendo l'estensione: `Lucia.ydd` diventa **`lucia_s.ydd`**, e lo stesso per `.yft`, `.ymt` e `.ytd`. Non cambiare nient'altro. |
| Un **`.yft`**, un **`.ymt`** e una **cartella** con lo stesso nome (piena di file come `head_000_r.ydd`) | **B** (`B - with a folder`) | Si chiamano già `lucia_s`? **Niente.** Si chiamano in un altro modo, per esempio `Lucia`? **Rinomina tutti e tre in `lucia_s`**: `Lucia.yft` diventa **`lucia_s.yft`**, `Lucia.ymt` diventa **`lucia_s.ymt`** e la cartella `Lucia` diventa **`lucia_s`**. Non rinominare i file dentro la cartella. C'è una seconda cartella che finisce in `_p` (i suoi cappelli e occhiali)? Rinominala in **`lucia_s_p`**. |

Fatto: l'HUD cerca **`lucia_s`**, quindi quando i suoi file si chiamano così, la trova. Non devi modificare
nessuna impostazione.

Qui sotto, **il suo archivio** è **`luciaped.rpf`** per il pack A, e **`luciaped_streamed.rpf`** per il pack B.

### Parte 3: Fai funzionare i suoi file su Enhanced

Enhanced non può usare i file così come sono, quindi prima convertili.
Nel suo download c'è una cartella **`enhanced`**? Quei file sono già convertiti: usa quelli e passa alla Parte 4.

1. Procurati **CodeWalker RPF Explorer** (versione **30_dev48** o più recente) dal canale releases del
   [Discord di dexyfex](https://discord.gg/BxfKHkk).
2. Crea sul Desktop una nuova cartella vuota chiamata **`Lucia converted`**.
3. In CodeWalker RPF Explorer, fai clic su **Tools** → **Asset Converter**.
4. Come cartella di **input**, scegli la cartella con i suoi file **`lucia_s`**. Come cartella di **output**, scegli
   **`Lucia converted`**. Fai clic su **Process**.
5. **Solo pack B:** crea una cartella vuota chiamata **`lucia_s`** dentro **`Lucia converted`**. Avvia di nuovo l'Asset Converter,
   con la sua **cartella `lucia_s`** come input e quella nuova cartella **`lucia_s`** come output. Hai una
   cartella **`lucia_s_p`**? Fai lo stesso anche per quella.
6. Apri **`Lucia converted`**. Se **`lucia_s.ymt`** non c'è, copialo tu dalla cartella di input.

### Parte 4: Mettila nel gioco

1. Scarica **[GTA-VI-Inspired-HUD-Lucia-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Lucia-Kit.zip)** ed estrailo.
2. Copia la cartella **`luciaped`** del tuo pack (**`A - 4 files`** o **`B - with a folder`**) nella tua cartella del gioco:
   - **Con NaturalVision Enhanced:** in **`onigiri\dlcpacks`**
   - **Senza NVE:** in **`mods\update\x64\dlcpacks`**
3. In CodeWalker RPF Explorer, fai clic su **Edit** → **Edit mode**, così è attiva.
4. Apri la cartella **`luciaped`** che hai appena copiato, poi: **`dlc.rpf`** → `x64` → `models` → `cdimages` →
   **il suo archivio**.
5. Trascina tutto il contenuto di **`Lucia converted`** in quella finestra.
6. Apri il tuo **`dlclist.xml`**:
   - **Con NaturalVision Enhanced:** apri **`onigiri\common\data\dlclist.xml`** con **Blocco note**.
   - **Senza NVE:** in CodeWalker RPF Explorer, apri `mods` → `update` → **`update.rpf`** → `common` → `data` →
     **`dlclist.xml`**.
7. Scorri fino in **fondo**. Metti il cursore all'inizio della riga che dice **`</Paths>`**, premi **Enter**,
   e incolla questo nella nuova riga vuota che compare sopra:
   ```xml
   <Item>dlcpacks:/luciaped/</Item>
   ```
8. Premi **Ctrl + S** per salvare, e chiudilo.
9. Avvia il gioco e tieni premuto **Left Alt** (controller: tieni premuto **D-pad Down**). La scheda di Lucia è l'ultima.

### Non funziona?

| Cosa vedi | Cosa fare |
| --- | --- |
| Un messaggio dice **Lucia is not registered** | In `dlclist.xml` (con **NaturalVision Enhanced**, il `onigiri\common\data\dlclist.xml` di NVE) manca la riga **`<Item>dlcpacks:/luciaped/</Item>`**. Aggiungila e riavvia il gioco. |
| Lucia non è sulla ruota | Apri **`scripts\GTA6Hud.perf.log`** nella cartella del gioco con Blocco note. Se dice **"lucia: no Lucia model in the game"**, c'è uno di questi errori: i suoi file non si chiamano `lucia_s` (Parte 2), non sono dentro **il suo archivio**, hai usato l'altro pack (4 file vogliono **A**, file con una cartella vogliono **B**), oppure in `dlclist.xml` manca la riga **`<Item>dlcpacks:/luciaped/</Item>`**. Correggi e riavvia il gioco. |
| Lucia è invisibile, o si vede solo in parte | È il pack sbagliato per i suoi file (Parte 2). Elimina la cartella `luciaped` che hai copiato e ricomincia con l'altro. |

### Altri modelli di Lucia (avanzato)

Ti serve solo se la Lucia che hai scaricato non è di uno dei due tipi della Parte 2.

- **Tanti file** con un `^` nel nome (come `Lucia^head_000_r.ydd`): sono le parti di una Lucia da pack **B**,
  organizzate per FiveM. Crea una cartella chiamata **`lucia_s`**, sposta dentro ogni file con un `^`, e rinomina ognuno con la
  parte **dopo** il `^` (`Lucia^head_000_r.ydd` diventa `head_000_r.ydd`). Rinomina i file senza `^` in
  `lucia_s` (`Lucia.yft` diventa `lucia_s.yft`), poi usa il pack B. I file con `_p^` (i suoi cappelli e occhiali) vanno allo stesso
  modo in una cartella chiamata **`lucia_s_p`**.
- **Arriva come pack a sé** (ha un suo `dlc.rpf`): il kit non ti serve. Installa quel pack come dice il suo
  readme. Poi apri **`scripts\GTA6Hud.ini`** con Blocco note, trova **`[Lucia]`** e cambia **`Model = lucia_s`**
  con il nome del suo ped (il nome del suo file `.yft`, prima del punto).
- **Tenere il suo nome originale** invece di rinominare i suoi file: il `README.txt` del kit spiega come, con il `peds.meta` del pack.

---

## Opzionale: NaturalVision Enhanced

**Cosa ottieni:** **NaturalVision Enhanced** (NVE) di **Razed**, funzionante con questo HUD.
**Cosa ti serve:** lo zip di NVE. NVE carica le mod con il suo loader (onigiri), quindi alcuni passi vanno fatti diversamente.

1. Fai i Passi da 1 a 3 di questa guida (Script Hook V e SHVDNE).
   - ⚠️ **Salta il Passo 4: non installare OpenRPF.** NVE avvisa di `OpenRPF.asi` quando il gioco parte.
   - Hai già copiato `OpenRPF.asi`? Eliminalo dalla cartella del gioco.
2. Installa NVE:
   1. Apri il suo zip.
   2. Entra nella cartella **`Main Package`**.
   3. Copia tutto quello che c'è dentro nella tua **cartella del gioco**. (Il `README Install Guide.html` di NVE ha i dettagli.)
   4. Windows chiede di sostituire **`xinput1_4.dll`**? Fai clic su **Sostituisci**. È lo stesso file che usa Script Hook V.
3. Installa l'HUD con i Passi 5 e 6.
4. Copia i pack dell'HUD in **`onigiri\dlcpacks`**:
   1. Nella cartella del gioco, apri **`mods\update\x64\dlcpacks`**. L'installer mette qui i pack dell'HUD.
      Il loader di NVE non legge questa cartella.
   2. Copia le cartelle **`vihud`**, **`vihudfonts`** e **`vihudanims`**.
   3. Incollale in **`onigiri\dlcpacks`**.
5. NVE porta il suo **`dlclist.xml`**, e il gioco usa quello. Aggiungici i pack dell'HUD:
   1. Apri **`onigiri\common\data\dlclist.xml`** (va bene Blocco note).
   2. Scorri fino in **fondo**.
   3. Aggiungi queste righe subito sopra il **`</Paths>`** di chiusura, ognuna sulla sua riga:
   ```xml
   <Item>dlcpacks:/vihud/</Item>
   <Item>dlcpacks:/vihudfonts/</Item>
   <Item>dlcpacks:/vihudanims/</Item>
   ```
   - ⚠️ Non sotto `<Paths>` in alto: lì il gioco salta le righe.
   - Hai un modello di Lucia? Aggiungi anche le righe del suo pack.
   - Lascia com'è il `gameconfig.xml` di NVE.
6. **Le schermate di caricamento.** Puoi saltare questo passo: avrai le schermate di caricamento originali del gioco. L'installer ha messo le
   schermate di caricamento dell'HUD in **`mods\update\update.rpf`**. Il loader di NVE non legge neanche quello, quindi copiale fuori:
   1. Procurati **CodeWalker RPF Explorer** (versione **30_dev48** o più recente) dal canale releases del
      [Discord di dexyfex](https://discord.gg/BxfKHkk).
   2. Nella cartella del gioco, apri **`onigiri\platform\data`** e crea lì una nuova cartella chiamata **`cdimages`**.
   3. Apri **CodeWalker RPF Explorer** e vai in: `mods` → `update` → **`update.rpf`** → `x64` → `data` →
      `cdimages`.
   4. Fai clic destro su **`scaleform_frontend.rpf`** → **Extract Raw...**. Scegli la cartella **`onigiri\platform\data\cdimages`**
      che hai appena creato, e fai clic su **Save**.

   💡 Quel file è una copia delle schermate dei menu del gioco, con dentro le schermate di caricamento dell'HUD.
   **Dopo un aggiornamento del gioco,** eliminalo, reinstalla l'HUD e rifai questo passo.
   Così ottieni le schermate dei menu del gioco aggiornato.
7. Fai il Passo 7 (Windowed Borderless), e gioca.

### Rimuovere NVE (e tenere l'HUD)

**Cosa ottieni:** il gioco senza NVE, con l'HUD che funziona ancora.

⚠️ **Lively World Expansion e Central Real-Life Traffic / Environment ("for NVE")** si caricano tramite NVE, quindi vanno via
anche loro.

Sposterai i file di NVE in una cartella invece di eliminarli. Così puoi rimettere NVE più avanti.

1. Chiudi il gioco.
2. Crea sul Desktop una nuova cartella chiamata **`NVE removed`**.
3. Dalla tua **cartella del gioco**, sposta questi in **`NVE removed`**:
   - **`NVE.asi`**, **`NVE.bin`** e **`NVE.ini`**
   - **`onigiri.asi`**, **`onigiri.ini`** e la cartella **`onigiri`**
     (contiene anche LWE, Central e le tue copie dei pack dell'HUD. L'HUD ha ancora i suoi in `mods`.)
   - **`ReShade.asi`**, **`ReShade.ini`**, **`ReShade2.ini`** e la cartella **`reshade-shaders`**
   - eventuali file **`renodx-*.addon64`** (li avevi spostati fuori per AuraV? Sposta anche quelli in `NVE removed`)
   - **Avevi Central?** Anche **`CentralLimits.asi`**, **`CentralLimits.ini`** e **`CentralDlcPatch.asi`**, e
     **`ResourceExtensions.asi`** e **`ResourceExtensions.ini`** se li avevi presi solo per Central.
4. ⚠️ **Tieni `xinput1_4.dll`.** NVE l'ha sostituito, ma è lo stesso file che Script Hook V usa per caricare le mod.
5. Fai il **Passo 4** (installa OpenRPF). Con NVE non doveva esserci. Senza NVE, l'HUD si carica dalla cartella `mods` tramite
   OpenRPF.
6. Controlla che l'HUD sia ancora nella cartella `mods`: apri **`mods\update\x64\dlcpacks`**. Dovresti vedere **`vihud`**,
   **`vihudfonts`** e **`vihudanims`**.
   - Non ci sono? Rifai il **Passo 6** (installa l'HUD).
7. **Avevi Lucia o Jason?** I loro pack erano in `onigiri\dlcpacks`, che hai appena spostato.
   1. Copia **`luciaped`** / **`jasonped`** da **`NVE removed\onigiri\dlcpacks`** in **`mods\update\x64\dlcpacks`**.
   2. Aggiungi le loro righe al `dlclist.xml` in `mods` → `update` → **`update.rpf`** → `common` → `data`
      (Parte 4 di "Opzionale: Jason" / "Opzionale: Lucia", nel modo **Senza NVE**).
8. Avvia il gioco.

✅ L'HUD si vede, con l'aspetto originale del gioco e le schermate di caricamento dell'HUD.

| Cosa vedi | Cosa fare |
| --- | --- |
| Un messaggio dice **VI UI pack (vihud) is not registered** | I pack dell'HUD non sono nella lista della cartella `mods`. Rifai il **Passo 6** (installa l'HUD). |
| Il gioco **si chiude da solo** o crasha durante il caricamento | Controlla di aver tenuto **`xinput1_4.dll`** e di aver fatto il **Passo 4** (OpenRPF). Poi controlla `asiloader.log` nella cartella del gioco: non dovrebbe più elencare `NVE.asi` o `onigiri.asi`. |

**Per rimettere NVE:** sposta di nuovo tutto da **`NVE removed`** nella cartella del gioco, ed elimina **`OpenRPF.asi`**.

---

## Opzionale: Lively World Expansion (con NaturalVision Enhanced)

**Cosa ottieni:** **Lively World Expansion** (LWE). Riempie la mappa di piante e oggetti in più.
**Cosa ti serve:** NVE e l'HUD già configurati. Anche il pacchetto NVE di LWE si carica tramite onigiri di NVE.

Lo zip di LWE ha il suo **`dlclist.xml`**. Quel file **sostituisce** quello che hai già.
La copia di LWE lascia fuori:
- i pack dell'HUD
- tre dei pack più recenti di NVE (`NVE_lights`, `NVE_vegetation`, `NVE_vehicles_addon`)
- l'aggiornamento 2026 di GTA

Quindi aggiungi invece i pack di LWE alla tua lista:

1. Configura prima NVE e l'HUD ("Opzionale: NaturalVision Enhanced" qui sopra).
2. Copia i pack di LWE:
   1. Apri lo zip di LWE.
   2. Scegli una cartella di densità: **`LWE Enhanced - High Density`**, **`Medium Density`** o **`Low Density`**.
   3. Entra nella sua cartella **`onigiri\dlcpacks`**.
   4. Copia le **5 cartelle** che ci sono dentro in **`onigiri\dlcpacks`** nella cartella del gioco.
3. ⚠️ **Non copiare il `onigiri\common\data\dlclist.xml` di LWE.** Usa invece la tua lista:
   1. Apri il tuo **`onigiri\common\data\dlclist.xml`**.
   2. Scorri fino in fondo.
   3. Aggiungi queste righe subito sopra il **`</Paths>`** di chiusura (non sotto `<Paths>` in alto), ognuna sulla sua riga.
      Queste sono per la densità High:
   ```xml
   <Item>dlcpacks:/lwecityhigh/</Item>
   <Item>dlcpacks:/lwedeserthigh/</Item>
   <Item>dlcpacks:/lweforesthigh/</Item>
   <Item>dlcpacks:/lwemountainshigh/</Item>
   <Item>dlcpacks:/lweprops/</Item>
   ```
   Per Medium o Low, cambia `high` in `medium` o `low` nelle prime 4 righe. `lweprops` resta uguale.
4. **Hai già copiato il `dlclist.xml` di LWE sopra il tuo?** Rimetti quello di NVE:
   1. Dallo zip di NVE, copia **`Main Package\onigiri\common\data\dlclist.xml`** in `onigiri\common\data` nella
      cartella del gioco.
   2. Aggiungi le righe dell'HUD (passo 5 della sezione NVE).
   3. Aggiungi le righe di LWE (passo 3 qui).
5. Lascia com'è il `gameconfig.xml` di NVE, e gioca.

La [pagina di installazione](https://livelyworldexpansion.com/installation/) di LWE ha i suoi consigli sulla grafica.

---

## Opzionale: Central Real-Life Traffic e Environment (con NaturalVision Enhanced)

**Cosa ottieni:** **Central Real-Life Traffic** e **Central Real-Life Environment** di **CentralMods**.
Riempiono Los Santos di auto reali e luoghi reali.
**Cosa ti serve:** NVE e l'HUD già configurati, e le build **"Enhanced (for NVE)"** di Central.
Si caricano tramite onigiri di NVE.

Funzionano con questo HUD. Ma la cartella **`Main Files`** di ognuna ha il suo **`dlclist.xml`**. **Sostituisce**
il tuo, e quella lista lascia fuori i pack dell'HUD. Quindi rimettili dopo aver installato Central:

1. Configura prima NVE e l'HUD ("Opzionale: NaturalVision Enhanced" qui sopra).
2. **Per Traffic:** installa [ResourceExtensions](https://www.gta5-mods.com/scripts/resource-extensions) di **Koukol**:
   - Copia **`ResourceExtensions.asi`** e **`ResourceExtensions.ini`** nella cartella del gioco.
   - ⚠️ Central Traffic ne ha bisogno. Senza, il gioco crasha subito dopo il caricamento.
3. Installa Central come dice il suo ReadMe:
   1. Copia tutto quello che c'è nella sua cartella **`Main Files`** nella cartella del gioco.
   2. Fai clic su **Sostituisci**.
   3. Usi entrambe? Fai lo stesso per l'altra mod Central.
4. **Usi anche Lively World Expansion?**
   1. Apri la cartella **`Optional`** di Central.
   2. Apri **`LWE Compatibility`**. È `2. LWE Compatibility` in Traffic e `1. LWE Compatibility` in
      Environment. Sono identiche.
   3. Apri la cartella per la tua densità di LWE: **`High density`**, **`Medium density`** o **`Low density`**.
   4. Copia da lì la cartella `onigiri` nella cartella del gioco.
   5. Fai clic su **Sostituisci**.
5. Rimetti le righe dell'HUD:
   1. Apri **`onigiri\common\data\dlclist.xml`**.
   2. Scorri fino in fondo.
   3. Aggiungi queste righe subito sopra il **`</Paths>`** di chiusura (non sotto `<Paths>` in alto), ognuna sulla sua riga:
   ```xml
   <Item>dlcpacks:/vihud/</Item>
   <Item>dlcpacks:/vihudfonts/</Item>
   <Item>dlcpacks:/vihudanims/</Item>
   ```
   Avevi altri pack nella tua lista, come Forests of San Andreas, GTA V Remastered o un modello di Lucia? Rimetti anche le loro righe.
6. **Tieni il `gameconfig.xml` che Central Traffic ha messo in `onigiri\common\data`.**
   - È quello originale del gioco. ResourceExtensions alza invece i limiti.
   - Non rimettere quello di NVE.
   - Non alzare nessuna dimensione dei pool in quel file.

Buono a sapersi:
- **Testato** con Central Real-Life Traffic 2.9.1 ed Environment 0.2 (entrambe "Enhanced (for NVE)"), NVE, Lively World
  Expansion High e l'HUD. La Story Mode si carica con tutti gli script dell'HUD attivi.
- **Installi entrambe?** Hanno lo stesso `dlclist.xml`, quindi l'ordine non conta. Aggiungi le righe dell'HUD una sola volta,
  dopo la seconda.
- **Usi solo Environment?** La sua lista lascia fuori anche i pack delle auto di NVE, perché Central Traffic sostituisce quelle auto.
  Per tenere le auto di NVE, rimetti anche `<Item>dlcpacks:/NVE_vehicles_addon/</Item>` e `<Item>dlcpacks:/NVE_vehicles/</Item>`
  nella lista.
- **Per rimuoverle**, segui il ReadMe di Central. Se rimetti il `dlclist.xml` di NVE, aggiungici di nuovo le righe dell'HUD.

---

## Opzionale: Forests of San Andreas e GTA V Remastered

**Cosa ottieni:** **Forests of San Andreas** e **GTA V Remastered** di **Larcius** (basate sulle mappe di JRod e le__AK).
Riempiono la mappa di decine di migliaia di alberi, cespugli e oggetti in più. Funzionano con questo HUD.
**Cosa ti serve:** l'HUD già installato.

Lo zip di ogni mod ha una cartella **`GTA V Enhanced`** con i suoi pack. Ogni pack è una cartella con dentro un `dlc.rpf`:

| Cartella del pack | Da |
| --- | --- |
| **`forest_n`** | Forests (nord) |
| **`forest_s`** | Forests (sud) |
| **`vremastered`** | Remastered |

Installa prima l'HUD. Poi scegli qui sotto la tua configurazione.

### Con NaturalVision Enhanced (niente OpenRPF, niente strumenti extra)

1. Copia le cartelle dei pack che vuoi (`forest_n`, `forest_s`, `vremastered`) in **`onigiri\dlcpacks`**.
2. Apri **`onigiri\common\data\dlclist.xml`**.
3. Aggiungi una riga per ognuno subito sopra `</Paths>`:
   ```xml
   <Item>dlcpacks:/forest_n/</Item>
   <Item>dlcpacks:/forest_s/</Item>
   <Item>dlcpacks:/vremastered/</Item>
   ```
4. Lascia com'è il `gameconfig.xml` di NVE.

### Senza NVE (con OpenRPF, Passo 4)

1. Copia le cartelle dei pack che vuoi in **`mods\update\x64\dlcpacks`** nella cartella del gioco, accanto a quelle dell'HUD.
2. Procurati **CodeWalker RPF Explorer** (versione 30_dev48 o più recente, dal canale releases del
   [Discord di dexyfex](https://discord.gg/BxfKHkk)).
   Ti serve perché le loro righe vanno in `mods\update\update.rpf\common\data\dlclist.xml`, che è dentro un archivio.
3. In CodeWalker RPF Explorer, fai clic su **File** → **Configure Game...**.
4. Scegli la cartella del gioco.
5. Apri `mods\update\update.rpf` → `common` → `data` → **`dlclist.xml`**.
6. Aggiungi le stesse righe di sopra, subito sopra `</Paths>`.
7. Salva.

Buono a sapersi:
- **Scegli queste o Lively World Expansion, non entrambe.** Piantano alberi nella stessa campagna, quindi avresti alberi
  dentro altri alberi. Forests e Remastered sono fatte per andare insieme.
- **Riflessi:** su Enhanced le mod non possono portare il loro shader dei riflessi. Quindi la vernice delle auto, le pozzanghere e le strade bagnate possono
  mostrare dei riquadri intorno ad alberi e cespugli. Non succede se **Ray Traced Reflections** (riflessi in ray tracing) è impostato su **Very High** (Molto alto) o **Ultra**.
- **Per rimuoverle:** togli le loro righe da `dlclist.xml` ed elimina le cartelle dei pack.

---

## Opzionale: AuraV-Lite (path tracing)

**Cosa ottieni:** il path tracing in GTA V Enhanced, da [AuraV-Lite](https://kyposmods.com) di **Kypos**.
Funziona insieme a questo HUD.
**Cosa ti serve:** AuraV-Lite. Scaricalo dal sito di Kypos.

⚠️ **Usi NaturalVision Enhanced?** Segui invece "Opzionale: AuraV-Lite con NaturalVision Enhanced" più sotto.

1. Copia **`AuraV.asi`**, **`msdmo.dll`** e **`nvngx_dlssd.dll`** nella tua **cartella del gioco**.
2. Copia anche la cartella **`AuraV`** nella cartella del gioco.
3. Nel gioco, vai in **Settings** → **Graphics**.
4. Attiva **Ray Tracing** e **NVIDIA DLSS**. Il path tracing richiede entrambi.
5. **Usi RenoDX** (add-on di ReShade chiamati `renodx-*.addon64`)? Aprire il menu di AuraV fa crashare il gioco finché
   sono nella cartella del gioco. Sposta i file `renodx-*.addon64` fuori dalla cartella del gioco mentre usi AuraV.
6. **Sposta il tasto del menu di AuraV da F10.** F10 è anche il tasto dell'HUD per ricaricare le sue impostazioni.
   1. Apri una volta il menu di AuraV con **F10**.
   2. Cambia **"The key that opens and closes this menu"** (il tasto che apre e chiude questo menu) con un altro tasto, come **F11**.

Buono a sapersi:
- Non lasciare che **DLSS Swapper** sostituisca i file DLSS di GTA. Il path tracing può bloccarsi.
- Vacci piano con le impostazioni più pesanti su una scheda video meno potente.
- Se qualcosa va storto, `AuraV\AuraV.log` dice perché.

**Per rimuoverlo:** elimina quei 3 file e la cartella `AuraV`.

---

## Opzionale: AuraV-Lite con NaturalVision Enhanced

**Cosa ottieni:** AuraV-Lite e **NaturalVision Enhanced** che funzionano insieme a questo HUD.
**Cosa ti serve:** NVE e l'HUD configurati, e il download di AuraV.

Due cose si mettono di mezzo:
- Le scorciatoie di avvio di AuraV fanno crashare il loader di NVE. (NVE salta già da solo le schermate di avvio del gioco.)
- NVE include gli add-on di ReShade di RenoDX, che fanno crashare il menu di AuraV.

Configuralo in quest'ordine:

1. Configura prima NVE e l'HUD ("Opzionale: NaturalVision Enhanced" qui sopra).
2. Avvia il gioco una volta per controllare che l'HUD si veda.
3. Copia **`AuraV.asi`**, **`msdmo.dll`** e **`nvngx_dlssd.dll`** dal download di AuraV nella tua **cartella del gioco**,
   accanto a `NVE.asi` e `onigiri.asi`.
4. Copia anche la cartella **`AuraV`** nella cartella del gioco. ⚠️ Niente di AuraV va in `onigiri`.
5. **Prima di avviare il gioco**, crea un file di testo chiamato **`AuraV.ini`** nella cartella **`AuraV`**, con solo
   queste righe:
   ```ini
   [boot]
   skip_legal=0
   cut_logos=0
   skip_menu=0
   story=0

   [AuraV]
   Key=F11
   ```
   - Le righe `[boot]` disattivano le scorciatoie di avvio di AuraV, così il loader di NVE non crasha.
   - `Key=F11` sposta il menu di AuraV da **F10**, il tasto dell'HUD per ricaricare le sue impostazioni.
   - AuraV tiene queste righe. Aggiunge il resto delle sue impostazioni al file la prima volta che parte.
   - ⚠️ Assicurati che il file si chiami `AuraV.ini`, non `AuraV.ini.txt`
     (Esplora file: **Visualizza** → **Mostra** → **Estensioni nomi file**).
   - **Hai già avviato il gioco con AuraV?** Allora `AuraV\AuraV.ini` c'è già. Aprilo, imposta le righe `[boot]`
     a `0` e `Key` (sotto `[AuraV]`) a `F11`, e salva.
6. **Sposta fuori gli add-on di RenoDX.** NVE mette dei file `renodx-*.addon64` nella cartella del gioco. Il menu di AuraV fa crashare il
   gioco finché sono lì.
   1. Spostali in una cartella fuori dalla cartella del gioco.
   2. Tienili lì, nel caso tu rimuova AuraV più avanti.
   3. Lascia dov'è il ReShade di NVE (`ReShade.asi`, con l'aspetto di NVE in `NVE.ini`). Funziona con AuraV.
7. Nel gioco, vai in **Settings** → **Graphics**.
8. Attiva **Ray Tracing** e **NVIDIA DLSS**. Il path tracing richiede entrambi.
9. Tieni **Windowed Borderless** (Passo 7).
10. Gioca. **F11** apre il menu di AuraV.

Buono a sapersi:
- **Testato** con AuraV-Lite 0.2.7, NVE e l'HUD. La Story Mode si carica con il path tracing attivo e l'HUD visibile.
- **AuraV è attivo?** `asiloader.log` nella cartella del gioco elenca `AuraV.asi`. `AuraV\AuraV.log` dice "in the
  world" quando la Story Mode ha finito di caricare. Se il gioco crasha, quel log dice perché.
- I consigli in "Opzionale: AuraV-Lite" qui sopra (DLSS Swapper, schede video meno potenti) valgono anche qui.
- **Per rimuoverlo:** elimina i 3 file di AuraV e la cartella `AuraV`. Rimetti gli add-on di RenoDX nella cartella del gioco.

---

## Opzionale: LSPDFR

**LSPDFR** (giochi nei panni di un agente di polizia) è fatto per **Legacy**.
LSPDFR per Enhanced è ancora un'anteprima iniziale. Non abbiamo provato l'HUD con quella versione.

Vuoi provarlo comunque? La **[parte su LSPDFR della guida Legacy](INSTALL-LEGACY.it.md#optional-lspdfr)** spiega:

- cosa fa l'HUD quando sei in servizio (non devi cambiare niente)
- quali tasti fanno due cose contemporaneamente, e come cambiarli

---

## Qualcosa è andato storto?

Trova nella colonna di sinistra quello che ti succede. Fai quello che dice la colonna di destra.

**Ancora bloccato?** Chiedi nel **[nostro Discord di supporto](https://discord.gg/C7gE5SHEzH)**, quello per questa versione. Oppure
[apri una segnalazione](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).

| Cosa succede | Cosa fare |
| --- | --- |
| Il gioco **si chiude da solo circa 20 secondi dopo il caricamento** | Hai lo Script Hook V .NET normale.<br>1. Rifai il Passo 3 con **SHVDNE**.<br>2. Elimina la cartella **`dl3`** in `%LOCALAPPDATA%\assembly` (una cache di quello vecchio).<br>**Per controllare quale hai:** nella cartella del gioco, fai clic destro su **`ScriptHookVDotNet.asi`** → **Proprietà (Properties)** → **Dettagli (Details)**. Lì SHVDNE dice **"Script Hook V .Net Enhanced"**. Quello normale no (la sua versione è 3.6.0.0).<br>**Succede ancora?** Potrebbe esserci un conflitto con un add-on di ReShade (un file che finisce in `.addon64` nella cartella del gioco). Sposta quei file da un'altra parte e riprova. |
| Nella cartella del gioco c'è **`MiniMap.asi`** o **`MiniMapHudPresent.asi`** (da un'altra mod di minimappa 3D o da un'installazione più vecchia) | 1. Eliminalo.<br>2. Elimina anche `scripts\MiniMap.dll`, se c'è. (Quello dell'HUD è `scripts\ViMinimap.dll`.)<br>3. Tieni `ViMinimap.asi` accanto a `GTA5_Enhanced.exe`.<br>È una copia più vecchia del plugin della minimappa che l'HUD porta già come `ViMinimap.asi`. Due copie caricate insieme si danno fastidio a vicenda. |
| Il gioco **crasha quando apri il menu di AuraV-Lite** | Hai gli add-on di ReShade di RenoDX (`renodx-*.addon64`) nella cartella del gioco. NVE li include.<br>Spostali fuori mentre usi AuraV.<br>Vedi "Opzionale: AuraV-Lite" qui sopra, oppure "Opzionale: AuraV-Lite con NaturalVision Enhanced". |
| Un riquadro **"Debug Assertion Failed!"** di Microsoft Visual C++ nomina **`renodx-dlssfix.addon64`** | È l'add-on DLSS fix di RenoDX, non l'HUD. È una sua build di debug.<br>Non usi DLSS 5? Allora non ti serve: elimina `renodx-dlssfix.addon64` dalla cartella del gioco.<br>`renodx-gtav-enhanced.addon64` può restare.<br>Per questo non serve reinstallare Visual C++. |
| Usi **AuraV-Lite** con **NaturalVision Enhanced**, e il gioco **crasha all'avvio** | 1. In `AuraV\AuraV.ini`, sotto `[boot]`, imposta `skip_legal`, `cut_logos`, `skip_menu` e `story` a `0`.<br>2. Riavvia il gioco.<br>Vedi "Opzionale: AuraV-Lite con NaturalVision Enhanced" qui sopra. |
| Usi **NaturalVision Enhanced**, e il gioco **crasha mentre carica la Story Mode** o **l'HUD non si vede** | Segui "Opzionale: NaturalVision Enhanced" qui sopra:<br>• niente OpenRPF<br>• i pack dell'HUD copiati in `onigiri\dlcpacks`<br>• il `dlclist.xml` di NVE che li elenca<br>NVE porta il suo `dlclist.xml` e il suo `gameconfig.xml` (in `onigiri\common\data`). Il gioco usa quelli invece di quelli preparati dal pacchetto dell'HUD.<br>Apri il `dlclist.xml` di NVE. Aggiungi queste righe subito sopra `</Paths>`, ognuna sulla sua riga:<br>`<Item>dlcpacks:/vihud/</Item>`<br>`<Item>dlcpacks:/vihudfonts/</Item>`<br>`<Item>dlcpacks:/vihudanims/</Item>`<br>⚠️ Lascia il `gameconfig.xml` di NVE così come lo fornisce NVE. Se alzi ancora le dimensioni dei suoi pool, il gioco finisce la memoria durante il caricamento. |
| Usi **NaturalVision Enhanced** e le **schermate di caricamento sono quelle originali del gioco**, non quelle GTA VI dell'HUD | Fai il passo 6 di "Opzionale: NaturalVision Enhanced" qui sopra.<br>Il loader di NVE non legge la cartella `mods`. Quindi le schermate di caricamento vanno copiate in `onigiri\platform\data\cdimages`. |
| Usi **NaturalVision Enhanced**, e **da un aggiornamento del gioco** i menu o le schermate di caricamento si vedono male | 1. Elimina `onigiri\platform\data\cdimages\scaleform_frontend.rpf`. È una copia delle schermate dei menu del gioco di prima dell'aggiornamento.<br>2. Reinstalla l'HUD.<br>3. Rifai il passo 6 di "Opzionale: NaturalVision Enhanced". |
| Hai installato **Lively World Expansion** (LWE), e ora **mancano parti dell'HUD** | Il `dlclist.xml` di LWE ha sostituito quello di NVE. Non elenca i pack dell'HUD (né alcuni di quelli di NVE).<br>1. Rimetti il `dlclist.xml` di NVE.<br>2. Aggiungici le righe di LWE e quelle dell'HUD.<br>Vedi "Opzionale: Lively World Expansion" qui sopra. |
| Hai installato **Central Real-Life Traffic** o **Environment**, e ora **mancano parti dell'HUD** | Il `dlclist.xml` di Central ha sostituito il tuo. Non elenca i pack dell'HUD.<br>Rimettici le righe dell'HUD: vedi "Opzionale: Central Real-Life Traffic e Environment" qui sopra. |
| Il gioco **crasha subito dopo la schermata di caricamento** da quando hai aggiunto **Central Real-Life Traffic** | 1. Installa **ResourceExtensions** (serve a Central).<br>2. Tieni il `gameconfig.xml` originale che Central ha messo in `onigiri\common\data`.<br>ResourceExtensions non supporta un file modificato, compreso quello di NVE. |
| Hai aggiunto **Forests of San Andreas** o **GTA V Remastered**, e **non compaiono alberi nuovi** | I loro pack devono stare dove il gioco carica i pack:<br>• **Con NaturalVision Enhanced:** in `onigiri\dlcpacks`, con le loro righe in `onigiri\common\data\dlclist.xml`.<br>• **Senza NVE:** in `mods\update\x64\dlcpacks`, con le loro righe nel `dlclist.xml` di `mods\update\update.rpf`.<br>Vedi "Opzionale: Forests of San Andreas e GTA V Remastered" qui sopra. |
| Un messaggio di Script Hook V dice **"unknown game version"** | GTA V si è appena aggiornato, e Script Hook V non si è ancora messo al passo.<br>1. Aspetta un nuovo Script Hook V.<br>2. Rifai il Passo 2. |
| Il gioco parte ma **il nuovo HUD non si vede** | Controlla che i 5 file di SHVDNE del Passo 3 siano accanto a `GTA5_Enhanced.exe`. |
| Hai **aggiornato** la mod, ma il gioco si comporta ancora come la **versione vecchia** | Script Hook V .NET può continuare a usare la sua copia salvata dei vecchi script.<br>1. Chiudi il gioco.<br>2. Premi **Win + R**.<br>3. Scrivi `%LOCALAPPDATA%\assembly` e premi Enter.<br>4. Elimina la cartella **`dl3`**. È solo una cache.<br>**Ancora la versione vecchia?** Reinstallala con l'OIV Package Installer eseguito come amministratore (Passo 6). Oppure copia tu gli script: vedi "Aggiornare a mano" più sotto. |
| **Niente minimappa** in auto | Imposta il gioco su **Windowed Borderless** (Passo 7). |
| Un errore su **.NET** o **Visual C++** | 1. Installa [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net48-web-installer).<br>2. Installa il [Visual C++ Redistributable (x64)](https://aka.ms/vs/17/release/vc_redist.x64.exe).<br>3. Riavvia il PC. |
| Un messaggio dice **Lucia is not registered** | Lucia non è più inclusa nell'HUD (vedi Passo 5).<br>**Non ce l'hai?** Imposta `Enabled = False` sotto `[Lucia]` in `scripts\GTA6Hud.ini`. Il messaggio sparisce.<br>**Hai un modello di Lucia?** In `dlclist.xml` (con **NaturalVision Enhanced**, in quello di NVE) manca il suo pack. Aggiungi la sua riga.<br>Se il messaggio parla di un `VIHudSetup.exe`, ignora quella parte: questa mod non lo usa. |
| **Cambiare personaggio fa crashare il gioco**, e il tuo gioco non è in inglese | I giocatori segnalano che succede solo quando il gioco è impostato in un'altra lingua.<br>Finché non viene risolto: imposta la lingua di GTA V su **inglese (English)** nelle impostazioni di GTA V del tuo launcher. |
| L'HUD dice che il gioco **non può salvare nella sua cartella**, e una **finestra di Windows** chiede di permettere a **icacls** di apportare modifiche | Il tuo gioco è in **Program Files**, dove Windows permette solo agli amministratori di modificare i file. Quindi l'HUD non può salvare le sue impostazioni (e ReShade non può salvare le sue).<br>1. Fai clic su **Sì**. Permette al tuo utente di Windows di modificare la cartella del gioco. È come farlo a mano in **Proprietà** → **Sicurezza (Security)** della cartella.<br>2. Riavvia il gioco una volta.<br>**Hai fatto clic su No?** Te lo chiede di nuovo la prossima volta che avvii il gioco.<br>`ViExtras.ini` `[GameFolder] FixAccess = False` fa smettere la domanda. |
| **Qualcos'altro** | Chiedi nel **[Discord di supporto](https://discord.gg/C7gE5SHEzH)**.<br>Oppure [apri una segnalazione](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues) e allega questi file dalla cartella del gioco:<br>• `ScriptHookVDotNet.log`<br>• `scripts\GTA6Hud.perf.log`<br>• `scripts\GTA6Hud.error.log` |

---

## Aggiornare a mano (gioco in Program Files)

**Quando ti serve:** il tuo gioco è in **Program Files** (dove lo mettono il Rockstar Games Launcher e Steam).
Windows protegge quella cartella. Se non esegui l'OIV Package Installer come amministratore (Passo 6), un aggiornamento fatto
con l'installer non sempre sostituisce i file vecchi.

Puoi anche copiare tu i nuovi script. Un `.oiv` è solo uno zip.

💡 Dalla 2.6, l'HUD si offre di sistemare la cartella al posto tuo la prima volta che parte. Vedi la riga sulla finestra di Windows qui sopra.
Dopo, gli aggiornamenti si installano normalmente.

1. Chiudi il gioco.
2. Copia **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`** sul Desktop.
3. Rinomina la copia in **`.zip`**.
   Non vedi la parte `.oiv` del nome? In Esplora file, attiva **Visualizza** → **Mostra** → **Estensioni nomi file**.
4. Apri lo zip → **`content`** → **`scripts`**.
5. Seleziona tutto quello che c'è dentro e copialo.
6. Incollalo nella cartella **`scripts`** della tua cartella del gioco. Per esempio:
   `C:\Program Files\Rockstar Games\Grand Theft Auto V Enhanced\scripts`.
7. Scegli **Sostituisci i file nella destinazione (Replace the files in the destination)**.
8. Fai clic su **Continua (Continue)** quando Windows chiede il permesso di amministratore.
9. Svuota la cache di Script Hook V .NET, così carica i nuovi file:
   1. Premi **Win + R**.
   2. Scrivi `%LOCALAPPDATA%\assembly` e premi Enter.
   3. Elimina la cartella **`dl3`**. È solo una cache. Viene ricreata la prossima volta che avvii il gioco.
10. Arrivi dalla 2.5.4 o da una versione precedente? La 2.5.5 aggiunge un pack:
    1. Copia la cartella **`content`** → **`dlcpacks`** → **`vihud`** dello zip in **`mods\update\x64\dlcpacks`** nella
       tua cartella del gioco.
    2. Installa la mod una volta con l'OIV Package Installer, come nel Passo 6. Questo attiva il nuovo pack.
11. Avvia il gioco. Ora `scripts\ViExtras.log` inizia con una riga `==== ViExtras …`.
    Mostra la versione che hai scaricato.

---

## Come rimuovere la mod

💡 Vuoi rimuovere **NaturalVision Enhanced** e tenere l'HUD? Vedi "Rimuovere NVE (e tenere l'HUD)" nella sezione
NVE.

### Il modo più facile

1. Apri l'**OIV Package Installer**.
2. Vai in **Manage Mods**.
3. Disinstalla entrambi i pacchetti.

### Oppure a mano

1. Nella tua **cartella del gioco**, elimina **`ViMinimap.asi`**.
2. Apri la cartella **`scripts`**.
3. Elimina questi:
   - **`GTA6Hud.dll`**
   - **`GTA6Hud.ini`**
   - **`ViMinimap.dll`**
   - **`LemonUI.SHVDN3.dll`**
   - i file **`MiniMap.*`**
   - la cartella **`Hud`**
4. Elimina la cartella **`mods`**. ⚠️ Così elimini anche tutte le altre mod che contiene.
5. Con NaturalVision Enhanced, elimina anche:
   - **`onigiri\platform\data\cdimages\scaleform_frontend.rpf`** (le schermate di caricamento dell'HUD)
   - i pack dell'HUD in `onigiri\dlcpacks`

✅ Il tuo gioco è tornato normale.

Script Hook V, SHVDNE e OpenRPF possono restare. Da soli non cambiano niente.
