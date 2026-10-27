# ⚡ 빠른 시작: 아주 간단한 버전

*이 가이드는 번역본이에요. 가장 최신 내용은 영어 가이드([QUICK-START.md](QUICK-START.md))에 있어요.*

*GTA VI-Inspired HUD는 **FBICat**이 만들었고, FiveM 버전의 기능이 더해졌어요. 크레딧은 [README](README.md#-credits)에 있어요.*

GTA V 모드를 한 번도 안 해 봤나요? 이 6단계를 순서대로 하세요. 더 자세히 알고 싶다면 각 단계에서 전체 가이드를 보면 돼요:
**[Legacy 가이드](INSTALL-LEGACY.ko.md)** · **[Enhanced 가이드](INSTALL-ENHANCED.ko.md)**.

> [!IMPORTANT]
> ### 💬 도움이 필요하신가요? 지원 Discord에 들어오세요
> 이 버전은 **FBICat의 VI HUD를 포크한 버전**으로, 기능이 더 들어 있어요. 그래서 지원 창구도 따로 있어요:
> **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**, 또는 [GitHub 이슈](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).
> FBICat의 Discord는 그분의 원작 모드를 위한 곳이니, 이 버전에 대한 질문은 저희 쪽으로 가져와 주세요. 감사합니다!

> ⚠️ 스토리 모드 전용이에요. 모드를 GTA Online에 절대 가져가지 마세요.

---

## 1. 내 GTA V는 어떤 버전일까?

GTA V 폴더를 여세요(런처의 "폴더 열기(open folder)" / "로컬 파일 보기(browse local files)" 버튼).

- **`GTA5.exe`** 파일이 보여요 → **Legacy** 버전이에요.
- **`GTA5_Enhanced.exe`** 파일이 보여요 → **Enhanced** 버전이에요.

아래에는 단계마다 어느 버전용인지 적혀 있어요. 헷갈리지 마세요: 잘못된 파일을 쓰면 게임이 튕겨요.

## 2. 그 폴더에 도구 3개 복사하기

| 도구 | Legacy | Enhanced |
| :--- | :--- | :--- |
| **[Script Hook V](http://www.dev-c.com/gtav/scripthookv/)** | `bin` 폴더에서: `ScriptHookV.dll`, `dinput8.dll`, `args.txt` | 똑같아요 |
| **Script Hook V .NET** | **[nightly](https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases)** zip: 그 안의 `ScriptHookVDotNet` 파일 4개 | **[SHVDNE](https://github.com/Chiheb-Bacha/ScriptHookVDotNetEnhanced/releases/latest)**(Enhanced용이에요!): 그 안의 파일 5개 |
| **모드 로더** | **[OpenIV](https://openiv.com/)**: 설치한 다음 **Tools → ASI Manager** → **ASI Loader**와 **OpenIV.asi** 설치 | **[OpenRPF](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced)**: `OpenRPF.asi`를 복사하고, `mods`라는 폴더를 만드세요 |

Enhanced에서 **NaturalVision Enhanced**, **Lively World Expansion**, **Central Real-Life Traffic / Environment**, **AuraV-Lite**를 쓰나요? **[Enhanced 가이드](INSTALL-ENHANCED.ko.md)에서** 각각의 "선택 사항" 섹션을 따르세요(NVE를 쓰면 OpenRPF는 건너뛰고, LWE를 쓰면 LWE의 `dlclist.xml`은 복사하지 말고, Central을 쓰면 Central의 `dlclist.xml`에 HUD의 줄을 다시 추가하세요).

## 3. 모드 다운로드하기

**[Releases](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest)에서** 다음을 받으세요:

- **`GTA-VI-Inspired-HUD-2.7.0-Legacy.oiv`** *또는* **`GTA-VI-Inspired-HUD-2.7.0-Enhanced.oiv`** (내 버전용 하나만)
- *(선택)* Lucia는 포함되어 있지 않아요: Lucia 모델은 다른 사람이 만든 유료 작품이에요. 제작자에게서 받으세요.
  **[Patreon의 Lucia Caminos](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)** 또는 gta5-mods.com의 **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)**(Jason은 같은 제작자의 **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)**, 또는 mod-hub.net의 **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)**).

## 4. 모드 설치하기

- **Legacy:** OpenIV → **Tools → Package Installer** → 파일 선택 → **mods folder**(mods 폴더)에 설치.
- **Enhanced:** **[OIV Package Installer](https://www.gta5-mods.com/tools/oiv-package-installer)**
  (`CodeWalker.OIVInstaller.exe`) → 파일을 끌어다 놓기 → GTA V 폴더 선택 → **Install**.
- **게임이 Program Files에 있나요?** (경로가 `C:\Program Files`로 시작해요.) 먼저 OpenIV나
  `CodeWalker.OIVInstaller.exe`를 마우스 오른쪽 버튼으로 클릭 → **관리자 권한으로 실행(Run as administrator)** 메뉴로 실행하세요. 안 그러면 설치 후에도 예전 파일이 남을 수 있어요.
- RegisterPatch는 이제 없어요: 모드 자체 파일이 팩을 켜 줘요.

## 5. 마지막 확인 하나

- **Legacy:** **`ViMinimap.asi`** 파일이 `GTA5.exe` 옆에 있는지 확인하세요. `mods` 안에 있다면 밖으로 옮기세요.
- **Enhanced:** 게임에서 **Settings → Graphics → Screen Type → Windowed Borderless**(설정 → 그래픽 → 화면 유형 → 테두리 없는 창 모드)로 설정하세요(안 그러면 미니맵이 안 보여요).

## 6. 플레이 🎉

**Story Mode**(스토리 모드)를 시작하세요. HUD가 저절로 나타나요.

- **V 누르고 있기:** HUD 설정
- **차 안에서 Q 누르고 있기:** 라디오. **Down**(또는 오른쪽 스틱 아래)을 눌러 **On Demand**로 가면 내 음악을 들을 수 있어요. 먼저
  `scripts\Hud\Radio\CustomMusic` 폴더에 `.mp3` 파일을 넣어 두세요.
- **Left Alt 누르고 있기**(컨트롤러: **D-pad Down**): 캐릭터 선택. GTA VI처럼 카드가 한 줄로 나와요(모델이 있다면 Jason과 Lucia도). **Q / E**(컨트롤러: **LB / RB**)로 고르고, 손을 떼면 바뀌어요

모든 조작법은 **[README](README.md#-controls)에** 있어요.
