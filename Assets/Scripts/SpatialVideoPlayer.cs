using UnityEngine;
using UnityEngine.Video;

namespace SpatialVideo
{
    public enum StereoMode
    {
        Mono,
        SideBySide,
        TopAndBottom
    }

    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(AudioSource))]
    public class SpatialVideoPlayer : MonoBehaviour
    {
        [SerializeField] private VideoClip clip;
        [SerializeField] private StereoMode stereoMode = StereoMode.Mono;
        [SerializeField] private Renderer screenRenderer;
        [SerializeField] private RenderTexture renderTexture;

        private VideoPlayer videoPlayer;
        private RenderTexture runtimeRenderTexture;

        public StereoMode Mode => stereoMode;

        private void Awake()
        {
            videoPlayer = GetComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, GetComponent<AudioSource>());
            videoPlayer.isLooping = false;

            ApplyStereoMode();
        }

        private void Start()
        {
            if (clip != null)
            {
                Play(clip);
            }
        }

        private void OnDestroy()
        {
            ReleaseRuntimeRenderTexture();
        }

        // 讓 Inspector 上「Spatial Video Player (Script)」元件裡的 Clip 欄位
        // 在 Play Mode 中被直接拖換時也能即時生效，不用重新進 Play。
        private void OnValidate()
        {
            if (videoPlayer == null)
            {
                videoPlayer = GetComponent<VideoPlayer>();
            }
            if (Application.isPlaying && videoPlayer != null && clip != null && videoPlayer.clip != clip)
            {
                Play(clip);
            }
        }

        public void Play(VideoClip newClip)
        {
            clip = newClip;
            videoPlayer.clip = clip;

            // RT 的長寬一定要跟 clip 原生解析度一致，不然 SBS 影片左右眼的分界
            // 會跟 shader 裡假設的 UV 中線對不上（畫面被裁切/擠壓，看起來就是變形）。
            // 不同來源的 SBS 影片解析度不一定一樣（自己拍的 3840x1080 vs 下載的
            // 1920x1080），所以改成依 clip 大小在執行期動態建立，不再依賴 Inspector
            // 手動配一顆尺寸剛好對上的 RenderTexture 資產（很容易忘記換而對不上）。
            EnsureRenderTextureMatchesClip(clip);
            videoPlayer.targetTexture = renderTexture;
            if (screenRenderer != null)
            {
                screenRenderer.material.mainTexture = renderTexture;
            }

            videoPlayer.Play();
        }

        private void EnsureRenderTextureMatchesClip(VideoClip forClip)
        {
            int width = (int)forClip.width;
            int height = (int)forClip.height;

            if (renderTexture != null && renderTexture.width == width && renderTexture.height == height)
            {
                return;
            }

            ReleaseRuntimeRenderTexture();
            runtimeRenderTexture = new RenderTexture(width, height, 0);
            renderTexture = runtimeRenderTexture;
        }

        private void ReleaseRuntimeRenderTexture()
        {
            if (runtimeRenderTexture == null) return;

            if (videoPlayer != null && videoPlayer.targetTexture == runtimeRenderTexture)
            {
                videoPlayer.targetTexture = null;
            }
            runtimeRenderTexture.Release();
            Destroy(runtimeRenderTexture);
            runtimeRenderTexture = null;
        }

        public void SetStereoMode(StereoMode mode)
        {
            stereoMode = mode;
            ApplyStereoMode();
        }

        // 材質用的是 SpatialVideo/StereoUnlit shader：因為 Quest 走 single-pass
        // instanced，兩眼共用同一個 draw call，沒辦法用 mainTextureScale/Offset
        // 這種材質屬性去分左右眼（那樣兩眼會看到一樣的半邊）。真正的左右眼分流
        // 是 shader 內部用 unity_StereoEyeIndex 做的，這裡只是把模式告訴 shader。
        private void ApplyStereoMode()
        {
            if (screenRenderer == null) return;

            screenRenderer.material.SetFloat("_StereoMode", (float)stereoMode);
        }
    }
}
