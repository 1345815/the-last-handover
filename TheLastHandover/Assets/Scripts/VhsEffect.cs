using UnityEngine;

namespace LastHandover
{
    [RequireComponent(typeof(Camera))]
    public sealed class VhsEffect : MonoBehaviour
    {
        [SerializeField] private Shader shader;
        [SerializeField, Range(0f, 0.2f)] private float noiseAmount = 0.06f;
        [SerializeField, Range(0f, 0.5f)] private float scanlineAmount = 0.17f;
        [SerializeField, Range(0f, 0.01f)] private float chromaAmount = 0.0028f;
        [SerializeField, Range(0f, 1f)] private float vignetteAmount = 0.45f;
        [SerializeField, Range(0f, 0.05f)] private float trackingJitter = 0.02f;
        private Material material;

        public void Configure(Shader source)
        {
            shader = source;
        }

        private void Start()
        {
            if (shader == null) shader = Shader.Find("Hidden/LastHandoverVhs");
            if (shader != null) material = new Material(shader);
            else enabled = false;
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (material == null)
            {
                Graphics.Blit(src, dst);
                return;
            }
            material.SetFloat("_NoiseAmount", noiseAmount);
            material.SetFloat("_ScanlineAmount", scanlineAmount);
            material.SetFloat("_ChromaAmount", chromaAmount);
            material.SetFloat("_VignetteAmount", vignetteAmount);
            material.SetFloat("_TrackingJitter", trackingJitter);
            Graphics.Blit(src, dst, material);
        }
    }
}
