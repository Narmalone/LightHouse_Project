using LightHouse.Audio;
using LightHouse.Game.Talkie;
using LightHouse.Utilities;
using LightHouse.Weather;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;

namespace LightHouse.Game.Tutorial
{
    /// <summary>
    /// Gère la logique du tutoriel d'intro :
    /// - Joue les dialogues radio séquentiels (Tutorial_1 -> _2 -> _3, etc.)
    /// - Ralentit/accélère le bateau du capitaine à l'approche des bouées
    /// - Débloque certaines features du joueur à des moments précis (ex: jumelles)
    /// - Réagit à la fin des dialogues via TalkieManager
    /// </summary>
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

        [Header("STEP 1")]
        [SerializeField] private LocalizedDialogueAudio _firstDialogueTest;
        [SerializeField] private IntroSplashController _blackScreenStart;

        [Header("UI / Localized Strings")]
        public LocalizedString HoldToAction;
        public LocalizedString Use;

        public WeatherTimeline timeline;
        #endregion


        #region Private State

        /// <summary>
        /// ITalkieService générique pour Enqueue().
        /// </summary>
        private ITalkieService _talkieService;

        private TalkieManager _talkieManager;

        #endregion


        #region Unity Lifecycle

        private void Awake()
        {
            // --- Souscriptions gameplay ---
            _captainBoat.OnPathCompleted += CaptainBoat_OnPathCompleted;
            _buoyancyEvent.OnEntered += BuoyancyEvent_OnEntered;

            // --- Register des dialogues (ex: LocalizedString tables, etc.) ---
            Tutorial_1.Register();
            Tutorial_2.Register();

            // --- Récup services Talkie ---
            _talkieService = _talkieRef.Current;
            _talkieManager = _talkieRef.Current as TalkieManager;
        }

        private void Start()
        {
            StartCoroutine(EarlyRoutine(7.5f));
/*            // On lance la séquence d'intro du tuto au début du jeu
            _talkieService.Enqueue(Tutorial_1);
            _talkieService.Enqueue(Tutorial_2);*/
        }

        private void OnDestroy()
        {
            // --- Unsubscribe events gameplay ---
            _captainBoat.OnPathCompleted -= CaptainBoat_OnPathCompleted;
            _buoyancyEvent.OnEntered -= BuoyancyEvent_OnEntered;

            // --- Cleanup localisation ---
            Tutorial_1.Unregister();
            Tutorial_2.Unregister();
        }

        #endregion

        private IEnumerator EarlyRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            _talkieManager.Bip();
            _blackScreenStart.gameObject.SetActive(false);
            StartCoroutine(SecondTestEarlyRoutine(delay));
        }

        private IEnumerator SecondTestEarlyRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            _talkieManager.StopBip();
            _talkieService.Enqueue(Tutorial_1);
        }

        private void CaptainBoat_OnPathCompleted()
        {
            //Le joueur est arrivé, il doit aller sur la plage vers la maison du gardien
            var datas = timeline.GenerateSingleWeatherData(WeatherType.Stormy, timeline.Weathers[timeline.CurrentIndex].StartTimeInSeconds, timeline.Weathers[timeline.CurrentIndex].DurationInSeconds);
            var data2 = timeline.GenerateSingleWeatherData(WeatherType.Stormy, timeline.Weathers[timeline.CurrentIndex + 1].StartTimeInSeconds, timeline.Weathers[timeline.CurrentIndex + 1].DurationInSeconds);
            timeline.Weathers[timeline.CurrentIndex] = datas;
            timeline.Weathers[timeline.CurrentIndex + 1] = data2;
            timeline.NotifyChanged();
        }


        #region Trigger / World Events Handlers

        /// <summary>
        /// Le joueur / bateau entre dans la zone de bouées tutorielle.
        /// On ralentit le bateau du capitaine et on joue Tutorial_5.
        /// </summary>
        private void BuoyancyEvent_OnEntered(GameObject obj)
        {
            if (_captainBoat == null)
                return;

            _captainBoat.Speed = 1f;
            _buoyancyEvent.gameObject.SetActive(false);
        }

        #endregion
    }
}
