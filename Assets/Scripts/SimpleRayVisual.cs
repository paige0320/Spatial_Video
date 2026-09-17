using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

namespace SpatialVideo
{
    // 完全不透過 XRI/EventSystem 的簡易手把 UI 互動：
    // 自己畫雷射光、自己判斷落點在哪個按鈕/滑桿上、自己讀扳機鍵原始訊號、
    // 命中就直接呼叫 onClick，不依賴中間任何一層可能故障的東西。
    //
    // 之所以繞過 XRI：Near-Far Interactor 的 hover 有作用（按鈕會有反應）、
    // 扳機鍵原始訊號也確認有進到 Unity（TriggerDebugLogger 驗證過），
    // 但 press 就是沒有真的觸發 UI 的 onClick，設定看起來都對，
    // 查不出問題出在 XRI 內部哪一層，所以乾脆整條自己接。
    [RequireComponent(typeof(LineRenderer))]
    public class SimpleRayVisual : MonoBehaviour
    {
        [SerializeField] private float length = 5f;
        [SerializeField] private float width = 0.02f;
        [SerializeField] private Color color = Color.red;
        [SerializeField] private Transform targetPlane;
        [SerializeField] private bool isLeftHand = true;

        private LineRenderer lineRenderer;
        private Transform dot;
        private List<Button> buttons = new List<Button>();
        private Slider slider;
        private bool wasTriggerPressed;

        private void Awake()
        {
            // 排除掉「物件在某個被攝影機排除的 Layer 上」這個可能性。
            gameObject.layer = 0;

            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;

            // 不管原本有沒有材質，強制換成保證會顯示顏色的簡單材質——
            // sample 附的材質可能靠 shader 內部邏輯（跟著 hover 狀態變）決定透明度，
            // 現在沒有東西在驅動它，很可能就直接變成完全透明看不到。
            var shader = Shader.Find("Sprites/Default");
            lineRenderer.material = new Material(shader);
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.numCapVertices = 4;
            lineRenderer.sortingOrder = 100;

            // 落點用一顆小球表示，執行期自己生成，不用另外在場景裡加物件。
            var dotObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dotObj.name = "RayDot";
            Destroy(dotObj.GetComponent<Collider>());
            // 落點球用固定大小，不要跟著雷射線的寬度一起變（線可能設得很細，球太小會完全看不到）。
            dotObj.transform.localScale = Vector3.one * 0.03f;
            dotObj.GetComponent<MeshRenderer>().material = new Material(shader) { color = color };
            dotObj.layer = 0;
            dot = dotObj.transform;
            dot.gameObject.SetActive(false);

            if (targetPlane != null)
            {
                buttons.AddRange(targetPlane.GetComponentsInChildren<Button>(true));
                slider = targetPlane.GetComponentInChildren<Slider>(true);
            }
        }

        private void Update()
        {
            Vector3 origin = transform.position;
            Vector3 direction = transform.forward;
            Vector3 endPoint = origin + direction * length;
            bool hasHit = false;

            if (targetPlane != null)
            {
                Vector3 planeNormal = targetPlane.forward;
                float denom = Vector3.Dot(planeNormal, direction);
                if (Mathf.Abs(denom) > 0.0001f)
                {
                    float t = Vector3.Dot(targetPlane.position - origin, planeNormal) / denom;
                    if (t > 0f && t < length)
                    {
                        endPoint = origin + direction * t;
                        hasHit = true;
                    }
                }
            }

            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, endPoint);

            dot.gameObject.SetActive(hasHit);
            if (hasHit)
            {
                dot.position = endPoint;
            }

            HandleUIInteraction(hasHit, endPoint);
        }

        private void HandleUIInteraction(bool hasHit, Vector3 hitPoint)
        {
            bool isPressed = ReadTriggerPressed();
            bool justPressed = isPressed && !wasTriggerPressed;
            wasTriggerPressed = isPressed;

            if (!hasHit) return;

            foreach (var button in buttons)
            {
                if (button == null || !button.isActiveAndEnabled) continue;

                var rect = (RectTransform)button.transform;
                Vector3 local = rect.InverseTransformPoint(hitPoint);
                if (rect.rect.Contains(local))
                {
                    if (justPressed)
                    {
                        button.onClick.Invoke();
                    }
                    return;
                }
            }

            if (slider != null && slider.isActiveAndEnabled && isPressed)
            {
                var rect = (RectTransform)slider.transform;
                Vector3 local = rect.InverseTransformPoint(hitPoint);
                if (rect.rect.Contains(local))
                {
                    float normalized = Mathf.InverseLerp(rect.rect.xMin, rect.rect.xMax, local.x);
                    slider.value = normalized;
                }
            }
        }

        private bool ReadTriggerPressed()
        {
            var usage = isLeftHand ? CommonUsages.LeftHand : CommonUsages.RightHand;
            var device = InputSystem.GetDevice<XRController>(usage);
            if (device == null) return false;

            var triggerControl = device.TryGetChildControl<AxisControl>("trigger");
            float value = triggerControl != null ? triggerControl.ReadValue() : 0f;
            return value > 0.5f;
        }
    }
}
