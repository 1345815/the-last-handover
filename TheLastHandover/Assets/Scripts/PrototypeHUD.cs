using UnityEngine;

namespace LastHandover
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        [SerializeField] private ShiftController shift;
        [SerializeField] private FirstStopEvent stopEvent;
        [SerializeField] private RepeatedStopEvent repeatedStop;
        [SerializeField] private CrossingFigureEvent crossingFigure;
        [SerializeField] private MirrorDriverEvent mirrorEvent;
        [SerializeField] private FinalShiftEvent finalEvent;
        private GUIStyle title;
        private GUIStyle small;

        public void Configure(ShiftController controller, FirstStopEvent firstStop = null, RepeatedStopEvent secondStop = null, MirrorDriverEvent thirdEvent = null, FinalShiftEvent ending = null, CrossingFigureEvent figure = null)
        {
            shift = controller;
            stopEvent = firstStop;
            repeatedStop = secondStop;
            mirrorEvent = thirdEvent;
            finalEvent = ending;
            crossingFigure = figure;
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
                string objective = finalEvent != null && finalEvent.IsActive
                    ? finalEvent.Objective
                    : mirrorEvent != null && mirrorEvent.IsActive
                    ? mirrorEvent.Objective
                    : crossingFigure != null && crossingFigure.IsActive
                    ? crossingFigure.Objective
                    : repeatedStop != null && repeatedStop.IsActive ? repeatedStop.Objective : stopEvent.Objective;
                GUI.Label(new Rect(40, Screen.height - 96, 620, 28), objective, small);
                GUI.Label(new Rect(40, Screen.height - 68, 240, 28), $"车内乘客：{stopEvent.PassengerCount}", small);
            }
            string gear = shift == null ? "?" : shift.CurrentGear == Gear.Neutral ? "N" : ((int)shift.CurrentGear).ToString();
            GUI.Label(new Rect(Screen.width - 110, Screen.height - 105, 90, 80), gear, title);

            if (finalEvent != null && finalEvent.IsEnding)
            {
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
                var endingTitle = new GUIStyle(title) { alignment = TextAnchor.MiddleCenter, fontSize = 38 };
                var endingText = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter, fontSize = 21 };
                GUI.Label(new Rect(0, Screen.height * 0.31f, Screen.width, 65), "末班交接", endingTitle);
                GUI.Label(new Rect(0, Screen.height * 0.45f, Screen.width, 45), "前任司机走下了车。", endingText);
                GUI.Label(new Rect(0, Screen.height * 0.53f, Screen.width, 45), "新的交接手册上，写着你的名字。", endingText);
                GUI.Label(new Rect(0, Screen.height * 0.68f, Screen.width, 45), "下一班将在 00:13 发车。", endingText);
            }
        }
    }
}

