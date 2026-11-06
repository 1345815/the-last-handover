using System.Collections;
using UnityEngine;

namespace LastHandover
{
    /// <summary>
    /// 程序化音频总线：引擎音随挡位变频，目标提示音随事件阶段切换，
    /// 开门气声随乘客数变化，结局时静电拉高、引擎淡出。
    /// 通过轮询各事件的 Objective 变化驱动，不侵入事件脚本。
    /// </summary>
    public sealed class AudioDirector : MonoBehaviour
    {
        private static readonly float[] GearPitch = { 0.85f, 1.0f, 1.22f, 1.48f };

        [SerializeField] private ShiftController shift;
        [SerializeField] private FirstStopEvent firstStop;
        [SerializeField] private RepeatedStopEvent repeatedStop;
        [SerializeField] private MirrorDriverEvent mirrorEvent;
        [SerializeField] private FinalShiftEvent finalEvent;

        private AudioSource engine;
        private AudioSource radio;
        private AudioSource fx;

        private string lastObjective;
        private int lastPassengers = int.MinValue;
        private bool endingPlayed;

        public void Configure(ShiftController controller, FirstStopEvent first, RepeatedStopEvent second,
            MirrorDriverEvent mirror, FinalShiftEvent final, AudioClip engineClip, AudioClip staticClip,
            AudioClip clunkClip, AudioClip hissClip, AudioClip chimeClip, AudioClip endingClip)
        {
            shift = controller;
            firstStop = first;
            repeatedStop = second;
            mirrorEvent = mirror;
            finalEvent = final;

            engine = AddLoopSource(engineClip, 0.5f);
            radio = AddLoopSource(staticClip, 0.13f);
            fx = gameObject.AddComponent<AudioSource>();
            fx.playOnAwake = false;
            fx.volume = 0.85f;

            clunk = clunkClip;
            hiss = hissClip;
            chime = chimeClip;
            ending = endingClip;
        }

        private AudioClip clunk;
        private AudioClip hiss;
        private AudioClip chime;
        private AudioClip ending;

        private void Start()
        {
            if (engine != null && !engine.isPlaying) engine.Play();
            if (radio != null && !radio.isPlaying) radio.Play();
            lastObjective = CurrentObjective();
            if (firstStop != null) lastPassengers = firstStop.PassengerCount;
        }

        private void OnEnable()
        {
            if (shift != null) shift.GearChanged += OnGearChanged;
        }

        private void OnDisable()
        {
            if (shift != null) shift.GearChanged -= OnGearChanged;
        }

        private void Update()
        {
            string objective = CurrentObjective();
            if (!string.IsNullOrEmpty(objective) && objective != lastObjective)
            {
                lastObjective = objective;
                Play(chime, 0.6f);
            }

            if (firstStop != null && firstStop.PassengerCount != lastPassengers)
            {
                lastPassengers = firstStop.PassengerCount;
                Play(hiss, 0.8f);
            }

            if (!endingPlayed && finalEvent != null && finalEvent.IsEnding)
            {
                endingPlayed = true;
                if (radio != null) radio.volume = 0.42f;
                Play(ending, 0.9f);
                StartCoroutine(FadeOutEngine(6f));
            }
        }

        private void OnGearChanged(Gear gear)
        {
            if (engine != null) engine.pitch = GearPitch[Mathf.Clamp((int)gear, 0, GearPitch.Length - 1)];
            Play(clunk, 0.7f);
        }

        private string CurrentObjective()
        {
            if (finalEvent != null && finalEvent.IsActive) return finalEvent.Objective;
            if (mirrorEvent != null && mirrorEvent.IsActive) return mirrorEvent.Objective;
            if (repeatedStop != null && repeatedStop.IsActive) return repeatedStop.Objective;
            return firstStop != null ? firstStop.Objective : "";
        }

        private void Play(AudioClip clip, float volume)
        {
            if (fx == null || clip == null) return;
            fx.PlayOneShot(clip, volume);
        }

        private AudioSource AddLoopSource(AudioClip clip, float volume)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.volume = volume;
            source.playOnAwake = false;
            return source;
        }

        private IEnumerator FadeOutEngine(float duration)
        {
            float elapsed = 0f;
            float start = engine != null ? engine.volume : 0f;
            while (engine != null && elapsed < duration)
            {
                elapsed += Time.deltaTime;
                engine.volume = Mathf.Lerp(start, 0f, elapsed / duration);
                yield return null;
            }
            if (engine != null) engine.Stop();
        }
    }
}
