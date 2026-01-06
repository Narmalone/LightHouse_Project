using LightHouse.Audio;
using LightHouse.Game.Talkie;
using LightHouse.Handlers;
using LightHouse.Inventory;
using LightHouse.Items.Inventory;
using LightHouse.Utilities;
using LightHouse.Weather;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;

namespace LightHouse.Game.Tutorial
{
    public sealed class TutorialManager : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Services / Références globales")]
        [SerializeField] private TalkieServiceReference _talkieRef;
        [SerializeField] private LayerMask _physicsMoverMask;
        [SerializeField] private BoatPathMover _captainBoat;

        [Header("Dialogues tutoriel (ordre chronologique)")]
        [SerializeField] private LocalizedDialogueAudio Tutorial_1;
        [SerializeField] private LocalizedDialogueAudio Tutorial_2;
        [SerializeField] private LocalizedDialogueAudio Tutorial_3;
        [SerializeField] private LocalizedDialogueAudio Tutorial_4;
        [SerializeField] private LocalizedDialogueAudio Tutorial_5;

        [Header("Items")]
        [SerializeField] private BinocularItem _binocular;

        [SerializeField] private Transform _lighthouseTransform;
        [SerializeField] private Transform _rocksTransform;
        [SerializeField] private Transform _moreNearbyBuoy;
        [SerializeField, Min(0f)] private float _buoyMaxDistanceToValidate = 50f;
        [SerializeField, Range(0.75f, 0.999f)] private float _buoyLookDotThreshold = 0.97f;

        [Header("Intro timings")]
        [SerializeField, Min(0f)] private float _delayBeforeBip = 7.5f;
        [SerializeField, Min(0f)] private float _delayBeforeFirstDialogue = 7.5f;

        [Header("Binoculars Look Check")]
        [Tooltip("Plus proche de 1 = plus strict. 0.95 ~ 18°, 0.93 ~ 21°, 0.90 ~ 26°.")]
        [SerializeField, Range(0.75f, 0.999f)] private float _lighthouseLookDotThreshold = 0.93f;

        [Tooltip("Optionnel : évite de valider si le joueur est trop près/loin (0 = ignore).")]
        [SerializeField, Min(0f)] private float _lighthouseMaxDistanceToValidate = 0f;

        [Header("UI / Localized Strings")]
        public LocalizedString HoldToAction;
        public LocalizedString Use;

        [Header("Weather")]
        public WeatherTimeline timeline;

        #endregion

        #region Private State

        private ITalkieService _talkieService;
        private TalkieManager _talkieManager;

        private bool _binocularEnabled;
        private bool _hasLookedWithBinocularsTheLighthouse;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Register dialogues once
            Tutorial_1.Register();
            Tutorial_2.Register();
            Tutorial_3.Register();
            Tutorial_4.Register();
            Tutorial_5.Register();
        }

        private void OnEnable()
        {
            if (_captainBoat != null) _captainBoat.OnPathCompleted += CaptainBoat_OnPathCompleted;

            if (_binocular != null)
            {
                _binocular.ItemAddedToInventory += Binocular_ItemAddedToInventory;
                _binocular.OnItemUsed += Binocular_OnItemUsed;
            }
        }

        private void Start()
        {
            _talkieService = _talkieRef.Current;
            _talkieManager = _talkieRef.Current as TalkieManager;

            StartCoroutine(StartIntroSequenceRoutine());
        }

        private void Update()
        {
            HandleBinocularLighthouseLook();
        }

        private void OnDisable()
        {
            if (_captainBoat != null) _captainBoat.OnPathCompleted -= CaptainBoat_OnPathCompleted;

            if (_binocular != null)
            {
                _binocular.ItemAddedToInventory -= Binocular_ItemAddedToInventory;
                _binocular.OnItemUsed -= Binocular_OnItemUsed;
            }
        }

        private void OnDestroy()
        {
            // Unregister dialogues once
            Tutorial_1.Unregister();
            Tutorial_2.Unregister();
            Tutorial_3.Unregister();
            Tutorial_4.Unregister();
            Tutorial_5.Unregister();
        }

        #endregion

        #region Intro Sequence

        private IEnumerator StartIntroSequenceRoutine()
        {
            if (_delayBeforeBip > 0f)
                yield return new WaitForSeconds(_delayBeforeBip);

            _talkieManager?.Bip();

            IInventoryItem generatedItem =
                PlayerHandlerData.MainPlayer.Inventory.GenerateAndAddItemToInventory(SlotManager.CurrentSlotIndex, 5, false);

            generatedItem.GetGameObject()
                .AddComponent<MoverFollower>()
                .Config(_physicsMoverMask, 0.2f);

            yield return StartCoroutine(StartFirstDialogueRoutine());
        }

        private IEnumerator StartFirstDialogueRoutine()
        {
            if (_delayBeforeFirstDialogue > 0f)
                yield return new WaitForSeconds(_delayBeforeFirstDialogue);

            _talkieManager?.StopBip();

            _talkieService?.Enqueue(Tutorial_1);
            _talkieService?.Enqueue(Tutorial_2);
        }

        #endregion

        #region Binoculars

        private void Binocular_ItemAddedToInventory()
        {
            _binocularEnabled = true;
            _talkieService?.Enqueue(Tutorial_3);
            // Ici tu peux afficher un hint UI si tu veux.
        }

        private void Binocular_OnItemUsed()
        {
            // Si tu veux déclencher un son, un hint, etc.
            // (Le vrai check "regarde le phare" est dans Update pendant le mode jumelles.)
        }

        private void HandleBuoyNearBy()
        {
            Transform buoyTransform = _moreNearbyBuoy;
            if (buoyTransform == null) return;

            Transform camT = GetViewTransform();
            if (camT == null) return;

            Vector3 toBuoy = buoyTransform.position - camT.position;

            if (_buoyMaxDistanceToValidate > 0f && toBuoy.sqrMagnitude > _buoyMaxDistanceToValidate * _buoyMaxDistanceToValidate)
                return;

            toBuoy.Normalize();
            float dot = Vector3.Dot(camT.forward, toBuoy);

            if (dot >= _buoyLookDotThreshold)
            {
                _talkieService?.Enqueue(Tutorial_5);

                //unlock d’étape, marker, etc.
            }
        }

        private void HandleBinocularLighthouseLook()
        {
            if (!_binocularEnabled) return;
            if (_hasLookedWithBinocularsTheLighthouse) return;
            if (_binocular == null) return;

            // On veut seulement quand le joueur EST VRAIMENT en mode jumelles
            if (!_binocular.IsBinocularModeUsed) return;

            Transform lighthouse = _lighthouseTransform;
            if (lighthouse == null) return;

            Transform camT = GetViewTransform();
            if (camT == null) return;

            Vector3 toLighthouse = lighthouse.position - camT.position;

            if (_lighthouseMaxDistanceToValidate > 0f && toLighthouse.sqrMagnitude > _lighthouseMaxDistanceToValidate * _lighthouseMaxDistanceToValidate)
                return;

            toLighthouse.Normalize();
            float dot = Vector3.Dot(camT.forward, toLighthouse);

            if (dot >= _lighthouseLookDotThreshold)
            {
                _hasLookedWithBinocularsTheLighthouse = true;
                _talkieService?.Enqueue(Tutorial_4);

                //unlock d’étape, marker, etc.
            }
        }

        private Transform GetViewTransform()
        {
            return PlayerHandlerData.MainPlayer?.PlayerCamera?.transform;
        }

        #endregion

        #region Boat / Weather

        private void CaptainBoat_OnPathCompleted()
        {
            if (timeline == null) return;

            int idx = timeline.CurrentIndex;
            if (idx < 0 || idx + 1 >= timeline.Weathers.Count) return;

            var w0 = timeline.GenerateSingleWeatherData(
                WeatherType.Stormy,
                timeline.Weathers[idx].StartTimeInSeconds,
                timeline.Weathers[idx].DurationInSeconds
            );

            var w1 = timeline.GenerateSingleWeatherData(
                WeatherType.Stormy,
                timeline.Weathers[idx + 1].StartTimeInSeconds,
                timeline.Weathers[idx + 1].DurationInSeconds
            );

            timeline.Weathers[idx] = w0;
            timeline.Weathers[idx + 1] = w1;
            timeline.NotifyChanged();
        }

        private void BuoyancyEvent_OnEntered(GameObject obj)
        {
            if (_captainBoat == null) return;

            _captainBoat.Speed = 1f;
        }

        #endregion
    }
}
