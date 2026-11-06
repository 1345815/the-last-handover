using UnityEngine;

namespace LastHandover
{
    public sealed class FinalShiftEvent : MonoBehaviour
    {
        private enum Phase { Locked, Waiting, Revealed, Ending }
        [SerializeField] private ShiftController shift;
        [SerializeField] private MirrorDriverEvent previous;
        [SerializeField] private GameObject fifthGearMark;
        private Phase phase;
        private float travelled;
        private float holdTime;

        public bool IsActive => phase != Phase.Locked;
        public bool IsEnding => phase == Phase.Ending;
        public string Objective { get; private set; } = "";

        public void Configure(ShiftController controller, MirrorDriverEvent previousEvent, GameObject mark)
        {
            shift = controller;
            previous = previousEvent;
            fifthGearMark = mark;
            fifthGearMark.SetActive(false);
        }

        private void Update()
        {
            if (phase == Phase.Locked)
            {
                if (previous == null || !previous.IsComplete) return;
                travelled += shift.RoadSpeed * Time.deltaTime;
                if (travelled < 44f) return;
                phase = Phase.Waiting;
                Objective = "终点再次变回第一站。换挡杆旁的刻痕像一个数字。按住 Shift。";
            }

            bool holdingShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (phase == Phase.Waiting)
            {
                holdTime = holdingShift ? holdTime + Time.deltaTime : 0f;
                if (holdTime >= 1.2f)
                {
                    phase = Phase.Revealed;
                    fifthGearMark.SetActive(true);
                    Objective = "不存在的第五档已经显形。保持 Shift 并按 W。";
                }
            }
            else if (phase == Phase.Revealed && holdingShift && Input.GetKeyDown(KeyCode.W))
            {
                phase = Phase.Ending;
                Objective = "交接完成。";
            }
        }
    }
}
