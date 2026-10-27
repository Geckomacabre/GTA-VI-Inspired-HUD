# ⚡ Início rápido: a versão bem simples

*Esta é uma tradução. O guia em inglês ([QUICK-START.md](QUICK-START.md)) é sempre o mais atualizado.*

*O GTA VI-Inspired HUD é feito pelo **FBICat**, com recursos da versão para FiveM. Os créditos estão no [README](README.md#-credits).*

Nunca usou mods no GTA V? Faça estes 6 passos na ordem. Cada um tem link para o guia completo, se você quiser mais detalhes:
**[Guia Legacy](INSTALL-LEGACY.pt-BR.md)** · **[Guia Enhanced](INSTALL-ENHANCED.pt-BR.md)**.

> [!IMPORTANT]
> ### 💬 Precisa de ajuda? Entre no nosso Discord de suporte
> Esta versão é um **fork do VI HUD do FBICat** com recursos extras, então ela tem o seu próprio suporte:
> **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**, ou uma [issue no GitHub](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).
> O Discord do FBICat é para o mod original dele, então traga as suas dúvidas sobre esta versão para o nosso. Obrigado!

> ⚠️ Só no Modo História (Story Mode). Nunca leve mods para o GTA Online.

---

## 1. Qual GTA V você tem?

Abra a pasta do seu GTA V (o botão "abrir pasta" / "ver arquivos locais" do launcher).

- Você vê o **`GTA5.exe`** → você tem o **Legacy**.
- Você vê o **`GTA5_Enhanced.exe`** → você tem o **Enhanced**.

Tudo abaixo diz para qual dos dois é cada passo. Não misture: os arquivos errados fazem o jogo dar crash.

## 2. Copie 3 ferramentas para essa pasta

| Ferramenta | Legacy | Enhanced |
| :--- | :--- | :--- |
| **[Script Hook V](http://www.dev-c.com/gtav/scripthookv/)** | da pasta `bin` dele: `ScriptHookV.dll`, `dinput8.dll`, `args.txt` | os mesmos |
| **Script Hook V .NET** | o zip **[nightly](https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases)**: os 4 arquivos `ScriptHookVDotNet` dele | o **[SHVDNE](https://github.com/Chiheb-Bacha/ScriptHookVDotNetEnhanced/releases/latest)** (o do Enhanced!): os 5 arquivos dele |
| **Carregador de mods** | **[OpenIV](https://openiv.com/)**: instale, depois **Tools → ASI Manager** → instale o **ASI Loader** e o **OpenIV.asi** | **[OpenRPF](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced)**: copie o `OpenRPF.asi` e crie uma pasta chamada `mods` |

Usa o **NaturalVision Enhanced**, o **Lively World Expansion**, o **Central Real-Life Traffic / Environment** ou o **AuraV-Lite** no Enhanced? Siga as seções "Opcional" deles no **[guia Enhanced](INSTALL-ENHANCED.pt-BR.md)** (com o NVE, pule o OpenRPF; com o LWE, não copie o `dlclist.xml` dele; com o Central, coloque as linhas do HUD de volta no `dlclist.xml` dele).

## 3. Baixe o mod

Em **[Releases](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest)**, pegue:

- o **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`** *ou* o **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`** (só o seu)
- *(opcional)* A Lucia não vem incluída: o modelo dela é trabalho pago de outra pessoa. Pegue com o criador dela,
  **[Lucia Caminos no Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)** ou **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** no gta5-mods.com (para o Jason, **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** do mesmo criador, ou **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** no mod-hub.net).

## 4. Instale o mod

- **Legacy:** OpenIV → **Tools → Package Installer** → escolha o arquivo → instale na **mods folder** (a pasta mods).
- **Enhanced:** o **[OIV Package Installer](https://www.gta5-mods.com/tools/oiv-package-installer)**
  (`CodeWalker.OIVInstaller.exe`) → arraste o arquivo para dentro dele → escolha a pasta do seu GTA V → **Install**.
- **Jogo na pasta Program Files?** (O caminho dele começa com `C:\Program Files`.) Clique com o botão direito no OpenIV ou no
  `CodeWalker.OIVInstaller.exe` → **Executar como administrador (Run as administrator)** antes, senão a instalação pode deixar arquivos antigos no lugar.
- Não existe mais RegisterPatch: o próprio arquivo do mod ativa os packs dele.

## 5. Uma última conferida

- **Legacy:** confira se o **`ViMinimap.asi`** está ao lado do `GTA5.exe`. Se ele estiver em `mods`, tire ele de lá.
- **Enhanced:** no jogo, **Settings → Graphics → Screen Type → Windowed Borderless** (Configurações → Gráficos → Tipo de tela → Janela sem bordas), senão o minimapa não aparece.

## 6. Jogue 🎉

Inicie o **Story Mode** (Modo História). O HUD aparece sozinho.

- **Segure V:** configurações do HUD
- **Segure Q no carro:** rádio. Aperte **Down** (a seta para baixo, ou o analógico direito para baixo) para **On Demand**: a sua própria música. Coloque
  arquivos `.mp3` em `scripts\Hud\Radio\CustomMusic` antes.
- **Segure Left Alt** (o Alt esquerdo; no controle: **D-pad para baixo**): personagens, a fileira de cards do GTA VI (o Jason e a Lucia também, se você tiver os modelos deles). **Q / E** (no controle: **LB / RB**) escolhe, soltar troca

Todos os controles estão no **[README](README.md#-controls)**.
