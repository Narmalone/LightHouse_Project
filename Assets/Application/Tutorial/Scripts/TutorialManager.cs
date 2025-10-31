using LightHouse.Audio;
using LightHouse.Game.Talkie;
using LightHouse.Utilities;
using LightHouse.Weather;
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
        [SerializeField] private BinocularItem _boatBinocular;
        [SerializeField] private TriggerEvent _buoyancyEvent;
        [SerializeField] private BoatPathMover _captainBoat;

        [Header("Dialogues tutoriel (ordre chronologique)")]
        [SerializeField] private LocalizedDialogueAudio Tutorial_1;
        [SerializeField] private LocalizedDialogueAudio Tutorial_2;
        [SerializeField] private LocalizedDialogueAudio Tutorial_3;
        [SerializeField] private LocalizedDialogueAudio Tutorial_4;
        [SerializeField] private LocalizedDialogueAudio Tutorial_5;

        [Header("UI / Localized Strings")]
        public LocalizedString HoldToAction;
        public LocalizedString Use;

        #endregion


        #region Private State

        /// <summary>
        /// ITalkieService générique pour Enqueue().
        /// </summary>
        private ITalkieService _talkieService;

        /// <summary>
        /// Référence concrète TalkieManager pour s'abonner aux events runtime
        /// (OnDialogueFinished, etc.). Peut être null si l'implémentation diffère.
        /// </summary>
        private TalkieManager _talkieManager;

        #endregion


        #region Unity Lifecycle

        private void Awake()
        {
            // --- Souscriptions gameplay ---
            _captainBoat.OnPathCompleted += CaptainBoat_OnPathCompleted;
            _boatBinocular.ItemAddedToInventory += BoatBinocular_ItemAddedToInventory;
            _buoyancyEvent.OnEntered += BuoyancyEvent_OnEntered;

            // --- Register des dialogues (ex: LocalizedString tables, etc.) ---
            Tutorial_1.Register();
            Tutorial_2.Register();
            Tutorial_3.Register();
            Tutorial_4.Register();
            Tutorial_5.Register();

            // --- Récup services Talkie ---
            _talkieService = _talkieRef.Current;
            _talkieManager = _talkieRef.Current as TalkieManager;

            // On écoute la fin des dialogues (ex: pour débloquer des features au bon timing)
            if (_talkieManager != null)
            {
                _talkieManager.OnDialogueFinished += OnDialogueFinished;
            }
            else
            {
                Debug.LogWarning("[TutorialManager] _talkieRef.Current n'est pas un TalkieManager concret, pas d'event dispo.");
            }
        }

        public WeatherTimeline timeline;

        private void CaptainBoat_OnPathCompleted()
        {
            //Le joueur est arrivé, il doit aller sur la plage vers la maison du gardien
            var datas = timeline.GenerateSingleWeatherData(WeatherType.Stormy, 0f);
            timeline.Weathers[timeline.CurrentIndex] = datas;
            WeatherHandlerData.SetCurrentWeatherDatas(datas);
        }

        private void Start()
        {
            // On lance la séquence d'intro du tuto au début du jeu
            _talkieService.Enqueue(Tutorial_1);
            _talkieService.Enqueue(Tutorial_2);
            _talkieService.Enqueue(Tutorial_3);
        }

        private void OnDestroy()
        {
            // --- Unsubscribe events gameplay ---
            _captainBoat.OnPathCompleted -= CaptainBoat_OnPathCompleted;
            _boatBinocular.ItemAddedToInventory -= BoatBinocular_ItemAddedToInventory;
            _buoyancyEvent.OnEntered -= BuoyancyEvent_OnEntered;

            // --- Unsubscribe Talkie events ---
            if (_talkieManager != null)
            {
                _talkieManager.OnDialogueFinished -= OnDialogueFinished;
            }

            // --- Cleanup localisation ---
            Tutorial_1.Unregister();
            Tutorial_2.Unregister();
            Tutorial_3.Unregister();
            Tutorial_4.Unregister();
            Tutorial_5.Unregister();
        }

        #endregion


        #region Talkie Callbacks

        /// <summary>
        /// Appelé par TalkieManager quand un dialogue radio vient de se terminer d'être joué/affiché.
        /// Sert à enchaîner le gameplay au bon moment.
        /// </summary>
        private void OnDialogueFinished(LocalizedDialogueAudio finishedDialogue)
        {
            // Quand Tutorial_3 se termine -> on débloque les jumelles du joueur
            if (finishedDialogue == Tutorial_3)
            {
                UnlockNextStepAfterTutorial3();
                if (_captainBoat != null)
                    _captainBoat.Speed = 0.5f;
            }
            // Quand Tutorial_5 se termine -> le capitaine repart à vitesse normale
            else if (finishedDialogue == Tutorial_5)
            {
                if (_captainBoat != null)
                    _captainBoat.Speed = _captainBoat.BaseMoveSpeed;
            }
        }

        #endregion


        #region Gameplay Logic (tutoriel steps)

        /// <summary>
        /// Appelé à la fin du Tutorial_3.
        /// Exemple : l'objet Jumelles devient cliquable / récupérable.
        /// </summary>
        private void UnlockNextStepAfterTutorial3()
        {
            _boatBinocular.CanBeRaycasted = true;
        }

        #endregion


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
            _talkieService.Enqueue(Tutorial_5);
            _buoyancyEvent.gameObject.SetActive(false);
        }

        #endregion


        #region Inventory / Items Events

        /// <summary>
        /// Appelé quand les jumelles du bateau sont ajoutées à l'inventaire du joueur.
        /// On joue Tutorial_4 (expliquera leur usage).
        /// </summary>
        private void BoatBinocular_ItemAddedToInventory()
        {
            _talkieService.Enqueue(Tutorial_4);

            if (_captainBoat != null && _captainBoat.Speed <= 0.5f)
                _captainBoat.Speed = _captainBoat.BaseMoveSpeed;
            // NOTE: InteractionTextBuilder async retiré pour l'instant.
            // string s = await InteractionTextBuilder.Build(
            //     Use,
            //     InputManager.InteractInInventory_Bind_Name,
            //     HoldToAction
            // );
            // Debug.Log(s);
        }

        #endregion
    }
}
