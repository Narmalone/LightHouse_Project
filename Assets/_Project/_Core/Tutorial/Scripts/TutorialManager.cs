using LightHouse.Audio;
using LightHouse.Game.Talkie;
using LightHouse.Utilities;
using LightHouse.Weather;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;

namespace LightHouse.Game.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Services / Références globales")]
        [SerializeField] private TalkieServiceReference _talkieRef;
        [SerializeField] private TriggerEvent _buoyancyEvent;
        [SerializeField] private BoatPathMover _captainBoat;

        [Header("Dialogues tutoriel (ordre chronologique)")]
        [SerializeField] private LocalizedDialogueAudio Tutorial_1;
        [SerializeField] private LocalizedDialogueAudio Tutorial_2;

        [Header("Intro timings")]
        [SerializeField, Min(0f)] private float _delayBeforeBip = 7.5f;
        [SerializeField, Min(0f)] private float _delayBeforeFirstDialogue = 7.5f;

        [Header("UI / Localized Strings")]
        public LocalizedString HoldToAction;
        public LocalizedString Use;

        public WeatherTimeline timeline;

        #endregion

        #region Private State
        private ITalkieService _talkieService;
        private TalkieManager _talkieManager;
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _captainBoat.OnPathCompleted += CaptainBoat_OnPathCompleted;
            _buoyancyEvent.OnEntered += BuoyancyEvent_OnEntered;

            Tutorial_1.Register();
            Tutorial_2.Register();

            _talkieService = _talkieRef.Current;
            _talkieManager = _talkieRef.Current as TalkieManager;
        }

        private void Start()
        {
            StartCoroutine(StartIntroSequenceRoutine());
        }

        private void OnDestroy()
        {
            _captainBoat.OnPathCompleted -= CaptainBoat_OnPathCompleted;
            _buoyancyEvent.OnEntered -= BuoyancyEvent_OnEntered;

            Tutorial_1.Unregister();
            Tutorial_2.Unregister();
        }

        #endregion

        private IEnumerator StartIntroSequenceRoutine()
        {
            if (_delayBeforeBip > 0f)
                yield return new WaitForSeconds(_delayBeforeBip);

            _talkieManager.Bip();

            yield return StartCoroutine(StartFirstDialogueRoutine());
        }

        private IEnumerator StartFirstDialogueRoutine()
        {
            if (_delayBeforeFirstDialogue > 0f)
                yield return new WaitForSeconds(_delayBeforeFirstDialogue);

            _talkieManager.StopBip();
            _talkieService.Enqueue(Tutorial_1);
        }

        private void CaptainBoat_OnPathCompleted()
        {
            var datas = timeline.GenerateSingleWeatherData(
                WeatherType.Stormy,
                timeline.Weathers[timeline.CurrentIndex].StartTimeInSeconds,
                timeline.Weathers[timeline.CurrentIndex].DurationInSeconds
            );

            var data2 = timeline.GenerateSingleWeatherData(
                WeatherType.Stormy,
                timeline.Weathers[timeline.CurrentIndex + 1].StartTimeInSeconds,
                timeline.Weathers[timeline.CurrentIndex + 1].DurationInSeconds
            );

            timeline.Weathers[timeline.CurrentIndex] = datas;
            timeline.Weathers[timeline.CurrentIndex + 1] = data2;
            timeline.NotifyChanged();
        }

        private void BuoyancyEvent_OnEntered(GameObject obj)
        {
            if (_captainBoat == null)
                return;

            _captainBoat.Speed = 1f;
            _buoyancyEvent.gameObject.SetActive(false);
        }
    }
}
