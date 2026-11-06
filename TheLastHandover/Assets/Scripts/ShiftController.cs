using System;
using UnityEngine;

namespace LastHandover
{
    public enum Gear { Neutral, One, Two, Three }

    public sealed class ShiftController : MonoBehaviour
    {
        [SerializeField] private Gear currentGear = Gear.Neutral;
        public Gear CurrentGear => currentGear;
        public float RoadSpeed => currentGear switch
        {
            Gear.One => 7f,
            Gear.Two => 12f,
            Gear.Three => 19f,
            _ => 0f
        };

        public event Action<Gear> GearChanged;

        private void Update()
        {
            var next = currentGear;
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                next = (Gear)Mathf.Min((int)currentGear + 1, (int)Gear.Three);
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                next = (Gear)Mathf.Max((int)currentGear - 1, (int)Gear.Neutral);

            if (next == currentGear) return;
            currentGear = next;
            GearChanged?.Invoke(currentGear);
        }
    }
}

