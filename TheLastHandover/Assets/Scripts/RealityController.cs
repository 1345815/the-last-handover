using UnityEngine;

namespace LastHandover
{
    public sealed class RealityController : MonoBehaviour
    {
        [SerializeField] private ShiftController shift;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Light cabinLight;

        private void Awake()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.018f;
        }

        private void OnEnable()
        {
            if (shift != null) shift.GearChanged += Apply;
        }

        private void Start() => Apply(shift != null ? shift.CurrentGear : Gear.Neutral);

        private void OnDisable()
        {
            if (shift != null) shift.GearChanged -= Apply;
        }

        public void Configure(ShiftController controller, Camera camera, Light light)
        {
            shift = controller;
            viewCamera = camera;
            cabinLight = light;
        }

        private void Apply(Gear gear)
        {
            Color fog;
            Color cabin;
            switch (gear)
            {
                case Gear.Two:
                    fog = new Color(0.07f, 0.10f, 0.09f);
                    cabin = new Color(0.55f, 0.72f, 0.60f);
                    break;
                case Gear.Three:
                    fog = new Color(0.16f, 0.025f, 0.02f);
                    cabin = new Color(0.85f, 0.16f, 0.08f);
                    break;
                case Gear.One:
                    fog = new Color(0.035f, 0.045f, 0.055f);
                    cabin = new Color(0.72f, 0.62f, 0.42f);
                    break;
                default:
                    fog = new Color(0.012f, 0.015f, 0.02f);
                    cabin = new Color(0.32f, 0.38f, 0.34f);
                    break;
            }

            RenderSettings.fogColor = fog;
            RenderSettings.ambientLight = fog * 1.8f;
            if (viewCamera != null) viewCamera.backgroundColor = fog;
            if (cabinLight != null) cabinLight.color = cabin;
        }
    }
}

