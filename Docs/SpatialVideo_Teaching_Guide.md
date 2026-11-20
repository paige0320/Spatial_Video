# 用 iPhone 拍空間影片，放進 Meta Quest 3 觀看：教學手冊

適用專案版本：Unity 6000.3.23f1 / Meta Quest 3 / OpenXR + Meta XR SDK 209（手把與互動用 Meta 官方 SDK）
（專案裡有哪些東西、播放器怎麼操作與修改，見第 9 節）

---

## 0. 先回答最常被問的問題

### Q1：iPhone 拍的空間影片，要轉檔才能在 Quest 看嗎？

**看你要用哪種方式播放：**

| | 路線 A：Meta 官方 App 直接看 | 路線 B：用本專案（Unity 播放器）看 |
|---|---|---|
| 需不需要 ffmpeg 轉檔 | **不用** | **要**（MV-HEVC → SBS mp4） |
| 需不需要電腦 | 不用（手機就能傳） | 要（Windows 或 Mac） |
| 需不需要 Unity | 不用 | 要（Unity 6000.3.23f1 + Android 模組） |
| 換一支影片要多久 | 1～3 分鐘 | 10～20 分鐘（轉檔 + 匯入 + Build APK） |
| 可以自訂播放介面嗎 | 不行 | 可以（播放列、按鈕、螢幕大小、場景都能改） |
| 適合誰 | 只想看自己拍的 3D 影片 | 想學 Unity / XR 開發、想做自己的 VR 播放器 |

### Q2：為什麼路線 B 一定要轉檔？

iPhone 拍的空間影片是 **MV-HEVC** 格式：左右眼畫面不是並排在同一張畫面裡，而是用「分層」的方式藏在同一條 HEVC 影像流中（一層是主畫面，另一層是另一隻眼睛）。

- Unity 的 VideoPlayer（以及大部分播放器）**只讀得到第一層**，等於只看得到一隻眼睛，沒有立體感。
- 在 Windows 上，Unity 匯入這種 `.MOV` 甚至會直接報 `WindowsVideoMedia error`，完全打不開。

所以要用 ffmpeg 把兩層畫面抽出來，**左右並排拼成一張畫面**（Side-by-Side，簡稱 SBS），存成一般的 H.264 mp4。本專案的 shader 會把左半邊送給左眼、右半邊送給右眼，戴上頭盔就有立體感。

### Q3：建議怎麼安排課程？

本專案的課程主軸是**路線 B**（拍攝 → 轉檔 → 放進本專案 → 裝到頭盔），可直接發 [學生講義](SpatialVideo_Shoot_and_View.md) 照著做。路線 A 可以當暖身：
1. （選用）第一堂開頭先走路線 A，讓學生 10 分鐘內在頭盔裡看到自己拍的 3D 影片，建立成就感、理解「空間影片」是什麼。
2. 主要時間走路線 B，教轉檔原理（左右眼、SBS）和 Unity 播放器，讓學生做出自己的 VR 播放器 App。

（建議課程時數規劃見第 7 節）

---

## 1. 課前準備（老師）

### 1.1 硬體

| 項目 | 說明 |
|---|---|
| iPhone（拍攝） | iPhone 15 Pro / 15 Pro Max、iPhone 16 / 16 Plus / 16 Pro / 16 Pro Max、iPhone 17 / 17 Pro / 17 Pro Max（**iPhone Air、16e 不支援**；以 [Apple 官方清單](https://support.apple.com/guide/iphone/ipheee23829a/ios) 為準）。目前所有機型空間影片都是 1080p 30fps |
| iPhone（只上傳，路線 A） | 任何 iOS 17 以上的 iPhone |
| Meta Quest 3 / 3S | 已完成初始設定、登入 Meta 帳號 |
| 電腦（路線 B） | Windows 10/11 或 Mac，建議 16GB RAM 以上，硬碟預留 30GB（Unity + 專案） |
| USB-C 傳輸線（路線 B） | 要能傳資料的線（不是只能充電的線），用來連 Quest 安裝 APK |

### 1.2 軟體（路線 B）

| 軟體 | 版本 | 用途 |
|---|---|---|
| Unity Hub + **Unity 6000.3.23f1** | 安裝時勾 **Android Build Support**（含 OpenJDK、Android SDK & NDK Tools） | 開專案、Build APK |
| **ffmpeg** | **7.1 以上**（舊版不支援 MV-HEVC 的雙眼抽取） | 轉檔 |
| Git（選用） | 任意版本 | 下載專案 |
| Meta Horizon Link（選用） | 最新版 | Quest Link 直接在 Editor 預覽，不用每次 Build |

### 1.3 Quest 開啟開發者模式（路線 B，每台頭盔做一次）

1. 手機安裝 **Meta Horizon** App，登入跟頭盔相同的帳號。
2. 到 Meta 開發者網站建立（或加入）一個組織，帳號才能開開發者模式。
3. Meta Horizon App →「裝置」→ 選這台頭盔 →「頭戴裝置設定」→「開發人員模式」→ 打開。
4. 頭盔重開機。第一次用 USB 接電腦時，頭盔裡會跳出「允許 USB 偵錯」→ 勾「一律允許」→ 允許。

> Meta App 介面常更新，選單名稱可能略有不同，以 Meta 官方說明為準。

---

## 2. 用 iPhone 拍空間影片

### 2.1 拍攝步驟

1. 打開「相機」App，切到 **空間（Spatial）** 模式（較舊的 iOS 版本是在「錄影」模式中點右上角的空間影片圖示）。
2. **手機一定要橫拿**（鏡頭在上方那種橫拿法，畫面會提示你轉向）。直拿拍不了空間影片。
3. 按錄影鍵開始拍。

### 2.2 拍出好效果的技巧（請教給學生）

| 技巧 | 原因 |
|---|---|
| 主體距離鏡頭約 **1～2.5 公尺** | 太近左右眼差異太大會不舒服，太遠立體感不明顯 |
| **手機保持水平、穩定**，盡量不要快速轉動 | 在頭盔裡畫面晃動非常容易頭暈 |
| **光線要充足** | 第二顆鏡頭在暗處畫質下降明顯 |
| 前景、中景、背景都有東西 | 層次越多，立體感越明顯（例如前面有桌上物品、後面有人） |
| 每段 **10～60 秒** 就好 | 1 分鐘約 130MB 起，太長轉檔、傳輸都很久 |
| 避免手指擋到鏡頭 | 兩顆鏡頭任一顆被擋，左右眼就不一致，看了會很不舒服 |

### 2.3 拍完先在手機上確認

打開「照片」App，空間影片左上角會有 **「空間」** 標籤。沒有這個標籤的就是一般 2D 影片，後面怎麼轉都不會有立體感。

---

## 3. 路線 A：用 Meta 官方 App 直接看（不用轉檔）

> 學生講義 [`SpatialVideo_Shoot_and_View.md`](SpatialVideo_Shoot_and_View.md) 的主線是路線 B（拍攝 → 轉檔 → 本專案 → 頭盔），路線 A 放在講義附錄。

1. iPhone 安裝 **Meta Horizon** App，登入**跟頭盔相同的 Meta 帳號**。
2. App 中點 Horizon 動態最上方的 **選單（Menu）** → **圖庫（Gallery）** → 右上角 **上傳（Upload）** → 選 **空間（Spatial）**。
3. 勾選要上傳的影片 → **上傳（Upload）**，等 App 通知影片已可在頭盔上觀看。
4. 戴上頭盔，打開 **檔案（Files）**（或 **圖庫 Gallery**）→ **空間影片（Spatial videos）** → 點影片播放，就是 3D 立體的。

- 單支空間影片最長 20 分鐘。
- 頭盔要開機並連上 Wi-Fi 才會同步。
- 步驟依據 [Meta 官方說明：Spatial video on Meta Quest](https://www.meta.com/help/quest/372981992147360/)，App 介面若更新請以官方為準。

這條路線的重點是讓學生先體驗「空間影片長什麼樣子」。

---

## 4. 路線 B 步驟一：把影片從 iPhone 傳到電腦（不能讓它被轉成 2D！）

**這是最容易出錯的一步。** iPhone 傳檔時如果被自動轉檔，或透過通訊軟體傳送，**第二隻眼睛的畫面會被丟掉**，變成一般 2D 影片，後面怎麼轉都沒有立體感。

### 4.1 先改 iPhone 設定（每支手機做一次）

`設定` →`App` → `照片` → 最下面 **「傳輸到 Mac 或 PC」** → 選 **「保留原始檔」**（不要選「自動」）。

### 4.2 傳輸方式

| 方式 | 可以嗎 | 說明 |
|---|---|---|
| **Mac：AirDrop** | ✅ | 分享時點上方「選項」，打開「**所有照片資料**」再傳 |
| **Windows：USB 線** | ✅ | iPhone 接電腦，按「信任」，用檔案總管進 `Apple iPhone > Internal Storage > DCIM` 複製 `.MOV`，或用 Windows「相片」App 匯入 |
| iCloud 照片網頁下載 | ⚠️ | 要選下載「未修改的原始檔」 |
| LINE / IG / FB Messenger / Email | ❌ | **會被壓縮成 2D，不能用** |

### 4.3 確認傳過來的是真的空間影片

傳過來的檔案應該是 **`.MOV`**，大小約每分鐘 130MB 以上。用 ffprobe 檢查（ffmpeg 安裝見 5.1）：

```bash
ffprobe -hide_banner IMG_1234.MOV
```

輸出中影像串流是 **hevc**，而且看得到 **Stereo 3D** 相關的資訊，就是正確的空間影片。

---

## 5. 路線 B 步驟二：用 ffmpeg 轉成 SBS 影片

### 5.1 安裝 ffmpeg（7.1 以上）

**Windows**（PowerShell）：
```powershell
winget install Gyan.FFmpeg
```
裝完**關掉再重開** PowerShell，輸入 `ffmpeg -version`，第一行版本號要 **7.1 以上**。

**Mac**（Terminal，需先裝 [Homebrew](https://brew.sh)）：
```bash
brew install ffmpeg
ffmpeg -version
```

### 5.2 轉檔指令

在影片所在的資料夾開終端機，把 `IMG_1234` 換成你的檔名：

**Windows（PowerShell，整行貼上）：**
```powershell
ffmpeg -i IMG_1234.MOV -filter_complex "[0:v:view:0][0:v:view:1]hstack[v]" -map "[v]" -map 0:a:0 -c:v libx264 -preset medium -crf 18 -pix_fmt yuv420p -c:a aac -b:a 128k IMG_1234_SBS.mp4
```

**Mac：**
```bash
ffmpeg -i IMG_1234.MOV \
  -filter_complex "[0:v:view:0][0:v:view:1]hstack[v]" \
  -map "[v]" -map 0:a:0 \
  -c:v libx264 -preset medium -crf 18 -pix_fmt yuv420p \
  -c:a aac -b:a 128k \
  IMG_1234_SBS.mp4
```

**參數說明（可以教給學生）：**

| 參數 | 意思 |
|---|---|
| `[0:v:view:0]` / `[0:v:view:1]` | 從 MV-HEVC 抽出第 0 層（左眼）和第 1 層（右眼）畫面 |
| `hstack` | 把兩張畫面**左右並排**拼成一張 → 這就是 SBS |
| `-c:v libx264` | 輸出成 H.264，Unity 和 Quest 都能順利解碼 |
| `-crf 18` | 畫質（數字越小畫質越好、檔案越大；18 幾乎看不出損失） |
| `-pix_fmt yuv420p` | 標準 8-bit 色彩，避免 HDR / 10-bit 在 Quest 上播不出來 |
| `-map 0:a:0` | 保留第一條音軌 |

轉出來的畫面寬度是兩眼相加（例如兩眼各 1920×1080 → 輸出 **3840×1080**），這叫「全寬 SBS」。

### 5.3 轉完檢查

1. 用電腦播放器打開 `IMG_1234_SBS.mp4`，應該看到**左右兩個幾乎一樣的畫面並排**（仔細看會有一點點位置差，那就是立體感的來源）。
2. 用 ffprobe 確認尺寸：
   ```bash
   ffprobe -v error -select_streams v:0 -show_entries stream=width,height -of csv=p=0 IMG_1234_SBS.mp4
   ```
   應該是 `3840,1080` 之類「寬是高的 3.5 倍左右」的數字。

> **⚠️ 網路上下載的 SBS 影片要特別注意**：有些影片裡帶有 H.264 的「Stereo 3D 並排」標記（frame packing SEI），Windows 解碼器看到會只輸出一隻眼睛，在 Unity Editor 裡變成**上下有黑邊的扁長條**。用下面指令把標記拿掉（**不重新壓縮、畫質完全不變**）：
> ```bash
> ffmpeg -i input_SBS.mp4 -c copy -bsf:v "filter_units=remove_types=6" output_SBS.mp4
> ```
> 自己用 5.2 指令轉出來的影片不會有這個標記，不用處理。

---

## 6. 路線 B 步驟三：放進 Unity 專案並裝到頭盔

### 6.1 取得並開啟專案

1. 下載專案（`git clone` 或在 GitHub 按 Code → Download ZIP 解壓縮）。
2. Unity Hub →「Add」→ 選專案資料夾 → 用 **6000.3.23f1** 開啟。第一次開啟要**連網**下載 Meta XR SDK 並匯入，很久（10～30 分鐘），請耐心等。
   - 如果跳出 **Meta Project Setup Tool** 視窗，可以先關掉，不影響播放。
3. 開啟 `Assets/Scenes/SampleScene`。

> 影片檔沒有放在 GitHub 上（檔案太大），所以剛下載的專案裡播放器的 Clip 會是空的，這是正常的，接下來放入自己的影片即可。

### 6.2 放入影片

1. 把 `IMG_1234_SBS.mp4` 拖進 Unity 的 `Assets/Videos_Spatial/` 資料夾（沒有就自己建一個）。
   - **不要**把原始的 `.MOV` 放進 `Assets/`，Unity 會報錯。
2. 在 Hierarchy 點選 **SpatialVideoPlayer**。
3. Inspector 的 **Spatial Video Player (Script)**：
   - **Clip**：拖入剛剛的 `IMG_1234_SBS`
   - **Stereo Mode**：選 **SideBySide**
4. Ctrl+S 存檔。

### 6.3 先用 Quest Link 預覽（選用，但很省時間）

1. 電腦安裝並開啟 **Meta Horizon Link**，頭盔用 USB 線或 Air Link 進入 Link 模式。
2. Windows 右下角喇叭圖示 → 輸出裝置選 **「耳機 (Oculus Virtual Audio Device)」**，聲音才會從頭盔出來（Unity 預設送到電腦喇叭）。
3. Unity 按 **Play**，戴上頭盔直接就能看，不用 Build。
4. 拿起手把按任一鍵喚醒，確認：有立體感、畫面沒有變形、手裡看到 Quest 3 手把模型、手把射線可以點播放列按鈕。

### 6.4 Build 並安裝到頭盔

1. 頭盔用 USB 線接電腦（已開開發者模式並允許 USB 偵錯）。
2. Unity：**File > Build Profiles** → 左側選 **Android**（不是 Active 就按 **Switch Platform**）。
3. 按 **Build And Run**，存檔名用好認的名字，例如 `SpatialVideo_小明_1120.apk`。
4. 等 Build 完成（第一次 5～15 分鐘），頭盔裡會自動開啟播放器。

**之後在頭盔裡再次開啟**：頭盔 App 資料庫 → 右上角篩選選「**未知來源**」→ 找到 `Spatial_Video`。

**只有 APK 檔、要手動安裝時**（`adb` 在 Unity 安裝目錄 `Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/` 裡）：
```bash
adb install -r SpatialVideo_小明_1120.apk
adb shell am start -n com.DefaultCompany.Spatial_Video/com.unity3d.player.UnityPlayerGameActivity
```

---

## 7. 建議課程規劃

| 堂次 | 內容 | 時間 | 產出 |
|---|---|---|---|
| 第 1 堂 | 空間影片原理（左右眼視差）、iPhone 拍攝技巧、分組拍攝、路線 A 上傳觀看 | 90 分 | 每組 2～3 支空間影片，在頭盔看過 |
| 第 2 堂 | 傳檔到電腦（保留原始檔）、安裝 ffmpeg、轉成 SBS、看懂 SBS 畫面 | 90 分 | 每人一支 `_SBS.mp4` |
| 第 3 堂 | Unity 開專案、放入影片、Quest Link 預覽、Build APK 裝到頭盔 | 90～120 分 | 頭盔裡有自己的播放器 App |
| 進階（選用） | 改播放列 UI、改螢幕大小／位置、加入多支影片切換 | 自訂 | 自己的 VR 影片作品 |

**課前老師要先做好的事：**
- 所有電腦預先裝好 Unity 6000.3.23f1（含 Android 模組）和 ffmpeg，並**先開過一次專案**（第一次匯入很久，不要在課堂上等）。
- 所有頭盔預先開好開發者模式，並跟上課用的電腦配對過 USB 偵錯。
- 準備一支轉好的 SBS 示範影片，學生拍攝失敗時也能繼續後面的步驟。

---

## 8. 常見問題排除

| 狀況 | 原因 | 解法 |
|---|---|---|
| 頭盔裡看影片**沒有立體感** | 傳檔時被轉成 2D（用通訊軟體傳、iPhone 設定是「自動」） | 照第 4 節重新傳，確認檔案是 `.MOV` 且 ffprobe 有 Stereo 3D |
| ffmpeg 報錯 `view` 相關錯誤／找不到 stream | ffmpeg 版本太舊 | `ffmpeg -version` 確認 7.1 以上 |
| Unity 匯入影片報 `WindowsVideoMedia error` | 把原始 `.MOV` 放進了 `Assets/` | 刪掉 `Assets/` 裡的 `.MOV`，只放轉好的 `_SBS.mp4` |
| Editor 裡畫面**上下有黑邊、變成扁長條** | 影片帶有 Stereo 3D 並排標記（網路下載的影片常見） | 用 5.3 的 `filter_units` 指令拿掉標記 |
| 畫面兩眼一樣、沒有景深 | **Stereo Mode** 沒選 SideBySide | Inspector 改成 SideBySide |
| 看到兩個畫面並排 | Stereo Mode 選了 Mono | 改成 SideBySide |
| App 開起來是**浮動的 2D 視窗**，不是沉浸式 | OpenXR 的 Meta Quest Support 沒勾 | **Edit > Project Settings > XR Plug-in Management > OpenXR**，切到 Android 分頁，勾選 **Meta Quest Support** |
| Build And Run 找不到裝置 | 頭盔沒開開發者模式／沒允許 USB 偵錯／傳輸線只能充電 | 依 1.3 節檢查，換一條傳輸線 |
| 看不到手把模型／沒有射線 | 手把在休眠，或頭盔切到手勢追蹤 | 拿起手把按任一鍵喚醒 |
| 自己新增的 UI 面板點不到 | 新 Canvas 沒有接上射線互動 | Canvas 設成 World Space → Hierarchy 對它按右鍵 → **Interaction SDK > Add Ray Interaction to Canvas** |
| Quest Link 預覽沒聲音 | Unity 把聲音送到電腦喇叭 | Windows 輸出裝置切到「耳機 (Oculus Virtual Audio Device)」（見 6.3） |
| 看了會頭暈 | 拍攝時晃動太大或主體太近 | 依 2.2 節重拍：穩定、距離 1～2.5 公尺 |

---

## 9. 專案結構與播放器操作

### 9.1 專案裡有什麼

| 位置 | 內容 |
|---|---|
| `Assets/Scenes/SampleScene` | 主場景，打開這個就對了 |
| `Assets/Prefabs/SpatialVideoPlayer` | 播放器本體：螢幕（Screen）＋播放列（Canvas） |
| `Assets/Scripts/SpatialVideoPlayer.cs` | 播放影片、依影片解析度自動建立畫面、設定立體模式 |
| `Assets/Scripts/SpatialVideoPlayerControls.cs` | 播放列按鈕與進度條的功能 |
| `Assets/Materials/SpatialVideoStereo.shader` | 把 SBS 左半邊給左眼、右半邊給右眼，產生立體感 |
| `Assets/Videos_Spatial/` | 放轉好的 `_SBS.mp4`（不在 GitHub 上，要自己放） |
| Meta XR SDK（Package Manager 自動下載） | 頭盔追蹤、Quest 3 手把模型、手把射線 |

場景裡的物件：

```
SampleScene
├── OVRCameraRig                     ← 頭盔與手把（Meta 官方）
├── EventSystem                      ← 讓手把射線可以點 UI
├── SpatialVideoPlayer               ← 播放器（螢幕 + 播放列）
└── Directional Light
```

### 9.2 在頭盔裡怎麼操作

手把射線指到播放列，按**扳機鍵**點選：

| 按鈕 | 功能 |
|---|---|
| ❚❚ / ▶ | 暫停／播放 |
| ◀◀ / ▶▶ | 倒退／快轉 10 秒 |
| ↺ | 從頭重播 |
| 1x | 切換速度：0.5x → 1x → 1.5x → 2x |
| 進度條 | 拖曳跳到指定位置 |

影片播完會停在最後一格，不會自動重播。

### 9.3 常見的修改（進階）

都在 Hierarchy 選 **SpatialVideoPlayer**，在 Inspector 調整：

| 想做的事 | 怎麼改 |
|---|---|
| 換影片 | **Spatial Video Player (Script) > Clip** 換成別的影片 |
| 播一般 2D 影片 | **Stereo Mode** 改成 **Mono** |
| 上下排列的 3D 影片 | **Stereo Mode** 改成 **TopAndBottom** |
| 螢幕大一點／遠一點 | 展開 SpatialVideoPlayer → 選 **Screen**，改 Transform 的 Scale（寬:高 維持 16:9，例如 4.8 × 2.7）和 Position 的 Z |
| 改播放列外觀 | 展開 **Canvas**，按鈕、文字都是一般的 Unity UI，可以直接改顏色、文字、大小 |
| 新增自己的 UI 面板 | 新增 World Space Canvas → 對它按右鍵 → **Interaction SDK > Add Ray Interaction to Canvas**，手把射線就能點 |

> 第一次開專案若跳出 **Meta Project Setup Tool** 視窗，可以先關掉，不影響播放。
