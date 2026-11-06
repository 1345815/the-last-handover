using UnityEngine;

namespace LastHandover
{
    public sealed class RepeatedStopEvent : MonoBehaviour
    {
        private enum Phase { Locked, Approaching, Listening, Heard, Complete }

        [SerializeField] private ShiftController shift;
        [SerializeField] private FirstStopEvent firstStop;
        [SerializeField] private Transform signsRoot;
        private Phase phase;
        private float travelled;
        private float silenceTime;

        public bool IsActive => phase != Phase.Locked;
        public bool IsComplete => phase == Phase.Complete;
        public string Objective { get; private set; } = "";

        public void Configure(ShiftController controller, FirstStopEvent previous, Transform root)
        {
            shift = controller;
            firstStop = previous;
            signsRoot = root;
            signsRoot.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (phase == Phase.Locked)
            {
                if (firstStop == null || !firstStop.IsComplete) return;
                travelled += shift.RoadSpeed * Time.deltaTime;
                if (travelled < 28f) return;
                phase = Phase.Approaching;
                signsRoot.gameObject.SetActive(true);
                Objective = "同一站牌连续出现。广播报出了三个不同站名。";
            }

            if (phase == Phase.Approaching)
            {
                signsRoot.Translate(0f, 0f, -shift.RoadSpeed * Time.deltaTime, Space.World);
                if (signsRoot.position.z <= 11f)
                {
                    phase = Phase.Listening;
                    Objective = "站牌正在重复。挂入空挡，让引擎安静下来。";
                }
            }
            else if (phase == Phase.Listening)
            {
                silenceTime = shift.CurrentGear == Gear.Neutral ? silenceTime + Time.deltaTime : 0f;
                if (silenceTime >= 2f)
                {
                    phase = Phase.Heard;
                    Objective = "噪声下的广播：『终点前站』。按 E 确认停车。";
                }
            }
            else if (phase == Phase.Heard && shift.CurrentGear == Gear.Neutral && Input.GetKeyDown(KeyCode.E))
            {
                phase = Phase.Complete;
                Objective = "正确站名已确认。道路循环解除，换入一档继续。";
            }

            if (phase == Phase.Complete)
                signsRoot.Translate(0f, 0f, -shift.RoadSpeed * Time.deltaTime, Space.World);
        }
    }
}
