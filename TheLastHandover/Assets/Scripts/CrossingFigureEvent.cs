using UnityEngine;

namespace LastHandover
{
    /// <summary>
    /// 路中央黑影：第二站确认后、镜中司机之前，路中央站着一个不躲雨的人。
    /// 它不要求操作，只要求你直视它并继续开——纯氛围节点，拉长站间呼吸。
    /// </summary>
    public sealed class CrossingFigureEvent : MonoBehaviour
    {
        private enum Phase { Locked, Approaching, Watching, Leaving, Complete }

        [SerializeField] private ShiftController shift;
        [SerializeField] private RepeatedStopEvent previous;
        [SerializeField] private Transform figureRoot;
        private Phase phase;
        private float travelled;
        private float walkTimer;

        public bool IsActive => phase != Phase.Locked && phase != Phase.Complete;
        public bool IsComplete => phase == Phase.Complete;
        public string Objective { get; private set; } = "";

        public void Configure(ShiftController controller, RepeatedStopEvent previousEvent, Transform figure)
        {
            shift = controller;
            previous = previousEvent;
            figureRoot = figure;
            figureRoot.gameObject.SetActive(false);
        }

        private void Update()
        {
            float speed = shift != null ? shift.RoadSpeed : 0f;

            if (phase == Phase.Locked)
            {
                if (previous == null || !previous.IsComplete) return;
                travelled += speed * Time.deltaTime;
                if (travelled < 14f) return;
                phase = Phase.Approaching;
                figureRoot.gameObject.SetActive(true);
                Objective = "路中央站着一只奶龙。它不躲雨。";
                return;
            }

            if (phase == Phase.Approaching)
            {
                travelled += speed * Time.deltaTime;
                figureRoot.Translate(0f, 0f, -speed * Time.deltaTime, Space.World);
                if (travelled >= 30f)
                {
                    phase = Phase.Watching;
                    Objective = "它不看车。它只数着雨。";
                }
                return;
            }

            if (phase == Phase.Watching)
            {
                travelled += speed * Time.deltaTime;
                if (travelled >= 44f)
                {
                    phase = Phase.Leaving;
                    walkTimer = 0f;
                    Objective = "它转身走向雨里。没有回头。继续开。";
                }
                return;
            }

            if (phase == Phase.Leaving)
            {
                walkTimer += Time.deltaTime;
                figureRoot.Translate(Time.deltaTime * 2.4f, 0f, -Time.deltaTime * 0.6f, Space.World);
                if (walkTimer >= 2.2f)
                {
                    figureRoot.gameObject.SetActive(false);
                    phase = Phase.Complete;
                    Objective = "";
                }
            }
        }
    }
}
