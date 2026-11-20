# Spatial Video Player for Meta Quest 3

用 iPhone 拍**空間影片（3D 立體影片）**，轉檔後放進這個 Unity 專案，做成自己的 Meta Quest 3 VR 播放器 App。
附手把射線可以點的播放列（播放／暫停、快轉、倒退、重播、調速、進度條）。

- Unity **6000.3.23f1**（Built-in Render Pipeline + OpenXR）
- Meta XR SDK **209**（Quest 3 原廠手把模型、手把射線）
- 支援影片：Side-by-Side（SBS，左右並排）、Top-and-Bottom、一般 2D 影片

📄 **詳細步驟講義（一步一步照做）**：[Docs/SpatialVideo_Shoot_and_View.md](Docs/SpatialVideo_Shoot_and_View.md)

---

## 流程總覽

```
iPhone 拍空間影片 → 傳到電腦（保留原始檔）→ ffmpeg 轉成 SBS → 放進本專案 → Build 到 Quest 3
```

## 需要準備

- 可拍空間影片的 iPhone：15 Pro / 15 Pro Max、16 全系列（不含 16e）、17 / 17 Pro / 17 Pro Max（不含 iPhone Air）
- Meta Quest 3 / 3S（已開**開發者模式**）＋ 可傳資料的 USB-C 線
- 電腦（Windows 或 Mac）
- [Unity Hub](https://unity.com/download) + **Unity 6000.3.23f1**（安裝時勾 **Android Build Support**）
- [ffmpeg](https://ffmpeg.org/download.html) **7.1 以上**
  - Windows：`winget install Gyan.FFmpeg`
  - Mac：`brew install ffmpeg`

---

## 5 步驟

### 1. 用 iPhone 拍空間影片
相機 App → **空間** 模式 → **手機橫拿**拍攝。主角距離 1～2.5 公尺，手機保持穩定。
拍完在「照片」App 確認影片左上角有 **空間** 標籤。

### 2. 傳到電腦（⚠️ 不能被轉成 2D）
- iPhone `設定 > App > 照片 > 傳輸到 Mac 或 PC` 選 **保留原始檔**
- Mac 用 AirDrop（選項打開「所有照片資料」）；Windows 用 USB 線從 `DCIM` 複製 `.MOV`
- ❌ 不要用 LINE / IG / Messenger 傳，會變成 2D

### 3. 用 ffmpeg 轉成左右並排（SBS）影片
在影片所在資料夾開終端機，執行（把 `IMG_1234` 換成你的檔名）：
```bash
ffmpeg -i IMG_1234.MOV -filter_complex "[0:v:view:0][0:v:view:1]hstack[v]" -map "[v]" -map 0:a:0 -c:v libx264 -preset medium -crf 18 -pix_fmt yuv420p -c:a aac -b:a 128k IMG_1234_SBS.mp4
```
轉完打開來看，應該是**左右兩個幾乎一樣的畫面並排**。

### 4. 放進 Unity 專案
1. 下載本專案（Code → Download ZIP，或 `git clone`），用 Unity Hub 以 **6000.3.23f1** 開啟
   （第一次要**連網**下載 Meta XR SDK，約 10～30 分鐘；跳出 Meta Project Setup Tool 可以先關掉）
2. 開啟 `Assets/Scenes/SampleScene`
3. 把 `IMG_1234_SBS.mp4` 拖進 `Assets/Videos_Spatial/`（**不要**放原始 `.MOV`）
4. Hierarchy 選 **SpatialVideoPlayer** → Inspector：
   - **Clip** = 你的影片
   - **Stereo Mode** = **SideBySide**
5. Ctrl+S 存檔

> 影片檔沒有放在 GitHub 上，剛下載時 Clip 是空的，這是正常的。

**（選用）先用 Quest Link 預覽**：頭盔進入 Link 模式，Windows 喇叭輸出切到「耳機 (Oculus Virtual Audio Device)」，Unity 按 **Play** 就能在頭盔裡看，不用 Build。

### 5. 裝到頭盔
1. 頭盔用 USB 接電腦，頭盔裡允許 USB 偵錯
2. **File > Build Profiles** → 選 **Android**（不是 Active 就按 Switch Platform）→ **Build And Run**
3. 完成後頭盔會自動開啟；之後在頭盔：App 資料庫 → 篩選「**未知來源**」→ 開啟 `Spatial_Video`

### 在頭盔裡操作
手把射線指到播放列，按**扳機鍵**：❚❚/▶ 暫停播放、◀◀/▶▶ 倒退快轉 10 秒、↺ 重播、1x 切換速度、拖曳進度條跳轉。

---

## 常見問題

| 狀況 | 解法 |
|---|---|
| 沒有立體感 | 傳檔時被轉成 2D → 重看步驟 2 |
| ffmpeg 報錯 | 版本要 7.1 以上：`ffmpeg -version` |
| Unity 報 `WindowsVideoMedia error` | `Assets/` 裡放了原始 `.MOV`，刪掉 |
| 兩個畫面並排／沒有景深 | **Stereo Mode** 要選 **SideBySide** |
| 畫面上下有黑邊變扁長條 | 網路下載的影片帶有 3D 標記，用這行拿掉：`ffmpeg -i 影片.mp4 -c copy -bsf:v "filter_units=remove_types=6" 影片_fixed.mp4` |
| 看不到手把／沒有射線 | 拿起手把按任一鍵喚醒（頭盔可能切到手勢追蹤） |
| Quest Link 預覽沒聲音 | Windows 喇叭輸出切到「耳機 (Oculus Virtual Audio Device)」 |

更多問題見[詳細步驟講義](Docs/SpatialVideo_Shoot_and_View.md#卡住了常見問題)。

## 想改播放器？

Hierarchy 選 **SpatialVideoPlayer**，在 Inspector 調整：

| 想做的事 | 怎麼改 |
|---|---|
| 換影片 | **Spatial Video Player (Script) > Clip** 換成別的影片 |
| 播一般 2D 影片 | **Stereo Mode** 選 **Mono** |
| 上下排列的 3D 影片 | **Stereo Mode** 選 **TopAndBottom** |
| 螢幕大一點／遠一點 | 展開後選 **Screen**，改 Transform 的 Scale（維持 16:9，例如 4.8 × 2.7）和 Position 的 Z |
| 改播放列外觀 | 展開 **Canvas**，按鈕、文字都是一般 Unity UI |
| 新增自己的 UI 面板 | 新增 World Space Canvas → 對它按右鍵 → **Interaction SDK > Add Ray Interaction to Canvas** |

---

### 附註：只想看、不做 App？
不用轉檔也不用這個專案：iPhone 拍好後，用 **Meta Horizon** App → 選單 → 圖庫 → 上傳 → **空間**，頭盔打開 **檔案 > 空間影片** 就能看。
