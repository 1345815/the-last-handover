using UnityEngine;

namespace LastHandover
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        [SerializeField] private ShiftController shift;
        [SerializeField] private FirstStopEvent stopEvent;
        private GUIStyle title;
        private GUIStyle small;

        public void Configure(ShiftController controller, FirstStopEvent firstStop = null)
        {
            shift = controller;
            stopEvent = firstStop;
        }

        private void EnsureStyles()
        {
            if (title != null) return;
            title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.78f, 0.86f, 0.72f) }
            };
            small = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                normal = { textColor = new Color(0.68f, 0.72f, 0.65f) }
            };
        }

        private void OnGUI()
        {
            EnsureStyles();
            GUI.Label(new Rect(28, 24, 500, 50), "末班交接 // 原型", title);
            GUI.Label(new Rect(30, 72, 600, 30), "W / S 换挡　鼠标观察　Esc 释放鼠标", small);
            if (stopEvent != null)
            {
                GUI.Box(new Rect(25, Screen.height - 105, 650, 70), "");
                GUI.Label(new Rect(40, Screen.height - 96, 620, 28), stopEvent.Objective, small);
                GUI.Label(new Rect(40, Screen.height - 68, 240, 28), $"车内乘客：{stopEvent.PassengerCount}", small);
            }
            string gear = shift == null ? "?" : shift.CurrentGear == Gear.Neutral ? "N" : ((int)shift.CurrentGear).ToString();
            GUI.Label(new Rect(Screen.width - 110, Screen.height - 105, 90, 80), gear, title);
        }
    }
}

