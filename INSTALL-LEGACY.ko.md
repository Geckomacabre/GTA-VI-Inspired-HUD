# 설치 방법: GTA V **Legacy**

*이 가이드는 번역본이에요. 가장 최신 내용은 영어 가이드([INSTALL-LEGACY.md](INSTALL-LEGACY.md))에 있어요.*

이 가이드는 GTA V **오리지널 PC 버전**용이에요.

게임 폴더에 **`GTA5_Enhanced.exe`** 파일이 있나요? 그렇다면 Enhanced 버전이에요.
이 가이드 대신 **[Enhanced 가이드](INSTALL-ENHANCED.ko.md)를** 보세요.

**GTA 모드가 처음이신가요?** 걱정 마세요. 단계를 순서대로 따라 하고, 하나도 건너뛰지 마세요. 20분 정도 걸려요.

> [!IMPORTANT]
> ### 💬 도움이 필요하신가요? 지원 Discord에 들어오세요
> 이 버전은 **FBICat의 VI HUD를 포크한 버전**으로, 기능이 더 들어 있어요. 그래서 지원 창구도 따로 있어요:
> - **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**
> - 또는 [GitHub 이슈](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues)
>
> FBICat의 Discord는 그분의 원작 모드를 위한 곳이에요. 이 버전에 대한 질문은 저희 쪽으로 가져와 주세요. 감사합니다!

> ⚠️ **모드는 스토리 모드에서만 쓰세요.** 모드를 설치한 채로 GTA Online에 절대 들어가지 마세요. 밴을 당할 수 있어요.
> (아래 도구 중 하나인 Script Hook V는 그렇게 하려고 하면 게임을 꺼 버려요.)

---

## 시작하기 전에: 간단한 Windows 설정 두 가지

### 1. 파일 확장명 보이게 하기

이 가이드에 나오는 일부 파일은 이름이 `.asi`나 `.dll`로 끝나요. Windows는 기본적으로 이런 끝부분(확장명)을 숨겨요.

1. 아무 폴더나 여세요.
2. 위쪽의 **보기(View)** 메뉴를 클릭하세요.
3. **표시(Show)** → **파일 확장명(File name extensions)** 순서로 클릭하세요.

### 2. 압축 푸는 법 알아 두기

다운로드 파일은 대부분 `.zip` 파일(폴더 하나를 파일 하나로 꾹 눌러 담은 것)이에요. 여는 방법:

1. 파일을 마우스 오른쪽 버튼으로 클릭하세요.
2. **모두 압축 풀기(Extract All…)** 메뉴를 고르세요.
3. **압축 풀기(Extract)** 버튼을 클릭하세요.

✅ 바로 옆에 일반 폴더가 생겨요.

---

## 1단계: GTA V 폴더 찾기

**`GTA5.exe`** 파일이 들어 있는 폴더예요. 이 가이드에서는 이 폴더를 **게임 폴더**라고 부를게요.

여는 방법:

| 사용하는 런처 | 할 일 |
| --- | --- |
| **Rockstar Games Launcher** | 1. **설정(Settings)** 메뉴를 여세요.<br>2. 왼쪽에서 **Grand Theft Auto V**를 클릭하세요.<br>3. **폴더 열기(Open Folder)** 버튼(또는 **설치 폴더 보기(View Installation Folder)**)을 클릭하세요. |
| **Steam** | 1. **라이브러리(Library)** 화면으로 가세요.<br>2. **Grand Theft Auto V**를 마우스 오른쪽 버튼으로 클릭하세요.<br>3. **관리(Manage)** → **로컬 파일 보기(Browse local files)** 순서로 클릭하세요. |
| **Epic Games** | 1. **라이브러리(Library)** 화면으로 가세요.<br>2. GTA V 아래의 **⋯** 버튼을 클릭하세요.<br>3. **관리(Manage)** 메뉴를 클릭하세요.<br>4. "설치(Installation)" 옆의 **폴더 아이콘**을 클릭하세요. |

✅ 이 폴더는 열어 두세요. 여기에 파일을 몇 번 복사하게 돼요.

---

## 2단계: Script Hook V 설치하기 (모드를 실행할 수 있게 해 줘요)

1. [Script Hook V 페이지](http://www.dev-c.com/gtav/scripthookv/)로 가세요.
2. **Download**를 클릭하세요.
3. 압축을 푸세요.
4. 안에 있는 **`bin`** 폴더를 여세요.
5. 다음 파일들을 **게임 폴더**에 복사하세요:
   - **`ScriptHookV.dll`**
   - **`dinput8.dll`** (`.asi`로 끝나는 모드를 불러와요)
   - **`args.txt`** (모드를 막는 Rockstar의 안티치트 BattlEye를 꺼 줘요)
   - **`xinput1_4.dll`** (들어 있다면)

⚠️ `NativeTrainer.asi`는 복사하지 **마세요**. 필요 없는 치트 메뉴예요.

✅ 이제 게임 폴더의 `GTA5.exe` 옆에 `ScriptHookV.dll`과 `dinput8.dll`이 있어요.

---

## 3단계: Script Hook V .NET 설치하기 (이 HUD를 실행할 수 있게 해 줘요)

이 HUD에는 **3.7.0 nightly 193 이상**이 필요해요. nightly 페이지의 **맨 위** 릴리스는 언제나 이 조건에 맞아요.
더 오래된 버전이면 HUD 스크립트가 시작되지 않거나, 그중 하나가 오류를 내며 멈춰요("문제가 생겼나요?" 참고).

1. [Script Hook V .NET nightly 페이지](https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases)로 가세요.
2. **맨 위** 릴리스를 찾으세요.
3. **Assets**를 펼치세요.
4. **`.zip`** 파일을 다운로드하세요.
5. 압축을 푸세요.
6. 다음 파일들을 **게임 폴더**에 복사하세요:
   - **`ScriptHookVDotNet.asi`**
   - **`ScriptHookVDotNet2.dll`**
   - **`ScriptHookVDotNet3.dll`**
   - **`ScriptHookVDotNet.ini`**

✅ 이제 그 4개 파일이 `GTA5.exe` 옆에 있어요.

---

## 4단계: OpenIV 설치하기 (모드를 안전하게 설치해 줘요)

OpenIV는 모드를 따로 떨어진 **`mods` 폴더**에 넣어요. 원래 게임 파일은 절대 바뀌지 않아요.

1. [openiv.com](https://openiv.com/)에서 OpenIV를 다운로드하세요.
2. 설치하세요.
3. OpenIV를 여세요.
4. **GTA V** → **Windows** 순서로 고르세요.
5. 물어보면 **게임 폴더**를 지정해 주세요.
6. 위쪽에서 **Tools** → **ASI Manager** 순서로 클릭하세요.
7. **ASI Loader** 옆의 **Install**을 클릭하세요.
8. **OpenIV.asi** 옆의 **Install**을 클릭하세요.
9. 창을 닫으세요.

✅ OpenIV 준비가 끝났어요.

---

## 5단계: 모드 다운로드하기

1. [**Releases 페이지**](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest)로 가세요.
2. **Assets** 아래에서 **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`** 파일(모드 본체, 약 100 MB)을 다운로드하세요.
   이 파일이 게임에서 모드를 켜는 일까지 해 줘요. 따로 받을 패치는 없어요.

❌ `GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`는 받지 마세요. 그건 다른 버전의 게임용이에요.

**Lucia나 Jason으로 플레이하고 싶나요?** 두 캐릭터의 모델은 포함되어 있지 않아요.

| 캐릭터 | 모델을 받을 곳 |
| --- | --- |
| **Lucia** | Lucia 모델은 다른 사람이 만든 유료 작품이에요. 제작자에게서 받으세요:<br>**[Patreon의 Lucia Caminos](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)** 또는 gta5-mods.com의 **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)**.<br>다른 Lucia 모델을 써도 돼요.<br>게임에 넣는 방법은 아래 "선택 사항: Lucia"를 보세요.<br>HUD는 Lucia 없이도 잘 돌아가요. |
| **Jason** | 같은 제작자의 **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)**, 또는 mod-hub.net의 **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)**.<br>게임에 넣는 방법은 아래 "선택 사항: Jason"을 보세요. |

💡 파일 이름에 있는 숫자가 버전이에요(여기서는 2.7.0). 그래서 어떤 버전을 설치했는지 언제든 알 수 있어요.
도움을 요청할 때 이 숫자도 알려 주세요.

---

## 6단계: OpenIV로 모드 설치하기

⚠️ **게임이 Program Files 안에 있나요?** 게임 폴더 경로가 `C:\Program Files`로 시작하나요?
(Rockstar Games Launcher, Epic Games, Steam은 게임을 그곳에 설치해요.)
그렇다면 **OpenIV**를 마우스 오른쪽 버튼으로 클릭 → **관리자 권한으로 실행(Run as administrator)** 메뉴를 누르고, **예(Yes)** 버튼을 클릭하세요.

💡 Windows는 그곳의 파일을 관리자만 바꿀 수 있게 해요. 관리자 권한 없이 설치하면 예전 파일이 그대로 남을 수 있어요.

1. OpenIV에서 **Tools** → **Package Installer** 순서로 클릭하세요.
2. **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`** 파일을 고르세요.
3. **Install**을 클릭하세요.
4. 어디에 설치할지 물으면 **mods folder**(mods 폴더)를 고르세요.
5. 폴더를 만들겠냐고 물으면 예라고 하세요.
6. 끝났다는 메시지가 나올 때까지 기다리세요.
7. **Close**를 클릭하세요.

✅ 모드를 다시 설치하거나 예전 버전 위에 덮어 설치해도 괜찮아요. 팩이 두 번 등록되는 일은 없어요.

---

## 7단계: 파일 하나 확인하기

**게임 폴더**에서 **`ViMinimap.asi`** 파일을 찾아보세요. `GTA5.exe` 바로 옆에 있어야 해요.

| 있나요? | 할 일 |
| --- | --- |
| **예** | 좋아요, 8단계로 가세요. |
| **아니요** | 1. 게임 폴더 안의 **`mods`** 폴더를 여세요.<br>2. **`ViMinimap.asi`** 파일을 찾으세요.<br>3. 그 파일을 **`GTA5.exe`** 옆으로 옮기세요. |

💡 이 파일이 없으면 미니맵이 안 보여요.

---

## 8단계: 플레이! 🎉

1. GTA V를 시작하세요.
2. **Story Mode**(스토리 모드)를 고르세요.

✅ 새 HUD가 저절로 나타나요.

| 하고 싶은 것 | 방법 |
| --- | --- |
| **캐릭터 바꾸기** | **Left Alt**를 누르고 있으세요(컨트롤러: **D-pad down**을 누르고 있기).<br>**Q / E** 또는 **Left / Right**로 고르세요(컨트롤러: **LB / RB**).<br>그다음 손을 떼세요.<br>Jason과 Lucia는 모델이 있어야 해요(포함 안 됨, 5단계 참고). |
| **미니맵 보기** | 차에 타세요. 미니맵은 탈것에 타고 있거나 수배 중일 때만 보여요. |
| **HUD 설정 열기** | **V**를 누르고 있으세요. |
| **내 음악 틀기** | 게임 폴더의 **`scripts\Hud\Radio\CustomMusic`** 폴더에 `.mp3` 파일을 넣으세요.<br>차 안에서 라디오 버튼(**Q**)을 누르고 있다가 **Down**(컨트롤러: 오른쪽 스틱 아래)을 눌러 **On Demand**로 가세요.<br>`CustomMusic` 안의 폴더 하나하나가 각각 채널이 돼요.<br>**Left / Right**(오른쪽 스틱)로 채널을 바꿔요.<br>**마우스 휠**(D-pad Right / Up)로 곡을 바꿔요. |

---

## 선택 사항: VI 로딩 화면

**얻는 것:** Rockstar의 Vice City 사진이 들어간 FBICat의 GTA VI 시작 화면과 로딩 화면.
**필요한 것:** HUD와 같은 릴리스에 있는 **`GTA-VI-Inspired-HUD-<version>-LoadingScreens-Legacy.oiv`** 파일.
HUD는 이것 없이도 잘 돌아가요. 원할 때만 설치하세요.

OpenIV에서: HUD 때처럼 **Tools** → **Package Installer**로 가서 파일을 고르고, **Install**을 누른 다음 **mods folder**를 고르세요.

### 이미 깔려 있는데, 게임 원래 화면으로 되돌리고 싶다면?

2.6.4까지는 로딩 화면이 HUD 패키지 안에 들어 있었어요. 빼는 방법:

1. **OpenIV**를 열고 위쪽의 **Edit mode**(편집 모드)를 클릭하세요.
2. 게임 원본의 **`update`** → **`update.rpf`** → `x64` → `data` → `cdimages`를 여세요(`mods` 안에 있는 것 말고요).
3. **`scaleform_frontend.rpf`** 파일을 마우스 오른쪽 버튼으로 클릭 → **Extract**(CodeWalker에서는 **Extract Raw...**)로 바탕 화면에 꺼내세요.
4. **`mods`** → `update` → **`update.rpf`** → `x64` → `data` → `cdimages`를 여세요.
5. 바탕 화면의 **`scaleform_frontend.rpf`** 파일을 그 창으로 끌어다 놓고, 원래 있던 파일을 바꾸세요.

---

## 선택 사항: Jason

**얻는 것:** 캐릭터 휠에서 Trevor 다음 자리에 Jason이 생겨요.
**필요한 것:** Jason 모델(포함 안 됨)과 무료 **Jason 키트**. 파트를 순서대로 따라 하세요.

### 파트 1: Jason 받기

1. 다음 중 **한 곳**에서 Jason을 다운로드하세요:
   - Lucia 제작자의 **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)**
   - mod-hub.net의 **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)**
2. 압축을 푸세요.
3. **`No Cap Version`** 폴더를 여세요(모자 쓴 모습을 원하면 **`Cap Version`** 폴더).
   대신 **`legacy`** 폴더와 **`enhanced`** 폴더가 있나요(Jason Duval Extended Look처럼)? 그럼 **`legacy`** 폴더를 여세요.
4. 안에 **파일 4개**가 있어요. 이름 끝부분은 각각 **`.ydd`**, **`.yft`**, **`.ymt`**, **`.ytd`** 이렇게 되어 있어요.

> 끝부분(`.ydd` 등)이 안 보이나요? 파일 탐색기(File Explorer)에서 **보기(View)** → **표시(Show)** → **파일 확장명(File name extensions)** 순서로 클릭하세요.

### 파트 2: 파일 이름 4개 확인하기

각 파일 이름에서 **점 앞부분**을 보세요:

| 파일 4개의 이름이 | 할 일 |
| --- | --- |
| `jdfinal.ydd`, `jdfinal.yft`, `jdfinal.ymt`, `jdfinal.ytd` | **아무것도 안 해도 돼요.** 파트 3으로 가세요. |
| 그 밖의 이름, 예를 들어 `Jason.ydd`, `Jason.yft`, `Jason.ymt`, `Jason.ytd` | 끝부분은 그대로 두고 **각 파일 이름을 `jdfinal`로 바꾸세요**: `Jason.ydd` → **`jdfinal.ydd`**, `Jason.yft` → **`jdfinal.yft`**, `Jason.ymt` → **`jdfinal.ymt`**, `Jason.ytd` → **`jdfinal.ytd`**. 다른 건 아무것도 바꾸지 마세요. |

이게 끝이에요. HUD는 **`jdfinal`이라는 이름**을 찾기 때문에, 파일 이름만 그렇게 바꾸면 Jason을 찾아내요. 설정을 따로
고칠 필요는 없어요.

### 파트 3: 게임에 넣기

1. **[GTA-VI-Inspired-HUD-Jason-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Jason-Kit.zip)** 파일을 다운로드하고 압축을 푸세요.
2. 키트의 **`jasonped`** 폴더를 게임 폴더 안의 **`mods\update\x64\dlcpacks`** 폴더에 복사하세요.
3. **OpenIV**를 열고 위쪽의 **Edit mode**를 클릭해서 켜세요.
4. OpenIV에서 다음을 차례대로 여세요: `mods` → `update` → `x64` → `dlcpacks` → `jasonped` → **`dlc.rpf`** → `x64`
   → `models` → `cdimages` → **`jasonped.rpf`**.
5. **`jdfinal`** 파일 4개를 그 창으로 끌어다 놓으세요.
6. OpenIV에서 `mods` → `update` → **`update.rpf`** → `common` → `data`를 여세요. **`dlclist.xml`** 파일을 마우스 오른쪽 버튼으로 클릭 → **Edit**를 고르세요.
7. **맨 아래**까지 스크롤하세요. **`</Paths>`** 줄의 맨 앞에 커서를 두고 **Enter**를 누른 다음,
   그 위에 새로 생긴 빈 줄에 이걸 붙여 넣으세요:
   ```xml
   <Item>dlcpacks:/jasonped/</Item>
   ```
8. **Ctrl + S**를 눌러 저장하고 창을 닫으세요.
9. 게임을 시작하고 **Left Alt**를 누르고 있으세요(컨트롤러: **D-pad Down**을 누르고 있기). Jason 카드는 Trevor 카드 다음에 있어요.

### 안 되나요?

| 보이는 증상 | 할 일 |
| --- | --- |
| 휠에 Jason이 없어요 | 게임 폴더의 **`scripts\ViSwitch.log`** 파일을 메모장(Notepad)으로 여세요. **"no Jason model in the game"**(게임에 Jason 모델이 없음)이라고 쓰여 있으면 다음 중 하나가 잘못된 거예요: 파일 4개의 이름이 `jdfinal`이 아니거나(파트 2), 파일이 **`jasonped.rpf`** 안에 없거나, `dlclist.xml`에 **`<Item>dlcpacks:/jasonped/</Item>`** 줄이 빠져 있어요. 고친 다음 게임을 다시 시작하세요. |

### 다른 Jason 모델 (고급)

Jason 다운로드가 위의 파일 4개 형태가 아닐 때만 필요해요.

- 이름에 `^`가 들어간 **파일이 많다면**(예: `Jason^head_000_r.ydd`, `Jason^uppr_000_u.ydd`): **모든** 파일에서
  **`^` 앞부분**을 `jdfinal`로 바꾸세요(`Jason^head_000_r.ydd` → `jdfinal^head_000_r.ydd`). `^`가 없는 파일도
  마찬가지예요(`Jason.ymt` → `jdfinal.ymt`). 전부 `jasonped.rpf`에 넣으세요.
- **자체 팩 형태라면**(자체 `dlc.rpf`가 있어요): 키트는 필요 없어요. 그 팩의 readme에 나온 대로 설치하세요.
  그런 다음 **`scripts\GTA6Hud.ini`** 파일을 메모장으로 열고, **`[Jason]`** 항목을 찾아 **`Model = jdfinal`** 부분을
  그 ped 이름(그 `.ydd` 파일 이름에서 점 앞부분)으로 바꾸세요.

---

## 선택 사항: Lucia

**얻는 것:** 캐릭터 휠의 마지막 카드로 Lucia가 생겨요.
**필요한 것:** Lucia 모델(포함 안 됨)과 무료 **Lucia 키트**. 파트를 순서대로 따라 하세요.

💡 dlcpacks 폴더에 이미 **`luciaped`** 폴더가 있나요? 그럼 Lucia가 이미 설치된 거예요. 이 부분은 건너뛰세요.

### 파트 1: Lucia 받기

1. 다음 중 **한 곳**에서 Lucia를 다운로드하거나, 다른 Lucia 모델을 쓰세요:
   - 제작자가 직접 올린 **[Patreon의 Lucia Caminos](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)**
   - gta5-mods.com의 **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)**
2. 압축을 풀고, Lucia 파일이 들어 있는 폴더를 찾으세요. 파일 이름은 `.yft`, `.ymt` 같은 것으로 끝나요.

> 끝부분(`.yft` 등)이 안 보이나요? 파일 탐색기에서 **보기(View)** → **표시(Show)** → **파일 확장명(File name extensions)** 순서로 클릭하세요.

### 파트 2: 어떤 팩을 쓸지, 그리고 파일 이름

키트에는 **A**와 **B**, 두 가지 팩이 있어요. Lucia 폴더 안에 뭐가 있는지 보세요:

| Lucia 폴더에 있는 것 | 팩 | 할 일 |
| --- | --- | --- |
| `.ydd`, `.yft`, `.ymt`, `.ytd`로 끝나는 **파일 4개** | **A** (`A - 4 files`) | 이미 이름이 `lucia_s`인가요? **아무것도 안 해도 돼요.** 다른 이름인가요(예: `Lucia.ydd`)? 끝부분은 그대로 두고 **각 파일 이름을 `lucia_s`로 바꾸세요**: `Lucia.ydd` → **`lucia_s.ydd`**, `.yft`, `.ymt`, `.ytd`도 똑같이요. 다른 건 아무것도 바꾸지 마세요. |
| **`.yft`** 하나, **`.ymt`** 하나, 그리고 같은 이름의 **폴더** 하나(안에 `head_000_r.ydd` 같은 파일이 가득) | **B** (`B - with a folder`) | 이미 이름이 `lucia_s`인가요? **아무것도 안 해도 돼요.** 다른 이름인가요(예: `Lucia`)? **세 개 모두 이름을 `lucia_s`로 바꾸세요**: `Lucia.yft` → **`lucia_s.yft`**, `Lucia.ymt` → **`lucia_s.ymt`**, 폴더 `Lucia` → **`lucia_s`**. 폴더 안의 파일 이름은 바꾸지 마세요. `_p`로 끝나는 두 번째 폴더(모자와 안경)도 있나요? 그 폴더 이름은 **`lucia_s_p`로** 바꾸세요. |

이게 끝이에요. HUD는 **`lucia_s`라는 이름**을 찾기 때문에, 파일 이름만 그렇게 바꾸면 Lucia를 찾아내요. 설정을 따로
고칠 필요는 없어요.

아래에서 말하는 **Lucia 아카이브**는 팩 A라면 **`luciaped.rpf`** 파일, 팩 B라면 **`luciaped_streamed.rpf`** 파일이에요.

### 파트 3: 게임에 넣기

1. **[GTA-VI-Inspired-HUD-Lucia-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Lucia-Kit.zip)** 파일을 다운로드하고 압축을 푸세요.
2. 내 팩(**`A - 4 files`** 또는 **`B - with a folder`**)에 있는 **`luciaped`** 폴더를
   게임 폴더 안의 **`mods\update\x64\dlcpacks`** 폴더에 복사하세요.
3. **OpenIV**를 열고 위쪽의 **Edit mode**를 클릭해서 켜세요.
4. OpenIV에서 다음을 차례대로 여세요: `mods` → `update` → `x64` → `dlcpacks` → `luciaped` → **`dlc.rpf`** → `x64`
   → `models` → `cdimages` → **Lucia 아카이브**.
5. **`lucia_s`** 파일들을 그 창으로 끌어다 놓으세요(팩 B: **`lucia_s`** 폴더도, 있다면 **`lucia_s_p`** 폴더도
   함께).
6. OpenIV에서 `mods` → `update` → **`update.rpf`** → `common` → `data`를 여세요. **`dlclist.xml`** 파일을 마우스 오른쪽 버튼으로 클릭 → **Edit**를 고르세요.
7. **맨 아래**까지 스크롤하세요. **`</Paths>`** 줄의 맨 앞에 커서를 두고 **Enter**를 누른 다음,
   그 위에 새로 생긴 빈 줄에 이걸 붙여 넣으세요:
   ```xml
   <Item>dlcpacks:/luciaped/</Item>
   ```
8. **Ctrl + S**를 눌러 저장하고 창을 닫으세요.
9. 게임을 시작하고 **Left Alt**를 누르고 있으세요(컨트롤러: **D-pad Down**을 누르고 있기). Lucia 카드는 맨 마지막에 있어요.

### 안 되나요?

| 보이는 증상 | 할 일 |
| --- | --- |
| **Lucia is not registered**(Lucia가 등록되지 않음)라는 메시지가 떠요 | `dlclist.xml`에 **`<Item>dlcpacks:/luciaped/</Item>`** 줄이 빠져 있어요. 추가하고 게임을 다시 시작하세요. |
| 휠에 Lucia가 없어요 | 게임 폴더의 **`scripts\GTA6Hud.perf.log`** 파일을 메모장으로 여세요. **"lucia: no Lucia model in the game"**(게임에 Lucia 모델이 없음)이라고 쓰여 있으면 다음 중 하나가 잘못된 거예요: 파일 이름이 `lucia_s`가 아니거나(파트 2), 파일이 **Lucia 아카이브** 안에 없거나, 다른 팩을 썼거나(파일 4개는 **A**, 폴더가 있는 파일은 **B**), `dlclist.xml`에 **`<Item>dlcpacks:/luciaped/</Item>`** 줄이 빠져 있어요. 고친 다음 게임을 다시 시작하세요. |
| Lucia가 투명하거나 일부만 보여요 | 파일에 맞지 않는 팩을 쓴 거예요(파트 2). 복사한 `luciaped` 폴더를 지우고 다른 팩으로 처음부터 다시 하세요. |

### 다른 Lucia 모델 (고급)

Lucia 다운로드가 파트 2의 두 가지 형태 중 어느 것도 아닐 때만 필요해요.

- 이름에 `^`가 들어간 **파일이 많다면**(예: `Lucia^head_000_r.ydd`): FiveM용으로 배치된 팩 **B** Lucia의 부품들이에요.
  **`lucia_s`** 폴더를 만들고, `^`가 들어간 파일을 전부 그 안으로 옮긴 다음, 각 파일 이름을 `^`
  **뒷부분**으로 바꾸세요(`Lucia^head_000_r.ydd` → `head_000_r.ydd`). `^`가 없는 파일은 이름을
  `lucia_s`로 바꾸고(`Lucia.yft` → `lucia_s.yft`), 팩 B를 쓰세요. `_p^`가 들어간 파일(모자와 안경)도 같은 방식으로
  **`lucia_s_p`** 폴더에 넣으세요.
- **자체 팩 형태라면**(자체 `dlc.rpf`가 있어요): 키트는 필요 없어요. 그 팩의 readme에 나온 대로 설치하세요.
  그런 다음 **`scripts\GTA6Hud.ini`** 파일을 메모장으로 열고, **`[Lucia]`** 항목을 찾아 **`Model = lucia_s`** 부분을
  그 ped 이름(그 `.yft` 파일 이름에서 점 앞부분)으로 바꾸세요.
- 파일 이름을 바꾸는 대신 **원래 이름을 그대로 쓰고 싶다면**: 키트의 `README.txt`에 팩의 `peds.meta`로 하는 방법이 나와 있어요.

---

## 선택 사항: Forests of San Andreas와 GTA V Remastered

**얻는 것:** 맵에 나무, 덤불, 소품이 수만 개 더 생겨요.
**필요한 것:** **Larcius**의 **Forests of San Andreas**, **GTA V Remastered**, 또는 둘 다(JRod와 le__AK의 맵을 바탕으로 만들어졌어요). 이 HUD와 함께 쓸 수 있어요.
이 모드들도 OIV로 나오기 때문에, HUD와 같은 방법으로 설치해요.

1. 먼저 HUD를 설치하세요(1단계부터 7단계까지).
2. 각 모드의 zip에서 **`GTA V Legacy`** 폴더를 여세요.
3. OpenIV에서 **Tools** → **Package Installer** 순서로 클릭하세요.
4. 원하는 것을 하나 고르세요:
   - **`Forests of San Andreas (North) Ultimate v….oiv`**
   - **`Forests of San Andreas (South) Ultimate v….oiv`**
   - **`GTA V Remastered Ultimate v….oiv`**
5. 6단계에서처럼 **mods folder**를 고르세요.
6. 원하는 것마다 3번부터 5번까지 다시 하세요.

   💡 각 모드는 게임의 팩 목록에 HUD 팩과 나란히 자기 팩을 추가해요. 그래서 따로 고칠 건 없어요.
7. **Liberty City V Remix**도 쓰나요? 그럼 각 모드의 **`Compatibility Liberty City V Remix`** OIV도 설치하세요.
8. ⚠️ **`gameconfig.xml`은 그대로 두세요.** 이 맵들에는 다른 파일이 필요 없어요.
   다른 `gameconfig.xml`을 설치하면 HUD가 설정해 둔 파일이 바뀌는데, HUD에는 그 파일이 필요해요.
   맵을 넣은 뒤로 로딩 중에 튕기나요? 아래 "문제가 생겼나요?"를 보세요.

**다시 빼려면:** 각 모드의 **`Uninstaller`** OIV를 같은 방법으로 mods folder에 설치하세요.

---

## 선택 사항: Rebalanced Dispatch Enhanced

**얻는 것:** **the RDE Team**의 **Rebalanced Dispatch Enhanced**(RDE). GTA의 경찰 대응을 바꿔 줘요:
새로운 기관, 도로 봉쇄, 헬리콥터, 그리고 최대 **별 10개**까지. HUD는 RDE를 방해하지 않게 비켜 줘요.
**필요한 것:** 먼저 설치한 HUD, 그리고 RDE 다운로드에 들어 있는 **`RDE 4.1.5.oiv`** 파일.

> [!NOTE]
> 2.6.3에서 새로 생겼고, 게임에서는 잠깐만 테스트했어요.
> RDE와 HUD를 같이 쓸 때 뭔가 이상한가요? Discord나 GitHub 이슈로 알려 주세요.

### 파트 1: RDE 설치하기

1. 먼저 HUD를 설치하세요(1단계부터 7단계까지).

   💡 3단계의 Script Hook V .NET이 바로 RDE가 요구하는 nightly 버전이에요.
2. OpenIV에서 **Tools** → **Package Installer** 순서로 클릭하세요.
3. RDE 다운로드에 있는 **`RDE 4.1.5.oiv`** 파일을 고르세요.
4. 6단계에서처럼 **mods folder**를 고르세요.

⚠️ RDE의 **`Compatibility - Community HUD`** 파일은 건너뛰세요. 다른 HUD 모드용이에요.

💡 RDE는 HUD에 필요한 것보다 훨씬 큰 한도가 들어 있는 자체 `gameconfig.xml`을 가져와요. 괜찮아요.
나중에 HUD를 설치하거나 업데이트해도 너무 낮은 한도만 올리기 때문에, RDE의 설정을 되돌리는 일은 없어요.

### 파트 2: RDE가 돌아가는 동안 바뀌는 것

- RDE의 경찰이 맡아요.
- 다음 기능은 빠져요:
  - GTA6Hud의 경찰 AI
  - GTA6Hud 자체의 별 6개(그리고 이 별이 부르는 군대)
  - HUD의 스턴건 체포
- HUD의 별은 RDE의 레벨을 **6개**까지 보여 줘요. 레벨 6부터 10까지는 모두 여섯 번째 별을 켜요.
- HUD를 그대로 두려면 `ViExtras.ini`에서 `[Compat] Rde = False`로 설정하세요.

### 파트 3: RDE 다시 빼기

1. RDE의 **`Uninstaller.oiv`** 파일을 같은 방법으로 설치하세요.
2. HUD를 다시 설치해서(6단계) HUD의 한도와 팩을 되돌리세요.

---

## 선택 사항: Los Santos RED

**얻는 것:** **Greskrendtregk**의 **Los Santos RED**(LSR). 게임을 범죄 샌드박스로 바꿔 줘요.
자체 경찰, 범죄, 수배 레벨 등 많은 걸 갖추고 있어요. HUD는 LSR을 방해하지 않게 비켜 줘요.
**필요한 것:** **RAGE Plugin Hook**과 **RAGENativeUI**. LSR은 Script Hook V .NET이 아니라 RAGE Plugin Hook에서 돌아가요.

> [!NOTE]
> 2.6.3에서 새로 생겼고, 게임에서는 잠깐만 테스트했어요.
> LSR과 HUD를 같이 쓸 때 뭔가 이상한가요? Discord나 GitHub 이슈로 알려 주세요.

⚠️ LSR은 단독으로 쓰세요. LSPDFR이나 Rebalanced Dispatch Enhanced와 같이 쓰지 마세요. 그 모드들도 경찰을 돌려요.

### 파트 1: LSR 설치하기

1. 먼저 HUD를 설치하세요(1단계부터 7단계까지).
2. **[RAGE Plugin Hook](https://ragepluginhook.net/)을** 설치하세요.
3. **[RAGENativeUI](https://github.com/alexguirre/RAGENativeUI/releases)를** 설치하세요. LSR 메뉴에 필요해요.
4. LSR 본체를 LSR 페이지에 나온 대로 설치하세요.
5. **`RAGEPluginHook.exe`** 파일로 게임을 시작하세요.

✅ 스토리 모드가 로드되면 LSR도 로드돼요.

### 파트 2: LSR이 돌아가는 동안 바뀌는 것

LSR이 경찰, 범죄, 수배 레벨을 맡아요. 그래서 HUD는 자기 기능을 하나도 더하지 않아요:

- 목격자 없음
- 경찰 AI, 경찰 기억 없음
- 히트(heat) 없음
- 별 6개 없음
- 스턴건 체포 없음

HUD를 그대로 두려면 `ViExtras.ini`에서 `[Compat] Lsr = False`로 설정하세요.

### 파트 3: 별

LSR은 GTA 자체의 수배 레벨을 0으로 유지하고, 자기만의 별을 그려요.
그래서 LSR을 쓰면 HUD의 별과 수배 상자(wanted box)가 나오지 않아요.

LSR 별 자리에 GTA VI 별을 쓰고 싶나요?

1. HUD의 [`extras/lsr-stars/wantedlevel`](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/tree/main/extras/lsr-stars/wantedlevel) 폴더에서 그림 4개를 찾으세요.
2. 그 그림들을 **`Plugins\LosSantosRED\images\wantedlevel`** 폴더에 복사하세요.
3. LSR의 그림을 이 그림들로 바꾸세요.

### 파트 4: LSR 설정

LSR은 처음 실행될 때 `Plugins\LosSantosRED\Settings.xml` 파일을 만들어요.

1. 게임을 닫으세요.
2. **`Plugins\LosSantosRED\Settings.xml`** 파일을 메모장으로 여세요.
3. 다음 값을 바꾸세요:

| 설정 | 바꿀 값 | 이유 |
| --- | --- | --- |
| `AlwaysShowRadar` | `false` | HUD의 3D 미니맵이 GTA 레이더를 대신해요. LSR이 레이더를 켜면 맵 밑에 레이더가 다시 생겨요. |
| `NeverShowRadar` | `true` | 위와 같아요. |
| `TopDisplayShowWeapon` | `false` | HUD가 이미 오른쪽 위에 무기를 보여 줘요. |
| `ShowStreetDisplay` | `false` | HUD가 이미 미니맵 아래에 거리 이름을 보여 줘요. |

### 파트 5: 키

LSR은 HUD 키 몇 개를 같이 써요. 쓰는 키 쌍마다 한쪽을 바꾸세요:

| 키 | HUD | LSR |
| --- | --- | --- |
| **Left Ctrl** | 웅크리기(`ViCrouch.ini` `[Crouch] Key`) | 웅크리기(LSR 설정의 `CrouchKey`) |
| **K** | 스킬 패널(`ViExtras.ini` `[Skills] Key`) | 대화 중 "아니요" |
| **O** | 라디오 음소거(`ViExtras.ini` `[Radio] MuteKey`) | 상호작용 시작 |
| **Shift + N** | 윗몸일으키기(`ViExtras.ini` `[Gym] SitUpsKey`) | 그룹 모드 |

---

<a id="optional-lspdfr"></a>
## 선택 사항: LSPDFR

**얻는 것:** **G17 Media**의 **LSPDFR**(LSPD First Response). 경찰관이 되어 플레이해요.
근무를 시작하고, 출동 요청(콜아웃)을 받고, 차를 세워 검문해요. HUD는 계속 작동하고, 내가 근무 중인지도 알아요.
**필요한 것:** 먼저 설치한 HUD, 그리고 **[lcpdfr.com](https://www.lcpdfr.com/)에서** 받은 LSPDFR.
LSPDFR은 Script Hook V .NET이 아니라 RAGE Plugin Hook에서 돌아가요.

> [!NOTE]
> **Legacy**에서만 테스트했어요.

⚠️ LSPDFR을 **Los Santos RED**와 같이 쓰지 마세요. 둘 다 경찰을 돌려요.

💡 **Rebalanced Dispatch Enhanced**에는 자체 LSPDFR 파일이 들어 있어서, 이 둘은 같이 쓸 수 있어요.
HUD를 이 둘과 동시에 쓰는 건 아직 테스트하지 않았어요.

### 파트 1: LSPDFR 설치하기

1. 먼저 HUD를 설치하세요(1단계부터 7단계까지).
2. LSPDFR을 LSPDFR 페이지에 나온 대로 설치하세요.
3. **`RAGEPluginHook.exe`** 파일로 게임을 시작하세요.

✅ 스토리 모드가 로드되면 LSPDFR도 로드돼요.

💡 LSPDFR 없이 플레이하나요? 평소처럼 게임을 시작하세요. LSPDFR은 `RAGEPluginHook.exe`를 통해서만 로드돼요.

### 파트 2: 근무 중에 HUD가 하는 일

**아무것도 바꿀 필요가 없어요.** HUD가 알아서 LSPDFR을 찾아요. 근무 중에는:

- **캐릭터 바꾸기 안 됨.** 캐릭터 휠이 열리지 않고, 캐릭터 바꾸기 키를 눌러도 아무 일도 안 일어나요.
  다른 캐릭터로 바꾸면 LSPDFR이 쓰고 있는 경찰관을 빼앗게 되거든요.
- **목격자 없음.** 아무도 도망치거나 전화로 신고하지 않아요. 내가 경찰이니까요.
- **수배 레벨 없음.** 그래도 수배 레벨이 생기면 바로 지워져요.

근무를 마치면 전부 예전처럼 작동해요.

💡 캐릭터를 바꾸고 싶나요? 먼저 근무를 마치세요.

### 파트 3: 키

키 세 개가 한 번에 두 가지 일을 해요. 쓰는 키 쌍마다 한쪽을 바꾸세요:

| 키 | HUD 쪽 | LSPDFR 쪽 |
| --- | --- | --- |
| **Y** | 사람에게 인사하기(`ViExtras.ini` `[Memory] GreetKey`) | 출동 요청 수락(`lspdfr\keys.ini`의 `ACCEPT_CALLOUT_Key`) |
| **N** | 팔굽혀펴기(`ViExtras.ini` `[Gym] PushUpsKey`) | 추격 메뉴(`lspdfr\keys.ini`의 `PURSUIT_MENU_Key`) |
| **F4** | Script Hook V .NET 콘솔(`ScriptHookVDotNet.ini`의 `ConsoleKeyBinding`) | RAGE Plugin Hook 콘솔(`RagePluginHook.ini`의 `ConsoleKey`) |

키를 바꾸는 방법:

1. 게임을 닫으세요.
2. 메모장으로 파일을 여세요.
   - `ViExtras.ini`는 **`scripts`** 폴더에 있어요.
   - 나머지 파일은 **게임 폴더**에 있어요.
3. `=` 뒤의 글자를 바꾸세요. 다른 데서 안 쓰는 키를 고르세요.
4. 파일을 저장하세요.

---

## 문제가 생겼나요?

**그래도 해결이 안 되나요?** 이 버전을 위한 **[저희 지원 Discord](https://discord.gg/C7gE5SHEzH)에서** 물어보세요.
또는 [이슈를 여세요](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).

| 증상 | 할 일 |
| --- | --- |
| Script Hook V 메시지에 **"unknown game version"**(알 수 없는 게임 버전)이라고 떠요 | GTA V가 막 업데이트됐어요. Script Hook V가 아직 따라오지 못한 거예요.<br>1. 새 Script Hook V가 나올 때까지 기다리세요.<br>2. 2단계를 다시 하세요. |
| 게임은 켜지는데 **새 HUD가 안 보여요** | Script Hook V .NET이 너무 오래된 버전이에요.<br>최신 버전으로 3단계를 다시 하세요. |
| **FileNotFoundException**과 함께 **Unhandled exception in script**(예: **"MiniMap"**)라는 메시지가 떠요 | Script Hook V .NET이 HUD에 필요한 버전(3.7.0 nightly 193)보다 오래됐어요.<br>1. nightly 페이지의 **맨 위** 릴리스로 3단계를 다시 하세요.<br>2. 게임을 닫으세요.<br>3. **Win + R**을 누르세요.<br>4. `%LOCALAPPDATA%\assembly`를 입력하고 **Enter**를 누르세요.<br>5. **`dl3`** 폴더를 지우세요(그냥 캐시예요).<br>💡 이 문제라면 게임 폴더의 `ScriptHookVDotNet.log`에 **"Unable to resolve API version"** 문구가 나와 있어요. |
| 모드를 **업데이트**했는데 게임이 아직 **예전 버전**처럼 동작해요 | Script Hook V .NET이 예전 스크립트의 저장된 사본을 계속 실행하고 있을 수 있어요.<br>1. 게임을 닫으세요.<br>2. **Win + R**을 누르세요.<br>3. `%LOCALAPPDATA%\assembly`를 입력하고 **Enter**를 누르세요.<br>4. **`dl3`** 폴더를 지우세요(그냥 캐시예요).<br>그래도 예전 버전인가요? OpenIV를 관리자 권한으로 실행해서 다시 설치하세요(6단계).<br>아니면 스크립트를 직접 복사해 넣으세요: 아래 "직접 업데이트하기"를 보세요. |
| 차 안에서 **미니맵이 없어요** | `ViMinimap.asi`가 `GTA5.exe` 옆에 없어요.<br>7단계를 다시 하세요. |
| 게임 폴더에 **`MiniMap.asi`** 또는 **`MiniMapHudPresent.asi`** 파일이 있어요(다른 3D 미니맵 모드나 예전 설치에서 남은 것) | 1. 그 파일을 지우세요.<br>2. **`scripts\MiniMap.dll`** 파일도 있다면 지우세요. (HUD의 파일은 `scripts\ViMinimap.dll`이에요.)<br>3. **`ViMinimap.asi`** 파일은 `GTA5.exe` 옆에 그대로 두세요.<br>💡 그건 미니맵 플러그인의 예전 사본이에요. HUD는 자체 사본을 `ViMinimap.asi`로 가져와요. 두 사본이 동시에 로드되면 서로 방해해요. |
| 게임이 **로딩 중에 튕겨요** | 1. 모드를 다시 설치하세요(6단계). HUD에 필요한 게임 한도를 올려 줘요.<br>2. 다른 큰 모드(추가 캐릭터, 맵)가 있나요? 그 모드들을 꺼 보세요. |
| **Forests of San Andreas**나 **GTA V Remastered**를 넣은 뒤로 게임이 **로딩 중에 튕겨요** | 맵의 많은 나무가 게임의 풀(pool) 세 개를 가득 채울 수 있어요.<br>1. OpenIV에서 **Edit mode**를 켜세요.<br>2. `mods` → `update` → `update.rpf` → `common` → `data` → **`gameconfig.xml`** 파일을 여세요.<br>3. **`Building`** 항목의 `PoolSize` `value`를 `70000`으로 설정하세요.<br>4. **`phInstGta`** 항목은 `20000`으로 설정하세요.<br>5. **`StaticBounds`** 항목은 `20000`으로 설정하세요.<br>이미 더 높은 값은 그대로 두세요.<br>⚠️ 대신 다른 `gameconfig.xml`을 설치하지 마세요. HUD가 설정해 둔 파일이 바뀌어요. (이미 그랬다면 6단계처럼 모드를 다시 설치하세요.)<br>그래도 튕기나요? 맵 자체 가이드에서도 권하는 [Heap Adjuster](https://www.gta5-mods.com/tools/heapadjuster)를 추가하세요. |
| **Lucia is not registered**라는 메시지가 떠요 | 이제 HUD에는 Lucia가 포함되어 있지 않아요(5단계 참고).<br>**Lucia가 없나요?** `scripts\GTA6Hud.ini`의 `[Lucia]` 아래에서 `Enabled = False`로 설정하세요. 메시지가 사라져요.<br>**Lucia 모델이 있나요?** `dlclist.xml`에 Lucia 팩이 빠져 있어요. 그 줄을 추가하세요.<br>💡 메시지에 `VIHudSetup.exe`가 나오면 그 부분은 무시하세요. 이 모드는 그걸 쓰지 않아요. |
| **캐릭터를 바꾸면 튕기고**, 게임 언어가 영어가 아니에요 | 플레이어들 말로는 게임이 다른 언어로 설정되어 있을 때만 이런 일이 생긴대요.<br>고쳐질 때까지 게임 런처의 GTA V 설정에서 GTA V 언어를 **English**(영어)로 바꾸세요. |
| HUD가 게임이 **폴더에 저장할 수 없다**고 알려 주고, **Windows 확인 창**이 **icacls**의 변경을 허용할지 물어봐요 | 게임이 **Program Files** 안에 있어요. Windows는 그곳의 파일을 관리자만 바꿀 수 있게 해요. 그래서 HUD가 설정을 저장하지 못해요(ReShade도 자기 설정을 저장하지 못해요).<br>1. **예(Yes)** 버튼을 클릭하세요. 내 Windows 사용자 계정이 게임 폴더를 바꿀 수 있게 해 줘요.<br>(폴더의 **속성(Properties)** → **보안(Security)** 탭에서 직접 하는 것과 같아요.)<br>2. 게임을 한 번 다시 시작하세요.<br>**아니요(No)를 눌렀나요?** 다음에 게임을 시작할 때 다시 물어봐요.<br>더 이상 묻지 않게 하려면 `ViExtras.ini`에서 `[GameFolder] FixAccess = False`로 설정하세요. |
| **그 밖의 문제** | **[지원 Discord](https://discord.gg/C7gE5SHEzH)에서** 물어보세요.<br>또는 [이슈를 열고](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues) 게임 폴더에서 다음 파일을 첨부하세요:<br>`ScriptHookVDotNet.log`<br>`ViMinimap.log`<br>`scripts\GTA6Hud.perf.log`<br>`scripts\GTA6Hud.error.log` |

---

## 직접 업데이트하기 (게임이 Program Files에 있을 때)

게임이 **Program Files** 안에 있나요? (Rockstar Games Launcher와 Steam은 게임을 그곳에 설치해요.)
Windows는 그 폴더를 보호해요. 그래서 OpenIV로 업데이트해도 그곳의 예전 파일이 항상 바뀌지는 않아요.
OpenIV를 관리자 권한으로 실행하거나(6단계), 새 스크립트를 직접 복사해 넣으세요.

💡 2.6부터는 HUD가 처음 실행될 때 폴더 문제를 대신 고쳐 주겠다고 물어봐요. 위의 Windows 확인 창 항목을 보세요.
그다음부터는 업데이트가 정상적으로 들어가요.

`.oiv` 파일은 사실 그냥 zip 파일이에요:

1. 게임을 닫으세요.
2. **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`** 파일을 바탕 화면에 복사하세요.
3. 복사본 이름의 끝부분을 **`.zip`으로** 바꾸세요.

   💡 이름에서 `.oiv` 부분이 안 보이나요? 파일 탐색기에서 **보기(View)** → **표시(Show)** → **파일 확장명(File name extensions)** 옵션을 켜세요.
4. zip을 열고 **`content`** → **`scripts`** 폴더로 들어가세요.
5. 그 안의 모든 것을 선택해서 복사하세요.
6. 게임 폴더 안의 **`scripts`** 폴더에 붙여 넣으세요.
   예: `C:\Program Files\Rockstar Games\Grand Theft Auto V\scripts`.
7. **대상 폴더의 파일 덮어쓰기(Replace the files in the destination)** 항목을 고르세요.
8. Windows가 관리자 권한을 요청하면 **계속(Continue)** 버튼을 클릭하세요.
9. 새 파일을 불러오도록 Script Hook V .NET의 캐시를 지우세요:
   - **Win + R**을 누르세요.
   - `%LOCALAPPDATA%\assembly`를 입력하고 Enter를 누르세요.
   - **`dl3`** 폴더를 지우세요.

   💡 그냥 캐시예요. 다음에 게임을 시작할 때 다시 만들어져요.
10. **2.5.4 이하 버전에서 업데이트하나요?** 2.5.5에서 팩이 하나 추가됐어요. 이것도 하세요:
    - zip의 **`content`** → **`dlcpacks`** → **`vihud`** 폴더를 게임 폴더 안의 **`mods\update\x64\dlcpacks`** 폴더에 복사하세요.
    - 6단계처럼 OpenIV로 모드를 한 번 설치하세요. 그러면 새 팩이 켜져요.
11. 게임을 시작하세요.

✅ 이제 `scripts\ViExtras.log`가 `==== ViExtras …` 줄로 시작해요. 그 줄에 다운로드한 버전이 나와요.

---

## 모드 지우는 방법

1. **게임 폴더**에서 **`ViMinimap.asi`** 파일을 지우세요.
2. **`scripts`** 폴더를 여세요.
3. 다음을 지우세요:
   - **`GTA6Hud.dll`**
   - **`GTA6Hud.ini`**
   - **`ViMinimap.dll`**
   - **`LemonUI.SHVDN3.dll`**
   - **`MiniMap.*`** 파일들
   - **`Hud`** 폴더
4. **`mods`** 폴더를 지우세요.

   ⚠️ OpenIV로 설치한 다른 모드도 같이 지워져요.

✅ 게임이 원래대로 돌아왔어요. Script Hook V와 Script Hook V .NET은 남겨 둬도 돼요. 그 자체로는 아무것도
바꾸지 않아요.
