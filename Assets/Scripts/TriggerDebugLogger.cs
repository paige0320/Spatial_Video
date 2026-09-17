using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;

namespace SpatialVideo
{
    // 診斷用：完全不透過 XRI，直接讀左手把的 trigger，按下就印 log。
    // 用來確認扳機鍵的原始輸入訊號有沒有進到 Unity。
    public class TriggerDebugLogger : MonoBehaviour
    {
        private bool wasPressed;

        private void Update()
        {
            var device = InputSystem.GetDevice<XRController>(CommonUsages.LeftHand);
            if (device == null)
            {
                return;
            }

            var triggerControl = device.TryGetChildControl<AxisControl>("trigger");
            float value = triggerControl != null ? triggerControl.ReadValue() : 0f;
            bool isPressed = value > 0.5f;

            if (isPressed && !wasPressed)
            {
                Debug.Log($"[TriggerDebugLogger] 左手扳機按下！value={value}");
            }
            wasPressed = isPressed;
        }
    }
}
