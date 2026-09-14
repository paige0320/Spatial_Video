# Spatial Video 播放器設定紀錄

Unity 2022.3.26f1 / Quest 3 / OpenXR

## 1. 播放架構總覽

目前的做法是先做一個「相容版」播放器:把現有的一般 2D 影片當作貼在使用者面前的一塊虛擬電影螢幕(平面 Quad)播放。之後如果要換成真正的 180°/360° spatial(stereo)影片,播放邏輯的接口已經預留好,詳見第 5 節。

**核心檔案:**

| 檔案 | 用途 |
|---|---|
| `Assets/Scripts/SpatialVideoPlayer.cs` | 播放器控制腳本 |
| `Assets/Editor/SpatialVideoPlayerPrefabCreator.cs` | 編輯器工具,一鍵重新產生 prefab |
| `Assets/Prefabs/SpatialVideoPlayer.prefab` | 播放器 prefab 本體 |
| `Assets/RenderTextures/SpatialVideoRT.renderTexture` | VideoPlayer 輸出的 RenderTexture(1920x1080) |
| `Assets/Materials/SpatialVideoScreen.mat` | 貼在螢幕 Quad 上的材質(Unlit/Texture) |
| `Assets/Videos_SDR/` | 轉檔後的影片(見第 4 節) |

**Prefab 階層:**

```
SpatialVideoPlayer (VideoPlayer, AudioSource, SpatialVideoPlayer.cs)
└── Screen (Quad, 16:9, local scale 3.2 x 1.8, local position 0,1.5,2)
```

`SpatialVideoPlayer.cs` 的欄位:

- `clip`:目前播放的 VideoClip
- `stereoMode`:Mono / SideBySide / TopAndBottom(目前固定 Mono)
- `screenRenderer`:螢幕 Quad 的 Renderer
- `renderTexture`:VideoPlayer 輸出用的 RenderTexture

要換片直接改 Inspector 上 **Spatial Video Player (Script)** 元件的 **Clip** 欄位,或呼叫 `Play(VideoClip)`。Play Mode 執行中改 Clip 欄位也會即時生效(靠 `OnValidate` 偵測)。

## 2. Unity 專案設定(Player Settings, Android tab)

- **Other Settings > Rendering**
  - Color Space:`Linear`
  - Graphics APIs:只留 **OpenGLES3**(取消 Auto Graphics API、移除 Vulkan)
- **Other Settings > Identification**
  - Minimum API Level:`Android 10.0 'Q' (API level 29)`
  - Target API Level:`Automatic`
- **Other Settings > Configuration**
  - Scripting Backend:`IL2CPP`
  - Target Architectures:只勾 `ARM64`
  - Active Input Handling:`Input System Package (New)`
- **Resolution and Presentation**
  - Default Orientation:`Landscape Left`

## 3. XR / OpenXR 設定

**安裝的套件(`Packages/manifest.json`):**
- `com.unity.xr.management`
- `com.unity.xr.openxr`
- `com.unity.xr.interaction.toolkit`(連帶裝入 `com.unity.inputsystem`)

**XR Plug-in Management > Android tab:** 勾選 `OpenXR`

**OpenXR 設定 > Android tab > Feature Groups,以下兩個要打勾:**
- `Meta Quest Support`(`MetaQuestFeature`)—— **這個一定要開**,負責讓 Android Manifest 正確標記成 Quest 原生沉浸式 VR App;沒開的話 Quest 系統會把 App 當一般 Android App,用浮動 2D 視窗面板顯示,進不了沉浸式模式。Target Devices 勾 Quest / Quest 2 / Quest Pro / Quest 3 / Quest 3S 全選。
- `Meta Quest Touch Plus Controller Profile`

**場景相機 Rig:**
`GameObject > XR > XR Origin (VR)` 建立,放在世界原點 `(0,0,0)`,結構為 `XR Origin > Camera Offset > Main Camera`(Camera Offset 預設身高偏移 1.1176)。

## 4. 影片格式規範

VideoPlayer 在 Quest 的硬體解碼器上,對來源影片格式比較挑,目前 `Assets/Videos_SDR/` 裡的檔案都是用以下 ffmpeg 參數轉出來的:

```bash
ffmpeg -i input.mp4 \
  -vf "format=yuv420p" \
  -c:v libx264 -profile:v baseline -level 4.0 -x264-params "bframes=0" \
  -preset medium -crf 20 \
  -avoid_negative_ts make_zero \
  -c:a aac -b:a 192k \
  -movflags +faststart \
  output.mp4
```

**規格重點(之後任何要放進這個播放器的影片都要符合):**
- 影像編碼:H.264,**Constrained Baseline profile**
- **不能有 B-frame**(`bframes=0`),避免時間戳記異常
- **不能是 HDR / Dolby Vision**,一律轉成標準 SDR(`yuv420p`)
- 容器用 `+faststart`(方便串流讀取)

`SpatialVideoPlayer` prefab 和場景裡的 clip 目前都指向 `Assets/Videos_SDR/` 底下同檔名的版本。

## 5. 以後要換成真正的 3D(Spatial)影片時怎麼做

現在的螢幕是一塊「平面 Quad」,播的是一般單眼 2D 影片,概念上等於「VR 裡的一台虛擬電視」。真正的 spatial video(180°/360° 立體全景)需要多做兩件事:**(a)** 影片本身要有左右眼視差的素材,**(b)** 播放的幾何體通常要包住觀眾(球體/半球),而不是一塊平面螢幕。

**步驟:**

1. **準備立體素材**:用支援 180°/360° stereo 錄製的相機(例如 Insta360 Pro 2、Kandao QooCam 3 Ultra 之類)拍攝,輸出成 **Side-by-Side(左右)** 或 **Top-and-Bottom(上下)** 封裝的單一影片檔。

2. **轉檔**:沿用第 4 節的 ffmpeg 參數模板(H.264 baseline、無 B-frame、SDR),只需要依素材實際解析度調整 `-crf` / bitrate,並確認來源沒有 Dolby Vision/HDR metadata(用 `ffprobe -show_entries stream_side_data` 檢查,不能出現 `DOVI configuration record`)。

3. **換 Clip**:把 `Assets/Prefabs/SpatialVideoPlayer.prefab`(或場景裡的實例)上 **Spatial Video Player (Script)** 元件的 **Clip** 換成新的立體影片。

4. **切換 Stereo Mode**:同一個元件上的 **Stereo Mode** 欄位,依素材封裝方式改成:
   - 左右並排素材 → `SideBySide`
   - 上下並排素材 → `TopAndBottom`

   這一步會自動把畫面 UV 切成左右半 / 上下半,目前程式邏輯已經寫好在 `SpatialVideoPlayer.ApplyStereoMode()`。

5. **⚠️ 幾何體要換**:如果是要做**環景沉浸**(180°/360°,不只是「螢幕比較大」而已),現在這塊平面 Quad 撐不起來,需要把 `Screen` 從 Quad 換成一個**內表面朝內的球體/半球網格**,並把材質換成能正確包覆等距柱狀(equirectangular)貼圖的 shader(而不是現在的 `Unlit/Texture` 平面貼圖)。換掉 Mesh/Material 之後記得回到 `SpatialVideoPlayer` 元件重新指定 `screenRenderer` 指向新的 Renderer。這部分還沒做,等真正的 3D 素材確定規格(180° 還是 360°、SBS 還是 TB)後再處理。

6. **測試順序建議**:先在 Editor Play Mode 用一支立體素材確認 Stereo Mode 切割正確、左右眼畫面沒有跑掉,再上 Quest 3 實機測試。
