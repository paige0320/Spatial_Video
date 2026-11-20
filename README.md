# Spatial Video Player for Meta Quest 3

把 iPhone 拍的**空間影片（3D）**轉檔後放進這個 Unity 專案，做成 Quest 3 上的 VR 播放器。

Unity **6000.3.23f1** · OpenXR · Meta XR SDK 209

📄 詳細步驟：[Docs/SpatialVideo_Shoot_and_View.md](Docs/SpatialVideo_Shoot_and_View.md)

## 需要準備

- iPhone 15 Pro 以上（含 16、17 系列；不含 16e、iPhone Air）
- Meta Quest 3 / 3S（開啟開發者模式）＋ 可傳資料的 USB-C 線
- Unity 6000.3.23f1（勾 **Android Build Support**）
- ffmpeg 7.1 以上（Windows：`winget install Gyan.FFmpeg`；Mac：`brew install ffmpeg`）

## 步驟

1. **拍攝**：相機 → **空間**模式 → 手機橫拿，主角距離 1～2.5 公尺。
2. **傳到電腦**：iPhone `設定 > App > 照片 > 傳輸到 Mac 或 PC` 選 **保留原始檔**，再用 AirDrop 或 USB 線傳 `.MOV`（不要用 LINE / IG，會變 2D）。
3. **轉檔**：
   ```bash
   ffmpeg -i IMG_1234.MOV -filter_complex "[0:v:view:0][0:v:view:1]hstack[v]" -map "[v]" -map 0:a:0 -c:v libx264 -crf 18 -pix_fmt yuv420p -c:a aac IMG_1234_SBS.mp4
   ```
4. **放進專案**：用 Unity 開啟本專案（第一次要連網下載套件）→ 開 `Assets/Scenes/SampleScene` → 影片拖進 `Assets/Videos_Spatial/` → 選 **SpatialVideoPlayer**，**Clip** 設成你的影片、**Stereo Mode** 選 **SideBySide** → 存檔。
5. **裝到頭盔**：頭盔接 USB → **File > Build Profiles > Android > Build And Run**。

頭盔裡用手把射線點播放列（暫停、快轉／倒退 10 秒、重播、調速、進度條）。

## 常見問題

| 狀況 | 解法 |
|---|---|
| 沒有立體感 | 傳檔時被轉成 2D，重看步驟 2 |
| 看到兩個畫面並排 | **Stereo Mode** 選 **SideBySide** |
| Unity 報 `WindowsVideoMedia error` | 刪掉 `Assets/` 裡的原始 `.MOV` |
| 看不到手把／射線 | 拿起手把按任一鍵喚醒 |
| Quest Link 預覽沒聲音 | Windows 輸出切到「耳機 (Oculus Virtual Audio Device)」 |
