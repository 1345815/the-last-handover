using UnityEngine;

namespace LastHandover
{
    public sealed class RoadLooper : MonoBehaviour
    {
        [SerializeField] private ShiftController shift;
        [SerializeField] private Transform[] segments;
        [SerializeField] private float segmentLength = 40f;
        [SerializeField] private float recycleZ = -25f;

        private void Update()
        {
            if (shift == null || segments == null) return;
            float movement = shift.RoadSpeed * Time.deltaTime;
            foreach (var segment in segments)
            {
                if (segment == null) continue;
                segment.Translate(0f, 0f, -movement, Space.World);
                if (segment.position.z < recycleZ)
                    segment.position += Vector3.forward * segmentLength * segments.Length;
            }
        }

        public void Configure(ShiftController controller, Transform[] roadSegments, float length)
        {
            shift = controller;
            segments = roadSegments;
            segmentLength = length;
        }
    }
}

