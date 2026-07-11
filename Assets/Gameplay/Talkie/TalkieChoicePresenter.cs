using System;
using System.Collections;
using System.Collections.Generic;
using LightHouse.Core.Audio;
using LightHouse.Core.Inputs;
using LightHouse.Core.Player;
using LightHouse.Core.Player.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LightHouse.Features.Talkie
{
    /// <summary>
    /// Implémentation par défaut de ITalkieChoicePresenter.
    ///
    /// Choix design : le jeu est en vue FPS avec le curseur verrouillé pendant
    /// le gameplay (voir StarterAssets + map "Debug" Lock/UnlockCursor). Faire
    /// répondre au joueur à la souris impliquerait de déverrouiller la caméra
    /// en pleine action, ce qui casse l'immersion. On réutilise donc l'action
    /// "Select" déjà bindée sur les touches clavier 1-4 (déjà utilisée pour le
    /// quick-select de la hotbar dans PlayerInventoryManager) : les choix sont
    /// numérotés à l'écran et se valident au clavier, sans jamais lâcher
    /// WASD/regard. On coupe l'inventaire pendant la fenêtre de choix pour que
    /// 1-4 ne fasse pas aussi défiler la hotbar en même temps.
    ///
    /// Scalable par construction côté data : 1 choix ou 4 choix, ce script ne
    /// change pas. Le clavier plafonne naturellement à 4 choix simultanés
    /// (touches 1-4), ce qui correspond de toute façon à la limite lisible
    /// pour un choix rapide à l'écran.
    /// </summary>
    public class TalkieChoicePresenter : MonoBehaviour, ITalkieChoicePresenter
    {
        [Header("Références UI")]
        [SerializeField] private CanvasGroup _choicesGroup;
        [SerializeField] private RectTransform _choicesContainer;
        [Tooltip("Prefab d'un slot de choix : doit contenir un TextMeshProUGUI. Pas besoin de Button/onClick, la sélection se fait au clavier (touches 1-4).")]
        [SerializeField] private GameObject _choiceSlotPrefab;

        [Header("Affichage")]
        [SerializeField] private float _fadeDuration = 0.25f;

        private readonly List<GameObject> _spawnedSlots = new();
        private TalkieChoice[] _currentChoices;
        private Action<TalkieChoice> _onSelected;
        private Coroutine _fadeRoutine;

        private void Awake()
        {
            Hide();
        }

        private void OnDisable()
        {
            // Sécurité : si ce composant est désactivé pendant qu'un choix est
            // en attente, on ne laisse pas l'input Select accroché ni
            // l'inventaire désactivé indéfiniment.
            StopListeningToSelect();
            RestorePlayerInventory();
        }

        public void Present(TalkieChoice[] choices, Action<TalkieChoice> onSelected)
        {
            if (choices == null || choices.Length == 0)
            {
                Debug.LogWarning("[TalkieChoicePresenter] Present appelé sans choix.");
                return;
            }

            if (_choiceSlotPrefab == null || _choicesContainer == null)
            {
                Debug.LogError("[TalkieChoicePresenter] _choiceSlotPrefab ou _choicesContainer non assigné dans l'inspecteur.");
                return;
            }

            if (choices.Length > 4)
                Debug.LogWarning($"[TalkieChoicePresenter] {choices.Length} choix fournis, mais seuls les 4 premiers seront jouables (touches 1-4).");

            _currentChoices = choices;
            _onSelected = onSelected;
            ClearSlots();

            for (int i = 0; i < choices.Length; i++)
            {
                TalkieChoice choice = choices[i];
                if (choice == null)
                    continue;

                GameObject slot = Instantiate(_choiceSlotPrefab, _choicesContainer);

                TextMeshProUGUI label = slot.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = $"[{i + 1}] {choice.GetLabelSafe()}";

                _spawnedSlots.Add(slot);
            }

            // Pendant que le joueur choisit, 1-4 doit sélectionner une réplique,
            // pas un slot de la hotbar.
            RestorePlayerInventory(); // sécurité si un ancien état trainait
            PlayerInventoryManager inventory = GetPlayerInventory();
            if (inventory != null)
                inventory.Disable();

            StartListeningToSelect();

            if (_choicesGroup != null)
            {
                _choicesGroup.interactable = true;
                _choicesGroup.blocksRaycasts = true;
                StartFade(1f);
            }
        }

        public void Hide()
        {
            StopListeningToSelect();
            RestorePlayerInventory();

            ClearSlots();
            _currentChoices = null;
            _onSelected = null;

            if (_choicesGroup != null)
            {
                _choicesGroup.interactable = false;
                _choicesGroup.blocksRaycasts = false;
                StartFade(0f);
            }
        }

        #region Sélection clavier (touches 1-4, action "Select" existante)
        private void StartListeningToSelect()
        {
            InputManager.Select.performed += OnSelectPerformed;
        }

        private void StopListeningToSelect()
        {
            if (InputManager.Select != null)
                InputManager.Select.performed -= OnSelectPerformed;
        }

        private void OnSelectPerformed(InputAction.CallbackContext ctx)
        {
            if (_currentChoices == null)
                return;

            // Même pattern que PlayerInventoryManager.Select_performed : le nom
            // du control pressé est directement "1", "2", "3" ou "4".
            if (!int.TryParse(ctx.control?.name, out int pressedNumber))
                return;

            int index = pressedNumber - 1;
            if (index < 0 || index >= _currentChoices.Length)
                return;

            TalkieChoice choice = _currentChoices[index];
            if (choice != null)
                Select(choice);
        }

        private void Select(TalkieChoice choice)
        {
            // On coupe le callback + l'écoute avant d'invoquer, pour éviter une
            // double sélection si le joueur presse deux touches très vite.
            StopListeningToSelect();

            Action<TalkieChoice> callback = _onSelected;
            _onSelected = null;
            callback?.Invoke(choice);
        }

        private PlayerInventoryManager GetPlayerInventory()
        {
            return PlayerHandlerData.MainPlayer != null ? PlayerHandlerData.MainPlayer.Inventory : null;
        }

        private void RestorePlayerInventory()
        {
            PlayerInventoryManager inventory = GetPlayerInventory();
            if (inventory != null && !inventory.IsEnabled)
                inventory.Enable();
        }
        #endregion

        #region Fade
        private void StartFade(float targetAlpha)
        {
            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);

            _fadeRoutine = StartCoroutine(FadeTo(targetAlpha));
        }

        private IEnumerator FadeTo(float targetAlpha)
        {
            if (_fadeDuration <= 0f)
            {
                _choicesGroup.alpha = targetAlpha;
                yield break;
            }

            float startAlpha = _choicesGroup.alpha;
            float t = 0f;

            while (t < _fadeDuration)
            {
                _choicesGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / _fadeDuration);
                t += Time.deltaTime;
                yield return null;
            }

            _choicesGroup.alpha = targetAlpha;
        }
        #endregion

        private void ClearSlots()
        {
            foreach (GameObject slot in _spawnedSlots)
            {
                if (slot != null)
                    Destroy(slot);
            }
            _spawnedSlots.Clear();
        }
    }
}
