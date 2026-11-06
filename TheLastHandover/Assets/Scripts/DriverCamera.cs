using UnityEngine;

namespace LastHandover
{
    public sealed class DriverCamera : MonoBehaviour
    {
        [SerializeField] private float sensitivity = 1.4f;
        [SerializeField] private float horizontalLimit = 58f;
        [SerializeField] private float verticalLimit = 24f;
        private float yaw;
        private float pitch;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            yaw = Mathf.Clamp(yaw + Input.GetAxis("Mouse X") * sensitivity, -horizontalLimit, horizontalLimit);
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * sensitivity, -verticalLimit, verticalLimit);
            transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                bool locked = Cursor.lockState == CursorLockMode.Locked;
                Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = locked;
            }
        }
    }
}

