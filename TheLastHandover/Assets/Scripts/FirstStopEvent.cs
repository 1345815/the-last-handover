using UnityEngine;

namespace LastHandover
{
    public sealed class FirstStopEvent : MonoBehaviour
    {
        private enum Phase { Approaching, Waiting, Boarded, Identified, Complete }

        [SerializeField] private ShiftController shift;
        [SerializeField] private Transform stopRoot;
        [SerializeField] private GameObject anomalyPassenger;
        private Phase phase;
        private float revealTime;
        private int passengers = 3;

        public int PassengerCount => passengers;
        public string Objective { get; private set; } = "前方站点：保持一档，准备停车";

        public void Configure(ShiftController controller, Transform root, GameObject anomaly)
        {
            shift = controller;
            stopRoot = root;
            anomalyPassenger = anomaly;
            anomalyPassenger.SetActive(false);
        }

        private void Update()
        {
            if (shift == null || stopRoot == null) return;

            if (phase == Phase.Approaching)
            {
                stopRoot.Translate(0f, 0f, -shift.RoadSpeed * Time.deltaTime, Space.World);
                if (stopRoot.position.z <= 9f)
                {
                    phase = Phase.Waiting;
                    Objective = "站点到达：挂入空挡 N，然后按 E 开门";
                }
            }

            if (phase == Phase.Waiting && shift.CurrentGear == Gear.Neutral && Input.GetKeyDown(KeyCode.E))
            {
                passengers = 4;
                anomalyPassenger.SetActive(true);
                phase = Phase.Boarded;
                Objective = "乘客计数：4。手册规定末班车只能有三人。换入二档观察。";
            }
            else if (phase == Phase.Boarded)
            {
                revealTime = shift.CurrentGear == Gear.Two ? revealTime + Time.deltaTime : 0f;
                anomalyPassenger.SetActive(shift.CurrentGear == Gear.Two);
                if (revealTime > 1.2f)
                {
                    phase = Phase.Identified;
                    anomalyPassenger.SetActive(true);
                    Objective = "后视镜中出现第四名乘客。挂回空挡并按 E 开门。";
                }
            }
            else if (phase == Phase.Identified && shift.CurrentGear == Gear.Neutral && Input.GetKeyDown(KeyCode.E))
            {
                passengers = 3;
                anomalyPassenger.SetActive(false);
                phase = Phase.Complete;
                Objective = "异常已离车。换入一档，继续完成交接。";
            }
            else if (phase == Phase.Complete)
            {
                stopRoot.Translate(0f, 0f, -shift.RoadSpeed * Time.deltaTime, Space.World);
            }
        }
    }
}

