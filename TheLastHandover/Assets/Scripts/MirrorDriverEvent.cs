using UnityEngine;

namespace LastHandover
{
    public sealed class MirrorDriverEvent : MonoBehaviour
    {
        private enum Phase { Locked, Watching, Synced, Complete }
        [SerializeField] private ShiftController shift;
        [SerializeField] private RepeatedStopEvent previous;
        [SerializeField] private GameObject mirrorDriver;
        private Phase phase;
        private float travelled;

        public bool IsActive => phase != Phase.Locked;
        public bool IsComplete => phase == Phase.Complete;
        public string Objective { get; private set; } = "";

        public void Configure(ShiftController controller, RepeatedStopEvent previousEvent, GameObject driver)
        {
            shift = controller;
            previous = previousEvent;
            mirrorDriver = driver;
            mirrorDriver.SetActive(false);
        }

        private void OnEnable()
        {
            if (shift != null) shift.GearChanged += OnGearChanged;
        }

        private void Start()
        {
            if (shift != null) shift.GearChanged += OnGearChanged;
        }

        private void OnDisable()
        {
            if (shift != null) shift.GearChanged -= OnGearChanged;
        }

        private void Update()
        {
            if (phase != Phase.Locked || previous == null || !previous.IsComplete) return;
            travelled += shift.RoadSpeed * Time.deltaTime;
            if (travelled < 62f) return;
            phase = Phase.Watching;
            mirrorDriver.SetActive(true);
            Objective = "后视镜里多出一名司机。升到三档，迫使它暴露动作。";
        }

        private void OnGearChanged(Gear gear)
        {
            if (phase == Phase.Watching && gear == Gear.Three)
            {
                phase = Phase.Synced;
                Objective = "它已经完全同步。立刻降回一档，打破镜中动作。";
            }
            else if (phase == Phase.Synced && gear == Gear.One)
            {
                phase = Phase.Complete;
                mirrorDriver.SetActive(false);
                Objective = "镜中司机消失了，但换挡杆旁出现了一道新的刻痕。";
            }
        }
    }
}
