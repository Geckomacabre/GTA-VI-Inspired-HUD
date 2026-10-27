# 설치 방법: GTA V **Enhanced**

*이 가이드는 번역본이에요. 가장 최신 내용은 영어 가이드([INSTALL-ENHANCED.md](INSTALL-ENHANCED.md))에 있어요.*

이 가이드는 GTA V **2025년 PC 버전**용이에요. 게임 폴더에 어떤 파일이 있는지 확인하세요:

| 게임 폴더에 있는 파일 | 내 버전 | 볼 가이드 |
| --- | --- | --- |
| **`GTA5_Enhanced.exe`** | Enhanced | **이 가이드** |
| **`GTA5.exe`** | Legacy | **[Legacy 가이드](INSTALL-LEGACY.ko.md)** |

GTA 모드가 처음이신가요? 걱정 마세요. 단계를 순서대로 따라 하고, 하나도 건너뛰지 마세요. 20분 정도 걸려요.

> [!IMPORTANT]
> ### 💬 도움이 필요하신가요? 지원 Discord에 들어오세요
> 이 버전은 **FBICat의 VI HUD를 포크한 버전**으로, 기능이 더 들어 있어요. 그래서 지원 창구도 따로 있어요:
> **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**, 또는 [GitHub 이슈](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).
> FBICat의 Discord는 그분의 원작 모드를 위한 곳이에요. 이 버전에 대한 질문은 저희 쪽으로 가져와 주세요. 감사합니다!

> ⚠️ **모드는 스토리 모드에서만 쓰세요.** 모드를 설치한 채로 GTA Online에 절대 들어가지 마세요. 밴을 당할 수 있어요.
> (아래 도구 중 하나인 Script Hook V는 그렇게 하려고 하면 게임을 꺼 버려요.)

> ⚠️ **Enhanced는 Legacy와 다른 도구를 써요.** 이 가이드에 링크된 다운로드만 쓰세요.
> Legacy 도구와 Legacy 모드를 쓰면 Enhanced가 튕겨요.

---

## 시작하기 전에: 간단한 Windows 설정 두 가지

### 1. 파일 확장명 보이게 하기

이 가이드에 나오는 일부 파일은 이름이 `.asi`나 `.dll`로 끝나요. 이 설정을 켜기 전까지 Windows는 이런 끝부분(확장명)을 숨겨요.

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

## 1단계: GTA V Enhanced 폴더 찾기

**`GTA5_Enhanced.exe`** 파일이 들어 있는 폴더예요. 이 가이드에서는 이 폴더를 **게임 폴더**라고 부를게요.

여는 방법:

| 사용하는 런처 | 할 일 |
| --- | --- |
| **Rockstar Games Launcher** | **설정(Settings)** 메뉴를 여세요.<br>왼쪽에서 **Grand Theft Auto V Enhanced**를 클릭하세요.<br>**폴더 열기(Open Folder)** 버튼(또는 **설치 폴더 보기(View Installation Folder)**)을 클릭하세요. |
| **Steam** | **라이브러리(Library)** 화면을 여세요.<br>**Grand Theft Auto V Enhanced**를 마우스 오른쪽 버튼으로 클릭하세요.<br>**관리(Manage)** → **로컬 파일 보기(Browse local files)** 순서로 클릭하세요. |
| **Epic Games** | **라이브러리(Library)** 화면을 여세요.<br>GTA V Enhanced 아래의 **⋯** 버튼을 클릭하세요.<br>**관리(Manage)** 메뉴를 클릭하세요.<br>"설치(Installation)" 옆의 **폴더 아이콘**을 클릭하세요. |

✅ 이 폴더는 열어 두세요. 여기에 파일을 몇 번 복사하게 돼요.

---

## 2단계: Script Hook V 설치하기 (모드를 실행할 수 있게 해 줘요)

1. [Script Hook V 페이지](http://www.dev-c.com/gtav/scripthookv/)로 가세요.
2. **Download**를 클릭하세요. Legacy와 Enhanced 모두 같은 다운로드를 써요.
3. 압축을 푸세요.
4. 안에 있는 **`bin`** 폴더를 여세요.
5. 다음 파일들을 **게임 폴더**에 복사하세요:
   - **`ScriptHookV.dll`**
   - **`xinput1_4.dll`** (Enhanced에서 `.asi`로 끝나는 모드를 불러와요)
   - **`args.txt`** (모드를 막는 Rockstar의 안티치트 BattlEye를 꺼 줘요)
   - **`dinput8.dll`** (Legacy용 모드 로더예요. Enhanced는 이 파일을 쓰지 않으니, 복사해도 아무 문제 없어요.)
6. ⚠️ `NativeTrainer.asi`는 복사하지 **마세요**. 필요 없는 치트 메뉴예요.

✅ 이제 게임 폴더의 `GTA5_Enhanced.exe` 옆에 `ScriptHookV.dll`과 `xinput1_4.dll`이 있어요.

---

## 3단계: Script Hook V .NET **Enhanced** 설치하기 (이 HUD를 실행할 수 있게 해 줘요)

⚠️ 이건 일반 Script Hook V .NET이 **아니에요**. 일반 버전을 쓰면 게임에 들어간 지 20초쯤 뒤에
Enhanced가 저절로 꺼져요. **SHVDNE**라는 **Enhanced** 에디션이 필요해요.

1. [SHVDNE 릴리스 페이지](https://github.com/Chiheb-Bacha/ScriptHookVDotNetEnhanced/releases/latest)로 가세요.
2. **Assets** 아래에서 **`.zip`** 파일을 다운로드하세요.
3. 압축을 푸세요.
4. 메인 폴더에 있는 **파일 5개**를 **게임 폴더**에 복사하세요:
   - **`ScriptHookVDotNet.asi`**
   - **`ScriptHookVDotNet.ini`**
   - **`ScriptHookVDotNet2.dll`**
   - **`ScriptHookVDotNet3.dll`**
   - **`MinHook.x64.dll`**
5. `Debug`, `Docs`, `Licenses` 폴더와 `README.txt`는 건너뛰세요. 필요 없어요.

Windows가 파일을 바꿀지 물어보나요? **덮어쓰기(Replace)** 버튼을 클릭하세요. (전에 일반 버전을 쓰고 있었다는 뜻이에요.)

✅ 이제 그 5개 파일이 `GTA5_Enhanced.exe` 옆에 있어요.

---

## 4단계: OpenRPF 설치하기 (모드를 안전한 `mods` 폴더에 따로 둬요)

OpenRPF는 게임이 따로 떨어진 **`mods` 폴더**에서 모드를 불러오게 해 줘요. 원래 게임 파일은 절대 바뀌지 않아요.

1. [OpenRPF 페이지](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced)로 가세요.
2. 다운로드하세요.
3. 압축을 푸세요.
4. **`OpenRPF.asi`** 파일을 **게임 폴더**에 복사하세요.
5. ⚠️ 다운로드에 `dsound.dll`도 들어 있나요? **그건 복사하지 마세요.** Script Hook V가 이미 모드
   로더를 줬어요: 2단계의 `xinput1_4.dll`. 로더 두 개가 동시에 있으면 튕겨요.
6. 게임 폴더에 **`mods`** 폴더가 아직 없다면 새로 만드세요.
   (마우스 오른쪽 버튼 클릭 → **새로 만들기(New)** → **폴더(Folder)**.)

✅ `OpenRPF.asi`가 `GTA5_Enhanced.exe` 옆에 있고, `mods` 폴더도 있어요.

---

## 5단계: 모드와 설치 프로그램 다운로드하기

### 파트 1: 모드 받기

1. [**Releases 페이지**](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest)로 가세요.
2. **Assets** 아래에서 **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`** 파일을 다운로드하세요. 모드 본체이고, 약 100 MB예요.
   이 파일이 게임에서 모드를 켜는 일까지 해 줘서, 따로 받을 패치는 없어요.

❌ `GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`는 받지 마세요. 다른 버전용이라 Enhanced를 튕기게 해요.

💡 파일 이름에 있는 숫자가 버전이에요(여기서는 2.7.0). 그래서 어떤 버전을 설치했는지 언제든 알 수 있어요.
도움을 요청할 때 이 숫자도 알려 주세요.

### Lucia로 플레이하고 싶나요?

Lucia는 포함되어 있지 않아요. Lucia 모델은 다른 사람이 만든 유료 작품이거든요. HUD는 Lucia 없이도 잘 돌아가요.

제작자에게서 받으세요:
- **[Patreon의 Lucia Caminos](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)**
- gta5-mods.com의 **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)**

다른 Lucia 모델을 써도 돼요. 그런 다음 아래 "선택 사항: Lucia"를 보고 게임에 넣으세요.

**Jason은** 다음 중 하나를 받으세요:
- 같은 제작자의 **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)**
- mod-hub.net의 **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)**

그런 다음 아래 "선택 사항: Jason"을 보고 게임에 넣으세요.

### 파트 2: 설치 프로그램 받기

OpenIV는 Enhanced에 모드를 설치하지 못해요. 그래서 대신 **OIV Package Installer**를 써요.

1. [gta5-mods 페이지](https://www.gta5-mods.com/tools/oiv-package-installer)로 가세요.
2. 다운로드하세요.
3. 바탕 화면 같은 아무 곳에나 압축을 푸세요.

---

## 6단계: 모드 설치하기

먼저 게임 폴더의 경로를 보세요. 경로에 따라 설치 프로그램을 실행하는 방법이 달라요:

| 게임 폴더 경로가 이렇게 시작하면 | 설치 프로그램 실행 방법 |
| --- | --- |
| `C:\Program Files` (Rockstar Games Launcher, Epic Games, Steam은 게임을 그곳에 설치해요) | **`CodeWalker.OIVInstaller.exe`** 파일을 마우스 오른쪽 버튼으로 클릭 → **관리자 권한으로 실행(Run as administrator)** 메뉴를 고르세요.<br>**예(Yes)** 버튼을 클릭하세요. |
| 그 밖의 경로 | **`CodeWalker.OIVInstaller.exe`** 파일을 실행하세요. |

⚠️ **게임이 Program Files 안에 있나요?** Windows는 그곳의 파일을 관리자만 바꿀 수 있게 해요. **관리자 권한으로 실행** 없이 설치하면
예전 파일이 그대로 남을 수 있어요.

1. OIV Package Installer 폴더를 여세요.
2. 표에 나온 방법대로 **`CodeWalker.OIVInstaller.exe`** 파일을 실행하세요.
3. **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`** 파일을 창 위로 끌어다 놓으세요. (또는 **Browse**를 클릭해서 고르세요.)
4. 게임 폴더를 물어봐요. **`GTA5_Enhanced.exe`** 파일이 있는 폴더를 고르세요.
5. **Install**을 클릭하세요.
6. 끝날 때까지 기다리세요.

✅ 모드를 다시 설치하거나 예전 버전 위에 덮어 설치해도 괜찮아요. 팩이 두 번 등록되는 일은 없어요.

---

## 7단계: 게임 설정 하나 바꾸기

1. 게임을 시작하세요.
2. **Settings**(설정) → **Graphics**(그래픽) 순서로 가세요.
3. **Screen Type**(화면 유형)을 **Windowed Borderless**(테두리 없는 창 모드)로 설정하세요.

💡 Enhanced에서는 미니맵이 게임 위에 떠 있는 작은 창에 그려져요. 그 창은
테두리 없는 창 모드(Borderless)에서만 보여요.

---

## 8단계: 플레이! 🎉

**Story Mode**(스토리 모드)를 불러오세요. 새 HUD가 저절로 나타나요.

| 하고 싶은 것 | 방법 |
| --- | --- |
| **캐릭터 바꾸기** | **Left Alt**를 누르고 있으세요(컨트롤러: **D-pad down**을 누르고 있기).<br>**Q / E** 또는 **Left / Right**로 고르세요(컨트롤러: **LB / RB**).<br>손을 떼세요.<br>Jason과 Lucia는 모델이 있어야 해요(포함 안 됨, 5단계 참고). |
| **미니맵 보기** | 차에 타세요.<br>미니맵은 탈것에 타고 있거나 수배 중일 때만 보여요. |
| **HUD 설정 열기** | **V**를 누르고 있으세요. |
| **내 음악 틀기** | 게임 폴더의 **`scripts\Hud\Radio\CustomMusic`** 폴더에 `.mp3` 파일을 넣으세요.<br>차 안에서 라디오 버튼(**Q**)을 누르고 있으세요.<br>**Down**(컨트롤러: 오른쪽 스틱 아래)을 눌러 **On Demand**로 가세요.<br>`CustomMusic` 안의 폴더 하나하나가 각각 채널이 돼요.<br>**Left / Right**(오른쪽 스틱)로 채널을 바꿔요.<br>**마우스 휠**(D-pad Right / Up)로 곡을 바꿔요. |

---

## 선택 사항: VI 로딩 화면

**얻는 것:** Rockstar의 Vice City 사진이 들어간 FBICat의 GTA VI 로딩 화면.
**필요한 것:** HUD와 같은 릴리스에 있는 **`GTA-VI-Inspired-HUD-<version>-LoadingScreens-Enhanced.oiv`** 파일.
HUD는 이것 없이도 잘 돌아가요. 원할 때만 설치하세요.

HUD 때처럼 **OIV Package Installer**(`CodeWalker.OIVInstaller.exe`) 창에 끌어다 놓으세요.

### 이미 깔려 있는데, 게임 원래 화면으로 되돌리고 싶다면?

2.6.4까지는 로딩 화면이 HUD 패키지 안에 들어 있었어요. 빼는 방법:

1. **CodeWalker RPF Explorer**를 열고 **Edit** → **Edit mode**(편집 모드)를 클릭하세요.
2. 게임 원본의 **`update`** → **`update.rpf`** → `x64` → `data` → `cdimages`를 여세요(`mods` 안에 있는 것 말고요).
3. **`scaleform_frontend.rpf`** 파일을 마우스 오른쪽 버튼으로 클릭 → **Extract**(CodeWalker에서는 **Extract Raw...**)로 바탕 화면에 꺼내세요.
4. **`mods`** → `update` → **`update.rpf`** → `x64` → `data` → `cdimages`를 여세요.
5. 바탕 화면의 **`scaleform_frontend.rpf`** 파일을 그 창으로 끌어다 놓고, 원래 있던 파일을 바꾸세요.

**NaturalVision Enhanced를 쓴다면**, **`onigiri\platform\data\cdimages\scaleform_frontend.rpf`** 파일을 그곳에 복사했을 경우 그 파일도 지우세요.

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
   대신 **`legacy`** 폴더와 **`enhanced`** 폴더가 있나요(Jason Duval Extended Look처럼)? 그럼 **`enhanced`** 폴더를 여세요.
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

### 파트 3: Jason 파일을 Enhanced에서 쓸 수 있게 만들기

Enhanced는 받은 파일을 그대로 쓰지 못해요. 그래서 먼저 변환해야 해요.
**`enhanced`** 폴더(파트 1)에서 꺼낸 파일인가요? 이미 변환된 거예요. 파트 4로 건너뛰세요.

1. **CodeWalker RPF Explorer**(**30_dev48** 버전 이상)를 받으세요. 받는 곳은
   [dexyfex의 Discord](https://discord.gg/BxfKHkk) releases 채널이에요.
2. 바탕 화면에 비어 있는 새 폴더를 만드세요. 이름: **`Jason converted`**
3. CodeWalker RPF Explorer에서 **Tools** → **Asset Converter** 순서로 클릭하세요.
4. **input** 폴더로는 **`jdfinal`** 파일 4개가 있는 폴더를 고르세요. **output** 폴더로는
   **`Jason converted`** 폴더를 고르세요. **Process**를 클릭하세요.
5. **`Jason converted`** 폴더를 여세요. 그 안에 **`jdfinal.ymt`** 파일이 없으면 input 폴더에서 직접 복사해 넣으세요.

### 파트 4: 게임에 넣기

1. **[GTA-VI-Inspired-HUD-Jason-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Jason-Kit.zip)** 파일을 다운로드하고 압축을 푸세요.
2. 키트의 **`jasonped`** 폴더를 게임 폴더 안에 복사하세요:
   - **NaturalVision Enhanced를 쓴다면:** **`onigiri\dlcpacks`** 폴더에
   - **안 쓴다면:** **`mods\update\x64\dlcpacks`** 폴더에
3. CodeWalker RPF Explorer에서 **Edit** → **Edit mode**를 클릭해서 켜세요.
4. 방금 복사한 **`jasonped`** 폴더를 열고, 이어서 **`dlc.rpf`** → `x64` → `models` → `cdimages` →
   **`jasonped.rpf`** 순서로 여세요.
5. **`Jason converted`** 폴더의 파일 4개를 그 창으로 끌어다 놓으세요.
6. **`dlclist.xml`** 파일을 여세요:
   - **NaturalVision Enhanced를 쓴다면:** **`onigiri\common\data\dlclist.xml`** 파일을 **메모장**(Notepad)으로 여세요.
   - **안 쓴다면:** CodeWalker RPF Explorer에서 `mods` → `update` → **`update.rpf`** → `common` → `data` →
     **`dlclist.xml`** 파일을 여세요.
7. **맨 아래**까지 스크롤하세요. **`</Paths>`** 줄의 맨 앞에 커서를 두고 **Enter**를 누른 다음,
   그 위에 새로 생긴 빈 줄에 이걸 붙여 넣으세요:
   ```xml
   <Item>dlcpacks:/jasonped/</Item>
   ```
8. **Ctrl + S**를 눌러 저장하고 닫으세요.
9. 게임을 시작하고 **Left Alt**를 누르고 있으세요(컨트롤러: **D-pad Down**을 누르고 있기). Jason 카드는 Trevor 카드 다음에 있어요.

### 안 되나요?

| 보이는 증상 | 할 일 |
| --- | --- |
| 휠에 Jason이 없어요 | 게임 폴더의 **`scripts\ViSwitch.log`** 파일을 메모장으로 여세요. **"no Jason model in the game"**(게임에 Jason 모델이 없음)이라고 쓰여 있으면 다음 중 하나가 잘못된 거예요: 파일 4개의 이름이 `jdfinal`이 아니거나(파트 2), 파일이 **`jasonped.rpf`** 안에 없거나, `dlclist.xml`에 **`<Item>dlcpacks:/jasonped/</Item>`** 줄이 빠져 있어요. 고친 다음 게임을 다시 시작하세요. |

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

### 파트 3: Lucia 파일을 Enhanced에서 쓸 수 있게 만들기

Enhanced는 받은 파일을 그대로 쓰지 못해요. 그래서 먼저 변환해야 해요.
Lucia 다운로드에 **`enhanced`** 폴더가 있나요? 그 파일들은 이미 변환된 거예요. 그 파일을 쓰고 파트 4로 건너뛰세요.

1. **CodeWalker RPF Explorer**(**30_dev48** 버전 이상)를 받으세요. 받는 곳은
   [dexyfex의 Discord](https://discord.gg/BxfKHkk) releases 채널이에요.
2. 바탕 화면에 비어 있는 새 폴더를 만드세요. 이름: **`Lucia converted`**
3. CodeWalker RPF Explorer에서 **Tools** → **Asset Converter** 순서로 클릭하세요.
4. **input** 폴더로는 Lucia의 **`lucia_s`** 파일이 있는 폴더를 고르세요. **output** 폴더로는
   **`Lucia converted`** 폴더를 고르세요. **Process**를 클릭하세요.
5. **팩 B만:** **`Lucia converted`** 폴더 안에 빈 폴더를 하나 만드세요(이름: **`lucia_s`**). Lucia의 **`lucia_s` 폴더**를
   input으로, 그 새 **`lucia_s`** 폴더를 output으로 해서 Asset Converter를 한 번 더 실행하세요.
   **`lucia_s_p`** 폴더도 있나요? 그 폴더도 똑같이 하세요.
6. **`Lucia converted`** 폴더를 여세요. 그 안에 **`lucia_s.ymt`** 파일이 없으면 input 폴더에서 직접 복사해 넣으세요.

### 파트 4: 게임에 넣기

1. **[GTA-VI-Inspired-HUD-Lucia-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Lucia-Kit.zip)** 파일을 다운로드하고 압축을 푸세요.
2. 내 팩(**`A - 4 files`** 또는 **`B - with a folder`**)에 있는 **`luciaped`** 폴더를 게임 폴더 안에 복사하세요:
   - **NaturalVision Enhanced를 쓴다면:** **`onigiri\dlcpacks`** 폴더에
   - **안 쓴다면:** **`mods\update\x64\dlcpacks`** 폴더에
3. CodeWalker RPF Explorer에서 **Edit** → **Edit mode**를 클릭해서 켜세요.
4. 방금 복사한 **`luciaped`** 폴더를 열고, 이어서 **`dlc.rpf`** → `x64` → `models` → `cdimages` →
   **Lucia 아카이브** 순서로 여세요.
5. **`Lucia converted`** 폴더 안의 모든 것을 그 창으로 끌어다 놓으세요.
6. **`dlclist.xml`** 파일을 여세요:
   - **NaturalVision Enhanced를 쓴다면:** **`onigiri\common\data\dlclist.xml`** 파일을 **메모장**(Notepad)으로 여세요.
   - **안 쓴다면:** CodeWalker RPF Explorer에서 `mods` → `update` → **`update.rpf`** → `common` → `data` →
     **`dlclist.xml`** 파일을 여세요.
7. **맨 아래**까지 스크롤하세요. **`</Paths>`** 줄의 맨 앞에 커서를 두고 **Enter**를 누른 다음,
   그 위에 새로 생긴 빈 줄에 이걸 붙여 넣으세요:
   ```xml
   <Item>dlcpacks:/luciaped/</Item>
   ```
8. **Ctrl + S**를 눌러 저장하고 닫으세요.
9. 게임을 시작하고 **Left Alt**를 누르고 있으세요(컨트롤러: **D-pad Down**을 누르고 있기). Lucia 카드는 맨 마지막에 있어요.

### 안 되나요?

| 보이는 증상 | 할 일 |
| --- | --- |
| **Lucia is not registered**(Lucia가 등록되지 않음)라는 메시지가 떠요 | `dlclist.xml`(**NaturalVision Enhanced**를 쓴다면 NVE의 `onigiri\common\data\dlclist.xml`)에 **`<Item>dlcpacks:/luciaped/</Item>`** 줄이 빠져 있어요. 추가하고 게임을 다시 시작하세요. |
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

## 선택 사항: NaturalVision Enhanced

**얻는 것:** **Razed**의 **NaturalVision Enhanced**(NVE)를 이 HUD와 함께 쓸 수 있어요.
**필요한 것:** NVE의 zip. NVE는 자체 로더(onigiri)로 모드를 불러오기 때문에, 몇 단계가 다르게 진행돼요.

1. 이 가이드의 1단계부터 3단계까지 하세요(Script Hook V와 SHVDNE).
   - ⚠️ **4단계는 건너뛰세요: OpenRPF를 설치하지 마세요.** 게임이 시작될 때 NVE가 `OpenRPF.asi`에 대해 경고해요.
   - 이미 `OpenRPF.asi`를 복사했나요? 게임 폴더에서 지우세요.
2. NVE 설치하기:
   1. NVE의 zip을 여세요.
   2. **`Main Package`** 폴더로 들어가세요.
   3. 그 안의 모든 것을 **게임 폴더**에 복사하세요. (자세한 내용은 NVE 자체의 `README Install Guide.html`에 있어요.)
   4. Windows가 **`xinput1_4.dll`** 파일을 바꿀지 물어보나요? **덮어쓰기(Replace)** 버튼을 클릭하세요. Script Hook V가 쓰는 것과 같은 파일이에요.
3. 5단계와 6단계로 HUD를 설치하세요.
4. HUD의 팩을 **`onigiri\dlcpacks`** 폴더에 복사하세요:
   1. 게임 폴더에서 **`mods\update\x64\dlcpacks`** 폴더를 여세요. 설치 프로그램이 HUD의 팩을 여기에 넣어요.
      NVE의 로더는 이 폴더를 읽지 않아요.
   2. **`vihud`**, **`vihudfonts`**, **`vihudanims`** 폴더를 복사하세요.
   3. **`onigiri\dlcpacks`** 폴더에 붙여 넣으세요.
5. NVE는 자체 **`dlclist.xml`** 파일을 가져오고, 게임은 그 파일을 써요. 거기에 HUD의 팩을 추가하세요:
   1. **`onigiri\common\data\dlclist.xml`** 파일을 여세요(메모장이면 돼요).
   2. **맨 아래**로 스크롤하세요.
   3. 닫는 **`</Paths>`** 줄 바로 위에 이 줄들을 각각 한 줄씩 추가하세요:
   ```xml
   <Item>dlcpacks:/vihud/</Item>
   <Item>dlcpacks:/vihudfonts/</Item>
   <Item>dlcpacks:/vihudanims/</Item>
   ```
   - ⚠️ 맨 위의 `<Paths>` 아래가 아니에요: 게임은 거기 있는 줄을 건너뛰어요.
   - Lucia 모델이 있나요? Lucia 팩의 줄도 추가하세요.
   - NVE의 `gameconfig.xml`은 그대로 두세요.
6. **로딩 화면.** 이 단계는 건너뛰어도 돼요: 그러면 게임 원래 로딩 화면이 나와요. 설치 프로그램은 HUD의
   로딩 화면을 **`mods\update\update.rpf`** 안에 넣었어요. NVE의 로더는 그것도 읽지 않으니, 꺼내서 복사해야 해요:
   1. **CodeWalker RPF Explorer**(**30_dev48** 버전 이상)를 받으세요. 받는 곳은
      [dexyfex의 Discord](https://discord.gg/BxfKHkk) releases 채널이에요.
   2. 게임 폴더에서 **`onigiri\platform\data`** 폴더를 열고, 그 안에 새 폴더를 만드세요. 이름: **`cdimages`**
   3. **CodeWalker RPF Explorer**를 열고 `mods` → `update` → **`update.rpf`** → `x64` → `data` →
      `cdimages`로 들어가세요.
   4. **`scaleform_frontend.rpf`** 파일을 마우스 오른쪽 버튼으로 클릭 → **Extract Raw...** 메뉴를 고르세요. 방금 만든 **`onigiri\platform\data\cdimages`**
      폴더를 고르고 **Save**를 클릭하세요.

   💡 그 파일은 HUD의 로딩 화면이 들어간 게임 메뉴 화면의 사본이에요.
   **게임이 업데이트되면** 이 파일을 지우고, HUD를 다시 설치한 다음, 이 단계를 다시 하세요.
   그래야 업데이트된 게임의 메뉴 화면을 쓸 수 있어요.
7. 7단계(Windowed Borderless)를 하고 플레이하세요.

### NVE 지우기 (HUD는 유지)

**얻는 것:** NVE가 없는 게임, 그리고 그대로 작동하는 HUD.

⚠️ **Lively World Expansion과 Central Real-Life Traffic / Environment("for NVE")** 모드는 NVE를 통해 로드되기 때문에 같이
빠져요.

NVE 파일은 지우지 말고 폴더 하나에 옮겨 둘 거예요. 그래야 나중에 NVE를 다시 넣을 수 있어요.

1. 게임을 닫으세요.
2. 바탕 화면에 새 폴더를 만드세요. 이름: **`NVE removed`**
3. **게임 폴더**에서 다음을 **`NVE removed`** 폴더로 옮기세요:
   - **`NVE.asi`**, **`NVE.bin`**, **`NVE.ini`**
   - **`onigiri.asi`**, **`onigiri.ini`**, **`onigiri`** 폴더
     (이 폴더에는 LWE, Central, 그리고 복사해 둔 HUD 팩도 들어 있어요. HUD는 `mods`에 자기 팩을 따로 갖고 있어요.)
   - **`ReShade.asi`**, **`ReShade.ini`**, **`ReShade2.ini`**, **`reshade-shaders`** 폴더
   - **`renodx-*.addon64`** 파일 전부(AuraV 때문에 따로 빼 두었나요? 그 파일들도 `NVE removed`로 옮기세요)
   - **Central을 썼나요?** 다음도 옮기세요: **`CentralLimits.asi`**, **`CentralLimits.ini`**, **`CentralDlcPatch.asi`**,
     그리고 Central 때문에만 받았다면 **`ResourceExtensions.asi`**, **`ResourceExtensions.ini`**.
4. ⚠️ **`xinput1_4.dll`은 남겨 두세요.** NVE가 바꿔 놓긴 했지만, Script Hook V가 모드를 불러올 때 쓰는 것과 같은 파일이에요.
5. **4단계**(OpenRPF 설치)를 하세요. NVE를 쓸 때는 OpenRPF가 없어야 했어요. NVE가 없으면 HUD는 OpenRPF를 통해
   `mods` 폴더에서 로드돼요.
6. HUD가 아직 `mods` 폴더에 있는지 확인하세요: **`mods\update\x64\dlcpacks`** 폴더를 여세요. **`vihud`**,
   **`vihudfonts`**, **`vihudanims`** 폴더가 보여야 해요.
   - 없나요? **6단계**(HUD 설치)를 다시 하세요.
7. **Lucia나 Jason을 썼나요?** 그 팩들은 방금 옮긴 `onigiri\dlcpacks` 안에 있었어요.
   1. **`NVE removed\onigiri\dlcpacks`** 폴더에서 **`luciaped`** / **`jasonped`** 폴더를 **`mods\update\x64\dlcpacks`** 폴더로 복사하세요.
   2. `mods` → `update` → **`update.rpf`** → `common` → `data` 안의 `dlclist.xml`에 그 줄들을 추가하세요
      ("선택 사항: Jason" / "선택 사항: Lucia"의 파트 4, **안 쓴다면** 방법).
8. 게임을 시작하세요.

✅ 게임 원래 모습과 HUD 자체 로딩 화면으로 HUD가 보여요.

| 보이는 증상 | 할 일 |
| --- | --- |
| **VI UI pack (vihud) is not registered**라는 메시지가 떠요 | HUD의 팩이 `mods` 폴더 목록에 없어요. **6단계**(HUD 설치)를 다시 하세요. |
| 게임이 **저절로 꺼지거나** 로딩 중에 튕겨요 | **`xinput1_4.dll`** 파일을 남겨 두었는지, **4단계**(OpenRPF)를 했는지 확인하세요. 그다음 게임 폴더의 `asiloader.log`를 확인하세요: 더 이상 `NVE.asi`나 `onigiri.asi`가 나오면 안 돼요. |

**NVE를 다시 넣으려면:** **`NVE removed`** 폴더의 모든 것을 게임 폴더로 다시 옮기고, **`OpenRPF.asi`** 파일을 지우세요.

---

## 선택 사항: Lively World Expansion (NaturalVision Enhanced와 함께)

**얻는 것:** **Lively World Expansion**(LWE). 맵을 식물과 소품으로 더 채워 줘요.
**필요한 것:** 먼저 설정해 둔 NVE와 HUD. LWE의 NVE 패키지도 NVE의 onigiri를 통해 로드돼요.

LWE의 zip에는 자체 **`dlclist.xml`** 파일이 들어 있어요. 이 파일은 지금 있는 파일을 **바꿔 버려요**.
LWE의 파일에는 다음이 빠져 있어요:
- HUD의 팩
- NVE의 새 팩 세 개(`NVE_lights`, `NVE_vegetation`, `NVE_vehicles_addon`)
- GTA의 2026년 업데이트

그러니 대신 LWE의 팩을 내 목록에 추가하세요:

1. 먼저 NVE와 HUD를 설정하세요(위의 "선택 사항: NaturalVision Enhanced").
2. LWE의 팩을 복사하세요:
   1. LWE의 zip을 여세요.
   2. 밀도 폴더를 하나 고르세요: **`LWE Enhanced - High Density`**, **`Medium Density`**, **`Low Density`** 중 하나.
   3. 그 안의 **`onigiri\dlcpacks`** 폴더로 들어가세요.
   4. 그 안의 **폴더 5개**를 게임 폴더의 **`onigiri\dlcpacks`** 폴더에 복사하세요.
3. ⚠️ **LWE의 `onigiri\common\data\dlclist.xml`은 복사하지 마세요.** 대신 내 목록을 쓰세요:
   1. 내 **`onigiri\common\data\dlclist.xml`** 파일을 여세요.
   2. 맨 아래로 스크롤하세요.
   3. 닫는 **`</Paths>`** 줄 바로 위에(맨 위의 `<Paths>` 아래가 아니에요) 이 줄들을 각각 한 줄씩 추가하세요.
      이건 High 밀도용이에요:
   ```xml
   <Item>dlcpacks:/lwecityhigh/</Item>
   <Item>dlcpacks:/lwedeserthigh/</Item>
   <Item>dlcpacks:/lweforesthigh/</Item>
   <Item>dlcpacks:/lwemountainshigh/</Item>
   <Item>dlcpacks:/lweprops/</Item>
   ```
   Medium이나 Low라면 처음 4줄의 `high`를 `medium`이나 `low`로 바꾸세요. `lweprops`는 그대로예요.
4. **이미 LWE의 `dlclist.xml`을 내 파일 위에 덮어썼나요?** NVE의 파일을 되돌리세요:
   1. NVE의 zip에서 **`Main Package\onigiri\common\data\dlclist.xml`** 파일을 게임 폴더의 `onigiri\common\data`에
      복사하세요.
   2. HUD의 줄을 추가하세요(NVE 섹션의 5번).
   3. LWE의 줄을 추가하세요(여기 3번).
5. NVE의 `gameconfig.xml`은 그대로 두고 플레이하세요.

LWE의 [설치 페이지](https://livelyworldexpansion.com/installation/)에 자체 그래픽 팁이 있어요.

---

## 선택 사항: Central Real-Life Traffic과 Environment (NaturalVision Enhanced와 함께)

**얻는 것:** **CentralMods**의 **Central Real-Life Traffic**과 **Central Real-Life Environment**.
Los Santos를 실제 자동차와 실제 장소로 채워 줘요.
**필요한 것:** 먼저 설정해 둔 NVE와 HUD, 그리고 Central의 **"Enhanced (for NVE)"** 빌드.
이 모드들은 NVE의 onigiri를 통해 로드돼요.

이 HUD와 함께 쓸 수 있어요. 그런데 각 모드의 **`Main Files`** 폴더에는 자체 **`dlclist.xml`** 파일이 들어 있어요. 이 파일이
내 파일을 **바꿔 버리고**, 그 목록에는 HUD의 팩이 빠져 있어요. 그래서 Central을 넣은 뒤에 HUD의 팩을 다시 추가하세요:

1. 먼저 NVE와 HUD를 설정하세요(위의 "선택 사항: NaturalVision Enhanced").
2. **Traffic을 쓴다면:** **Koukol**의 [ResourceExtensions](https://www.gta5-mods.com/scripts/resource-extensions)를 설치하세요:
   - **`ResourceExtensions.asi`**, **`ResourceExtensions.ini`** 파일을 게임 폴더에 복사하세요.
   - ⚠️ Central Traffic에는 이게 꼭 필요해요. 없으면 로딩이 끝나자마자 게임이 튕겨요.
3. Central을 ReadMe에 나온 대로 설치하세요:
   1. **`Main Files`** 폴더 안의 모든 것을 게임 폴더에 복사하세요.
   2. **덮어쓰기(Replace)** 버튼을 클릭하세요.
   3. 둘 다 쓰나요? 다른 Central 모드도 똑같이 하세요.
4. **Lively World Expansion도 쓰나요?**
   1. Central의 **`Optional`** 폴더를 여세요.
   2. **`LWE Compatibility`** 폴더를 여세요. Traffic에서는 `2. LWE Compatibility`, Environment에서는
      `1. LWE Compatibility`예요. 둘은 같은 거예요.
   3. 내 LWE 밀도에 맞는 폴더를 여세요: **`High density`**, **`Medium density`**, **`Low density`** 중 하나.
   4. 거기 있는 `onigiri` 폴더를 게임 폴더에 복사하세요.
   5. **덮어쓰기(Replace)** 버튼을 클릭하세요.
5. HUD의 줄을 다시 추가하세요:
   1. **`onigiri\common\data\dlclist.xml`** 파일을 여세요.
   2. 맨 아래로 스크롤하세요.
   3. 닫는 **`</Paths>`** 줄 바로 위에(맨 위의 `<Paths>` 아래가 아니에요) 이 줄들을 각각 한 줄씩 추가하세요:
   ```xml
   <Item>dlcpacks:/vihud/</Item>
   <Item>dlcpacks:/vihudfonts/</Item>
   <Item>dlcpacks:/vihudanims/</Item>
   ```
   목록에 Forests of San Andreas, GTA V Remastered, Lucia 모델 같은 다른 팩도 있었나요? 그 줄들도 다시 추가하세요.
6. **Central Traffic이 `onigiri\common\data`에 넣은 `gameconfig.xml`은 그대로 두세요.**
   - 게임 원래 파일이에요. 대신 ResourceExtensions가 한도를 올려 줘요.
   - NVE의 파일을 되돌려 놓지 마세요.
   - 그 파일의 풀(pool) 크기를 올리지 마세요.

알아 두면 좋은 것:
- Central Real-Life Traffic 2.9.1과 Environment 0.2(둘 다 "Enhanced (for NVE)"), NVE, Lively World
  Expansion High, HUD로 **테스트했어요**. 모든 HUD 스크립트가 돌아가는 상태로 스토리 모드가 로드돼요.
- **둘 다 설치하나요?** 두 모드는 같은 `dlclist.xml`을 가져오니까 순서는 상관없어요. HUD의 줄은 두 번째 모드를 넣은 뒤에
  한 번만 추가하세요.
- **Environment만 쓰나요?** 그 목록에는 NVE의 자동차 팩도 빠져 있어요. Central Traffic이 그 자동차를 바꾸기 때문이에요.
  NVE의 자동차를 유지하려면 `<Item>dlcpacks:/NVE_vehicles_addon/</Item>`과 `<Item>dlcpacks:/NVE_vehicles/</Item>`도
  다시 추가하세요.
- **지우려면** Central의 ReadMe를 따르세요. NVE의 `dlclist.xml`을 되돌려 놓았다면 거기에 HUD의 줄을 다시 추가하세요.

---

## 선택 사항: Forests of San Andreas와 GTA V Remastered

**얻는 것:** **Larcius**의 **Forests of San Andreas**와 **GTA V Remastered**(JRod와 le__AK의 맵을 바탕으로 만들어졌어요).
맵을 나무, 덤불, 소품 수만 개로 더 채워 줘요. 이 HUD와 함께 쓸 수 있어요.
**필요한 것:** 먼저 설치한 HUD.

각 모드의 zip에는 팩이 들어 있는 **`GTA V Enhanced`** 폴더가 있어요. 각 팩은 안에 `dlc.rpf`가 들어 있는 폴더예요:

| 팩 폴더 | 출처 |
| --- | --- |
| **`forest_n`** | Forests (북쪽) |
| **`forest_s`** | Forests (남쪽) |
| **`vremastered`** | Remastered |

먼저 HUD를 설치하세요. 그다음 아래에서 내 구성에 맞는 쪽을 고르세요.

### NaturalVision Enhanced를 쓴다면 (OpenRPF 없음, 추가 도구 없음)

1. 원하는 팩 폴더(`forest_n`, `forest_s`, `vremastered`)를 **`onigiri\dlcpacks`** 폴더에 복사하세요.
2. **`onigiri\common\data\dlclist.xml`** 파일을 여세요.
3. `</Paths>` 바로 위에 팩마다 한 줄씩 추가하세요:
   ```xml
   <Item>dlcpacks:/forest_n/</Item>
   <Item>dlcpacks:/forest_s/</Item>
   <Item>dlcpacks:/vremastered/</Item>
   ```
4. NVE의 `gameconfig.xml`은 그대로 두세요.

### NVE 없이 (OpenRPF 사용, 4단계)

1. 원하는 팩 폴더를 게임 폴더의 **`mods\update\x64\dlcpacks`** 폴더에, HUD 팩 옆에 복사하세요.
2. **CodeWalker RPF Explorer**를 받으세요(30_dev48 버전 이상, 받는 곳은
   [dexyfex의 Discord](https://discord.gg/BxfKHkk) releases 채널).
   이 도구가 필요한 이유는, 팩의 줄을 아카이브 안에 있는 `mods\update\update.rpf\common\data\dlclist.xml`에 넣어야 하기 때문이에요.
3. CodeWalker RPF Explorer에서 **File** → **Configure Game...** 순서로 클릭하세요.
4. 게임 폴더를 고르세요.
5. `mods\update\update.rpf` → `common` → `data` → **`dlclist.xml`** 파일을 여세요.
6. 위와 같은 줄을 `</Paths>` 바로 위에 추가하세요.
7. 저장하세요.

알아 두면 좋은 것:
- **이 모드들과 Lively World Expansion 중 하나만 고르세요.** 둘 다 같은 시골 지역에 나무를 심어서, 나무 안에 나무가
  생겨요. Forests와 Remastered는 같이 쓰도록 만들어졌어요.
- **반사:** Enhanced에서는 이 모드들이 반사 셰이더를 가져오지 못해요. 그래서 차 도색, 물웅덩이, 젖은 도로에서 나무와 덤불 주위에
  네모난 상자가 보일 수 있어요. **Ray Traced Reflections**(레이 트레이싱 반사)를 **Very High**나 **Ultra**로 설정하면 이런 일이 없어요.
- **지우려면:** `dlclist.xml`에서 그 줄들을 빼고 팩 폴더를 지우세요.

---

## 선택 사항: AuraV-Lite (패스 트레이싱)

**얻는 것:** **Kypos**의 [AuraV-Lite](https://kyposmods.com)로 GTA V Enhanced에서 패스 트레이싱을 쓸 수 있어요.
이 HUD와 함께 작동해요.
**필요한 것:** AuraV-Lite. Kypos의 사이트에서 다운로드하세요.

⚠️ **NaturalVision Enhanced를 쓰나요?** 대신 아래 "선택 사항: AuraV-Lite와 NaturalVision Enhanced"를 따르세요.

1. **`AuraV.asi`**, **`msdmo.dll`**, **`nvngx_dlssd.dll`** 파일을 **게임 폴더**에 복사하세요.
2. **`AuraV`** 폴더도 게임 폴더에 복사하세요.
3. 게임에서 **Settings**(설정) → **Graphics**(그래픽)로 가세요.
4. **Ray Tracing**과 **NVIDIA DLSS**를 켜세요. 패스 트레이싱에는 둘 다 필요해요.
5. **RenoDX를 쓰나요**(이름이 `renodx-*.addon64`인 ReShade 애드온)? 이 파일들이 게임 폴더에 있으면 AuraV 메뉴를 열 때
   게임이 튕겨요. AuraV를 쓰는 동안에는 `renodx-*.addon64` 파일들을 게임 폴더 밖으로 옮기세요.
6. **AuraV의 메뉴 키를 F10에서 다른 키로 바꾸세요.** F10은 HUD가 설정을 다시 불러올 때 쓰는 키이기도 해요.
   1. **F10**으로 AuraV 메뉴를 한 번 여세요.
   2. **"The key that opens and closes this menu"**(이 메뉴를 열고 닫는 키)를 **F11** 같은 다른 키로 바꾸세요.

알아 두면 좋은 것:
- **DLSS Swapper**가 GTA의 DLSS 파일을 바꾸지 않게 하세요. 패스 트레이싱이 멈출 수 있어요.
- 사양이 낮은 그래픽 카드에서는 가장 무거운 설정을 조심해서 쓰세요.
- 문제가 생기면 `AuraV\AuraV.log`에 이유가 나와요.

**지우려면:** 그 파일 3개와 `AuraV` 폴더를 지우세요.

---

## 선택 사항: AuraV-Lite와 NaturalVision Enhanced

**얻는 것:** AuraV-Lite와 **NaturalVision Enhanced**를 이 HUD와 함께 쓸 수 있어요.
**필요한 것:** 설정해 둔 NVE와 HUD, 그리고 AuraV 다운로드.

방해가 되는 게 두 가지 있어요:
- AuraV의 시작 단축 기능이 NVE의 로더를 튕기게 해요. (NVE는 이미 게임 시작 화면을 알아서 건너뛰어요.)
- NVE에는 RenoDX의 ReShade 애드온이 들어 있는데, 이게 AuraV 메뉴를 튕기게 해요.

이 순서대로 설정하세요:

1. 먼저 NVE와 HUD를 설정하세요(위의 "선택 사항: NaturalVision Enhanced").
2. 게임을 한 번 시작해서 HUD가 보이는지 확인하세요.
3. AuraV 다운로드에서 **`AuraV.asi`**, **`msdmo.dll`**, **`nvngx_dlssd.dll`** 파일을 **게임 폴더**의
   `NVE.asi`, `onigiri.asi` 옆에 복사하세요.
4. **`AuraV`** 폴더도 게임 폴더에 복사하세요. ⚠️ AuraV의 파일은 하나도 `onigiri`에 넣지 마세요.
5. **게임을 시작하기 전에**, **`AuraV`** 폴더 안에 텍스트 파일을 하나 만드세요(이름: **`AuraV.ini`**). 내용은
   다음 줄만 넣으세요:
   ```ini
   [boot]
   skip_legal=0
   cut_logos=0
   skip_menu=0
   story=0

   [AuraV]
   Key=F11
   ```
   - `[boot]` 줄은 AuraV의 시작 단축 기능을 꺼서, NVE의 로더가 튕기지 않게 해요.
   - `Key=F11`은 AuraV의 메뉴 키를 **F10**에서 다른 키로 옮겨요. F10은 HUD가 설정을 다시 불러올 때 쓰는 키예요.
   - AuraV는 이 줄들을 그대로 둬요. 처음 실행될 때 나머지 설정을 파일에 추가해요.
   - ⚠️ 파일 이름이 `AuraV.ini.txt`가 아니라 `AuraV.ini`인지 꼭 확인하세요
     (파일 탐색기: **보기(View)** → **표시(Show)** → **파일 확장명(File name extensions)**).
   - **이미 AuraV를 넣고 게임을 시작했나요?** 그럼 `AuraV\AuraV.ini`가 이미 있어요. 파일을 열고 `[boot]`
     줄을 `0`으로, (`[AuraV]` 아래의) `Key`를 `F11`로 바꾼 다음 저장하세요.
6. **RenoDX의 애드온을 빼 두세요.** NVE는 게임 폴더에 `renodx-*.addon64` 파일을 넣어요. 이 파일들이 있으면 AuraV 메뉴를 열 때
   게임이 튕겨요.
   1. 게임 폴더 밖의 다른 폴더로 옮기세요.
   2. 나중에 AuraV를 지울 때를 대비해 그곳에 보관해 두세요.
   3. NVE 자체의 ReShade(`ReShade.asi`, NVE의 그래픽 설정은 `NVE.ini`에 있음)는 그대로 두세요. AuraV와 같이 작동해요.
7. 게임에서 **Settings**(설정) → **Graphics**(그래픽)로 가세요.
8. **Ray Tracing**과 **NVIDIA DLSS**를 켜세요. 패스 트레이싱에는 둘 다 필요해요.
9. **Windowed Borderless**(7단계)를 유지하세요.
10. 플레이하세요. **F11**로 AuraV 메뉴를 열어요.

알아 두면 좋은 것:
- AuraV-Lite 0.2.7, NVE, HUD로 **테스트했어요**. 패스 트레이싱을 켜고 HUD가 보이는 상태로 스토리 모드가 로드돼요.
- **AuraV가 돌아가고 있나요?** 게임 폴더의 `asiloader.log`에 `AuraV.asi`가 나와요. 스토리 모드가 로드되면 `AuraV\AuraV.log`에 "in the
  world"라고 나와요. 게임이 튕기면 그 로그에 이유가 나와요.
- 위의 "선택 사항: AuraV-Lite"에 있는 팁(DLSS Swapper, 사양이 낮은 그래픽 카드)도 여기에 똑같이 해당돼요.
- **지우려면:** AuraV 파일 3개와 `AuraV` 폴더를 지우세요. RenoDX의 애드온은 게임 폴더에 다시 넣으세요.

---

## 선택 사항: LSPDFR

**LSPDFR**(경찰관이 되어 플레이)은 **Legacy**용으로 만들어졌어요.
Enhanced용 LSPDFR은 아직 초기 프리뷰 단계예요. HUD를 그것과 함께 테스트하지는 않았어요.

그래도 해 보고 싶나요? **[Legacy 가이드의 LSPDFR 부분](INSTALL-LEGACY.ko.md#optional-lspdfr)을** 보면 다음 내용이 나와요:

- 근무 중에 HUD가 하는 일(아무것도 바꿀 필요 없어요)
- 어떤 키가 한 번에 두 가지 일을 하는지, 그리고 그 키를 바꾸는 방법

---

## 문제가 생겼나요?

왼쪽 열에서 내 증상을 찾으세요. 오른쪽 열에 나온 대로 하세요.

**그래도 해결이 안 되나요?** 이 버전을 위한 **[저희 지원 Discord](https://discord.gg/C7gE5SHEzH)에서** 물어보세요. 또는
[이슈를 여세요](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).

| 증상 | 할 일 |
| --- | --- |
| 게임이 **로딩 후 20초쯤 지나서 저절로 꺼져요** | 일반 Script Hook V .NET을 쓰고 있어요.<br>1. **SHVDNE**로 3단계를 다시 하세요.<br>2. `%LOCALAPPDATA%\assembly` 안의 **`dl3`** 폴더를 지우세요(예전 버전의 캐시예요).<br>**어떤 버전인지 확인하려면:** 게임 폴더에서 **`ScriptHookVDotNet.asi`** 파일을 마우스 오른쪽 버튼으로 클릭 → **속성(Properties)** → **자세히(Details)** 탭을 보세요. SHVDNE라면 거기에 **"Script Hook V .Net Enhanced"** 문구가 나와요. 일반 버전은 그렇게 나오지 않아요(버전이 3.6.0.0이에요).<br>**그래도 계속 그런가요?** ReShade 애드온(게임 폴더에 있는 `.addon64`로 끝나는 파일)이 충돌하고 있을 수 있어요. 그 파일들을 다른 곳으로 옮기고 다시 해 보세요. |
| 게임 폴더에 **`MiniMap.asi`** 또는 **`MiniMapHudPresent.asi`** 파일이 있어요(다른 3D 미니맵 모드나 예전 설치에서 남은 것) | 1. 그 파일을 지우세요.<br>2. `scripts\MiniMap.dll` 파일도 있다면 지우세요. (HUD의 파일은 `scripts\ViMinimap.dll`이에요.)<br>3. `ViMinimap.asi`는 `GTA5_Enhanced.exe` 옆에 그대로 두세요.<br>그건 HUD가 `ViMinimap.asi`로 직접 가져오는 미니맵 플러그인의 예전 사본이에요. 두 사본이 동시에 로드되면 서로 방해해요. |
| **AuraV-Lite 메뉴를 열면 게임이 튕겨요** | 게임 폴더에 RenoDX의 ReShade 애드온(`renodx-*.addon64`)이 있어요. NVE에 들어 있는 파일이에요.<br>AuraV를 쓰는 동안에는 그 파일들을 빼 두세요.<br>위의 "선택 사항: AuraV-Lite"나 "선택 사항: AuraV-Lite와 NaturalVision Enhanced"를 보세요. |
| Microsoft Visual C++의 **"Debug Assertion Failed!"** 창에 **`renodx-dlssfix.addon64`** 파일 이름이 나와요 | 그건 HUD가 아니라 RenoDX의 DLSS 수정 애드온이에요. 그 애드온의 디버그 빌드예요.<br>DLSS 5를 안 쓰나요? 그럼 필요 없어요: 게임 폴더에서 `renodx-dlssfix.addon64`를 지우세요.<br>`renodx-gtav-enhanced.addon64`는 남겨 둬도 돼요.<br>이 문제 때문에 Visual C++를 다시 설치할 필요는 없어요. |
| **AuraV-Lite**를 **NaturalVision Enhanced**와 같이 쓰는데, 게임이 **시작할 때 튕겨요** | 1. `AuraV\AuraV.ini`의 `[boot]` 아래에서 `skip_legal`, `cut_logos`, `skip_menu`, `story`를 `0`으로 설정하세요.<br>2. 게임을 다시 시작하세요.<br>위의 "선택 사항: AuraV-Lite와 NaturalVision Enhanced"를 보세요. |
| **NaturalVision Enhanced**를 쓰는데, 게임이 **스토리 모드를 로딩하다가 튕기거나** **HUD가 안 보여요** | 위의 "선택 사항: NaturalVision Enhanced"를 따르세요:<br>• OpenRPF 없음<br>• HUD의 팩을 `onigiri\dlcpacks`에 복사<br>• NVE의 `dlclist.xml`에 그 팩들을 등록<br>NVE는 자체 `dlclist.xml`과 `gameconfig.xml`을 가져와요(`onigiri\common\data`에 있어요). 게임은 HUD 패키지가 설정한 파일 대신 그 파일들을 써요.<br>NVE의 `dlclist.xml`을 여세요. `</Paths>` 바로 위에 이 줄들을 각각 한 줄씩 추가하세요:<br>`<Item>dlcpacks:/vihud/</Item>`<br>`<Item>dlcpacks:/vihudfonts/</Item>`<br>`<Item>dlcpacks:/vihudanims/</Item>`<br>⚠️ NVE의 `gameconfig.xml`은 NVE가 준 그대로 두세요. 풀(pool) 크기를 더 올리면 로딩 중에 게임 메모리가 부족해져요. |
| **NaturalVision Enhanced**를 쓰는데, HUD의 GTA VI 화면 대신 **게임 원래 로딩 화면이 나와요** | 위의 "선택 사항: NaturalVision Enhanced"의 6번을 하세요.<br>NVE의 로더는 `mods` 폴더를 읽지 않아요. 그래서 로딩 화면을 `onigiri\platform\data\cdimages`에 복사해야 해요. |
| **NaturalVision Enhanced**를 쓰는데, **게임 업데이트 이후로** 게임 메뉴나 로딩 화면이 이상해요 | 1. `onigiri\platform\data\cdimages\scaleform_frontend.rpf`를 지우세요. 업데이트 전 게임 메뉴 화면의 사본이에요.<br>2. HUD를 다시 설치하세요.<br>3. "선택 사항: NaturalVision Enhanced"의 6번을 다시 하세요. |
| **Lively World Expansion**(LWE)을 설치했더니 **HUD 일부가 사라졌어요** | LWE의 `dlclist.xml`이 NVE의 파일을 바꿔 버렸어요. 그 파일에는 HUD의 팩(그리고 NVE 자체 팩 일부)이 없어요.<br>1. NVE의 `dlclist.xml`을 되돌리세요.<br>2. 거기에 LWE의 줄과 HUD의 줄을 추가하세요.<br>위의 "선택 사항: Lively World Expansion"을 보세요. |
| **Central Real-Life Traffic**이나 **Environment**를 설치했더니 **HUD 일부가 사라졌어요** | Central의 `dlclist.xml`이 내 파일을 바꿔 버렸어요. 그 파일에는 HUD의 팩이 없어요.<br>거기에 HUD의 줄을 다시 추가하세요: 위의 "선택 사항: Central Real-Life Traffic과 Environment"를 보세요. |
| **Central Real-Life Traffic**을 넣은 뒤로 게임이 **로딩 화면이 끝나자마자 튕겨요** | 1. **ResourceExtensions**를 설치하세요(Central에 필요해요).<br>2. Central이 `onigiri\common\data`에 넣은 기본 `gameconfig.xml`을 그대로 두세요.<br>ResourceExtensions는 수정된 파일을 지원하지 않아요. NVE의 파일도 마찬가지예요. |
| **Forests of San Andreas**나 **GTA V Remastered**를 넣었는데 **새 나무가 안 보여요** | 이 팩들은 게임이 팩을 불러오는 곳에 있어야 해요:<br>• **NaturalVision Enhanced를 쓴다면:** `onigiri\dlcpacks`에 넣고, 그 줄을 `onigiri\common\data\dlclist.xml`에 추가.<br>• **안 쓴다면:** `mods\update\x64\dlcpacks`에 넣고, 그 줄을 `mods\update\update.rpf`의 `dlclist.xml`에 추가.<br>위의 "선택 사항: Forests of San Andreas와 GTA V Remastered"를 보세요. |
| Script Hook V 메시지에 **"unknown game version"**(알 수 없는 게임 버전)이라고 떠요 | GTA V가 막 업데이트됐는데, Script Hook V가 아직 따라오지 못한 거예요.<br>1. 새 Script Hook V가 나올 때까지 기다리세요.<br>2. 2단계를 다시 하세요. |
| 게임은 켜지는데 **새 HUD가 안 보여요** | 3단계의 SHVDNE 파일 5개가 `GTA5_Enhanced.exe` 옆에 있는지 확인하세요. |
| 모드를 **업데이트**했는데 게임이 아직 **예전 버전**처럼 동작해요 | Script Hook V .NET이 예전 스크립트의 저장된 사본을 계속 실행하고 있을 수 있어요.<br>1. 게임을 닫으세요.<br>2. **Win + R**을 누르세요.<br>3. `%LOCALAPPDATA%\assembly`를 입력하고 Enter를 누르세요.<br>4. **`dl3`** 폴더를 지우세요. 그냥 캐시예요.<br>**그래도 예전 버전인가요?** OIV Package Installer를 관리자 권한으로 실행해서 다시 설치하세요(6단계). 아니면 스크립트를 직접 복사해 넣으세요: 아래 "직접 업데이트하기"를 보세요. |
| 차 안에서 **미니맵이 없어요** | 게임을 **Windowed Borderless**(테두리 없는 창 모드)로 설정하세요(7단계). |
| **.NET**이나 **Visual C++** 관련 오류가 나와요 | 1. [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net48-web-installer)을 설치하세요.<br>2. [Visual C++ Redistributable (x64)](https://aka.ms/vs/17/release/vc_redist.x64.exe)를 설치하세요.<br>3. PC를 다시 시작하세요. |
| **Lucia is not registered**라는 메시지가 떠요 | 이제 HUD에는 Lucia가 포함되어 있지 않아요(5단계 참고).<br>**Lucia가 없나요?** `scripts\GTA6Hud.ini`의 `[Lucia]` 아래에서 `Enabled = False`로 설정하세요. 메시지가 사라져요.<br>**Lucia 모델이 있나요?** `dlclist.xml`(**NaturalVision Enhanced**를 쓴다면 NVE의 파일)에 Lucia 팩이 빠져 있어요. 그 줄을 추가하세요.<br>메시지에 `VIHudSetup.exe`가 나오면 그 부분은 무시하세요: 이 모드는 그걸 쓰지 않아요. |
| **캐릭터를 바꾸면 튕기고**, 게임 언어가 영어가 아니에요 | 플레이어들 말로는 게임이 다른 언어로 설정되어 있을 때만 이런 일이 생긴대요.<br>고쳐질 때까지: 게임 런처의 GTA V 설정에서 GTA V 언어를 **English**(영어)로 바꾸세요. |
| HUD가 게임이 **폴더에 저장할 수 없다**고 알려 주고, **Windows 확인 창**이 **icacls**의 변경을 허용할지 물어봐요 | 게임이 **Program Files** 안에 있는데, Windows는 그곳의 파일을 관리자만 바꿀 수 있게 해요. 그래서 HUD가 설정을 저장하지 못해요(ReShade도 자기 설정을 저장하지 못해요).<br>1. **예(Yes)** 버튼을 클릭하세요. 내 Windows 사용자 계정이 게임 폴더를 바꿀 수 있게 해 줘요. 폴더의 **속성(Properties)** → **보안(Security)** 탭에서 직접 하는 것과 같아요.<br>2. 게임을 한 번 다시 시작하세요.<br>**아니요(No)를 눌렀나요?** 다음에 게임을 시작할 때 다시 물어봐요.<br>`ViExtras.ini`의 `[GameFolder] FixAccess = False` 설정으로 더 이상 묻지 않게 할 수 있어요. |
| **그 밖의 문제** | **[지원 Discord](https://discord.gg/C7gE5SHEzH)에서** 물어보세요.<br>또는 [이슈를 열고](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues) 게임 폴더에서 다음 파일을 첨부하세요:<br>• `ScriptHookVDotNet.log`<br>• `scripts\GTA6Hud.perf.log`<br>• `scripts\GTA6Hud.error.log` |

---

## 직접 업데이트하기 (게임이 Program Files에 있을 때)

**이게 필요한 경우:** 게임이 **Program Files** 안에 있을 때예요(Rockstar Games Launcher와 Steam은 게임을 그곳에 설치해요).
Windows는 그 폴더를 보호해요. OIV Package Installer를 관리자 권한으로 실행하지 않으면(6단계), 그 프로그램으로
업데이트해도 예전 파일이 항상 바뀌지는 않아요.

새 스크립트를 직접 복사해 넣을 수도 있어요. `.oiv` 파일은 사실 그냥 zip 파일이에요.

💡 2.6부터는 HUD가 처음 실행될 때 폴더 문제를 대신 고쳐 주겠다고 물어봐요. 위의 Windows 확인 창 항목을 보세요.
그다음부터는 업데이트가 정상적으로 들어가요.

1. 게임을 닫으세요.
2. **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`** 파일을 바탕 화면에 복사하세요.
3. 복사본 이름의 끝부분을 **`.zip`으로** 바꾸세요.
   이름에서 `.oiv` 부분이 안 보이나요? 파일 탐색기에서 **보기(View)** → **표시(Show)** → **파일 확장명(File name extensions)** 옵션을 켜세요.
4. zip을 열고 **`content`** → **`scripts`** 폴더로 들어가세요.
5. 그 안의 모든 것을 선택해서 복사하세요.
6. 게임 폴더 안의 **`scripts`** 폴더에 붙여 넣으세요. 예:
   `C:\Program Files\Rockstar Games\Grand Theft Auto V Enhanced\scripts`.
7. **대상 폴더의 파일 덮어쓰기(Replace the files in the destination)** 항목을 고르세요.
8. Windows가 관리자 권한을 요청하면 **계속(Continue)** 버튼을 클릭하세요.
9. 새 파일을 불러오도록 Script Hook V .NET의 캐시를 지우세요:
   1. **Win + R**을 누르세요.
   2. `%LOCALAPPDATA%\assembly`를 입력하고 Enter를 누르세요.
   3. **`dl3`** 폴더를 지우세요. 그냥 캐시예요. 다음에 게임을 시작할 때 다시 만들어져요.
10. 2.5.4 이하 버전에서 업데이트하나요? 2.5.5에서 팩이 하나 추가됐어요:
    1. zip의 **`content`** → **`dlcpacks`** → **`vihud`** 폴더를 게임 폴더 안의 **`mods\update\x64\dlcpacks`** 폴더에
       복사하세요.
    2. 6단계처럼 OIV Package Installer로 모드를 한 번 설치하세요. 그러면 새 팩이 켜져요.
11. 게임을 시작하세요. 이제 `scripts\ViExtras.log`가 `==== ViExtras …` 줄로 시작해요.
    그 줄에 다운로드한 버전이 나와요.

---

## 모드 지우는 방법

💡 **NaturalVision Enhanced**만 지우고 HUD는 남기고 싶나요? NVE 섹션의 "NVE 지우기 (HUD는 유지)"를
보세요.

### 가장 쉬운 방법

1. **OIV Package Installer**를 여세요.
2. **Manage Mods**로 가세요.
3. 두 패키지를 모두 제거(Uninstall)하세요.

### 또는 직접 지우기

1. **게임 폴더**에서 **`ViMinimap.asi`** 파일을 지우세요.
2. **`scripts`** 폴더를 여세요.
3. 다음을 지우세요:
   - **`GTA6Hud.dll`**
   - **`GTA6Hud.ini`**
   - **`ViMinimap.dll`**
   - **`LemonUI.SHVDN3.dll`**
   - **`MiniMap.*`** 파일들
   - **`Hud`** 폴더
4. **`mods`** 폴더를 지우세요. ⚠️ 그 안에 있는 다른 모드도 같이 지워져요.
5. NaturalVision Enhanced를 쓴다면 다음도 지우세요:
   - **`onigiri\platform\data\cdimages\scaleform_frontend.rpf`** (HUD의 로딩 화면)
   - `onigiri\dlcpacks` 안의 HUD 팩

✅ 게임이 원래대로 돌아왔어요.

Script Hook V, SHVDNE, OpenRPF는 남겨 둬도 돼요. 그 자체로는 아무것도 바꾸지 않아요.
