# 學生講義：用 iPhone 拍空間影片，做成自己的 Quest 3 VR 播放器

> 流程：**拍攝 → 傳到電腦 → 轉檔 → 放進 Unity 專案 → 裝到頭盔**
> 全程約 60～90 分鐘（第一次開 Unity 專案要等比較久）。

---

## 你需要

| 項目 | 條件 |
|---|---|
| **iPhone** | 15 Pro / 15 Pro Max、16 / 16 Plus / 16 Pro / 16 Pro Max、17 / 17 Pro / 17 Pro Max（**iPhone Air、16e 不能拍**） |
| **頭盔** | Meta Quest 3 / 3S，已開**開發者模式** |
| **電腦** | Windows 或 Mac，已裝好 **Unity 6000.3.23f1**（含 Android Build Support）和 **ffmpeg 7.1 以上**（安裝方法見[附錄：安裝 ffmpeg](#附錄安裝-ffmpeg)） |
| **線材** | 可以傳資料的 USB-C 線（只能充電的線不行） |

---

## 第 1 步：拍空間影片（約 10 分鐘）

1. 打開 iPhone **相機** App，左右滑動切到 **「空間」** 模式。
2. **把手機橫拿**（直拿不能拍），按紅色按鈕開始，再按一次停止。

### 拍好 3D 的 6 個重點

| ✅ 這樣做 | 為什麼 |
|---|---|
| 主角離鏡頭 **1～2.5 公尺** | 太近會鬥雞眼、不舒服；太遠沒有立體感 |
| **手機穩住、慢慢移動** | 在頭盔裡畫面一晃就容易頭暈 |
| **光線充足** | 暗的地方畫質會變差 |
| 畫面裡有**前、中、後**不同距離的東西 | 層次越多，立體感越明顯 |
| 每段 **10～60 秒** | 檔案小、轉檔快 |
| **手指不要擋到鏡頭** | 任一顆鏡頭被擋，左右眼畫面就不一樣，會很不舒服 |

**拍完檢查：** 打開「照片」App，影片左上角有 **「空間」** 標籤就對了。

---

## 第 2 步：把影片傳到電腦（約 5 分鐘）

⚠️ **這一步最容易出錯：傳錯方式，影片會被轉成 2D，就沒有立體感了。**

1. iPhone 先改設定：`設定 > App > 照片 > 傳輸到 Mac 或 PC` → 選 **保留原始檔**。
2. 傳檔：
   - **Mac**：AirDrop，分享前點上方「選項」→ 打開 **所有照片資料**。
   - **Windows**：USB 線接電腦，iPhone 按「信任」，到 `本機 > Apple iPhone > Internal Storage > DCIM` 複製 `.MOV` 檔。
3. ❌ **不要**用 LINE、IG、Messenger、Email 傳。

---

## 第 3 步：用 ffmpeg 轉成左右並排（SBS）影片（約 5 分鐘）

**為什麼要轉？** iPhone 把左右眼畫面「疊」在同一個檔案裡，Unity 只讀得到一隻眼。轉檔會把兩隻眼睛的畫面**左右並排**成一張畫面（SBS），播放器再把左半邊給左眼、右半邊給右眼。

1. 在影片所在的資料夾開終端機：
   - **Windows**：在資料夾空白處按右鍵 → **在終端機中開啟**
   - **Mac**：Finder 對資料夾按右鍵 → **服務 > 新增位於檔案夾位置的終端機視窗**
2. 貼上這行（把 `IMG_1234` 換成你的檔名），按 Enter：

```bash
ffmpeg -i IMG_1234.MOV -filter_complex "[0:v:view:0][0:v:view:1]hstack[v]" -map "[v]" -map 0:a:0 -c:v libx264 -preset medium -crf 18 -pix_fmt yuv420p -c:a aac -b:a 128k IMG_1234_SBS.mp4
```

3. 跑完會多一個 `IMG_1234_SBS.mp4`。打開來看：應該是**左右兩個幾乎一樣的畫面並排**（仔細看左右有一點點位移，這就是立體感的來源）。

---

## 第 4 步：放進 Unity 專案（約 10 分鐘，第一次開專案另外要 10～30 分鐘）

1. 用 Unity Hub 開啟老師給的專案（版本 **6000.3.23f1**）。
   - 第一次開要連網下載套件，請耐心等。
   - 跳出 **Meta Project Setup Tool** 視窗可以直接關掉。
2. Project 視窗打開 `Assets/Scenes/SampleScene`。
3. 把 `IMG_1234_SBS.mp4` 拖進 Project 視窗的 `Assets/Videos_Spatial/` 資料夾。
   - ❌ **不要**把原始 `.MOV` 放進來，Unity 會報錯。
4. Hierarchy 點選 **SpatialVideoPlayer**，在 Inspector 的 **Spatial Video Player (Script)**：
   - **Clip**：拖入你的 `IMG_1234_SBS`
   - **Stereo Mode**：選 **SideBySide**
5. 按 **Ctrl+S**（Mac：Cmd+S）存檔。

---

## 第 5 步：裝到頭盔（約 10～15 分鐘）

1. 頭盔用 USB 線接電腦，戴上頭盔，跳出「允許 USB 偵錯」按 **允許**。
2. Unity 選單 **File > Build Profiles** → 左邊選 **Android**（如果不是 Active，先按 **Switch Platform**）。
3. 按 **Build And Run**，檔名取好認的名字，例如 `SpatialVideo_小明.apk`。
4. 等 Build 完成，頭盔裡會自動開啟你的播放器 🎉

**之後要再打開**：頭盔 App 資料庫 → 右上角篩選選 **未知來源** → 開啟 `Spatial_Video`。

### 在頭盔裡怎麼操作

用手把射線指著影片下方的播放列，按**扳機鍵**：

| 按鈕 | 功能 |
|---|---|
| ❚❚ / ▶ | 暫停／播放 |
| ◀◀ / ▶▶ | 倒退／快轉 10 秒 |
| ↺ | 從頭重播 |
| 1x | 切換速度（0.5x / 1x / 1.5x / 2x） |
| 進度條 | 拖曳跳到想看的位置 |

---

## 卡住了？常見問題

| 狀況 | 解法 |
|---|---|
| 相機找不到「空間」模式 | 這支 iPhone 不支援（見「你需要」），或 iOS 不是最新版 |
| 頭盔裡**沒有立體感** | 傳檔時被轉成 2D → 照第 2 步重新傳 |
| ffmpeg 出現錯誤 | 輸入 `ffmpeg -version` 確認是 7.1 以上；檔名要跟指令裡的一樣 |
| Unity 報 `WindowsVideoMedia error` | `Assets/` 裡放了原始 `.MOV`，刪掉，只留 `_SBS.mp4` |
| 看到兩個畫面並排、沒有景深 | **Stereo Mode** 要選 **SideBySide** |
| Build And Run 找不到頭盔 | 確認開發者模式、頭盔裡按了「允許 USB 偵錯」、換一條可傳資料的線 |
| 看不到手把或射線 | 拿起手把按任一鍵把它喚醒 |
| 看了會頭暈 | 拍攝時晃太多或主角太近 → 照「6 個重點」重拍 |

---

## 附錄：安裝 ffmpeg

裝完一定要**關掉終端機再重新打開**，不然會找不到 ffmpeg。

### Windows（10／11）

1. 開始選單搜尋 **終端機**（或 PowerShell）並打開。
2. 貼上這行，按 Enter（第一次用會問是否同意條款，輸入 `Y` 再按 Enter）：
   ```
   winget install Gyan.FFmpeg
   ```
3. 顯示「已成功安裝」後，**關掉終端機再重新打開**。
4. 輸入 `ffmpeg -version`，第一行出現 `ffmpeg version 7.1` 以上（例如 `8.x`、`9.x`）就完成了。

**不能用 `winget` 時（例如學校電腦）：**

1. 到 https://www.gyan.dev/ffmpeg/builds/ 下載 **ffmpeg-release-essentials.zip**。
2. 解壓縮到 `C:\ffmpeg`，確認有 `C:\ffmpeg\bin\ffmpeg.exe`。
3. 開始選單搜尋 **編輯系統環境變數** → **環境變數** → 上方「使用者變數」點 **Path** → **編輯** → **新增**，填入 `C:\ffmpeg\bin` → 全部按確定。
4. 重開終端機，輸入 `ffmpeg -version` 確認。

### Mac

1. 打開 **終端機**（Launchpad → 其他 → 終端機）。
2. 還沒有 Homebrew 的話，先貼這行，照畫面輸入電腦密碼：
   ```
   /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
   ```
   裝完後畫面最後的 **Next steps** 會列出兩行指令，也複製貼上執行。
3. 安裝 ffmpeg：
   ```
   brew install ffmpeg
   ```
4. 重開終端機，輸入 `ffmpeg -version` 確認。

### 安裝時的常見問題

| 狀況 | 解法 |
|---|---|
| `ffmpeg 不是內部或外部命令`／`command not found` | 終端機沒重開，或 Path 沒設好 |
| 版本低於 7.1 | Windows：`winget upgrade Gyan.FFmpeg`；Mac：`brew upgrade ffmpeg` |
| 沒有管理員權限 | 用 Windows 的手動方法，Path 加在「使用者變數」不需要管理員權限 |

---

## 附錄：不想做 App，只想快速看？

不用電腦、不用轉檔：

1. iPhone 安裝 **Meta Horizon** App，登入跟頭盔同一個帳號。
2. App → 選單 → **圖庫** → 右上角 **上傳** → **空間** → 選影片上傳。
3. 頭盔打開 **檔案（Files）** → **空間影片** → 播放。
