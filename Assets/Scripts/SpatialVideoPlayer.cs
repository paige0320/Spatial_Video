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

        public StereoMode Mode => stereoMode;

        private void Awake()
        {
            videoPlayer = GetComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, GetComponent<AudioSource>());
            videoPlayer.isLooping = true;

            ApplyStereoMode();
        }

        private void Start()
        {
            if (clip != null)
            {
                Play(clip);
            }
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
            videoPlayer.Play();
        }

        public void SetStereoMode(StereoMode mode)
        {
            stereoMode = mode;
            ApplyStereoMode();
        }

        // 目前素材都是平面 2D 影片，一律當作 Mono 鋪滿畫面。
        // 之後換成真正的 180/360 spatial（stereo）影片時，
        // 在這裡依 stereoMode 把 UV 切成左右半（SideBySide）或上下半（TopAndBottom），
        // 分別餵給左右眼的攝影機/材質即可，播放流程本身不用改。
        private void ApplyStereoMode()
        {
            if (screenRenderer == null) return;

            switch (stereoMode)
            {
                case StereoMode.Mono:
                    screenRenderer.material.mainTextureScale = new Vector2(1f, 1f);
                    screenRenderer.material.mainTextureOffset = Vector2.zero;
                    break;
                case StereoMode.SideBySide:
                    screenRenderer.material.mainTextureScale = new Vector2(0.5f, 1f);
                    screenRenderer.material.mainTextureOffset = Vector2.zero;
                    break;
                case StereoMode.TopAndBottom:
                    screenRenderer.material.mainTextureScale = new Vector2(1f, 0.5f);
                    screenRenderer.material.mainTextureOffset = new Vector2(0f, 0.5f);
                    break;
            }
        }
    }
}
