# Como instalar: GTA V **Enhanced**

*Esta é uma tradução. O guia em inglês ([INSTALL-ENHANCED.md](INSTALL-ENHANCED.md)) é sempre o mais atualizado.*

Este guia é para a **versão de PC de 2025** do GTA V. Veja qual arquivo a pasta do seu jogo tem:

| A pasta do seu jogo tem | Você tem | Use |
| --- | --- | --- |
| **`GTA5_Enhanced.exe`** | Enhanced | **Este guia** |
| **`GTA5.exe`** | Legacy | O **[guia Legacy](INSTALL-LEGACY.pt-BR.md)** |

Nunca usou mods no GTA? Sem problema. Siga os passos na ordem. Não pule nenhum. Leva uns 20 minutos.

> [!IMPORTANT]
> ### 💬 Precisa de ajuda? Entre no nosso Discord de suporte
> Esta versão é um **fork do VI HUD do FBICat** com recursos extras. Por isso ela tem o seu próprio suporte:
> **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**, ou uma [issue no GitHub](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).
> O Discord do FBICat é para o mod original dele. Traga as suas dúvidas sobre esta versão para o nosso. Obrigado!

> ⚠️ **Mods são só para o Modo História (Story Mode).** Nunca entre no GTA Online com mods instalados. Você pode ser banido.
> (O Script Hook V, uma das ferramentas abaixo, fecha o jogo se você tentar.)

> ⚠️ **O Enhanced usa ferramentas diferentes das do Legacy.** Use só os downloads com link neste guia.
> Ferramentas e mods do Legacy fazem o Enhanced dar crash.

---

## Antes de começar: duas configurações rápidas do Windows

### 1. Mostrar as extensões dos arquivos

Alguns arquivos neste guia terminam em `.asi` ou `.dll`. O Windows esconde esses finais até você ligar esta opção.

1. Abra qualquer pasta.
2. Clique em **Exibir (View)** lá em cima.
3. Clique em **Mostrar (Show)** → **Extensões de nomes de arquivos (File name extensions)**.

### 2. Saber descompactar

A maioria dos downloads são arquivos `.zip` (uma pasta espremida em um arquivo só). Para abrir um:

1. Clique nele com o botão direito.
2. Escolha **Extrair Tudo… (Extract All…)**.
3. Clique em **Extrair (Extract)**.

✅ Uma pasta normal aparece ao lado dele.

---

## Passo 1: Encontre a pasta do seu GTA V Enhanced

É a pasta que tem o **`GTA5_Enhanced.exe`** dentro. Vamos chamar ela de **pasta do jogo**.

Como abrir:

| O seu launcher | Faça isto |
| --- | --- |
| **Rockstar Games Launcher** | Abra as **Configurações (Settings)**.<br>Clique em **Grand Theft Auto V Enhanced** à esquerda.<br>Clique em **Abrir pasta (Open Folder)** (ou **Ver pasta de instalação (View Installation Folder)**). |
| **Steam** | Abra a sua **Biblioteca (Library)**.<br>Clique com o botão direito em **Grand Theft Auto V Enhanced**.<br>Clique em **Gerenciar (Manage)** → **Ver arquivos locais (Browse local files)**. |
| **Epic Games** | Abra a sua **Biblioteca (Library)**.<br>Clique no **⋯** embaixo do GTA V Enhanced.<br>Clique em **Gerenciar (Manage)**.<br>Clique no **ícone de pasta** ao lado de "Instalação" (Installation). |

✅ Deixe essa pasta aberta. Você vai copiar arquivos para ela algumas vezes.

---

## Passo 2: Instale o Script Hook V (permite que os mods rodem)

1. Vá até a [página do Script Hook V](http://www.dev-c.com/gtav/scripthookv/).
2. Clique em **Download**. O mesmo download serve para o Legacy e o Enhanced.
3. Descompacte.
4. Abra a pasta **`bin`** que está dentro.
5. Copie estes arquivos para a sua **pasta do jogo**:
   - **`ScriptHookV.dll`**
   - **`xinput1_4.dll`** (carrega mods que terminam em `.asi` no Enhanced)
   - **`args.txt`** (desliga o BattlEye, o anti-cheat da Rockstar, que bloqueia mods)
   - **`dinput8.dll`** (o carregador de mods do Legacy. O Enhanced não usa ele, então copiar não faz mal nenhum.)
6. ⚠️ **Não** copie o `NativeTrainer.asi`. É um menu de trapaças de que você não precisa.

✅ A sua pasta do jogo agora tem o `ScriptHookV.dll` e o `xinput1_4.dll` ao lado do `GTA5_Enhanced.exe`.

---

## Passo 3: Instale o Script Hook V .NET **Enhanced** (permite que este HUD rode)

⚠️ Este **não** é o Script Hook V .NET normal. O normal faz o Enhanced fechar sozinho,
uns 20 segundos depois que você entra no jogo. Você precisa da edição **Enhanced**, chamada **SHVDNE**.

1. Vá até a [página de releases do SHVDNE](https://github.com/Chiheb-Bacha/ScriptHookVDotNetEnhanced/releases/latest).
2. Em **Assets**, baixe o arquivo **`.zip`**.
3. Descompacte.
4. Copie os **5 arquivos** da pasta principal dele para a sua **pasta do jogo**:
   - **`ScriptHookVDotNet.asi`**
   - **`ScriptHookVDotNet.ini`**
   - **`ScriptHookVDotNet2.dll`**
   - **`ScriptHookVDotNet3.dll`**
   - **`MinHook.x64.dll`**
5. Pule as pastas `Debug`, `Docs` e `Licenses` e o `README.txt`. Você não precisa deles.

O Windows pergunta se quer substituir arquivos? Clique em **Substituir (Replace)**. (Quer dizer que você tinha a versão normal antes.)

✅ Esses 5 arquivos agora estão ao lado do `GTA5_Enhanced.exe`.

---

## Passo 4: Instale o OpenRPF (deixa o mod em uma pasta `mods` segura)

O OpenRPF deixa o jogo carregar mods de uma **pasta `mods`** separada. Os arquivos originais do seu jogo nunca são alterados.

1. Vá até a [página do OpenRPF](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced).
2. Baixe.
3. Descompacte.
4. Copie o **`OpenRPF.asi`** para a sua **pasta do jogo**.
5. ⚠️ O download também tem um `dsound.dll`? **Não copie ele.** O Script Hook V já te deu um carregador
   de mods: o `xinput1_4.dll` do Passo 2. Dois carregadores ao mesmo tempo causam crashes.
6. Na sua pasta do jogo, crie uma pasta nova chamada **`mods`**, se ainda não tiver uma.
   (Botão direito → **Novo (New)** → **Pasta (Folder)**.)

✅ O `OpenRPF.asi` está ao lado do `GTA5_Enhanced.exe`, e existe uma pasta `mods`.

---

## Passo 5: Baixe o mod e o instalador

### Parte 1: Pegue o mod

1. Vá até a [**página de Releases**](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest).
2. Em **Assets**, baixe o **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`**. É o mod, uns 100 MB.
   Ele também ativa o mod no jogo, então não tem nenhum patch separado para baixar.

❌ Não baixe o `GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`. Ele é para a outra versão e vai fazer o Enhanced dar crash.

💡 O número no nome do arquivo é a versão (2.7.0 aqui). Assim você sempre sabe qual instalou.
Diga qual é quando pedir ajuda.

### Quer jogar como a Lucia?

Ela não vem incluída: o modelo dela é trabalho pago de outra pessoa. O HUD funciona sem ela.

Pegue com o criador dela:
- **[Lucia Caminos no Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)**
- **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** no gta5-mods.com

Ou use outro modelo da Lucia. Depois veja "Opcional: Lucia" abaixo para colocar ela no jogo.

**Para o Jason**, pegue um destes:
- **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** do mesmo criador
- **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** no mod-hub.net

Depois veja "Opcional: Jason" abaixo para colocar ele no jogo.

### Parte 2: Pegue o instalador

O OpenIV não instala mods no Enhanced. Então você vai usar o **OIV Package Installer** no lugar dele.

1. Vá até a [página dele no gta5-mods](https://www.gta5-mods.com/tools/oiv-package-installer).
2. Baixe.
3. Descompacte em qualquer lugar, como a sua Área de Trabalho.

---

## Passo 6: Instale o mod

Primeiro, olhe o caminho da sua pasta do jogo. Ele decide como você abre o instalador:

| O caminho da sua pasta do jogo começa com | Como abrir o instalador |
| --- | --- |
| `C:\Program Files` (o Rockstar Games Launcher, a Epic Games e a Steam colocam ele lá) | Clique com o botão direito em **`CodeWalker.OIVInstaller.exe`** → **Executar como administrador (Run as administrator)**.<br>Clique em **Sim (Yes)**. |
| Qualquer outra coisa | Rode o **`CodeWalker.OIVInstaller.exe`**. |

⚠️ **Jogo na pasta Program Files?** O Windows só deixa administradores alterarem arquivos lá. Sem **Executar como administrador**,
a instalação pode deixar os arquivos antigos no lugar.

1. Abra a pasta do OIV Package Installer.
2. Rode o **`CodeWalker.OIVInstaller.exe`** do jeito que a tabela diz.
3. Arraste o **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`** para a janela dele. (Ou clique em **Browse** e escolha ele.)
4. Ele pede a sua pasta do jogo. Escolha a pasta com o **`GTA5_Enhanced.exe`**.
5. Clique em **Install**.
6. Espere até terminar.

✅ Instalar o mod de novo, ou por cima de uma versão mais antiga, não tem problema. Ele nunca coloca um pack duas vezes na lista.

---

## Passo 7: Mude uma configuração do jogo

1. Inicie o jogo.
2. Vá em **Settings** (Configurações) → **Graphics** (Gráficos).
3. Coloque **Screen Type** (Tipo de tela) em **Windowed Borderless** (Janela sem bordas).

💡 No Enhanced, o minimapa é desenhado em uma janelinha por cima do jogo. Essa janela só aparece no
modo Borderless (sem bordas).

---

## Passo 8: Jogue! 🎉

Carregue o **Story Mode** (Modo História). O HUD novo aparece sozinho.

| Para... | Faça isto |
| --- | --- |
| **Trocar de personagem** | Segure **Left Alt** (o Alt esquerdo; no controle: segure o **D-pad para baixo**).<br>Escolha com **Q / E** ou **Left / Right** (as setas; no controle: **LB / RB**).<br>Solte.<br>O Jason e a Lucia precisam dos modelos deles (não incluídos, veja o Passo 5). |
| **Ver o minimapa** | Entre em um carro.<br>Ele só aparece em veículos, ou quando você está sendo procurado. |
| **Abrir as configurações do HUD** | Segure **V**. |
| **Tocar a sua própria música** | Coloque arquivos `.mp3` em **`scripts\Hud\Radio\CustomMusic`** na sua pasta do jogo.<br>No carro, segure o botão do rádio (**Q**).<br>Aperte **Down** (a seta para baixo; no controle: analógico direito para baixo) para **On Demand**.<br>Uma pasta dentro de `CustomMusic` vira uma estação própria.<br>**Left / Right** (analógico direito) trocam de estação.<br>A **roda do mouse** (D-pad para a direita / para cima) troca a música. |

---

## Opcional: telas de carregamento VI

**O que você ganha:** as telas de carregamento de GTA VI do FBICat, com as imagens de Vice City da Rockstar.
**Do que você precisa:** do **`GTA-VI-Inspired-HUD-<version>-LoadingScreens-Enhanced.oiv`** da mesma release do HUD.
O HUD funciona sem ele: instale só se você quiser essas telas.

Arraste ele para o **OIV Package Installer** (`CodeWalker.OIVInstaller.exe`), igual ao HUD.

### Já tem essas telas e quer as do próprio jogo de volta?

Até a 2.6.4, as telas de carregamento vinham dentro do pacote do HUD. Para tirar elas:

1. Abra o **CodeWalker RPF Explorer** e clique em **Edit** → **Edit mode** (modo de edição).
2. Abra o **`update`** do próprio jogo → **`update.rpf`** → `x64` → `data` → `cdimages` (não o que está em `mods`).
3. Clique com o botão direito em **`scaleform_frontend.rpf`** → **Extract** (no CodeWalker: **Extract Raw...**) para a sua Área de Trabalho.
4. Abra **`mods`** → `update` → **`update.rpf`** → `x64` → `data` → `cdimages`.
5. Arraste o **`scaleform_frontend.rpf`** da sua Área de Trabalho para essa janela, e substitua o que está lá.

**Com o NaturalVision Enhanced**, apague também o **`onigiri\platform\data\cdimages\scaleform_frontend.rpf`** se você copiou ele para lá.

---

## Opcional: Jason

**O que você ganha:** o Jason na roda de personagens, depois do Trevor.
**Do que você precisa:** do modelo dele (não vem incluído) e do **kit do Jason**, que é grátis. Siga as partes na ordem.

### Parte 1: Pegue o Jason

1. Baixe o Jason de **um** destes:
   - **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** do criador da Lucia
   - **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** no mod-hub.net
2. Descompacte.
3. Abra a pasta **`No Cap Version`** (ou **`Cap Version`** se você quiser ele de boné).
   Em vez disso, tem uma pasta **`legacy`** e uma **`enhanced`** (como no Jason Duval Extended Look)? Abra a **`enhanced`**.
4. Dentro tem **4 arquivos**. Os nomes deles terminam em **`.ydd`**, **`.yft`**, **`.ymt`** e **`.ytd`**.

> Não consegue ver os finais (`.ydd` e por aí vai)? No **Explorador de Arquivos (File Explorer)**, clique em **Exibir** → **Mostrar** → **Extensões de nomes de arquivos**.

### Parte 2: Confira os nomes dos 4 arquivos

Olhe a parte de cada nome **antes do ponto**:

| Os seus 4 arquivos se chamam | O que fazer |
| --- | --- |
| `jdfinal.ydd`, `jdfinal.yft`, `jdfinal.ymt`, `jdfinal.ytd` | **Nada.** Vá para a Parte 3. |
| Qualquer outra coisa, por exemplo `Jason.ydd`, `Jason.yft`, `Jason.ymt`, `Jason.ytd` | **Renomeie cada arquivo para `jdfinal`**, mantendo o final: `Jason.ydd` vira **`jdfinal.ydd`**, `Jason.yft` vira **`jdfinal.yft`**, `Jason.ymt` vira **`jdfinal.ymt`**, `Jason.ytd` vira **`jdfinal.ytd`**. Não mude mais nada. |

Pronto: o HUD procura por **`jdfinal`**, então quando os seus arquivos tiverem esse nome, ele é encontrado. Você não precisa editar
nenhuma configuração.

### Parte 3: Faça os arquivos dele funcionarem no Enhanced

O Enhanced não consegue usar os arquivos do jeito que eles vêm, então converta eles primeiro.
Pegou eles de uma pasta **`enhanced`** (Parte 1)? Eles já estão convertidos: pule para a Parte 4.

1. Pegue o **CodeWalker RPF Explorer** (versão **30_dev48** ou mais nova) no canal de releases do
   [Discord do dexyfex](https://discord.gg/BxfKHkk).
2. Crie uma pasta nova e vazia na sua Área de Trabalho chamada **`Jason converted`**.
3. No CodeWalker RPF Explorer, clique em **Tools** → **Asset Converter**.
4. Como pasta de **input** (entrada), escolha a pasta com os seus 4 arquivos **`jdfinal`**. Como pasta de **output** (saída), escolha
   a **`Jason converted`**. Clique em **Process**.
5. Abra a **`Jason converted`**. Se o **`jdfinal.ymt`** não estiver nela, copie ele da pasta de input você mesmo.

### Parte 4: Coloque ele no jogo

1. Baixe o **[GTA-VI-Inspired-HUD-Jason-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Jason-Kit.zip)** e descompacte.
2. Copie a pasta **`jasonped`** do kit para a sua pasta do jogo:
   - **Com o NaturalVision Enhanced:** para **`onigiri\dlcpacks`**
   - **Sem ele:** para **`mods\update\x64\dlcpacks`**
3. No CodeWalker RPF Explorer, clique em **Edit** → **Edit mode**, para ligar.
4. Abra a pasta **`jasonped`** que você acabou de copiar, e depois: **`dlc.rpf`** → `x64` → `models` → `cdimages` →
   **`jasonped.rpf`**.
5. Arraste os 4 arquivos da **`Jason converted`** para essa janela.
6. Abra o seu **`dlclist.xml`**:
   - **Com o NaturalVision Enhanced:** abra o **`onigiri\common\data\dlclist.xml`** com o **Bloco de Notas (Notepad)**.
   - **Sem ele:** no CodeWalker RPF Explorer, abra `mods` → `update` → **`update.rpf`** → `common` → `data` →
     **`dlclist.xml`**.
7. Role até o **finalzinho**. Coloque o cursor no começo da linha que diz **`</Paths>`**, aperte **Enter**,
   e cole isto na nova linha vazia acima dela:
   ```xml
   <Item>dlcpacks:/jasonped/</Item>
   ```
8. Aperte **Ctrl + S** para salvar, e feche.
9. Inicie o jogo e segure **Left Alt** (no controle: segure o **D-pad para baixo**). O card do Jason vem depois do card do Trevor.

### Não funcionou?

| O que você vê | O que fazer |
| --- | --- |
| O Jason não está na roda | Abra o **`scripts\ViSwitch.log`** na sua pasta do jogo com o Bloco de Notas. Se ele disser **"no Jason model in the game"** (nenhum modelo do Jason no jogo), uma destas coisas está errada: os seus 4 arquivos não se chamam `jdfinal` (Parte 2), eles não estão dentro do **`jasonped.rpf`**, ou falta a linha **`<Item>dlcpacks:/jasonped/</Item>`** no `dlclist.xml`. Corrija e inicie o jogo de novo. |

### Outros modelos do Jason (avançado)

Você só precisa disto se o seu download do Jason não for os 4 arquivos acima.

- **Muitos arquivos** com um `^` no nome (como `Jason^head_000_r.ydd`, `Jason^uppr_000_u.ydd`): renomeie a parte
  **antes do `^`** para `jdfinal` em **todos** os arquivos (`Jason^head_000_r.ydd` vira `jdfinal^head_000_r.ydd`), e
  nos arquivos sem `^` também (`Jason.ymt` vira `jdfinal.ymt`). Coloque todos no `jasonped.rpf`.
- **Ele vem como um pack próprio** (tem o seu próprio `dlc.rpf`): você não precisa do kit. Instale esse pack do jeito que o
  readme dele diz. Depois abra o **`scripts\GTA6Hud.ini`** com o Bloco de Notas, encontre **`[Jason]`**, e troque **`Model = jdfinal`**
  pelo nome do ped dele (o nome do arquivo `.ydd` dele, antes do ponto).

---

## Opcional: Lucia

**O que você ganha:** a Lucia na roda de personagens, o último card.
**Do que você precisa:** do modelo dela (não vem incluído) e do **kit da Lucia**, que é grátis. Siga as partes na ordem.

💡 Já tem uma pasta **`luciaped`** na sua pasta dlcpacks? Então uma Lucia já está instalada: pule esta parte.

### Parte 1: Pegue a Lucia

1. Baixe a Lucia de **um** destes, ou use outro modelo da Lucia:
   - **[Lucia Caminos no Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)** do criador dela
   - **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** no gta5-mods.com
2. Descompacte, e encontre a pasta com os arquivos dela. Os nomes deles terminam em `.yft`, `.ymt` e por aí vai.

> Não consegue ver os finais (`.yft` e por aí vai)? No Explorador de Arquivos, clique em **Exibir** → **Mostrar** → **Extensões de nomes de arquivos**.

### Parte 2: Qual pack, e os nomes dos arquivos dela

O kit tem dois packs, **A** e **B**. Veja o que tem na pasta dela:

| A pasta dela tem | Pack | O que fazer |
| --- | --- | --- |
| **4 arquivos**, terminando em `.ydd`, `.yft`, `.ymt` e `.ytd` | **A** (`A - 4 files`) | Já se chamam `lucia_s`? **Nada.** Se chamam outra coisa, por exemplo `Lucia.ydd`? **Renomeie cada arquivo para `lucia_s`**, mantendo o final: `Lucia.ydd` vira **`lucia_s.ydd`**, e o mesmo para `.yft`, `.ymt` e `.ytd`. Não mude mais nada. |
| Um **`.yft`**, um **`.ymt`** e uma **pasta** com o mesmo nome (cheia de arquivos como `head_000_r.ydd`) | **B** (`B - with a folder`) | Já se chamam `lucia_s`? **Nada.** Se chamam outra coisa, por exemplo `Lucia`? **Renomeie os três para `lucia_s`**: `Lucia.yft` vira **`lucia_s.yft`**, `Lucia.ymt` vira **`lucia_s.ymt`** e a pasta `Lucia` vira **`lucia_s`**. Não renomeie os arquivos dentro da pasta. Tem uma segunda pasta terminando em `_p` (os chapéus e óculos dela)? Renomeie ela para **`lucia_s_p`**. |

Pronto: o HUD procura por **`lucia_s`**, então quando os arquivos dela tiverem esse nome, ela é encontrada. Você não precisa editar
nenhuma configuração.

Abaixo, **o RPF dela** é o **`luciaped.rpf`** no pack A, e o **`luciaped_streamed.rpf`** no pack B.

### Parte 3: Faça os arquivos dela funcionarem no Enhanced

O Enhanced não consegue usar os arquivos do jeito que eles vêm, então converta eles primeiro.
O download dela tem uma pasta **`enhanced`**? Esses arquivos já estão convertidos: use eles e pule para a Parte 4.

1. Pegue o **CodeWalker RPF Explorer** (versão **30_dev48** ou mais nova) no canal de releases do
   [Discord do dexyfex](https://discord.gg/BxfKHkk).
2. Crie uma pasta nova e vazia na sua Área de Trabalho chamada **`Lucia converted`**.
3. No CodeWalker RPF Explorer, clique em **Tools** → **Asset Converter**.
4. Como pasta de **input** (entrada), escolha a pasta com os arquivos **`lucia_s`** dela. Como pasta de **output** (saída), escolha
   a **`Lucia converted`**. Clique em **Process**.
5. **Só no pack B:** crie uma pasta vazia chamada **`lucia_s`** dentro da **`Lucia converted`**. Rode o Asset Converter
   de novo, com a **pasta `lucia_s`** dela como input e essa nova pasta **`lucia_s`** como output. Tem uma
   pasta **`lucia_s_p`**? Faça o mesmo com ela.
6. Abra a **`Lucia converted`**. Se o **`lucia_s.ymt`** não estiver nela, copie ele da pasta de input você mesmo.

### Parte 4: Coloque ela no jogo

1. Baixe o **[GTA-VI-Inspired-HUD-Lucia-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Lucia-Kit.zip)** e descompacte.
2. Copie a pasta **`luciaped`** do seu pack (**`A - 4 files`** ou **`B - with a folder`**) para a sua pasta do jogo:
   - **Com o NaturalVision Enhanced:** para **`onigiri\dlcpacks`**
   - **Sem ele:** para **`mods\update\x64\dlcpacks`**
3. No CodeWalker RPF Explorer, clique em **Edit** → **Edit mode**, para ligar.
4. Abra a pasta **`luciaped`** que você acabou de copiar, e depois: **`dlc.rpf`** → `x64` → `models` → `cdimages` →
   **o RPF dela**.
5. Arraste tudo o que está na **`Lucia converted`** para essa janela.
6. Abra o seu **`dlclist.xml`**:
   - **Com o NaturalVision Enhanced:** abra o **`onigiri\common\data\dlclist.xml`** com o **Bloco de Notas**.
   - **Sem ele:** no CodeWalker RPF Explorer, abra `mods` → `update` → **`update.rpf`** → `common` → `data` →
     **`dlclist.xml`**.
7. Role até o **finalzinho**. Coloque o cursor no começo da linha que diz **`</Paths>`**, aperte **Enter**,
   e cole isto na nova linha vazia acima dela:
   ```xml
   <Item>dlcpacks:/luciaped/</Item>
   ```
8. Aperte **Ctrl + S** para salvar, e feche.
9. Inicie o jogo e segure **Left Alt** (no controle: segure o **D-pad para baixo**). O card da Lucia é o último.

### Não funcionou?

| O que você vê | O que fazer |
| --- | --- |
| Uma mensagem diz **Lucia is not registered** (a Lucia não está registrada) | Falta a linha **`<Item>dlcpacks:/luciaped/</Item>`** no `dlclist.xml` (com o **NaturalVision Enhanced**, no `onigiri\common\data\dlclist.xml` do NVE). Adicione e inicie o jogo de novo. |
| A Lucia não está na roda | Abra o **`scripts\GTA6Hud.perf.log`** na sua pasta do jogo com o Bloco de Notas. Se ele disser **"lucia: no Lucia model in the game"**, uma destas coisas está errada: os arquivos dela não se chamam `lucia_s` (Parte 2), eles não estão dentro do **RPF dela**, você usou o outro pack (4 arquivos precisam do **A**, arquivos com uma pasta precisam do **B**), ou falta a linha **`<Item>dlcpacks:/luciaped/</Item>`** no `dlclist.xml`. Corrija e inicie o jogo de novo. |
| A Lucia está invisível, ou só aparece em parte | É o pack errado para os arquivos dela (Parte 2). Apague a pasta `luciaped` que você copiou e comece de novo com o outro pack. |

### Outros modelos da Lucia (avançado)

Você só precisa disto se o seu download da Lucia não for de nenhum dos dois tipos da Parte 2.

- **Muitos arquivos** com um `^` no nome (como `Lucia^head_000_r.ydd`): são as partes de uma Lucia de pack **B**,
  organizadas para o FiveM. Crie uma pasta chamada **`lucia_s`**, mova todos os arquivos com um `^` para ela, e renomeie cada um para a
  parte **depois** do `^` (`Lucia^head_000_r.ydd` vira `head_000_r.ydd`). Renomeie os arquivos sem `^` para
  `lucia_s` (`Lucia.yft` vira `lucia_s.yft`), e depois use o pack B. Os arquivos com `_p^` (os chapéus e óculos dela) vão do mesmo
  jeito para uma pasta chamada **`lucia_s_p`**.
- **Ela vem como um pack próprio** (tem o seu próprio `dlc.rpf`): você não precisa do kit. Instale esse pack do jeito que o
  readme dele diz. Depois abra o **`scripts\GTA6Hud.ini`** com o Bloco de Notas, encontre **`[Lucia]`**, e troque **`Model = lucia_s`**
  pelo nome do ped dela (o nome do arquivo `.yft` dela, antes do ponto).
- **Para manter o nome original dela** em vez de renomear os arquivos: o `README.txt` do kit mostra como, com o `peds.meta` do pack.

---

## Opcional: NaturalVision Enhanced

**O que você ganha:** o **NaturalVision Enhanced** (NVE), do **Razed**, funcionando com este HUD.
**Do que você precisa:** do zip do NVE. O NVE carrega mods com o seu próprio carregador (onigiri), então alguns passos são diferentes.

1. Faça os Passos 1 a 3 deste guia (Script Hook V e SHVDNE).
   - ⚠️ **Pule o Passo 4: não instale o OpenRPF.** O NVE avisa sobre o `OpenRPF.asi` quando o jogo inicia.
   - Já copiou o `OpenRPF.asi`? Apague ele da sua pasta do jogo.
2. Instale o NVE:
   1. Abra o zip dele.
   2. Entre na pasta **`Main Package`**.
   3. Copie tudo o que tem dentro dela para a sua **pasta do jogo**. (O `README Install Guide.html` do próprio NVE tem os detalhes.)
   4. O Windows pergunta se quer substituir o **`xinput1_4.dll`**? Clique em **Substituir**. É o mesmo arquivo que o Script Hook V usa.
3. Instale o HUD com os Passos 5 e 6.
4. Copie os packs do HUD para **`onigiri\dlcpacks`**:
   1. Na sua pasta do jogo, abra **`mods\update\x64\dlcpacks`**. O instalador coloca os packs do HUD aqui.
      O carregador do NVE não lê essa pasta.
   2. Copie as pastas **`vihud`**, **`vihudfonts`** e **`vihudanims`**.
   3. Cole elas em **`onigiri\dlcpacks`**.
5. O NVE traz o seu próprio **`dlclist.xml`**, e o jogo usa esse. Adicione os packs do HUD nele:
   1. Abra o **`onigiri\common\data\dlclist.xml`** (o Bloco de Notas serve).
   2. Role até o **final**.
   3. Adicione estas linhas logo acima do **`</Paths>`** de fechamento, cada uma na sua própria linha:
   ```xml
   <Item>dlcpacks:/vihud/</Item>
   <Item>dlcpacks:/vihudfonts/</Item>
   <Item>dlcpacks:/vihudanims/</Item>
   ```
   - ⚠️ Não embaixo do `<Paths>` lá no topo: o jogo pula as linhas que ficam lá.
   - Tem um modelo da Lucia? Adicione as linhas do pack dela também.
   - Deixe o `gameconfig.xml` do NVE como está.
6. **As telas de carregamento.** Você pode pular isto: aí você fica com as telas de carregamento do próprio jogo. O instalador colocou as
   telas de carregamento do HUD no **`mods\update\update.rpf`**. O carregador do NVE também não lê esse, então copie elas para fora:
   1. Pegue o **CodeWalker RPF Explorer** (versão **30_dev48** ou mais nova) no canal de releases do
      [Discord do dexyfex](https://discord.gg/BxfKHkk).
   2. Na sua pasta do jogo, abra **`onigiri\platform\data`** e crie uma pasta nova lá chamada **`cdimages`**.
   3. Abra o **CodeWalker RPF Explorer** e entre em: `mods` → `update` → **`update.rpf`** → `x64` → `data` →
      `cdimages`.
   4. Clique com o botão direito em **`scaleform_frontend.rpf`** → **Extract Raw...**. Escolha a pasta **`onigiri\platform\data\cdimages`**
      que você acabou de criar, e clique em **Salvar (Save)**.

   💡 Esse arquivo é uma cópia das telas de menu do jogo com as telas de carregamento do HUD dentro.
   **Depois de uma atualização do jogo,** apague ele, instale o HUD de novo, e faça este passo de novo.
   Assim você fica com as telas de menu do jogo atualizado.
7. Faça o Passo 7 (Windowed Borderless), e jogue.

### Removendo o NVE (e mantendo o HUD)

**O que você ganha:** o jogo sem o NVE, e o HUD ainda funcionando.

⚠️ **O Lively World Expansion e o Central Real-Life Traffic / Environment ("for NVE")** carregam pelo NVE, então eles saem
também.

Você vai mover os arquivos do NVE para uma pasta em vez de apagar eles. Assim você pode colocar o NVE de volta depois.

1. Feche o jogo.
2. Crie uma pasta nova na sua Área de Trabalho chamada **`NVE removed`**.
3. Da sua **pasta do jogo**, mova estes para a **`NVE removed`**:
   - **`NVE.asi`**, **`NVE.bin`** e **`NVE.ini`**
   - **`onigiri.asi`**, **`onigiri.ini`** e a pasta **`onigiri`**
     (ela também guarda o LWE, o Central e as suas cópias dos packs do HUD. O HUD ainda tem os dele em `mods`.)
   - **`ReShade.asi`**, **`ReShade.ini`**, **`ReShade2.ini`** e a pasta **`reshade-shaders`**
   - qualquer arquivo **`renodx-*.addon64`** (tirou eles da pasta por causa do AuraV? Mova esses para a `NVE removed` também)
   - **Tinha o Central?** Também o **`CentralLimits.asi`**, o **`CentralLimits.ini`** e o **`CentralDlcPatch.asi`**, e
     o **`ResourceExtensions.asi`** e o **`ResourceExtensions.ini`** se você só pegou eles por causa do Central.
4. ⚠️ **Mantenha o `xinput1_4.dll`.** O NVE substituiu ele, mas é o mesmo arquivo que o Script Hook V usa para carregar mods.
5. Faça o **Passo 4** (instalar o OpenRPF). O NVE precisava que ele não estivesse lá. Sem o NVE, o HUD carrega da pasta `mods` pelo
   OpenRPF.
6. Confira se o HUD ainda está na pasta `mods`: abra **`mods\update\x64\dlcpacks`**. Você deve ver **`vihud`**,
   **`vihudfonts`** e **`vihudanims`**.
   - Não estão lá? Faça o **Passo 6** de novo (instalar o HUD).
7. **Tinha a Lucia ou o Jason?** Os packs deles estavam em `onigiri\dlcpacks`, que você acabou de mover.
   1. Copie **`luciaped`** / **`jasonped`** de **`NVE removed\onigiri\dlcpacks`** para **`mods\update\x64\dlcpacks`**.
   2. Adicione as linhas deles ao `dlclist.xml` em `mods` → `update` → **`update.rpf`** → `common` → `data`
      (Parte 4 de "Opcional: Jason" / "Opcional: Lucia", do jeito **Sem ele**).
8. Inicie o jogo.

✅ O HUD aparece, com o visual do próprio jogo e as telas de carregamento do HUD.

| O que você vê | O que fazer |
| --- | --- |
| Uma mensagem diz **VI UI pack (vihud) is not registered** (o pack não está registrado) | Os packs do HUD não estão na lista da pasta `mods`. Faça o **Passo 6** de novo (instalar o HUD). |
| O jogo **fecha sozinho** ou dá crash ao carregar | Confira se você manteve o **`xinput1_4.dll`** e fez o **Passo 4** (OpenRPF). Depois confira o `asiloader.log` na sua pasta do jogo: ele não deve mais listar o `NVE.asi` nem o `onigiri.asi`. |

**Para colocar o NVE de volta:** mova tudo da **`NVE removed`** de volta para a sua pasta do jogo, e apague o **`OpenRPF.asi`**.

---

## Opcional: Lively World Expansion (com o NaturalVision Enhanced)

**O que você ganha:** o **Lively World Expansion** (LWE). Ele enche o mapa de mais plantas e objetos.
**Do que você precisa:** do NVE e do HUD configurados primeiro. O pacote NVE do LWE também carrega pelo onigiri do NVE.

O zip do LWE vem com o seu próprio **`dlclist.xml`**. Esse arquivo **substitui** o que você já tem.
A cópia do LWE deixa de fora:
- os packs do HUD
- três dos packs mais novos do NVE (`NVE_lights`, `NVE_vegetation`, `NVE_vehicles_addon`)
- a atualização de 2026 do GTA

Então, em vez disso, adicione os packs do LWE à sua lista:

1. Configure o NVE e o HUD primeiro ("Opcional: NaturalVision Enhanced" acima).
2. Copie os packs do LWE:
   1. Abra o zip do LWE.
   2. Escolha uma pasta de densidade: **`LWE Enhanced - High Density`**, **`Medium Density`** ou **`Low Density`**.
   3. Entre na pasta **`onigiri\dlcpacks`** dela.
   4. Copie as **5 pastas** que estão lá para **`onigiri\dlcpacks`** na sua pasta do jogo.
3. ⚠️ **Não copie o `onigiri\common\data\dlclist.xml` do LWE.** Use a sua própria lista:
   1. Abra o seu próprio **`onigiri\common\data\dlclist.xml`**.
   2. Role até o final.
   3. Adicione estas linhas logo acima do **`</Paths>`** de fechamento (não embaixo do `<Paths>` lá no topo), cada uma na sua própria linha.
      Estas são para a densidade High (alta):
   ```xml
   <Item>dlcpacks:/lwecityhigh/</Item>
   <Item>dlcpacks:/lwedeserthigh/</Item>
   <Item>dlcpacks:/lweforesthigh/</Item>
   <Item>dlcpacks:/lwemountainshigh/</Item>
   <Item>dlcpacks:/lweprops/</Item>
   ```
   Para Medium ou Low, troque `high` por `medium` ou `low` nas 4 primeiras linhas. O `lweprops` continua igual.
4. **Já copiou o `dlclist.xml` do LWE por cima do seu?** Coloque o do NVE de volta:
   1. Do zip do NVE, copie o **`Main Package\onigiri\common\data\dlclist.xml`** para `onigiri\common\data` na sua
      pasta do jogo.
   2. Adicione as linhas do HUD (passo 5 da seção do NVE).
   3. Adicione as linhas do LWE (passo 3 aqui).
5. Deixe o `gameconfig.xml` do NVE como está, e jogue.

A [página de instalação](https://livelyworldexpansion.com/installation/) do LWE tem as suas próprias dicas de gráficos.

---

## Opcional: Central Real-Life Traffic e Environment (com o NaturalVision Enhanced)

**O que você ganha:** o **Central Real-Life Traffic** e o **Central Real-Life Environment**, da **CentralMods**.
Eles enchem Los Santos de carros reais e lugares reais.
**Do que você precisa:** do NVE e do HUD configurados primeiro, e das versões **"Enhanced (for NVE)"** do Central.
Elas carregam pelo onigiri do NVE.

Eles funcionam com este HUD. Mas a pasta **`Main Files`** de cada um vem com o seu próprio **`dlclist.xml`**. Ele **substitui**
o seu, e essa lista deixa de fora os packs do HUD. Então coloque eles de volta depois que o Central estiver instalado:

1. Configure o NVE e o HUD primeiro ("Opcional: NaturalVision Enhanced" acima).
2. **Para o Traffic:** instale o [ResourceExtensions](https://www.gta5-mods.com/scripts/resource-extensions), do **Koukol**:
   - Copie o **`ResourceExtensions.asi`** e o **`ResourceExtensions.ini`** para a sua pasta do jogo.
   - ⚠️ O Central Traffic precisa dele. Sem ele, o jogo dá crash logo depois de carregar.
3. Instale o Central do jeito que o ReadMe dele diz:
   1. Copie tudo o que está na pasta **`Main Files`** dele para a sua pasta do jogo.
   2. Clique em **Substituir**.
   3. Vai usar os dois? Faça o mesmo com o outro mod do Central.
4. **Usa o Lively World Expansion também?**
   1. Abra a pasta **`Optional`** do Central.
   2. Abra a **`LWE Compatibility`**. Ela é a `2. LWE Compatibility` no Traffic e a `1. LWE Compatibility` no
      Environment. As duas são iguais.
   3. Abra a pasta da sua densidade do LWE: **`High density`**, **`Medium density`** ou **`Low density`**.
   4. Copie a pasta `onigiri` de lá para a sua pasta do jogo.
   5. Clique em **Substituir**.
5. Coloque as linhas do HUD de volta:
   1. Abra o **`onigiri\common\data\dlclist.xml`**.
   2. Role até o final.
   3. Adicione estas linhas logo acima do **`</Paths>`** de fechamento (não embaixo do `<Paths>` lá no topo), cada uma na sua própria linha:
   ```xml
   <Item>dlcpacks:/vihud/</Item>
   <Item>dlcpacks:/vihudfonts/</Item>
   <Item>dlcpacks:/vihudanims/</Item>
   ```
   Tinha outros packs na sua lista, como o Forests of San Andreas, o GTA V Remastered ou um modelo da Lucia? Coloque as linhas deles de volta também.
6. **Mantenha o `gameconfig.xml` que o Central Traffic colocou em `onigiri\common\data`.**
   - É o do próprio jogo. O ResourceExtensions aumenta os limites no lugar dele.
   - Não coloque o do NVE de volta.
   - Não aumente nenhum tamanho de pool nele.

Bom saber:
- **Testado** com o Central Real-Life Traffic 2.9.1 e o Environment 0.2 (os dois "Enhanced (for NVE)"), o NVE, o Lively World
  Expansion High e o HUD. O Modo História carrega com todos os scripts do HUD rodando.
- **Vai instalar os dois?** Eles vêm com o mesmo `dlclist.xml`, então a ordem não importa. Adicione as linhas do HUD uma vez só,
  depois do segundo.
- **Só vai usar o Environment?** A lista dele também deixa de fora os packs de carros do NVE, porque o Central Traffic substitui esses carros.
  Para manter os carros do NVE, coloque de volta também o `<Item>dlcpacks:/NVE_vehicles_addon/</Item>` e o `<Item>dlcpacks:/NVE_vehicles/</Item>`
  na lista.
- **Para remover eles**, siga o ReadMe do Central. Se você colocar o `dlclist.xml` do NVE de volta, adicione as linhas do HUD nele de novo.

---

## Opcional: Forests of San Andreas e GTA V Remastered

**O que você ganha:** o **Forests of San Andreas** e o **GTA V Remastered**, do **Larcius** (feitos a partir dos mapas do JRod e do le__AK).
Eles enchem o mapa com dezenas de milhares de árvores, arbustos e objetos extras. Eles funcionam com este HUD.
**Do que você precisa:** do HUD instalado primeiro.

O zip de cada mod tem uma pasta **`GTA V Enhanced`** com os packs dele. Cada pack é uma pasta com um `dlc.rpf` dentro:

| Pasta do pack | De onde vem |
| --- | --- |
| **`forest_n`** | Forests (norte) |
| **`forest_s`** | Forests (sul) |
| **`vremastered`** | Remastered |

Instale o HUD primeiro. Depois escolha a sua configuração abaixo.

### Com o NaturalVision Enhanced (sem OpenRPF, sem ferramentas extras)

1. Copie as pastas de pack que você quiser (`forest_n`, `forest_s`, `vremastered`) para **`onigiri\dlcpacks`**.
2. Abra o **`onigiri\common\data\dlclist.xml`**.
3. Adicione uma linha para cada uma logo acima do `</Paths>`:
   ```xml
   <Item>dlcpacks:/forest_n/</Item>
   <Item>dlcpacks:/forest_s/</Item>
   <Item>dlcpacks:/vremastered/</Item>
   ```
4. Deixe o `gameconfig.xml` do NVE como está.

### Sem o NVE (com o OpenRPF, Passo 4)

1. Copie as pastas de pack que você quiser para **`mods\update\x64\dlcpacks`** na sua pasta do jogo, ao lado das do HUD.
2. Pegue o **CodeWalker RPF Explorer** (versão 30_dev48 ou mais nova, no canal de releases do
   [Discord do dexyfex](https://discord.gg/BxfKHkk)).
   Você precisa dele porque as linhas deles vão no `mods\update\update.rpf\common\data\dlclist.xml`, que fica dentro de um arquivo RPF.
3. No CodeWalker RPF Explorer, clique em **File** → **Configure Game...**.
4. Escolha a sua pasta do jogo.
5. Abra `mods\update\update.rpf` → `common` → `data` → **`dlclist.xml`**.
6. Adicione as mesmas linhas de cima, logo acima do `</Paths>`.
7. Salve.

Bom saber:
- **Escolha estes ou o Lively World Expansion, não os dois.** Eles plantam árvores nas mesmas áreas rurais, então você ficaria com árvores
  dentro de árvores. O Forests e o Remastered foram feitos para funcionar juntos.
- **Reflexos:** no Enhanced, os mods não conseguem trazer o shader de reflexo deles. Então pintura de carro, poças e ruas molhadas podem
  mostrar caixas em volta de árvores e arbustos. Isso não acontece se **Ray Traced Reflections** estiver em **Very High** ou **Ultra**.
- **Para remover eles:** tire as linhas deles do `dlclist.xml` e apague as pastas dos packs.

---

## Opcional: AuraV-Lite (path tracing)

**O que você ganha:** path tracing no GTA V Enhanced, com o [AuraV-Lite](https://kyposmods.com), do **Kypos**.
Ele funciona junto com este HUD.
**Do que você precisa:** do AuraV-Lite. Baixe no site do Kypos.

⚠️ **Usa o NaturalVision Enhanced?** Siga "Opcional: AuraV-Lite com o NaturalVision Enhanced" abaixo no lugar desta seção.

1. Copie o **`AuraV.asi`**, o **`msdmo.dll`** e o **`nvngx_dlssd.dll`** para a sua **pasta do jogo**.
2. Copie a pasta **`AuraV`** para a sua pasta do jogo também.
3. No jogo, vá em **Settings** → **Graphics**.
4. Ligue o **Ray Tracing** e o **NVIDIA DLSS**. O path tracing precisa dos dois.
5. **Usa o RenoDX** (add-ons do ReShade com nomes `renodx-*.addon64`)? Abrir o menu do AuraV dá crash no jogo enquanto
   eles estiverem na sua pasta do jogo. Tire os arquivos `renodx-*.addon64` da pasta do jogo enquanto você usar o AuraV.
6. **Tire a tecla do menu do AuraV do F10.** O F10 também é a tecla do próprio HUD para recarregar as configurações dele.
   1. Abra o menu do AuraV uma vez com o **F10**.
   2. Mude **"The key that opens and closes this menu"** (a tecla que abre e fecha este menu) para outra tecla, como o **F11**.

Bom saber:
- Não deixe o **DLSS Swapper** substituir os arquivos de DLSS do GTA. O path tracing pode congelar.
- Vá com calma nas configurações mais pesadas em uma placa de vídeo mais fraca.
- Se algo der errado, o `AuraV\AuraV.log` diz o porquê.

**Para remover:** apague esses 3 arquivos e a pasta `AuraV`.

---

## Opcional: AuraV-Lite com o NaturalVision Enhanced

**O que você ganha:** o AuraV-Lite e o **NaturalVision Enhanced** funcionando juntos com este HUD.
**Do que você precisa:** do NVE e do HUD configurados, e do download do AuraV.

Duas coisas atrapalham:
- Os atalhos de inicialização do AuraV dão crash no carregador do NVE. (O NVE já pula as telas de inicialização do jogo sozinho.)
- O NVE vem com os add-ons de ReShade do RenoDX, que dão crash no menu do AuraV.

Configure nesta ordem:

1. Configure o NVE e o HUD primeiro ("Opcional: NaturalVision Enhanced" acima).
2. Inicie o jogo uma vez para conferir se o HUD aparece.
3. Copie o **`AuraV.asi`**, o **`msdmo.dll`** e o **`nvngx_dlssd.dll`** do download do AuraV para a sua **pasta do jogo**,
   ao lado do `NVE.asi` e do `onigiri.asi`.
4. Copie a pasta **`AuraV`** para a sua pasta do jogo também. ⚠️ Nada do AuraV vai no `onigiri`.
5. **Antes de iniciar o jogo**, crie um arquivo de texto chamado **`AuraV.ini`** na pasta **`AuraV`**, só com
   estas linhas:
   ```ini
   [boot]
   skip_legal=0
   cut_logos=0
   skip_menu=0
   story=0

   [AuraV]
   Key=F11
   ```
   - As linhas de `[boot]` desligam os atalhos de inicialização do AuraV, para o carregador do NVE não dar crash.
   - O `Key=F11` tira o menu do AuraV do **F10**, a tecla do HUD para recarregar as configurações dele.
   - O AuraV mantém essas linhas. Ele adiciona o resto das configurações dele ao arquivo na primeira vez que roda.
   - ⚠️ Confira se o arquivo se chama `AuraV.ini`, e não `AuraV.ini.txt`
     (Explorador de Arquivos: **Exibir** → **Mostrar** → **Extensões de nomes de arquivos**).
   - **Já iniciou o jogo com o AuraV?** Então o `AuraV\AuraV.ini` já existe. Abra ele, coloque as linhas de `[boot]`
     em `0` e o `Key` (embaixo de `[AuraV]`) em `F11`, e salve.
6. **Tire os add-ons do RenoDX.** O NVE coloca arquivos `renodx-*.addon64` na sua pasta do jogo. O menu do AuraV dá crash no
   jogo enquanto eles estiverem lá.
   1. Mova eles para uma pasta fora da pasta do jogo.
   2. Deixe eles lá, caso você remova o AuraV depois.
   3. Deixe o ReShade do próprio NVE (`ReShade.asi`, com o visual do NVE no `NVE.ini`) onde está. Ele funciona com o AuraV.
7. No jogo, vá em **Settings** → **Graphics**.
8. Ligue o **Ray Tracing** e o **NVIDIA DLSS**. O path tracing precisa dos dois.
9. Mantenha o **Windowed Borderless** (Passo 7).
10. Jogue. O **F11** abre o menu do AuraV.

Bom saber:
- **Testado** com o AuraV-Lite 0.2.7, o NVE e o HUD. O Modo História carrega com o path tracing ligado e o HUD aparecendo.
- **O AuraV está rodando?** O `asiloader.log` na sua pasta do jogo lista o `AuraV.asi`. O `AuraV\AuraV.log` diz "in the
  world" quando o Modo História termina de carregar. Se o jogo der crash, esse log diz o porquê.
- As dicas em "Opcional: AuraV-Lite" acima (DLSS Swapper, placas de vídeo mais fracas) valem aqui também.
- **Para remover:** apague os 3 arquivos do AuraV e a pasta `AuraV`. Coloque os add-ons do RenoDX de volta na pasta do jogo.

---

## Opcional: LSPDFR

O **LSPDFR** (jogar como policial) é feito para o **Legacy**.
O LSPDFR para o Enhanced ainda é uma prévia inicial. Ainda não testamos o HUD com ele.

Vai tentar mesmo assim? A **[parte do LSPDFR no guia Legacy](INSTALL-LEGACY.pt-BR.md#optional-lspdfr)** explica:

- o que o HUD faz em serviço (você não precisa mudar nada)
- quais teclas fazem duas coisas ao mesmo tempo, e como mudar elas

---

## Deu algo errado?

Encontre o que está acontecendo com você na coluna da esquerda. Faça o que a coluna da direita diz.

**Continua com problema?** Pergunte no **[nosso Discord de suporte](https://discord.gg/C7gE5SHEzH)**, o desta versão. Ou
[abra uma issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).

| O que acontece | O que fazer |
| --- | --- |
| O jogo **fecha sozinho uns 20 segundos depois de carregar** | Você tem o Script Hook V .NET normal.<br>1. Refaça o Passo 3 com o **SHVDNE**.<br>2. Apague a pasta **`dl3`** em `%LOCALAPPDATA%\assembly` (um cache do antigo).<br>**Para conferir qual você tem:** na sua pasta do jogo, clique com o botão direito em **`ScriptHookVDotNet.asi`** → **Propriedades (Properties)** → **Detalhes (Details)**. O SHVDNE diz **"Script Hook V .Net Enhanced"** ali. O normal não diz (a versão dele é 3.6.0.0).<br>**Ainda acontece?** Um add-on do ReShade (um arquivo que termina em `.addon64` na sua pasta do jogo) pode estar dando conflito. Mova esses arquivos para outro lugar e tente de novo. |
| A sua pasta do jogo tem **`MiniMap.asi`** ou **`MiniMapHudPresent.asi`** (de outro mod de minimapa 3D ou de uma instalação mais antiga) | 1. Apague ele.<br>2. Apague o `scripts\MiniMap.dll` também, se ele estiver lá. (O do HUD é o `scripts\ViMinimap.dll`.)<br>3. Mantenha o `ViMinimap.asi` ao lado do `GTA5_Enhanced.exe`.<br>É uma cópia mais antiga do plugin do minimapa que o próprio HUD traz como `ViMinimap.asi`. Duas cópias carregadas ao mesmo tempo atrapalham uma à outra. |
| O jogo **dá crash quando você abre o menu do AuraV-Lite** | Você tem os add-ons de ReShade do RenoDX (`renodx-*.addon64`) na sua pasta do jogo. O NVE vem com eles.<br>Tire eles de lá enquanto você usar o AuraV.<br>Veja "Opcional: AuraV-Lite" acima, ou "Opcional: AuraV-Lite com o NaturalVision Enhanced". |
| Uma caixa **"Debug Assertion Failed!"** do Microsoft Visual C++ cita o **`renodx-dlssfix.addon64`** | Esse é o add-on de correção de DLSS do RenoDX, não o HUD. É uma versão de debug dele.<br>Não usa DLSS 5? Então você não precisa dele: apague o `renodx-dlssfix.addon64` da sua pasta do jogo.<br>O `renodx-gtav-enhanced.addon64` pode ficar.<br>Você não precisa reinstalar o Visual C++ por causa deste. |
| Você usa o **AuraV-Lite** com o **NaturalVision Enhanced**, e o jogo **dá crash na inicialização** | 1. No `AuraV\AuraV.ini`, embaixo de `[boot]`, coloque `skip_legal`, `cut_logos`, `skip_menu` e `story` em `0`.<br>2. Inicie o jogo de novo.<br>Veja "Opcional: AuraV-Lite com o NaturalVision Enhanced" acima. |
| Você usa o **NaturalVision Enhanced**, e o jogo **dá crash ao carregar o Modo História** ou **o HUD não aparece** | Siga "Opcional: NaturalVision Enhanced" acima:<br>• sem OpenRPF<br>• os packs do HUD copiados para `onigiri\dlcpacks`<br>• o `dlclist.xml` do NVE listando eles<br>O NVE traz o seu próprio `dlclist.xml` e `gameconfig.xml` (em `onigiri\common\data`). O jogo usa esses no lugar dos que o pacote do HUD configurou.<br>Abra o `dlclist.xml` do NVE. Adicione estas linhas logo acima do `</Paths>`, cada uma na sua própria linha:<br>`<Item>dlcpacks:/vihud/</Item>`<br>`<Item>dlcpacks:/vihudfonts/</Item>`<br>`<Item>dlcpacks:/vihudanims/</Item>`<br>⚠️ Deixe o `gameconfig.xml` do NVE do jeito que o NVE entrega. Aumentar ainda mais os tamanhos de pool dele faz o jogo ficar sem memória ao carregar. |
| Você usa o **NaturalVision Enhanced** e as **telas de carregamento são as do próprio jogo**, e não as de GTA VI do HUD | Faça o passo 6 de "Opcional: NaturalVision Enhanced" acima.<br>O carregador do NVE não lê a pasta `mods`. Então as telas de carregamento têm que ser copiadas para `onigiri\platform\data\cdimages`. |
| Você usa o **NaturalVision Enhanced**, e **desde uma atualização do jogo** os menus ou as telas de carregamento do jogo estão estranhos | 1. Apague o `onigiri\platform\data\cdimages\scaleform_frontend.rpf`. Ele é uma cópia das telas de menu do jogo de antes da atualização.<br>2. Instale o HUD de novo.<br>3. Faça o passo 6 de "Opcional: NaturalVision Enhanced" de novo. |
| Você instalou o **Lively World Expansion** (LWE), e agora **partes do HUD sumiram** | O `dlclist.xml` do LWE substituiu o do NVE. Ele não lista os packs do HUD (nem alguns do próprio NVE).<br>1. Coloque o `dlclist.xml` do NVE de volta.<br>2. Adicione nele as linhas do LWE e as linhas do HUD.<br>Veja "Opcional: Lively World Expansion" acima. |
| Você instalou o **Central Real-Life Traffic** ou o **Environment**, e agora **partes do HUD sumiram** | O `dlclist.xml` do Central substituiu o seu. Ele não lista os packs do HUD.<br>Coloque as linhas do HUD de volta nele: veja "Opcional: Central Real-Life Traffic e Environment" acima. |
| O jogo **dá crash logo depois da tela de carregamento** desde que você adicionou o **Central Real-Life Traffic** | 1. Instale o **ResourceExtensions** (o Central precisa dele).<br>2. Mantenha o `gameconfig.xml` original que o Central colocou em `onigiri\common\data`.<br>O ResourceExtensions não suporta um modificado, nem mesmo o do NVE. |
| Você adicionou o **Forests of San Andreas** ou o **GTA V Remastered**, e **nenhuma árvore nova aparece** | Os packs deles precisam estar onde o seu jogo carrega os packs:<br>• **Com o NaturalVision Enhanced:** em `onigiri\dlcpacks`, com as linhas deles no `onigiri\common\data\dlclist.xml`.<br>• **Sem ele:** em `mods\update\x64\dlcpacks`, com as linhas deles no `dlclist.xml` do `mods\update\update.rpf`.<br>Veja "Opcional: Forests of San Andreas e GTA V Remastered" acima. |
| Uma mensagem do Script Hook V diz **"unknown game version"** (versão do jogo desconhecida) | O GTA V acabou de ser atualizado, e o Script Hook V ainda não acompanhou.<br>1. Espere sair um Script Hook V novo.<br>2. Refaça o Passo 2. |
| O jogo inicia, mas **o HUD novo não aparece** | Confira se os 5 arquivos do SHVDNE do Passo 3 estão ao lado do `GTA5_Enhanced.exe`. |
| Você **atualizou** o mod, mas o jogo ainda age como a **versão antiga** | O Script Hook V .NET pode continuar rodando a cópia salva dos scripts antigos.<br>1. Feche o jogo.<br>2. Aperte **Win + R**.<br>3. Digite `%LOCALAPPDATA%\assembly` e aperte Enter.<br>4. Apague a pasta **`dl3`**. É só um cache.<br>**Ainda está na versão antiga?** Instale de novo com o OIV Package Installer rodando como administrador (Passo 6). Ou copie os scripts você mesmo: veja "Atualizando manualmente" abaixo. |
| **Sem minimapa** no carro | Coloque o jogo em **Windowed Borderless** (Passo 7). |
| Um erro sobre **.NET** ou **Visual C++** | 1. Instale o [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net48-web-installer).<br>2. Instale o [Visual C++ Redistributable (x64)](https://aka.ms/vs/17/release/vc_redist.x64.exe).<br>3. Reinicie o seu PC. |
| Uma mensagem diz **Lucia is not registered** | A Lucia não vem mais incluída com o HUD (veja o Passo 5).<br>**Não tem ela?** Coloque `Enabled = False` embaixo de `[Lucia]` no `scripts\GTA6Hud.ini`. A mensagem some.<br>**Tem um modelo da Lucia?** O pack dela não está no `dlclist.xml` (com o **NaturalVision Enhanced**, no do NVE). Adicione a linha dele.<br>Se a mensagem falar de um `VIHudSetup.exe`, ignore essa parte: este mod não usa ele. |
| **Trocar de personagem dá crash**, e o seu jogo não está em inglês | Os jogadores relatam que isso só acontece quando o jogo está em outro idioma.<br>Até ser corrigido: coloque o idioma do GTA V em **inglês** (English) nas configurações do GTA V no seu launcher. |
| O HUD diz que o seu jogo **não consegue salvar na pasta dele**, e um **aviso do Windows** pede para deixar o **icacls** fazer alterações | O seu jogo está em **Program Files**, onde o Windows só deixa administradores alterarem arquivos. Então o HUD não consegue salvar as configurações dele (e o ReShade não consegue salvar as dele).<br>1. Clique em **Sim**. Isso deixa o seu usuário do Windows alterar a pasta do jogo. É o mesmo que fazer isso na mão em **Propriedades** → **Segurança (Security)** da pasta.<br>2. Reinicie o jogo uma vez.<br>**Clicou em Não?** Você vai ser perguntado de novo na próxima vez que iniciar o jogo.<br>Colocar `[GameFolder] FixAccess = False` no `ViExtras.ini` faz a pergunta parar. |
| **Outra coisa** | Pergunte no **[Discord de suporte](https://discord.gg/C7gE5SHEzH)**.<br>Ou [abra uma issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues) e anexe estes arquivos da sua pasta do jogo:<br>• `ScriptHookVDotNet.log`<br>• `scripts\GTA6Hud.perf.log`<br>• `scripts\GTA6Hud.error.log` |

---

## Atualizando manualmente (jogo em Program Files)

**Quando você precisa disto:** o seu jogo está em **Program Files** (onde o Rockstar Games Launcher e a Steam colocam ele).
O Windows protege essa pasta. A não ser que você rode o OIV Package Installer como administrador (Passo 6), uma atualização por
ele nem sempre substitui os arquivos antigos.

Você também pode copiar os scripts novos você mesmo. Um `.oiv` é só um zip.

💡 A partir da 2.6, o HUD se oferece para consertar a pasta para você na primeira vez que roda. Veja a linha do aviso do Windows acima.
Depois disso, as atualizações entram normalmente.

1. Feche o jogo.
2. Copie o **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`** para a sua Área de Trabalho.
3. Renomeie a cópia para **`.zip`**.
   Não consegue ver a parte `.oiv` do nome? No Explorador de Arquivos, ligue **Exibir** → **Mostrar** → **Extensões de nomes de arquivos**.
4. Abra o zip → **`content`** → **`scripts`**.
5. Selecione tudo o que tem lá e copie.
6. Cole na pasta **`scripts`** da sua pasta do jogo. Por exemplo:
   `C:\Program Files\Rockstar Games\Grand Theft Auto V Enhanced\scripts`.
7. Escolha **Substituir os arquivos no destino (Replace the files in the destination)**.
8. Clique em **Continuar (Continue)** quando o Windows pedir permissão de administrador.
9. Limpe o cache do Script Hook V .NET, para ele carregar os arquivos novos:
   1. Aperte **Win + R**.
   2. Digite `%LOCALAPPDATA%\assembly` e aperte Enter.
   3. Apague a pasta **`dl3`**. É só um cache. Ele é criado de novo na próxima vez que você iniciar o jogo.
10. Vindo da 2.5.4 ou mais antiga? A 2.5.5 adiciona um pack:
    1. Copie a pasta **`content`** → **`dlcpacks`** → **`vihud`** do zip para **`mods\update\x64\dlcpacks`** na
       sua pasta do jogo.
    2. Instale o mod uma vez com o OIV Package Installer, como no Passo 6. Isso ativa o pack novo.
11. Inicie o jogo. O `scripts\ViExtras.log` agora começa com uma linha `==== ViExtras …`.
    Ela mostra a versão que você baixou.

---

## Como remover o mod

💡 Quer remover o **NaturalVision Enhanced** e manter o HUD? Veja "Removendo o NVE (e mantendo o HUD)" na seção do
NVE.

### O jeito mais fácil

1. Abra o **OIV Package Installer**.
2. Vá em **Manage Mods**.
3. Desinstale os dois pacotes.

### Ou na mão

1. Na sua **pasta do jogo**, apague o **`ViMinimap.asi`**.
2. Abra a pasta **`scripts`**.
3. Apague estes:
   - **`GTA6Hud.dll`**
   - **`GTA6Hud.ini`**
   - **`ViMinimap.dll`**
   - **`LemonUI.SHVDN3.dll`**
   - os arquivos **`MiniMap.*`**
   - a pasta **`Hud`**
4. Apague a pasta **`mods`**. ⚠️ Isso também remove qualquer outro mod que estiver nela.
5. Com o NaturalVision Enhanced, apague também:
   - o **`onigiri\platform\data\cdimages\scaleform_frontend.rpf`** (as telas de carregamento do HUD)
   - os packs do HUD em `onigiri\dlcpacks`

✅ O seu jogo voltou ao normal.

O Script Hook V, o SHVDNE e o OpenRPF podem ficar. Eles não mudam nada por conta própria.
