# Como instalar: GTA V **Legacy**

*Esta é uma tradução. O guia em inglês ([INSTALL-LEGACY.md](INSTALL-LEGACY.md)) é sempre o mais atualizado.*

Este guia é para a **versão original de PC** do GTA V.

A pasta do seu jogo tem um arquivo chamado **`GTA5_Enhanced.exe`**? Então você tem o Enhanced.
Use o **[guia Enhanced](INSTALL-ENHANCED.pt-BR.md)** no lugar deste.

**Nunca usou mods no GTA?** Sem problema. Siga os passos na ordem. Não pule nenhum. Leva uns 20 minutos.

> [!IMPORTANT]
> ### 💬 Precisa de ajuda? Entre no nosso Discord de suporte
> Esta versão é um **fork do VI HUD do FBICat** com recursos extras. Por isso ela tem o seu próprio suporte:
> - **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**
> - ou uma [issue no GitHub](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues)
>
> O Discord do FBICat é para o mod original dele. Traga as suas dúvidas sobre esta versão para o nosso. Obrigado!

> ⚠️ **Mods são só para o Modo História (Story Mode).** Nunca entre no GTA Online com mods instalados. Você pode ser banido.
> (O Script Hook V, uma das ferramentas abaixo, fecha o jogo se você tentar.)

---

## Antes de começar: duas configurações rápidas do Windows

### 1. Mostrar as extensões dos arquivos

Alguns arquivos neste guia terminam em `.asi` ou `.dll`. O Windows esconde esses finais por padrão.

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

## Passo 1: Encontre a pasta do seu GTA V

É a pasta que tem o **`GTA5.exe`** dentro. Vamos chamar ela de **pasta do jogo**.

Como abrir:

| O seu launcher | O que fazer |
| --- | --- |
| **Rockstar Games Launcher** | 1. Abra as **Configurações (Settings)**.<br>2. Clique em **Grand Theft Auto V** à esquerda.<br>3. Clique em **Abrir pasta (Open Folder)** (ou **Ver pasta de instalação (View Installation Folder)**). |
| **Steam** | 1. Vá até a sua **Biblioteca (Library)**.<br>2. Clique com o botão direito em **Grand Theft Auto V**.<br>3. Clique em **Gerenciar (Manage)** → **Ver arquivos locais (Browse local files)**. |
| **Epic Games** | 1. Vá até a sua **Biblioteca (Library)**.<br>2. Clique no **⋯** embaixo do GTA V.<br>3. Clique em **Gerenciar (Manage)**.<br>4. Clique no **ícone de pasta** ao lado de "Instalação" (Installation). |

✅ Deixe essa pasta aberta. Você vai copiar arquivos para ela algumas vezes.

---

## Passo 2: Instale o Script Hook V (permite que os mods rodem)

1. Vá até a [página do Script Hook V](http://www.dev-c.com/gtav/scripthookv/).
2. Clique em **Download**.
3. Descompacte.
4. Abra a pasta **`bin`** que está dentro.
5. Copie estes arquivos para a sua **pasta do jogo**:
   - **`ScriptHookV.dll`**
   - **`dinput8.dll`** (carrega mods que terminam em `.asi`)
   - **`args.txt`** (desliga o BattlEye, o anti-cheat da Rockstar, que bloqueia mods)
   - **`xinput1_4.dll`**, se ele estiver lá

⚠️ **Não** copie o `NativeTrainer.asi`. É um menu de trapaças de que você não precisa.

✅ A sua pasta do jogo agora tem o `ScriptHookV.dll` e o `dinput8.dll` ao lado do `GTA5.exe`.

---

## Passo 3: Instale o Script Hook V .NET (permite que este HUD rode)

O HUD precisa do **3.7.0 nightly 193 ou mais novo**. A release do **topo** da página nightly sempre é.
Com um mais antigo, os scripts do HUD não iniciam, ou um deles para com um erro (veja "Deu algo errado?").

1. Vá até a [página nightly do Script Hook V .NET](https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases).
2. Encontre a release do **topo**.
3. Abra **Assets**.
4. Baixe o arquivo **`.zip`**.
5. Descompacte.
6. Copie estes para a sua **pasta do jogo**:
   - **`ScriptHookVDotNet.asi`**
   - **`ScriptHookVDotNet2.dll`**
   - **`ScriptHookVDotNet3.dll`**
   - **`ScriptHookVDotNet.ini`**

✅ Esses 4 arquivos agora estão ao lado do `GTA5.exe`.

---

## Passo 4: Instale o OpenIV (instala o mod com segurança)

O OpenIV coloca os mods em uma **pasta `mods`** separada. Os arquivos originais do seu jogo nunca são alterados.

1. Baixe o OpenIV em [openiv.com](https://openiv.com/).
2. Instale.
3. Abra o OpenIV.
4. Escolha **GTA V** → **Windows**.
5. Se ele pedir, aponte para a sua **pasta do jogo**.
6. Lá em cima, clique em **Tools** → **ASI Manager**.
7. Clique em **Install** ao lado de **ASI Loader**.
8. Clique em **Install** ao lado de **OpenIV.asi**.
9. Feche a janela.

✅ O OpenIV está pronto.

---

## Passo 5: Baixe o mod

1. Vá até a [**página de Releases**](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest).
2. Em **Assets**, baixe o **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`** (o mod, uns 100 MB).
   Ele também ativa o mod no jogo. Não tem nenhum patch separado para baixar.

❌ Não baixe o `GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`. Esse é para a outra versão do jogo.

**Quer jogar como a Lucia ou o Jason?** Os modelos deles não vêm incluídos.

| Personagem | Onde pegar o modelo |
| --- | --- |
| **Lucia** | O modelo dela é trabalho pago de outra pessoa. Pegue com o criador dela:<br>**[Lucia Caminos no Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)** ou **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** no gta5-mods.com.<br>Ou use outro modelo da Lucia.<br>Para colocar ela no jogo, veja "Opcional: Lucia" abaixo.<br>O HUD funciona sem ela. |
| **Jason** | **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** do mesmo criador, ou **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** no mod-hub.net.<br>Para colocar ele no jogo, veja "Opcional: Jason" abaixo. |

💡 O número no nome do arquivo é a versão (2.7.0 aqui). Assim você sempre sabe qual instalou.
Diga qual é quando pedir ajuda.

---

## Passo 6: Instale o mod com o OpenIV

⚠️ **Jogo na pasta Program Files?** O caminho da sua pasta do jogo começa com `C:\Program Files`?
(O Rockstar Games Launcher, a Epic Games e a Steam colocam ele lá.)
Então clique com o botão direito no **OpenIV** → **Executar como administrador (Run as administrator)**, e clique em **Sim (Yes)**.

💡 O Windows só deixa administradores alterarem arquivos lá. Sem isso, a instalação pode deixar os arquivos antigos no lugar.

1. No OpenIV, clique em **Tools** → **Package Installer**.
2. Escolha o **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`**.
3. Clique em **Install**.
4. Quando ele perguntar onde instalar, escolha **mods folder** (a pasta mods).
5. Se ele oferecer para criar a pasta, diga que sim.
6. Espere até ele dizer que terminou.
7. Clique em **Close**.

✅ Instalar o mod de novo, ou por cima de uma versão mais antiga, não tem problema. Ele nunca coloca um pack duas vezes na lista.

---

## Passo 7: Confira um arquivo

Procure o **`ViMinimap.asi`** na sua **pasta do jogo**. Ele tem que estar bem ao lado do `GTA5.exe`.

| Está lá? | O que fazer |
| --- | --- |
| **Sim** | Ótimo, vá para o Passo 8. |
| **Não** | 1. Abra a pasta **`mods`** na sua pasta do jogo.<br>2. Encontre o **`ViMinimap.asi`**.<br>3. Mova ele para o lado do **`GTA5.exe`**. |

💡 Sem ele, o minimapa não aparece.

---

## Passo 8: Jogue! 🎉

1. Inicie o GTA V.
2. Escolha **Story Mode**.

✅ O HUD novo aparece sozinho.

| Para... | Faça isto |
| --- | --- |
| **Trocar de personagem** | Segure **Left Alt** (o Alt esquerdo; no controle: segure o **D-pad para baixo**).<br>Escolha com **Q / E** ou **Left / Right** (as setas; no controle: **LB / RB**).<br>Depois solte.<br>O Jason e a Lucia precisam dos modelos deles (não incluídos, veja o Passo 5). |
| **Ver o minimapa** | Entre em um carro. Ele só aparece em veículos, ou quando você está sendo procurado. |
| **Abrir as configurações do HUD** | Segure **V**. |
| **Tocar a sua própria música** | Coloque arquivos `.mp3` em **`scripts\Hud\Radio\CustomMusic`** na sua pasta do jogo.<br>No carro, segure o botão do rádio (**Q**) e aperte **Down** (a seta para baixo; no controle: analógico direito para baixo) para **On Demand**.<br>Uma pasta dentro de `CustomMusic` vira uma estação própria.<br>**Left / Right** (analógico direito) trocam de estação.<br>A **roda do mouse** (D-pad para a direita / para cima) troca a música. |

---

## Opcional: telas de carregamento VI

**O que você ganha:** a tela inicial e as telas de carregamento de GTA VI do FBICat, com as imagens de Vice City da Rockstar.
**Do que você precisa:** do **`GTA-VI-Inspired-HUD-<version>-LoadingScreens-Legacy.oiv`** da mesma release do HUD.
O HUD funciona sem ele: instale só se você quiser essas telas.

No OpenIV: **Tools** → **Package Installer**, escolha o arquivo, **Install**, e escolha a **mods folder**, igual ao HUD.

### Já tem essas telas e quer as do próprio jogo de volta?

Até a 2.6.4, as telas de carregamento vinham dentro do pacote do HUD. Para tirar elas:

1. Abra o **OpenIV** e clique em **Edit mode** (modo de edição) lá em cima.
2. Abra o **`update`** do próprio jogo → **`update.rpf`** → `x64` → `data` → `cdimages` (não o que está em `mods`).
3. Clique com o botão direito em **`scaleform_frontend.rpf`** → **Extract** (no CodeWalker: **Extract Raw...**) para a sua Área de Trabalho.
4. Abra **`mods`** → `update` → **`update.rpf`** → `x64` → `data` → `cdimages`.
5. Arraste o **`scaleform_frontend.rpf`** da sua Área de Trabalho para essa janela, e substitua o que está lá.

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
   Em vez disso, tem uma pasta **`legacy`** e uma **`enhanced`** (como no Jason Duval Extended Look)? Abra a **`legacy`**.
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

### Parte 3: Coloque ele no jogo

1. Baixe o **[GTA-VI-Inspired-HUD-Jason-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Jason-Kit.zip)** e descompacte.
2. Copie a pasta **`jasonped`** do kit para **`mods\update\x64\dlcpacks`** na sua pasta do jogo.
3. Abra o **OpenIV** e clique em **Edit mode** lá em cima, para ligar.
4. No OpenIV, abra estes, um depois do outro: `mods` → `update` → `x64` → `dlcpacks` → `jasonped` → **`dlc.rpf`** → `x64`
   → `models` → `cdimages` → **`jasonped.rpf`**.
5. Arraste os seus 4 arquivos **`jdfinal`** para essa janela.
6. No OpenIV, abra: `mods` → `update` → **`update.rpf`** → `common` → `data`. Clique com o botão direito em **`dlclist.xml`** → **Edit**.
7. Role até o **finalzinho**. Coloque o cursor no começo da linha que diz **`</Paths>`**, aperte **Enter**,
   e cole isto na nova linha vazia acima dela:
   ```xml
   <Item>dlcpacks:/jasonped/</Item>
   ```
8. Aperte **Ctrl + S** para salvar, e feche a janela.
9. Inicie o jogo e segure **Left Alt** (no controle: segure o **D-pad para baixo**). O card do Jason vem depois do card do Trevor.

### Não funcionou?

| O que você vê | O que fazer |
| --- | --- |
| O Jason não está na roda | Abra o **`scripts\ViSwitch.log`** na sua pasta do jogo com o **Bloco de Notas (Notepad)**. Se ele disser **"no Jason model in the game"** (nenhum modelo do Jason no jogo), uma destas coisas está errada: os seus 4 arquivos não se chamam `jdfinal` (Parte 2), eles não estão dentro do **`jasonped.rpf`**, ou falta a linha **`<Item>dlcpacks:/jasonped/</Item>`** no `dlclist.xml`. Corrija e inicie o jogo de novo. |

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

### Parte 3: Coloque ela no jogo

1. Baixe o **[GTA-VI-Inspired-HUD-Lucia-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Lucia-Kit.zip)** e descompacte.
2. Copie a pasta **`luciaped`** do seu pack (**`A - 4 files`** ou **`B - with a folder`**) para
   **`mods\update\x64\dlcpacks`** na sua pasta do jogo.
3. Abra o **OpenIV** e clique em **Edit mode** lá em cima, para ligar.
4. No OpenIV, abra estes, um depois do outro: `mods` → `update` → `x64` → `dlcpacks` → `luciaped` → **`dlc.rpf`** → `x64`
   → `models` → `cdimages` → **o RPF dela**.
5. Arraste os arquivos **`lucia_s`** dela para essa janela (pack B: a pasta **`lucia_s`** também, e a **`lucia_s_p`** se você
   tiver).
6. No OpenIV, abra: `mods` → `update` → **`update.rpf`** → `common` → `data`. Clique com o botão direito em **`dlclist.xml`** → **Edit**.
7. Role até o **finalzinho**. Coloque o cursor no começo da linha que diz **`</Paths>`**, aperte **Enter**,
   e cole isto na nova linha vazia acima dela:
   ```xml
   <Item>dlcpacks:/luciaped/</Item>
   ```
8. Aperte **Ctrl + S** para salvar, e feche a janela.
9. Inicie o jogo e segure **Left Alt** (no controle: segure o **D-pad para baixo**). O card da Lucia é o último.

### Não funcionou?

| O que você vê | O que fazer |
| --- | --- |
| Uma mensagem diz **Lucia is not registered** (a Lucia não está registrada) | Falta a linha **`<Item>dlcpacks:/luciaped/</Item>`** no `dlclist.xml`. Adicione e inicie o jogo de novo. |
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

## Opcional: Forests of San Andreas e GTA V Remastered

**O que você ganha:** dezenas de milhares de árvores, arbustos e objetos extras no mapa.
**Do que você precisa:** do **Forests of San Andreas**, do **GTA V Remastered**, ou dos dois, do **Larcius** (feitos a partir dos mapas do JRod e do le__AK). Eles funcionam com este HUD.
Eles também vêm como OIVs, então são instalados do mesmo jeito que o HUD.

1. Instale o HUD primeiro (Passos 1 a 7).
2. No zip de cada mod, abra a pasta **`GTA V Legacy`**.
3. No OpenIV, clique em **Tools** → **Package Installer**.
4. Escolha um que você queira:
   - **`Forests of San Andreas (North) Ultimate v….oiv`**
   - **`Forests of San Andreas (South) Ultimate v….oiv`**
   - **`GTA V Remastered Ultimate v….oiv`**
5. Escolha **mods folder**, como no Passo 6.
6. Faça os passos 3 a 5 de novo para cada um que você quiser.

   💡 Cada um adiciona o seu próprio pack à lista de packs do jogo, ao lado dos packs do HUD. Então não tem nada para editar.
7. Usa o **Liberty City V Remix** também? Instale também os OIVs **`Compatibility Liberty City V Remix`** dos mods.
8. ⚠️ **Deixe o `gameconfig.xml` como está.** Os mapas não precisam de outro.
   Instalar outro `gameconfig.xml` substituiria o que o HUD configurou, e o HUD precisa dele.
   O jogo dá crash ao carregar com os mapas instalados? Veja "Deu algo errado?" abaixo.

**Para tirar eles de novo:** instale os OIVs **`Uninstaller`** dos mods do mesmo jeito, na mods folder.

---

## Opcional: Rebalanced Dispatch Enhanced

**O que você ganha:** o **Rebalanced Dispatch Enhanced** (RDE), do **RDE Team**. Ele substitui a resposta da polícia do GTA:
novas agências, bloqueios, helicópteros, e até **10 estrelas**. O HUD sai do caminho dele.
**Do que você precisa:** do HUD instalado primeiro, e do **`RDE 4.1.5.oiv`** que vem no download do RDE.

> [!NOTE]
> Novo na 2.6.3, e só testado rapidamente no jogo.
> Algo estranho com o RDE e o HUD juntos? Conte para a gente no Discord ou em uma issue no GitHub.

### Parte 1: Instale o RDE

1. Instale o HUD primeiro (Passos 1 a 7).

   💡 O Script Hook V .NET do Passo 3 é o nightly que o RDE pede.
2. No OpenIV, clique em **Tools** → **Package Installer**.
3. Escolha o **`RDE 4.1.5.oiv`** do download do RDE.
4. Escolha **mods folder**, como no Passo 6.

⚠️ Pule os arquivos **`Compatibility - Community HUD`** do RDE. Eles são para outro mod de HUD.

💡 O RDE traz o seu próprio `gameconfig.xml`, com limites muito maiores do que o HUD precisa. Tudo bem.
Instalar ou atualizar o HUD depois só aumenta um limite que esteja baixo demais. Então ele nunca desfaz os do RDE.

### Parte 2: O que muda enquanto o RDE roda

- A polícia do RDE assume.
- Estes ficam de fora:
  - o cérebro da polícia do GTA6Hud
  - as seis estrelas próprias do GTA6Hud (e os militares que elas chamam)
  - as prisões com arma de choque do HUD
- As estrelas do HUD mostram o nível do RDE até **seis**. Os níveis 6 a 10 acendem, todos, a sexta estrela.
- Para deixar o HUD como está, coloque `[Compat] Rde = False` no `ViExtras.ini`.

### Parte 3: Tire o RDE de novo

1. Instale o **`Uninstaller.oiv`** do RDE do mesmo jeito.
2. Instale o HUD de novo (Passo 6), para os limites e os packs dele voltarem.

---

## Opcional: Los Santos RED

**O que você ganha:** o **Los Santos RED** (LSR), do **Greskrendtregk**. Ele transforma o jogo em um sandbox de crime.
Ele tem a sua própria polícia, crimes, nível de procurado e muito mais. O HUD sai do caminho dele.
**Do que você precisa:** do **RAGE Plugin Hook** e do **RAGENativeUI**. O LSR roda no RAGE Plugin Hook, não no Script Hook V .NET.

> [!NOTE]
> Novo na 2.6.3, e só testado rapidamente no jogo.
> Algo estranho com o LSR e o HUD juntos? Conte para a gente no Discord ou em uma issue no GitHub.

⚠️ Use o LSR sozinho. Não junto com o LSPDFR ou o Rebalanced Dispatch Enhanced: eles também controlam a polícia.

### Parte 1: Instale o LSR

1. Instale o HUD primeiro (Passos 1 a 7).
2. Instale o **[RAGE Plugin Hook](https://ragepluginhook.net/)**.
3. Instale o **[RAGENativeUI](https://github.com/alexguirre/RAGENativeUI/releases)**. Os menus do LSR precisam dele.
4. Instale o próprio LSR, como diz a página do LSR.
5. Inicie o jogo com o **`RAGEPluginHook.exe`**.

✅ O LSR carrega quando o Modo História termina de carregar.

### Parte 2: O que muda enquanto o LSR roda

O LSR cuida da polícia, dos crimes e do nível de procurado. Então o HUD não adiciona nada disso por conta própria:

- sem testemunhas
- sem cérebro da polícia nem memória da polícia
- sem heat
- sem seis estrelas
- sem prisões com arma de choque

Para deixar o HUD como está, coloque `[Compat] Lsr = False` no `ViExtras.ini`.

### Parte 3: Estrelas

O LSR mantém o nível de procurado do próprio GTA em zero e desenha estrelas próprias.
Por isso as estrelas e a caixa de procurado do HUD não aparecem com o LSR.

Quer estrelas de GTA VI no lugar das do LSR?

1. Encontre as 4 imagens na pasta [`extras/lsr-stars/wantedlevel`](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/tree/main/extras/lsr-stars/wantedlevel) do HUD.
2. Copie elas para **`Plugins\LosSantosRED\images\wantedlevel`**.
3. Substitua as imagens do LSR por elas.

### Parte 4: As configurações do LSR

O LSR cria o `Plugins\LosSantosRED\Settings.xml` na primeira vez que roda.

1. Feche o jogo.
2. Abra o **`Plugins\LosSantosRED\Settings.xml`** no Bloco de Notas.
3. Mude estes valores:

| Configuração | Coloque | Por quê |
| --- | --- | --- |
| `AlwaysShowRadar` | `false` | O minimapa 3D do HUD substitui o radar do GTA. Se o LSR ligar o radar, ele volta a aparecer embaixo do mapa. |
| `NeverShowRadar` | `true` | O mesmo motivo. |
| `TopDisplayShowWeapon` | `false` | O HUD já mostra a sua arma no canto superior direito. |
| `ShowStreetDisplay` | `false` | O HUD já mostra a rua embaixo do minimapa. |

### Parte 5: Teclas

O LSR usa algumas das teclas do HUD. Mude um dos lados de cada par que você usa:

| Tecla | HUD | LSR |
| --- | --- | --- |
| **Left Ctrl** | Agachar (`ViCrouch.ini` `[Crouch] Key`) | Agachar (`CrouchKey` nas configurações do LSR) |
| **K** | Painel de habilidades (`ViExtras.ini` `[Skills] Key`) | Responder "não" em conversas |
| **O** | Silenciar o rádio (`ViExtras.ini` `[Radio] MuteKey`) | Iniciar uma interação |
| **Shift + N** | Abdominais (`ViExtras.ini` `[Gym] SitUpsKey`) | Modo de grupo |

---

<a id="optional-lspdfr"></a>
## Opcional: LSPDFR

**O que você ganha:** o **LSPDFR** (LSPD First Response), da **G17 Media**. Você joga como policial.
Entre em serviço, atenda chamados (callouts) e faça abordagens no trânsito. O HUD continua funcionando, e sabe quando você está em serviço.
**Do que você precisa:** do HUD instalado primeiro, e do LSPDFR do **[lcpdfr.com](https://www.lcpdfr.com/)**.
O LSPDFR roda no RAGE Plugin Hook, não no Script Hook V .NET.

> [!NOTE]
> Testado só no **Legacy**.

⚠️ Não use o LSPDFR com o **Los Santos RED**. Os dois controlam a polícia.

💡 O **Rebalanced Dispatch Enhanced** vem com os seus próprios arquivos para o LSPDFR, então esses dois podem ser usados juntos.
Ainda não testamos o HUD com os dois ao mesmo tempo.

### Parte 1: Instale o LSPDFR

1. Instale o HUD primeiro (Passos 1 a 7).
2. Instale o LSPDFR, como diz a página do LSPDFR.
3. Inicie o jogo com o **`RAGEPluginHook.exe`**.

✅ O LSPDFR carrega quando o Modo História termina de carregar.

💡 Vai jogar sem o LSPDFR? Inicie o jogo do jeito normal. O LSPDFR só carrega pelo `RAGEPluginHook.exe`.

### Parte 2: O que o HUD faz em serviço

**Você não precisa mudar nada.** O HUD encontra o LSPDFR sozinho. Enquanto você está em serviço:

- **Sem troca de personagem.** A roda de personagens fica fechada, e as teclas de troca não fazem nada.
  Trocar de personagem tiraria do LSPDFR o policial dele.
- **Sem testemunhas.** Ninguém sai correndo nem liga para denunciar você. Você é a polícia.
- **Sem nível de procurado.** Se aparecer um mesmo assim, ele é zerado na hora.

Saia de serviço, e tudo funciona como antes.

💡 Quer trocar de personagem? Saia de serviço primeiro.

### Parte 3: Teclas

Três teclas fazem duas coisas ao mesmo tempo. Mude um dos lados de cada par que você usa:

| Tecla | Lado do HUD | Lado do LSPDFR |
| --- | --- | --- |
| **Y** | Cumprimentar alguém (`ViExtras.ini` `[Memory] GreetKey`) | Aceitar um chamado (`ACCEPT_CALLOUT_Key` no `lspdfr\keys.ini`) |
| **N** | Flexões (`ViExtras.ini` `[Gym] PushUpsKey`) | Menu de perseguição (`PURSUIT_MENU_Key` no `lspdfr\keys.ini`) |
| **F4** | Console do Script Hook V .NET (`ConsoleKeyBinding` no `ScriptHookVDotNet.ini`) | Console do RAGE Plugin Hook (`ConsoleKey` no `RagePluginHook.ini`) |

Para mudar uma tecla:

1. Feche o jogo.
2. Abra o arquivo no Bloco de Notas.
   - O `ViExtras.ini` fica na sua pasta **`scripts`**.
   - Os outros arquivos ficam na sua **pasta do jogo**.
3. Mude a letra depois do `=`. Escolha uma tecla que nada mais use.
4. Salve o arquivo.

---

## Deu algo errado?

**Continua com problema?** Pergunte no **[nosso Discord de suporte](https://discord.gg/C7gE5SHEzH)**, o desta versão.
Ou [abra uma issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).

| O que acontece | O que fazer |
| --- | --- |
| Uma mensagem do Script Hook V diz **"unknown game version"** (versão do jogo desconhecida) | O GTA V acabou de ser atualizado. O Script Hook V ainda não acompanhou.<br>1. Espere sair um Script Hook V novo.<br>2. Refaça o Passo 2. |
| O jogo inicia, mas **o HUD novo não aparece** | O seu Script Hook V .NET é antigo demais.<br>Refaça o Passo 3 com a versão mais nova. |
| Uma mensagem diz **Unhandled exception in script** (por exemplo **"MiniMap"**) com uma **FileNotFoundException** | O seu Script Hook V .NET é mais antigo do que o HUD precisa (3.7.0 nightly 193).<br>1. Refaça o Passo 3 com a release do **topo** da página nightly.<br>2. Feche o jogo.<br>3. Aperte **Win + R**.<br>4. Digite `%LOCALAPPDATA%\assembly` e aperte **Enter**.<br>5. Apague a pasta **`dl3`** (é só um cache).<br>💡 Quando é isso, o `ScriptHookVDotNet.log` na sua pasta do jogo diz **"Unable to resolve API version"**. |
| Você **atualizou** o mod, mas o jogo ainda age como a **versão antiga** | O Script Hook V .NET pode continuar rodando a cópia salva dos scripts antigos.<br>1. Feche o jogo.<br>2. Aperte **Win + R**.<br>3. Digite `%LOCALAPPDATA%\assembly` e aperte **Enter**.<br>4. Apague a pasta **`dl3`** (é só um cache).<br>Ainda está na versão antiga? Instale de novo com o OpenIV rodando como administrador (Passo 6).<br>Ou copie os scripts você mesmo: veja "Atualizando manualmente" abaixo. |
| **Sem minimapa** no carro | O `ViMinimap.asi` não está ao lado do `GTA5.exe`.<br>Refaça o Passo 7. |
| A sua pasta do jogo tem **`MiniMap.asi`** ou **`MiniMapHudPresent.asi`** (de outro mod de minimapa 3D ou de uma instalação mais antiga) | 1. Apague ele.<br>2. Apague o **`scripts\MiniMap.dll`** também, se ele estiver lá. (O do HUD é o `scripts\ViMinimap.dll`.)<br>3. Mantenha o **`ViMinimap.asi`** ao lado do `GTA5.exe`.<br>💡 É uma cópia mais antiga do plugin do minimapa. O HUD traz a sua própria cópia como `ViMinimap.asi`. Duas cópias carregadas ao mesmo tempo atrapalham uma à outra. |
| O jogo **dá crash ao carregar** | 1. Instale o mod de novo (Passo 6). Ele aumenta os limites do jogo de que o HUD precisa.<br>2. Tem outros mods grandes (personagens extras, mapas)? Tente desligar eles. |
| O jogo **dá crash ao carregar** desde que você adicionou o **Forests of San Andreas** ou o **GTA V Remastered** | As muitas árvores dos mapas podem lotar três dos pools do jogo.<br>1. No OpenIV, ligue o **Edit mode**.<br>2. Abra: `mods` → `update` → `update.rpf` → `common` → `data` → **`gameconfig.xml`**.<br>3. Coloque o `value` do `PoolSize` de **`Building`** em `70000`.<br>4. Coloque **`phInstGta`** em `20000`.<br>5. Coloque **`StaticBounds`** em `20000`.<br>Se um valor já for maior, deixe como está.<br>⚠️ Não instale um `gameconfig.xml` diferente no lugar. Ele substituiria o que o HUD configurou. (Se você fez isso, instale o mod de novo, como no Passo 6.)<br>Ainda dá crash? Adicione o [Heap Adjuster](https://www.gta5-mods.com/tools/heapadjuster), que o próprio guia dos mapas também sugere. |
| Uma mensagem diz **Lucia is not registered** | A Lucia não vem mais incluída com o HUD (veja o Passo 5).<br>**Não tem ela?** Coloque `Enabled = False` embaixo de `[Lucia]` no `scripts\GTA6Hud.ini`. A mensagem some.<br>**Tem um modelo da Lucia?** O pack dela não está no `dlclist.xml`. Adicione a linha dele.<br>💡 Se a mensagem falar de um `VIHudSetup.exe`, ignore essa parte. Este mod não usa ele. |
| **Trocar de personagem dá crash**, e o seu jogo não está em inglês | Os jogadores relatam que isso só acontece quando o jogo está em outro idioma.<br>Até ser corrigido, coloque o idioma do GTA V em **inglês** (English) nas configurações do GTA V no seu launcher. |
| O HUD diz que o seu jogo **não consegue salvar na pasta dele**, e um **aviso do Windows** pede para deixar o **icacls** fazer alterações | O seu jogo está em **Program Files**. O Windows só deixa administradores alterarem arquivos lá. Então o HUD não consegue salvar as configurações dele (e o ReShade não consegue salvar as dele).<br>1. Clique em **Sim**. Isso deixa o seu usuário do Windows alterar a pasta do jogo.<br>(É o mesmo que fazer isso na mão em **Propriedades (Properties)** → **Segurança (Security)** da pasta.)<br>2. Reinicie o jogo uma vez.<br>**Clicou em Não?** Você vai ser perguntado de novo na próxima vez que iniciar o jogo.<br>Para parar a pergunta, coloque `[GameFolder] FixAccess = False` no `ViExtras.ini`. |
| **Outra coisa** | Pergunte no **[Discord de suporte](https://discord.gg/C7gE5SHEzH)**.<br>Ou [abra uma issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues) e anexe estes arquivos da sua pasta do jogo:<br>`ScriptHookVDotNet.log`<br>`ViMinimap.log`<br>`scripts\GTA6Hud.perf.log`<br>`scripts\GTA6Hud.error.log` |

---

## Atualizando manualmente (jogo em Program Files)

O seu jogo está em **Program Files**? (O Rockstar Games Launcher e a Steam colocam ele lá.)
O Windows protege essa pasta. Uma atualização pelo OpenIV nem sempre substitui os arquivos antigos lá.
Rode o OpenIV como administrador (Passo 6), ou copie os scripts novos você mesmo.

💡 A partir da 2.6, o HUD se oferece para consertar a pasta para você na primeira vez que roda: veja a linha do aviso do Windows acima.
Depois disso, as atualizações entram normalmente.

Um `.oiv` é só um zip:

1. Feche o jogo.
2. Copie o **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`** para a sua Área de Trabalho.
3. Renomeie a cópia para **`.zip`**.

   💡 Não consegue ver a parte `.oiv` do nome? No Explorador de Arquivos, ligue **Exibir** → **Mostrar** → **Extensões de nomes de arquivos**.
4. Abra o zip → **`content`** → **`scripts`**.
5. Selecione tudo o que tem lá e copie.
6. Cole na pasta **`scripts`** da sua pasta do jogo.
   Por exemplo: `C:\Program Files\Rockstar Games\Grand Theft Auto V\scripts`.
7. Escolha **Substituir os arquivos no destino (Replace the files in the destination)**.
8. Clique em **Continuar (Continue)** quando o Windows pedir permissão de administrador.
9. Limpe o cache do Script Hook V .NET, para ele carregar os arquivos novos:
   - Aperte **Win + R**.
   - Digite `%LOCALAPPDATA%\assembly` e aperte Enter.
   - Apague a pasta **`dl3`**.

   💡 É só um cache. Ele é criado de novo na próxima vez que você iniciar o jogo.
10. **Vindo da 2.5.4 ou mais antiga?** A 2.5.5 adiciona um pack. Faça isto também:
    - Copie a pasta **`content`** → **`dlcpacks`** → **`vihud`** do zip para **`mods\update\x64\dlcpacks`** na sua pasta do jogo.
    - Instale o mod uma vez com o OpenIV, como no Passo 6. Isso ativa o pack novo.
11. Inicie o jogo.

✅ O `scripts\ViExtras.log` agora começa com uma linha `==== ViExtras …`. Ela mostra a versão que você baixou.

---

## Como remover o mod

1. Na sua **pasta do jogo**, apague o **`ViMinimap.asi`**.
2. Abra a pasta **`scripts`**.
3. Apague estes:
   - **`GTA6Hud.dll`**
   - **`GTA6Hud.ini`**
   - **`ViMinimap.dll`**
   - **`LemonUI.SHVDN3.dll`**
   - os arquivos **`MiniMap.*`**
   - a pasta **`Hud`**
4. Apague a pasta **`mods`**.

   ⚠️ Isso também remove qualquer outro mod que você instalou com o OpenIV.

✅ O seu jogo voltou ao normal. O Script Hook V e o Script Hook V .NET podem ficar. Eles não mudam nada
por conta própria.
