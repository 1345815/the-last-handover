using UnityEngine;

namespace LastHandover
{
    /// <summary>
    /// 雨刮：两根雨刮臂绕挡风玻璃底部的轴往复摆动，右臂相位镜像。
    /// </summary>
    public sealed class WiperController : MonoBehaviour
    {
        [SerializeField] private Transform leftPivot;
        [SerializeField] private Transform rightPivot;
        [SerializeField] private float sweepSpeed = 1.15f;
        [SerializeField] private float maxAngle = 42f;

        public void Configure(Transform left, Transform right)
        {
            leftPivot = left;
            rightPivot = right;
        }

        private void Update()
        {
            float angle = Mathf.Sin(Time.time * sweepSpeed) * maxAngle;
            if (leftPivot != null) leftPivot.localRotation = Quaternion.Euler(0f, 0f, angle);
            if (rightPivot != null) rightPivot.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }
    }
}
