using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace SpatialVideo
{
    // YouTube 風格的播放列，靠手把 Ray Interactor 點 world space Canvas 上的按鈕操作。
    [RequireComponent(typeof(VideoPlayer))]
    public class SpatialVideoPlayerControls : MonoBehaviour
    {
        private static readonly float[] PlaybackSpeeds = { 0.5f, 1f, 1.5f, 2f };
        private const float SkipSeconds = 10f;

        [SerializeField] private Slider seekSlider;
        [SerializeField] private Text timeText;
        [SerializeField] private Button playPauseButton;
        [SerializeField] private Text playPauseButtonText;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button rewindButton;
        [SerializeField] private Button forwardButton;
        [SerializeField] private Button speedButton;
        [SerializeField] private Text speedButtonText;

        private VideoPlayer videoPlayer;
        private int speedIndex = 1; // PlaybackSpeeds[1] == 1x
        private bool suppressSliderCallback;

        private void Awake()
        {
            videoPlayer = GetComponent<VideoPlayer>();

            if (playPauseButton != null) playPauseButton.onClick.AddListener(TogglePlayPause);
            if (replayButton != null) replayButton.onClick.AddListener(Replay);
            if (rewindButton != null) rewindButton.onClick.AddListener(() => Skip(-SkipSeconds));
            if (forwardButton != null) forwardButton.onClick.AddListener(() => Skip(SkipSeconds));
            if (speedButton != null) speedButton.onClick.AddListener(CycleSpeed);
            if (seekSlider != null) seekSlider.onValueChanged.AddListener(OnSeekSliderChanged);

            UpdatePlayPauseLabel();
            UpdateSpeedLabel();
        }

        private void Update()
        {
            if (videoPlayer == null || videoPlayer.length <= 0) return;

            double progress = videoPlayer.time / videoPlayer.length;

            if (seekSlider != null)
            {
                suppressSliderCallback = true;
                seekSlider.SetValueWithoutNotify((float)progress);
                suppressSliderCallback = false;
            }

            if (timeText != null)
            {
                timeText.text = $"{FormatTime(videoPlayer.time)} / {FormatTime(videoPlayer.length)}";
            }

            UpdatePlayPauseLabel();
        }

        public void TogglePlayPause()
        {
            if (videoPlayer.isPlaying) videoPlayer.Pause();
            else videoPlayer.Play();

            UpdatePlayPauseLabel();
        }

        public void Replay()
        {
            videoPlayer.time = 0;
            videoPlayer.Play();
        }

        public void Skip(float seconds)
        {
            if (videoPlayer.length <= 0) return;

            double target = videoPlayer.time + seconds;
            videoPlayer.time = System.Math.Clamp(target, 0, videoPlayer.length);
        }

        public void CycleSpeed()
        {
            speedIndex = (speedIndex + 1) % PlaybackSpeeds.Length;
            videoPlayer.playbackSpeed = PlaybackSpeeds[speedIndex];
            UpdateSpeedLabel();
        }

        private void OnSeekSliderChanged(float value)
        {
            // Update() 每幀用 SetValueWithoutNotify 同步進度條時不會觸發這個 callback；
            // 只有使用者實際拖曳滑桿才會走到這裡，用 suppressSliderCallback 做保險。
            if (suppressSliderCallback || videoPlayer.length <= 0) return;

            videoPlayer.time = value * videoPlayer.length;
        }

        private void UpdatePlayPauseLabel()
        {
            if (playPauseButtonText != null)
            {
                playPauseButtonText.text = videoPlayer.isPlaying ? "❚❚" : "▶";
            }
        }

        private void UpdateSpeedLabel()
        {
            if (speedButtonText != null)
            {
                speedButtonText.text = $"{PlaybackSpeeds[speedIndex]:0.#}x";
            }
        }

        private static string FormatTime(double seconds)
        {
            if (seconds < 0) seconds = 0;
            var t = System.TimeSpan.FromSeconds(seconds);
            return t.Hours > 0
                ? $"{t.Hours}:{t.Minutes:00}:{t.Seconds:00}"
                : $"{t.Minutes:00}:{t.Seconds:00}";
        }
    }
}
