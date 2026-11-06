using UnityEngine;

namespace LastHandover
{
    public sealed class DriverCamera : MonoBehaviour
    {
        [SerializeField] private float sensitivity = 1.4f;
        [SerializeField] private float horizontalLimit = 58f;
        [SerializeField] private float verticalLimit = 24f;
        [SerializeField] private ShiftController shift;
        private float yaw;
        private float pitch;
        private Vector3 basePosition;

        public void Configure(ShiftController controller)
        {
            shift = controller;
        }

        private void Start()
        {
            basePosition = transform.localPosition;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            yaw = Mathf.Clamp(yaw + Input.GetAxis("Mouse X") * sensitivity, -horizontalLimit, horizontalLimit);
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * sensitivity, -verticalLimit, verticalLimit);

            float speedFactor = shift == null ? 0.5f : (int)shift.CurrentGear * 0.28f + 0.5f;
            float t = Time.time;
            Vector3 sway = new Vector3(
                (Mathf.PerlinNoise(t * 0.7f, 3.1f) - 0.5f) * 0.022f * speedFactor,
                (Mathf.PerlinNoise(t * 0.6f, 7.7f) - 0.5f) * 0.018f * speedFactor,
                0f);
            transform.localPosition = basePosition + sway;
            float roll = (Mathf.PerlinNoise(t * 0.5f, 11.3f) - 0.5f) * 1.4f * speedFactor;
            transform.localRotation = Quaternion.Euler(pitch, yaw, roll);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                bool locked = Cursor.lockState == CursorLockMode.Locked;
                Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = locked;
            }
        }
    }
}
