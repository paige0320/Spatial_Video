# Spatial Video 播放器設定紀錄

Unity 2022.3.26f1 / Quest 3 / OpenXR

## 1. 播放架構總覽

播放器同時支援兩種素材:

- **平面 2D 影片**(`Assets/Videos_SDR/`):當作貼在使用者面前的一塊虛擬電影螢幕(平面 Quad)播放,雙眼看到同一張畫面,沒有景深。
- **Side-by-Side / Top-and-Bottom 立體影片**(`Assets/Videos_Spatial/`):同一塊 Quad,但材質改用自訂 shader,依 `unity_StereoEyeIndex` 把畫面左右半(或上下半)分別餵給左右眼,兩眼看到的畫面有視差,戴上頭顯會有景深感。詳細原理見第 5 節。

目前還**沒有**做到的是真正的 180°/360° 環景沉浸(球體/半球包覆式播放),見第 6 節。

**核心檔案:**

| 檔案 | 用途 |
|---|---|
| `Assets/Scripts/SpatialVideoPlayer.cs` | 播放器控制腳本 |
| `Assets/Editor/SpatialVideoPlayerPrefabCreator.cs` | 編輯器工具,一鍵重新產生 prefab |
| `Assets/Prefabs/SpatialVideoPlayer.prefab` | 播放器 prefab 本體 |
| `Assets/Materials/SpatialVideoStereo.shader` | 螢幕 Quad 用的自訂 shader,負責依眼睛分流 UV |
| `Assets/Materials/SpatialVideoScreen.mat` | 貼在螢幕 Quad 上的材質(套用上面的 shader) |
| `Assets/RenderTextures/SpatialVideoRT.renderTexture` | 舊版預設 RenderTexture(1920x1080);**現在只是保底值**,實際播放時腳本會依 clip 解析度動態建立新的,見下方說明 |
| `Assets/Videos_SDR/` | 平面 2D 影片(轉檔規格見第 4 節) |
| `Assets/Videos_Spatial/` | 立體(SBS)影片,見第 5 節 |
| `RawFootage/`(專案根目錄,**不在 Assets 裡**) | iPhone 拍的原始 MV-HEVC(`.MOV`)保留備份,故意不放進 Assets,見第 5.1 節 |

**Prefab 階層:**

```
SpatialVideoPlayer (VideoPlayer, AudioSource, SpatialVideoPlayer.cs)
└── Screen (Quad, 16:9, local scale 3.2 x 1.8, local position 0,1.5,2)
```

`SpatialVideoPlayer.cs` 的欄位:

- `clip`:目前播放的 VideoClip
- `stereoMode`:`Mono` / `SideBySide` / `TopAndBottom`
- `screenRenderer`:螢幕 Quad 的 Renderer
- `renderTexture`:VideoPlayer 輸出用的 RenderTexture(只是初始/保底值,見下方)

要換片直接改 Inspector 上 **Spatial Video Player (Script)** 元件的 **Clip** 欄位,或呼叫 `Play(VideoClip)`。Play Mode 執行中改 Clip 欄位也會即時生效(靠 `OnValidate` 偵測)。

**⚠️ RenderTexture 現在是自動的,不用手動配對:**
早期版本要求 Inspector 上的 `renderTexture` 欄位手動指到一顆跟 clip 解析度完全吻合的 RenderTexture 資產,忘記換就會造成畫面裁切/擠壓(看起來像「變形」)。現在 `Play()` 內的 `EnsureRenderTextureMatchesClip()` 會自動依 `clip.width` / `clip.height` 在執行期建立剛好吻合的 RenderTexture,並直接指定給 material 的貼圖,不再需要手動配對,也不會有解析度對不上的問題。

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

**✅ Quest Link(Editor Play Mode 直接用頭顯預覽)設定步驟:**
1. `Edit > Project Settings > XR Plug-in Management`,切到 **PC, Mac & Linux Standalone** 分頁,勾選 `OpenXR`。
2. 左側 `OpenXR` 子頁面同樣切到 PC 分頁,**`Enabled Interaction Profiles`** 那個方框預設是空的——這個不是打勾式的,要點方框右下角的 **`+`** 才會跳出選單,加一個 `Oculus Touch Controller Profile`(或 `Meta Quest Touch Pro Controller Profile`,Quest 3 的 Touch Plus 手把用這兩個都能動,只是 Plus 專屬功能沒有)。
   - **這兩個是分開的區塊**:下面另外有一個 `OpenXR Feature Groups`,那裡的 `Meta Quest Support` 是給「功能」開關用的,跟上面 `Enabled Interaction Profiles`(給「手把型號綁定」用的)是兩件事,兩個都要設,不要只設一個。
3. 頭顯用 USB 或 Air Link 連進 **Quest Link** 模式後,Editor 直接按 Play 即可,不用重新 build,比實機測試快很多,建議之後有 XR 互動相關改動優先用這個測。

## 4. 平面 2D 影片格式規範(`Assets/Videos_SDR/`)

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

**規格重點(之後任何要放進這個播放器的平面影片都要符合):**
- 影像編碼:H.264,**Constrained Baseline profile**
- **不能有 B-frame**(`bframes=0`),避免時間戳記異常
- **不能是 HDR / Dolby Vision**,一律轉成標準 SDR(`yuv420p`)
- 容器用 `+faststart`(方便串流讀取)

`SpatialVideoPlayer` prefab 和場景裡的 clip 目前都指向 `Assets/Videos_SDR/` 底下同檔名的版本。

## 5. 立體(SBS/TnB)影片:shader 原理與素材來源

### 5.1 兩種素材來源

**來源 A:自己用 iPhone 拍的 Apple Spatial Video**

iPhone 15 Pro 以上拍「空間視訊」存的是 **MV-HEVC** 格式——左右眼畫面是用「分層」方式藏在同一條 HEVC 視訊流裡(base layer + 額外的 view layer),不是並排畫在同一張畫面上。一般播放器、Android、Unity 的 VideoPlayer 只吃得到 base layer,等於只看得到一隻眼睛,而且 Windows 上 Unity 匯入這種檔案會直接報 `WindowsVideoMedia error`,完全打不開。

**處理流程**(範例:`Assets/IMG_0106.MOV` → `Assets/Videos_Spatial/IMG_0106_SBS.mp4`):

```bash
# 1. 確認真的是 MV-HEVC 立體素材(要看到 Stereo 3D side data)
ffprobe -show_streams -show_format input.MOV

# 2. 用 ffmpeg 7.1+ 的 view 語法把兩隻眼睛的畫面分別抽出來,左右拼接成一張畫面
ffmpeg -i input.MOV \
  -filter_complex "[0:v:view:0][0:v:view:1]hstack[v]" \
  -map "[v]" -map 0:a:0 \
  -c:v libx264 -preset medium -crf 18 -pix_fmt yuv420p \
  -c:a aac -b:a 128k \
  output_SBS.mp4
```

拼出來的畫面總寬度是兩隻眼睛原生解析度相加(例如兩眼各 1920x1080 → 輸出 3840x1080),這種叫 **「全寬 SBS」**,每一半本身就是完整未壓縮比例的畫面。

原始 `.MOV` **不要放進 `Assets/` 裡**(Unity 打不開,還會報錯),搬去專案根目錄的 `RawFootage/` 資料夾保留備份就好(這個資料夾已加進 `.gitignore`)。

**來源 B:別人已經匯出好的 SBS/TnB 影片**(例如網路上下載的立體影片參考素材)

很多創作者上傳到 YouTube 之類平台前,自己就已經把左右眼畫面拼在同一張畫面裡匯出了(這種叫 **「半寬壓縮 SBS」**:總畫面還是標準 16:9,兩眼各自被水平壓縮塞進一半寬度)。這種檔案下載下來(例如用 `yt-dlp`)**不需要再做任何拼接**,可以直接丟進 `Assets/Videos_Spatial/` 用。

範例:`Assets/Videos_Spatial/LakeTahoe_SBS.mp4` 就是這樣來的(來源影片本身標題就寫明是「3D Spatial Video」,ffprobe 也能看到 `Stereo 3D: side by side, view: packed` 的 metadata,截幀出來肉眼也能看到左右兩半有視差)。

> 全寬 SBS 跟半寬壓縮 SBS 這兩種封裝方式,下面第 5.2 節的 shader 邏輯**兩種都能正確處理**,不需要分開寫程式判斷——因為 shader 只是照 UV 比例取樣,不管實際像素解析度,半寬壓縮的內容被拉伸回全畫面寬度時,正好抵銷掉原本匯出時做的水平壓縮。

### 5.2 Shader 原理:`SpatialVideo/StereoUnlit`

Quest 上 Unity 走 **single-pass instanced** 立體渲染,兩眼共用同一個 draw call、同一份材質屬性,所以**不能**用 C# 端改 `mainTextureScale` / `mainTextureOffset` 這種材質屬性去區分左右眼(兩眼改到的是同一份值,只會兩眼看到一樣的半邊畫面)。

正確做法是在 shader 內部用 `unity_StereoEyeIndex`(Unity 內建、single-pass instanced 下會自動依目前渲染的是哪隻眼睛而變化)去決定要取樣紋理的哪一半:

```hlsl
uint eye = unity_StereoEyeIndex; // 0 = 左眼, 1 = 右眼

if (_StereoMode > 1.5) // TopAndBottom
    uv.y = uv.y * 0.5 + (eye == 0 ? 0.5 : 0.0);
else if (_StereoMode > 0.5) // SideBySide
    uv.x = uv.x * 0.5 + (eye == 0 ? 0.0 : 0.5);
```

`SpatialVideoPlayer.ApplyStereoMode()` 只是把 `stereoMode` 這個 enum 值寫進材質的 `_StereoMode` float 屬性,真正的左右眼分流全部在 shader 裡完成。

### 5.3 換成立體影片的步驟(現在已經是完整功能,不是規劃中)

1. 準備好 SBS 或 TnB 影片(見 5.1),放進 `Assets/Videos_Spatial/`。
2. 把 `SpatialVideoPlayer` 元件的 **Clip** 換成這支影片。
3. **Stereo Mode** 依封裝方式選 `SideBySide` 或 `TopAndBottom`。
4. RenderTexture 不用管,`Play()` 會自動依 clip 解析度建立吻合的 RT(見第 1 節說明)。
5. 先在 Editor(或等第 3 節的 Quest Link 設定弄好後)確認畫面沒有裁切/變形,再上機測試。

## 6. 還沒做的:真正 180°/360° 環景沉浸

現在的螢幕還是一塊「平面 Quad」,即使切成立體(SideBySide/TopAndBottom)也只是「畫面比較有景深的虛擬電視」,不是把使用者包在畫面裡的環景體驗。如果之後要做真正的 180°/360° 環景:

- 影片來源需要用支援 180°/360° stereo 錄製的相機(例如 Insta360 Pro 2、Kandao QooCam 3 Ultra 之類),輸出等距柱狀(equirectangular)投影的 SBS/TnB 素材。
- `Screen` 要從 Quad 換成**內表面朝內的球體/半球網格**,材質/shader 也要能正確處理 equirectangular 貼圖的球面 UV 展開(不是現在這種平面矩形 UV 切半)。
- 換掉 Mesh/Material 之後記得回到 `SpatialVideoPlayer` 元件重新指定 `screenRenderer`。

這部分本次沒有動,等真正的 180°/360° 素材規格確定後再處理。

## 7. Build 與安裝到 Quest 3 的注意事項

- **Build 跟 Build And Run 不一樣**:`Build` 只會把 APK 存到電腦上,不會自動裝到頭顯、也不會啟動。要一步到位要用 **Build And Run**,前提是頭顯要用 USB 連電腦並且已經在頭顯裡按過「允許 USB 偵錯」的授權。
- **每次 build 建議換不同檔名**(例如 `Spatial_Video_0916_LakeTahoe.apk`),存檔對話框裡改檔名就好,不然同名檔案被鎖住時 Unity 會自動加數字尾巴另存一份(例如變成 `Spatial_Video2.apk`),容易搞混新舊版本。
- 如果只有 `Build` 出 APK、沒有自動裝到頭顯,可以手動用 adb 裝上去再啟動(Unity 內建 Android SDK 路徑通常是 `<Unity安裝路徑>/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb.exe`):

  ```bash
  adb install -r path\to\Spatial_Video.apk
  # package name 可以用 aapt dump badging 那個 apk 查出來,通常是 com.DefaultCompany.Spatial_Video
  adb shell am start -n com.DefaultCompany.Spatial_Video/com.unity3d.player.UnityPlayerActivity
  ```

## 8. 手把 UI 互動(播放列按鈕)

播放器下方現在有一塊 YouTube 風格的控制面板(進度條、Play/Pause、-10s/+10s、Replay、Speed),可以用 Quest 手把的雷射光點選。這塊做起來比預期麻煩很多,以下記錄怎麼做、以及踩過的坑。

### 8.1 元件總覽

| 檔案/物件 | 用途 |
|---|---|
| `Assets/Scripts/SpatialVideoPlayerControls.cs` | 掛在 `SpatialVideoPlayer` 上,把 UI 按鈕/滑桿接到 VideoPlayer(Play/Pause、快轉、Replay、調速、進度同步) |
| `Assets/Scripts/SimpleRayVisual.cs` | 掛在每支手把的 `LineVisual` 物件上,自己畫雷射光、自己判定點擊(原因見 8.3) |
| `Assets/Scripts/TriggerDebugLogger.cs` | 診斷用小工具,繞過 XRI 直接讀扳機鍵原始訊號,印到 Console。平時不用管,除錯時很好用 |
| `Assets/Samples/XR Interaction Toolkit/3.5.0/Starter Assets/` | 透過 Package Manager 匯入的官方樣例,`XR Origin (XR Rig)` prefab 提供手把追蹤+雷射光互動的基礎 |
| 場景裡的 `Controls Canvas` | World Space Canvas,掛在 `SpatialVideoPlayer` 底下,`TrackedDeviceGraphicRaycaster` 元件(理論上用來給官方系統判定點擊,但實際判定改走 8.3 的繞道方案) |

### 8.2 ⚠️ 最重要的教訓:PrefabInstance 不要手動改 YAML

一開始想直接手寫場景/prefab 的 YAML 檔案加手把、加 UI,**兩次把 Unity 整個弄到開啟就 crash**(native crash,在 `MergePrefabInstanceInfosDuringLoad` 底層炸掉,不是正常的錯誤訊息)。原因是手動加的 `PrefabInstance` 修改區塊(尤其是引用外部 prefab、或欄位跟 Unity 實際序列化格式有細微差異時),Unity 這個版本的 prefab merge 邏輯處理不了,會直接 segfault,連 log 都不會正常報錯。

**結論:凡是要新增/修改 GameObject 層級結構(尤其牽涉到 PrefabInstance)的東西,一定要在 Editor 裡用滑鼠拖拉/勾選做,不要手動編輯 `.unity` / `.prefab` 檔案。** 單純改現有元件的欄位數值(位置、文字、顏色、勾選框)是安全的,可以直接改檔案;但新增物件、新增 PrefabInstance、新增元件到 PrefabInstance 底下的子物件,一定要走 Editor UI。

### 8.3 為什麼要自己寫 `SimpleRayVisual`,不用官方的 Near-Far Interactor

`XR Origin (XR Rig)` 官方 prefab 本身有一套完整的手把互動系統(`Near-Far Interactor` + `EventSystem` + `TrackedDeviceGraphicRaycaster`)。設定上全部看起來都是對的(Interaction Profile、UI Press Input binding、Select Input binding 都正確指到 `XRI Left/Right Interaction` 動作),雷射光 hover 到按鈕也真的有反應(按鈕會有 highlight),但**按下扳機鍵就是不會觸發 `Button.onClick`**,查了非常久都沒找到卡在哪一層。

用 `TriggerDebugLogger.cs` 直接繞過 XRI、用 `UnityEngine.InputSystem.XR.XRController` 讀原始扳機值,確認硬體訊號本身完全正常(按下去 Console 會印出 0~1 的類比值)。所以問題確定卡在 XRI 的 `Near-Far Interactor → EventSystem` 這段中間的判定鏈路裡,而不是輸入或設定本身。

因為debug 不出根本原因,`SimpleRayVisual.cs` 直接把整條路自己接:
1. 用 `LineRenderer` 自己畫線,跟 `targetPlane`(`Controls Canvas` 的 Transform)算平面交點當作雷射光落點,超過落點的地方裁掉,並生成一顆小球標示落點方便瞄準。
2. 用跟 `TriggerDebugLogger` 一樣的方式直接讀 `XRController` 的 `trigger` 原始值判斷有沒有按下。
3. 落點如果落在某個 `Button`/`Slider` 的 `RectTransform.rect` 範圍內(用 `RectTransform.InverseTransformPoint` 轉成該元件的本地座標判斷),扳機鍵剛按下的那一刻直接呼叫 `button.onClick.Invoke()` 或設定 `slider.value`。

完全不經過 `XRInteractionManager`/`EventSystem`/`TrackedDeviceGraphicRaycaster`,所以官方系統設定對不對都不影響它能不能用。

### 8.4 要幫新的手把/新的 UI 面板加上這套怎麼做

1. Hierarchy 找到該手把底下的 `LineVisual` 物件(在 `XR Origin (XR Rig) > Camera Offset > Left/Right Controller` 底下,`Near-Far Interactor` 附近)。
2. 取消勾選它身上原本的 **Curve Visual Controller**(官方視覺效果,跟我們自己的畫線衝突,關掉避免互搶)。
3. Add Component 加 **Simple Ray Visual**。
4. **Target Plane** 欄位拖上要互動的 Canvas 的 Transform。
5. **Is Left Hand** 勾選框:左手手把打勾,右手手把取消勾選(決定要讀哪隻手的 `XRController` 裝置)。
6. `SimpleRayVisual` 會自動抓 `targetPlane` 底下所有的 `Button`/第一個 `Slider`,不用額外指定要互動哪些元件。

### 8.5 其他小改動

- `SpatialVideoPlayer.cs` 的 `videoPlayer.isLooping` 改成 `false`——影片播完就停在最後一幀,不會自動重播。
- Play/Pause 按鈕文字用 `▶` / `❚❚` 符號(Unicode 字元直接當文字,不用額外圖片資源),-10s/+10s 用 `◀◀`/`▶▶`,Replay 用 `↺`。用的是舊版 `UI > Legacy > Text/Button`(不是 TextMeshPro),因為 `SpatialVideoPlayerControls.cs` 的欄位型別是 `UnityEngine.UI.Text`/`Button`。
