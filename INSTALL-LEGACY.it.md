# Come installare: GTA V **Legacy**

*Questa è una traduzione. La guida in inglese è sempre la più aggiornata: [INSTALL-LEGACY.md](INSTALL-LEGACY.md).*

Questa guida è per la **versione PC originale** di GTA V.

Nella cartella del gioco c'è un file chiamato **`GTA5_Enhanced.exe`**? Allora hai Enhanced.
Usa invece la **[guida Enhanced](INSTALL-ENHANCED.it.md)**.

**Non hai mai moddato GTA?** Nessun problema. Segui i passi in ordine. Non saltarne nessuno. Ci vogliono circa 20 minuti.

> [!IMPORTANT]
> ### 💬 Serve aiuto? Entra nel nostro Discord di supporto
> Questa versione è un **fork del VI HUD di FBICat** con funzioni in più. Per questo ha un supporto tutto suo:
> - **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**
> - oppure una [segnalazione su GitHub](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues)
>
> Il Discord di FBICat è dedicato alla sua mod originale. Per le domande su questa versione, vieni pure nel nostro. Grazie!

> ⚠️ **Le mod sono solo per la Story Mode (modalità storia).** Non entrare mai in GTA Online con le mod installate. Puoi essere bannato.
> (Script Hook V, uno degli strumenti qui sotto, chiude il gioco se ci provi.)

---

## Prima di iniziare: due rapide impostazioni di Windows

### 1. Mostra le estensioni dei file

Alcuni file in questa guida finiscono in `.asi` o `.dll`. Di base Windows nasconde queste estensioni.

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

## Passo 1: Trova la cartella di GTA V

È la cartella che contiene **`GTA5.exe`**. La chiameremo **cartella del gioco**.

Come aprirla:

| Il tuo launcher | Cosa fare |
| --- | --- |
| **Rockstar Games Launcher** | 1. Apri **Impostazioni (Settings)**.<br>2. Fai clic su **Grand Theft Auto V** a sinistra.<br>3. Fai clic su **Apri cartella (Open Folder)** (o **Visualizza cartella di installazione (View Installation Folder)**). |
| **Steam** | 1. Vai nella tua **Libreria (Library)**.<br>2. Fai clic destro su **Grand Theft Auto V**.<br>3. Fai clic su **Gestisci (Manage)** → **Sfoglia i file locali (Browse local files)**. |
| **Epic Games** | 1. Vai nella tua **Libreria (Library)**.<br>2. Fai clic sui **⋯** sotto GTA V.<br>3. Fai clic su **Gestisci (Manage)**.<br>4. Fai clic sull'**icona della cartella** accanto a "Installazione" ("Installation"). |

✅ Tieni aperta questa cartella. Ci copierai dei file più volte.

---

## Passo 2: Installa Script Hook V (permette alle mod di funzionare)

1. Vai alla [pagina di Script Hook V](http://www.dev-c.com/gtav/scripthookv/).
2. Fai clic su **Download**.
3. Estrai lo zip.
4. Apri la cartella **`bin`** al suo interno.
5. Copia questi file nella tua **cartella del gioco**:
   - **`ScriptHookV.dll`**
   - **`dinput8.dll`** (carica le mod che finiscono in `.asi`)
   - **`args.txt`** (disattiva BattlEye, l'anti-cheat di Rockstar, che blocca le mod)
   - **`xinput1_4.dll`**, se c'è

⚠️ **Non** copiare `NativeTrainer.asi`. È un menu di trucchi che non ti serve.

✅ Ora nella cartella del gioco ci sono `ScriptHookV.dll` e `dinput8.dll` accanto a `GTA5.exe`.

---

## Passo 3: Installa Script Hook V .NET (permette a questo HUD di funzionare)

L'HUD richiede la **3.7.0 nightly 193 o più recente**. La release più **in alto** nella pagina delle nightly lo è sempre.
Con una più vecchia, gli script dell'HUD non partono, oppure uno si ferma con un errore (vedi "Qualcosa è andato storto?").

1. Vai alla [pagina delle nightly di Script Hook V .NET](https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases).
2. Trova la release più **in alto**.
3. Apri **Assets**.
4. Scarica il file **`.zip`**.
5. Estrailo.
6. Copia questi file nella tua **cartella del gioco**:
   - **`ScriptHookVDotNet.asi`**
   - **`ScriptHookVDotNet2.dll`**
   - **`ScriptHookVDotNet3.dll`**
   - **`ScriptHookVDotNet.ini`**

✅ Ora quei 4 file sono accanto a `GTA5.exe`.

---

## Passo 4: Installa OpenIV (installa la mod in modo sicuro)

OpenIV mette le mod in una **cartella `mods`** separata. I file originali del gioco non vengono mai modificati.

1. Scarica OpenIV da [openiv.com](https://openiv.com/).
2. Installalo.
3. Apri OpenIV.
4. Scegli **GTA V** → **Windows**.
5. Se te lo chiede, indicagli la tua **cartella del gioco**.
6. In alto, fai clic su **Tools** → **ASI Manager**.
7. Fai clic su **Install** accanto a **ASI Loader**.
8. Fai clic su **Install** accanto a **OpenIV.asi**.
9. Chiudi la finestra.

✅ OpenIV è pronto.

---

## Passo 5: Scarica la mod

1. Vai alla [**pagina Releases**](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest).
2. Sotto **Assets**, scarica **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`** (la mod, circa 100 MB).
   Attiva anche la mod nel gioco. Non c'è nessuna patch separata da scaricare.

❌ Non scaricare `GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`. Quello è per l'altra versione del gioco.

**Vuoi giocare con Lucia o Jason?** I loro modelli non sono inclusi.

| Personaggio | Dove trovare il modello |
| --- | --- |
| **Lucia** | Il suo modello è un lavoro a pagamento di qualcun altro. Prendila dal suo creatore:<br>**[Lucia Caminos su Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)** o **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** su gta5-mods.com.<br>Oppure usa un altro modello di Lucia.<br>Per metterla nel gioco, vedi "Opzionale: Lucia" più sotto.<br>L'HUD funziona anche senza di lei. |
| **Jason** | **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** dallo stesso creatore, oppure **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** su mod-hub.net.<br>Per metterlo nel gioco, vedi "Opzionale: Jason" più sotto. |

💡 Il numero nel nome del file è la versione (qui 2.7.0). Così sai sempre quale hai installato.
Indicalo quando chiedi aiuto.

---

## Passo 6: Installa la mod con OpenIV

⚠️ **Gioco in Program Files?** Il percorso della tua cartella del gioco inizia con `C:\Program Files`?
(Il Rockstar Games Launcher, Epic Games e Steam lo mettono lì.)
Allora fai clic destro su **OpenIV** → **Esegui come amministratore (Run as administrator)**, e fai clic su **Sì (Yes)**.

💡 Windows permette solo agli amministratori di modificare i file lì. Senza, l'installazione può lasciare al loro posto i file vecchi.

1. In OpenIV, fai clic su **Tools** → **Package Installer**.
2. Scegli **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`**.
3. Fai clic su **Install**.
4. Quando ti chiede dove installare, scegli **mods folder** (cartella mods).
5. Se ti propone di creare la cartella, rispondi di sì.
6. Aspetta che dica che ha finito.
7. Fai clic su **Close**.

✅ Reinstallare la mod, o installarla sopra una versione più vecchia, va bene. Non elenca mai un pack due volte.

---

## Passo 7: Controlla un file

Cerca **`ViMinimap.asi`** nella tua **cartella del gioco**. Deve stare proprio accanto a `GTA5.exe`.

| C'è? | Cosa fare |
| --- | --- |
| **Sì** | Perfetto, vai al Passo 8. |
| **No** | 1. Apri la cartella **`mods`** nella cartella del gioco.<br>2. Trova **`ViMinimap.asi`**.<br>3. Spostalo accanto a **`GTA5.exe`**. |

💡 Senza questo file, la minimappa non si vede.

---

## Passo 8: Gioca! 🎉

1. Avvia GTA V.
2. Scegli **Story Mode**.

✅ Il nuovo HUD compare da solo.

| Per... | Fai così |
| --- | --- |
| **Cambiare personaggio** | Tieni premuto **Left Alt** (Alt sinistro; controller: tieni premuto **D-pad down**).<br>Scegli con **Q / E** o **Left / Right** (controller: **LB / RB**).<br>Poi lascia andare.<br>Jason e Lucia richiedono i loro modelli (non inclusi, vedi Passo 5). |
| **Vedere la minimappa** | Sali su un'auto. Si vede solo nei veicoli, o quando sei ricercato. |
| **Aprire le impostazioni dell'HUD** | Tieni premuto **V**. |
| **Ascoltare la tua musica** | Metti dei file `.mp3` in **`scripts\Hud\Radio\CustomMusic`** nella cartella del gioco.<br>In auto, tieni premuto il tasto della radio (**Q**) e premi **Down** (freccia giù; controller: levetta destra in giù) per **On Demand**.<br>Una cartella dentro `CustomMusic` è una stazione a sé.<br>**Left / Right** (levetta destra) cambiano stazione.<br>La **rotellina del mouse** (D-pad Right / Up) cambia canzone. |

---

## Opzionale: schermate di caricamento VI

**Cosa ottieni:** la schermata iniziale e le schermate di caricamento GTA VI di FBICat, con le immagini di Vice City di Rockstar.
**Cosa ti serve:** **`GTA-VI-Inspired-HUD-<version>-LoadingScreens-Legacy.oiv`** dalla stessa release dell'HUD.
L'HUD funziona anche senza: installalo solo se le vuoi.

In OpenIV: **Tools** → **Package Installer**, sceglilo, **Install**, e scegli la **mods folder**, come per l'HUD.

### Le hai già e rivuoi quelle originali del gioco?

Fino alla 2.6.4 le schermate di caricamento erano dentro il pacchetto dell'HUD. Per toglierle:

1. Apri **OpenIV** e fai clic su **Edit mode** (modalità modifica) in alto.
2. Apri, nei file originali del gioco, **`update`** → **`update.rpf`** → `x64` → `data` → `cdimages` (non quello dentro `mods`).
3. Fai clic destro su **`scaleform_frontend.rpf`** → **Extract** (in CodeWalker: **Extract Raw...**) e salvalo sul Desktop.
4. Apri **`mods`** → `update` → **`update.rpf`** → `x64` → `data` → `cdimages`.
5. Trascina lo **`scaleform_frontend.rpf`** dal Desktop in quella finestra, e sostituisci quello che c'è.

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
   Ci sono invece una cartella **`legacy`** e una **`enhanced`** (come in Jason Duval Extended Look)? Apri **`legacy`**.
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

### Parte 3: Mettilo nel gioco

1. Scarica **[GTA-VI-Inspired-HUD-Jason-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Jason-Kit.zip)** ed estrailo.
2. Copia la cartella **`jasonped`** del kit in **`mods\update\x64\dlcpacks`** nella tua cartella del gioco.
3. Apri **OpenIV** e fai clic su **Edit mode** in alto, così è attiva.
4. In OpenIV, apri uno dopo l'altro: `mods` → `update` → `x64` → `dlcpacks` → `jasonped` → **`dlc.rpf`** → `x64`
   → `models` → `cdimages` → **`jasonped.rpf`**.
5. Trascina i tuoi 4 file **`jdfinal`** in quella finestra.
6. In OpenIV, apri: `mods` → `update` → **`update.rpf`** → `common` → `data`. Fai clic destro su **`dlclist.xml`** → **Edit**.
7. Scorri fino in **fondo**. Metti il cursore all'inizio della riga che dice **`</Paths>`**, premi **Enter** (Invio),
   e incolla questo nella nuova riga vuota che compare sopra:
   ```xml
   <Item>dlcpacks:/jasonped/</Item>
   ```
8. Premi **Ctrl + S** per salvare, e chiudi la finestra.
9. Avvia il gioco e tieni premuto **Left Alt** (controller: tieni premuto **D-pad Down**). La scheda di Jason è dopo quella di Trevor.

### Non funziona?

| Cosa vedi | Cosa fare |
| --- | --- |
| Jason non è sulla ruota | Apri **`scripts\ViSwitch.log`** nella cartella del gioco con Blocco note (Notepad). Se dice **"no Jason model in the game"** (nessun modello di Jason nel gioco), c'è uno di questi errori: i tuoi 4 file non si chiamano `jdfinal` (Parte 2), non sono dentro **`jasonped.rpf`**, oppure in `dlclist.xml` manca la riga **`<Item>dlcpacks:/jasonped/</Item>`**. Correggi e riavvia il gioco. |

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

### Parte 3: Mettila nel gioco

1. Scarica **[GTA-VI-Inspired-HUD-Lucia-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Lucia-Kit.zip)** ed estrailo.
2. Copia la cartella **`luciaped`** del tuo pack (**`A - 4 files`** o **`B - with a folder`**) in
   **`mods\update\x64\dlcpacks`** nella tua cartella del gioco.
3. Apri **OpenIV** e fai clic su **Edit mode** in alto, così è attiva.
4. In OpenIV, apri uno dopo l'altro: `mods` → `update` → `x64` → `dlcpacks` → `luciaped` → **`dlc.rpf`** → `x64`
   → `models` → `cdimages` → **il suo archivio**.
5. Trascina i suoi file **`lucia_s`** in quella finestra (pack B: anche la cartella **`lucia_s`**, e **`lucia_s_p`** se
   ce l'hai).
6. In OpenIV, apri: `mods` → `update` → **`update.rpf`** → `common` → `data`. Fai clic destro su **`dlclist.xml`** → **Edit**.
7. Scorri fino in **fondo**. Metti il cursore all'inizio della riga che dice **`</Paths>`**, premi **Enter**,
   e incolla questo nella nuova riga vuota che compare sopra:
   ```xml
   <Item>dlcpacks:/luciaped/</Item>
   ```
8. Premi **Ctrl + S** per salvare, e chiudi la finestra.
9. Avvia il gioco e tieni premuto **Left Alt** (controller: tieni premuto **D-pad Down**). La scheda di Lucia è l'ultima.

### Non funziona?

| Cosa vedi | Cosa fare |
| --- | --- |
| Un messaggio dice **Lucia is not registered** | In `dlclist.xml` manca la riga **`<Item>dlcpacks:/luciaped/</Item>`**. Aggiungila e riavvia il gioco. |
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

## Opzionale: Forests of San Andreas e GTA V Remastered

**Cosa ottieni:** decine di migliaia di alberi, cespugli e oggetti in più sulla mappa.
**Cosa ti serve:** **Forests of San Andreas**, **GTA V Remastered**, o entrambe, di **Larcius** (basate sulle mappe di JRod e le__AK). Funzionano con questo HUD.
Anche loro sono OIV, quindi si installano come l'HUD.

1. Installa prima l'HUD (Passi da 1 a 7).
2. Nello zip di ogni mod, apri la cartella **`GTA V Legacy`**.
3. In OpenIV, fai clic su **Tools** → **Package Installer**.
4. Scegli quella che vuoi:
   - **`Forests of San Andreas (North) Ultimate v….oiv`**
   - **`Forests of San Andreas (South) Ultimate v….oiv`**
   - **`GTA V Remastered Ultimate v….oiv`**
5. Scegli **mods folder**, come nel Passo 6.
6. Ripeti i passi da 3 a 5 per ognuna che vuoi.

   💡 Ognuna aggiunge il suo pack alla lista dei pack del gioco, accanto a quelli dell'HUD. Quindi non c'è niente da modificare.
7. Usi anche **Liberty City V Remix**? Installa anche gli OIV **`Compatibility Liberty City V Remix`** delle mod.
8. ⚠️ **Lascia `gameconfig.xml` com'è.** Le mappe non ne richiedono uno diverso.
   Installare un altro `gameconfig.xml` sostituirebbe quello preparato dall'HUD, che all'HUD serve.
   Crash durante il caricamento con le mappe installate? Vedi "Qualcosa è andato storto?" più sotto.

**Per toglierle di nuovo:** installa gli OIV **`Uninstaller`** delle mod nello stesso modo, nella mods folder.

---

## Opzionale: Rebalanced Dispatch Enhanced

**Cosa ottieni:** **Rebalanced Dispatch Enhanced** (RDE) del **RDE Team**. Sostituisce la risposta della polizia di GTA:
nuove agenzie, posti di blocco, elicotteri, e fino a **10 stelle**. L'HUD si fa da parte.
**Cosa ti serve:** l'HUD già installato, e **`RDE 4.1.5.oiv`** dal download di RDE.

> [!NOTE]
> Novità della 2.6.3, provata solo brevemente nel gioco.
> Qualcosa non va con RDE e l'HUD insieme? Diccelo su Discord o in una segnalazione su GitHub.

### Parte 1: Installa RDE

1. Installa prima l'HUD (Passi da 1 a 7).

   💡 Lo Script Hook V .NET del Passo 3 è la nightly che RDE richiede.
2. In OpenIV, fai clic su **Tools** → **Package Installer**.
3. Scegli **`RDE 4.1.5.oiv`** dal download di RDE.
4. Scegli **mods folder**, come nel Passo 6.

⚠️ Salta i file **`Compatibility - Community HUD`** di RDE. Sono per un'altra mod HUD.

💡 RDE porta il suo `gameconfig.xml`, con limiti molto più alti di quelli che servono all'HUD. Va bene così.
Installare o aggiornare l'HUD dopo alza solo un limite troppo basso. Quindi non annulla mai quelli di RDE.

### Parte 2: Cosa cambia mentre RDE è attivo

- Prende il controllo la polizia di RDE.
- Restano fuori:
  - il cervello della polizia di GTA6Hud
  - le sei stelle proprie di GTA6Hud (e i militari che fanno intervenire)
  - gli arresti con il taser dell'HUD
- Le stelle dell'HUD mostrano il livello di RDE fino a **sei**. I livelli da 6 a 10 accendono tutti la sesta stella.
- Per lasciare l'HUD così com'è, imposta `[Compat] Rde = False` in `ViExtras.ini`.

### Parte 3: Togliere di nuovo RDE

1. Installa nello stesso modo l'**`Uninstaller.oiv`** di RDE.
2. Reinstalla l'HUD (Passo 6), così tornano i suoi limiti e i suoi pack.

---

## Opzionale: Los Santos RED

**Cosa ottieni:** **Los Santos RED** (LSR) di **Greskrendtregk**. Trasforma il gioco in un sandbox criminale.
Ha la sua polizia, i suoi crimini, il suo livello di ricercato e molto altro. L'HUD si fa da parte.
**Cosa ti serve:** **RAGE Plugin Hook** e **RAGENativeUI**. LSR gira in RAGE Plugin Hook, non in Script Hook V .NET.

> [!NOTE]
> Novità della 2.6.3, provata solo brevemente nel gioco.
> Qualcosa non va con LSR e l'HUD insieme? Diccelo su Discord o in una segnalazione su GitHub.

⚠️ Usa LSR da solo. Non insieme a LSPDFR o Rebalanced Dispatch Enhanced: anche loro gestiscono la polizia.

### Parte 1: Installa LSR

1. Installa prima l'HUD (Passi da 1 a 7).
2. Installa **[RAGE Plugin Hook](https://ragepluginhook.net/)**.
3. Installa **[RAGENativeUI](https://github.com/alexguirre/RAGENativeUI/releases)**. Serve ai menu di LSR.
4. Installa LSR stesso, come dice la pagina di LSR.
5. Avvia il gioco con **`RAGEPluginHook.exe`**.

✅ LSR si carica quando la Story Mode ha finito di caricare.

### Parte 2: Cosa cambia mentre LSR è attivo

LSR gestisce la polizia, i crimini e il livello di ricercato. Quindi l'HUD non aggiunge niente di suo:

- niente testimoni
- niente cervello della polizia né memoria della polizia
- niente calore (heat)
- niente sei stelle
- niente arresti con il taser

Per lasciare l'HUD così com'è, imposta `[Compat] Lsr = False` in `ViExtras.ini`.

### Parte 3: Stelle

LSR tiene a zero il livello di ricercato di GTA e disegna stelle tutte sue.
Quindi con LSR le stelle e il riquadro del ricercato dell'HUD non compaiono.

Vuoi le stelle di GTA VI al posto di quelle di LSR?

1. Trova le 4 immagini nella cartella [`extras/lsr-stars/wantedlevel`](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/tree/main/extras/lsr-stars/wantedlevel) dell'HUD.
2. Copiale in **`Plugins\LosSantosRED\images\wantedlevel`**.
3. Sostituisci con queste le immagini di LSR.

### Parte 4: Le impostazioni di LSR

LSR scrive `Plugins\LosSantosRED\Settings.xml` la prima volta che parte.

1. Chiudi il gioco.
2. Apri **`Plugins\LosSantosRED\Settings.xml`** con Blocco note.
3. Cambia questi valori:

| Impostazione | Valore | Perché |
| --- | --- | --- |
| `AlwaysShowRadar` | `false` | La minimappa 3D dell'HUD sostituisce il radar di GTA. Se LSR accende il radar, lo rimette sotto la mappa. |
| `NeverShowRadar` | `true` | Stesso motivo. |
| `TopDisplayShowWeapon` | `false` | L'HUD mostra già la tua arma in alto a destra. |
| `ShowStreetDisplay` | `false` | L'HUD mostra già la via sotto la minimappa. |

### Parte 5: Tasti

LSR usa alcuni dei tasti dell'HUD. Per ogni coppia che usi, cambia il tasto da una delle due parti:

| Tasto | HUD | LSR |
| --- | --- | --- |
| **Left Ctrl** | Accovacciarsi (`ViCrouch.ini` `[Crouch] Key`) | Accovacciarsi (`CrouchKey` nelle impostazioni di LSR) |
| **K** | Pannello abilità (`ViExtras.ini` `[Skills] Key`) | Rispondere "No" nelle conversazioni |
| **O** | Muto della radio (`ViExtras.ini` `[Radio] MuteKey`) | Iniziare un'interazione |
| **Shift + N** | Addominali (`ViExtras.ini` `[Gym] SitUpsKey`) | Modalità gruppo |

---

<a id="optional-lspdfr"></a>
## Opzionale: LSPDFR

**Cosa ottieni:** **LSPDFR** (LSPD First Response) di **G17 Media**. Giochi nei panni di un agente di polizia.
Entri in servizio, rispondi alle chiamate (callout) e fermi i veicoli per i controlli. L'HUD continua a funzionare, e sa quando sei in servizio.
**Cosa ti serve:** l'HUD già installato, e LSPDFR da **[lcpdfr.com](https://www.lcpdfr.com/)**.
LSPDFR gira in RAGE Plugin Hook, non in Script Hook V .NET.

> [!NOTE]
> Provato solo su **Legacy**.

⚠️ Non usare LSPDFR con **Los Santos RED**. Gestiscono entrambi la polizia.

💡 **Rebalanced Dispatch Enhanced** ha i suoi file per LSPDFR, quindi quei due possono andare insieme.
Non abbiamo provato l'HUD con tutti e due insieme.

### Parte 1: Installa LSPDFR

1. Installa prima l'HUD (Passi da 1 a 7).
2. Installa LSPDFR, come dice la pagina di LSPDFR.
3. Avvia il gioco con **`RAGEPluginHook.exe`**.

✅ LSPDFR si carica quando la Story Mode ha finito di caricare.

💡 Vuoi giocare senza LSPDFR? Avvia il gioco nel modo normale. LSPDFR si carica solo tramite `RAGEPluginHook.exe`.

### Parte 2: Cosa fa l'HUD quando sei in servizio

**Non devi cambiare niente.** L'HUD trova LSPDFR da solo. Mentre sei in servizio:

- **Niente cambio di personaggio.** La ruota dei personaggi resta chiusa, e i tasti per cambiare non fanno nulla.
  Cambiare personaggio porterebbe via a LSPDFR il suo agente.
- **Niente testimoni.** Nessuno scappa o telefona per denunciarti. La polizia sei tu.
- **Niente livello di ricercato.** Se ne compare uno comunque, viene azzerato subito.

Quando esci dal servizio, tutto funziona come prima.

💡 Vuoi cambiare personaggio? Prima esci dal servizio.

### Parte 3: Tasti

Tre tasti fanno due cose contemporaneamente. Per ogni coppia che usi, cambia il tasto da una delle due parti:

| Tasto | Lato HUD | Lato LSPDFR |
| --- | --- | --- |
| **Y** | Salutare qualcuno (`ViExtras.ini` `[Memory] GreetKey`) | Accettare una chiamata (`ACCEPT_CALLOUT_Key` in `lspdfr\keys.ini`) |
| **N** | Flessioni (`ViExtras.ini` `[Gym] PushUpsKey`) | Menu inseguimento (`PURSUIT_MENU_Key` in `lspdfr\keys.ini`) |
| **F4** | La console di Script Hook V .NET (`ConsoleKeyBinding` in `ScriptHookVDotNet.ini`) | La console di RAGE Plugin Hook (`ConsoleKey` in `RagePluginHook.ini`) |

Per cambiare un tasto:

1. Chiudi il gioco.
2. Apri il file con Blocco note.
   - `ViExtras.ini` è nella tua cartella **`scripts`**.
   - Gli altri file sono nella tua **cartella del gioco**.
3. Cambia la lettera dopo il `=`. Scegli un tasto che non usa nient'altro.
4. Salva il file.

---

## Qualcosa è andato storto?

**Ancora bloccato?** Chiedi nel **[nostro Discord di supporto](https://discord.gg/C7gE5SHEzH)**, quello per questa versione.
Oppure [apri una segnalazione](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).

| Cosa succede | Cosa fare |
| --- | --- |
| Un messaggio di Script Hook V dice **"unknown game version"** | GTA V si è appena aggiornato. Script Hook V non si è ancora messo al passo.<br>1. Aspetta un nuovo Script Hook V.<br>2. Rifai il Passo 2. |
| Il gioco parte ma **il nuovo HUD non si vede** | Il tuo Script Hook V .NET è troppo vecchio.<br>Rifai il Passo 3 con la versione più recente. |
| Un messaggio dice **Unhandled exception in script** (per esempio **"MiniMap"**) con una **FileNotFoundException** | Il tuo Script Hook V .NET è più vecchio di quello che serve all'HUD (3.7.0 nightly 193).<br>1. Rifai il Passo 3 con la release più **in alto** nella pagina delle nightly.<br>2. Chiudi il gioco.<br>3. Premi **Win + R**.<br>4. Scrivi `%LOCALAPPDATA%\assembly` e premi **Enter**.<br>5. Elimina la cartella **`dl3`** (è solo una cache).<br>💡 Quando il problema è questo, `ScriptHookVDotNet.log` nella cartella del gioco dice **"Unable to resolve API version"**. |
| Hai **aggiornato** la mod, ma il gioco si comporta ancora come la **versione vecchia** | Script Hook V .NET può continuare a usare la sua copia salvata dei vecchi script.<br>1. Chiudi il gioco.<br>2. Premi **Win + R**.<br>3. Scrivi `%LOCALAPPDATA%\assembly` e premi **Enter**.<br>4. Elimina la cartella **`dl3`** (è solo una cache).<br>Ancora la versione vecchia? Reinstallala con OpenIV eseguito come amministratore (Passo 6).<br>Oppure copia tu gli script: vedi "Aggiornare a mano" più sotto. |
| **Niente minimappa** in auto | `ViMinimap.asi` non è accanto a `GTA5.exe`.<br>Rifai il Passo 7. |
| Nella cartella del gioco c'è **`MiniMap.asi`** o **`MiniMapHudPresent.asi`** (da un'altra mod di minimappa 3D o da un'installazione più vecchia) | 1. Eliminalo.<br>2. Elimina anche **`scripts\MiniMap.dll`**, se c'è. (Quello dell'HUD è `scripts\ViMinimap.dll`.)<br>3. Tieni **`ViMinimap.asi`** accanto a `GTA5.exe`.<br>💡 È una copia più vecchia del plugin della minimappa. L'HUD porta la sua copia come `ViMinimap.asi`. Due copie caricate insieme si danno fastidio a vicenda. |
| Il gioco **crasha durante il caricamento** | 1. Reinstalla la mod (Passo 6). Alza i limiti del gioco che servono all'HUD.<br>2. Hai altre mod grandi (personaggi extra, mappe)? Prova a disattivarle. |
| Il gioco **crasha durante il caricamento** da quando hai aggiunto **Forests of San Andreas** o **GTA V Remastered** | I tanti alberi delle mappe possono riempire tre dei pool del gioco.<br>1. In OpenIV, attiva **Edit mode**.<br>2. Apri: `mods` → `update` → `update.rpf` → `common` → `data` → **`gameconfig.xml`**.<br>3. Imposta il `value` di `PoolSize` di **`Building`** a `70000`.<br>4. Imposta **`phInstGta`** a `20000`.<br>5. Imposta **`StaticBounds`** a `20000`.<br>Se un valore è già più alto, lascialo com'è.<br>⚠️ Non installare invece un `gameconfig.xml` diverso. Sostituirebbe quello preparato dall'HUD. (Se l'hai fatto, reinstalla la mod, come nel Passo 6.)<br>Crasha ancora? Aggiungi l'[Heap Adjuster](https://www.gta5-mods.com/tools/heapadjuster), che consiglia anche la guida delle mappe stesse. |
| Un messaggio dice **Lucia is not registered** | Lucia non è più inclusa nell'HUD (vedi Passo 5).<br>**Non ce l'hai?** Imposta `Enabled = False` sotto `[Lucia]` in `scripts\GTA6Hud.ini`. Il messaggio sparisce.<br>**Hai un modello di Lucia?** In `dlclist.xml` manca il suo pack. Aggiungi la sua riga.<br>💡 Se il messaggio parla di un `VIHudSetup.exe`, ignora quella parte. Questa mod non lo usa. |
| **Cambiare personaggio fa crashare il gioco**, e il tuo gioco non è in inglese | I giocatori segnalano che succede solo quando il gioco è impostato in un'altra lingua.<br>Finché non viene risolto, imposta la lingua di GTA V su **inglese (English)** nelle impostazioni di GTA V del tuo launcher. |
| L'HUD dice che il gioco **non può salvare nella sua cartella**, e una **finestra di Windows** chiede di permettere a **icacls** di apportare modifiche | Il tuo gioco è in **Program Files**. Windows permette solo agli amministratori di modificare i file lì. Quindi l'HUD non può salvare le sue impostazioni (e ReShade non può salvare le sue).<br>1. Fai clic su **Sì**. Permette al tuo utente di Windows di modificare la cartella del gioco.<br>(È come farlo a mano in **Proprietà (Properties)** → **Sicurezza (Security)** della cartella.)<br>2. Riavvia il gioco una volta.<br>**Hai fatto clic su No?** Te lo chiede di nuovo la prossima volta che avvii il gioco.<br>Per non ricevere più la domanda, imposta `[GameFolder] FixAccess = False` in `ViExtras.ini`. |
| **Qualcos'altro** | Chiedi nel **[Discord di supporto](https://discord.gg/C7gE5SHEzH)**.<br>Oppure [apri una segnalazione](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues) e allega questi file dalla cartella del gioco:<br>`ScriptHookVDotNet.log`<br>`ViMinimap.log`<br>`scripts\GTA6Hud.perf.log`<br>`scripts\GTA6Hud.error.log` |

---

## Aggiornare a mano (gioco in Program Files)

Il tuo gioco è in **Program Files**? (Il Rockstar Games Launcher e Steam lo mettono lì.)
Windows protegge quella cartella. Un aggiornamento con OpenIV non sempre sostituisce i file vecchi lì.
Esegui OpenIV come amministratore (Passo 6), oppure copia tu i nuovi script.

💡 Dalla 2.6, l'HUD si offre di sistemare la cartella al posto tuo la prima volta che parte: vedi la riga sulla finestra di Windows qui sopra.
Dopo, gli aggiornamenti si installano normalmente.

Un `.oiv` è solo uno zip:

1. Chiudi il gioco.
2. Copia **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`** sul Desktop.
3. Rinomina la copia in **`.zip`**.

   💡 Non vedi la parte `.oiv` del nome? In Esplora file, attiva **Visualizza** → **Mostra** → **Estensioni nomi file**.
4. Apri lo zip → **`content`** → **`scripts`**.
5. Seleziona tutto quello che c'è dentro e copialo.
6. Incollalo nella cartella **`scripts`** della tua cartella del gioco.
   Per esempio: `C:\Program Files\Rockstar Games\Grand Theft Auto V\scripts`.
7. Scegli **Sostituisci i file nella destinazione (Replace the files in the destination)**.
8. Fai clic su **Continua (Continue)** quando Windows chiede il permesso di amministratore.
9. Svuota la cache di Script Hook V .NET, così carica i nuovi file:
   - Premi **Win + R**.
   - Scrivi `%LOCALAPPDATA%\assembly` e premi Enter.
   - Elimina la cartella **`dl3`**.

   💡 È solo una cache. Viene ricreata la prossima volta che avvii il gioco.
10. **Arrivi dalla 2.5.4 o da una versione precedente?** La 2.5.5 aggiunge un pack. Fai anche questo:
    - Copia la cartella **`content`** → **`dlcpacks`** → **`vihud`** dello zip in **`mods\update\x64\dlcpacks`** nella tua cartella del gioco.
    - Installa la mod una volta con OpenIV, come nel Passo 6. Questo attiva il nuovo pack.
11. Avvia il gioco.

✅ Ora `scripts\ViExtras.log` inizia con una riga `==== ViExtras …`. Mostra la versione che hai scaricato.

---

## Come rimuovere la mod

1. Nella tua **cartella del gioco**, elimina **`ViMinimap.asi`**.
2. Apri la cartella **`scripts`**.
3. Elimina questi:
   - **`GTA6Hud.dll`**
   - **`GTA6Hud.ini`**
   - **`ViMinimap.dll`**
   - **`LemonUI.SHVDN3.dll`**
   - i file **`MiniMap.*`**
   - la cartella **`Hud`**
4. Elimina la cartella **`mods`**.

   ⚠️ Così elimini anche tutte le altre mod che hai installato con OpenIV.

✅ Il tuo gioco è tornato normale. Script Hook V e Script Hook V .NET possono restare. Da soli non cambiano
niente.
