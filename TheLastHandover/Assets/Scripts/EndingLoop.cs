using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastHandover
{
    /// <summary>
    /// 交接循环：结局画面停留数秒后淡黑并重载场景，
    /// 呼应"下一班将在 00:13 发车"——司机永远在交接。
    /// </summary>
    public sealed class EndingLoop : MonoBehaviour
    {
        [SerializeField] private FinalShiftEvent finalEvent;
        [SerializeField] private float fadeDelay = 3f;
        [SerializeField] private float fadeDuration = 4f;
        [SerializeField] private float restartAt = 7.5f;

        private float timer;
        private bool ending;

        public void Configure(FinalShiftEvent eventToWatch)
        {
            finalEvent = eventToWatch;
        }

        private void Update()
        {
            if (!ending && finalEvent != null && finalEvent.IsEnding)
            {
                ending = true;
                timer = 0f;
            }
            if (ending)
            {
                timer += Time.deltaTime;
                if (timer >= restartAt) SceneManager.LoadScene(0);
            }
        }

        private void OnGUI()
        {
            if (!ending) return;
            float alpha = Mathf.Clamp01((timer - fadeDelay) / fadeDuration);
            if (alpha <= 0f) return;
            var previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, alpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
