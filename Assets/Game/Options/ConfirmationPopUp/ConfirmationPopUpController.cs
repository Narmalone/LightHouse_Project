using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

namespace LightHouse.Game.Options
{
    public class ConfirmationPopupController : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField] private UIDocument uiDocument;

        private VisualElement popup;
        private Button confirmButton;
        private Button cancelButton;
        private Label _confirmationTimePopup;

        private Action onConfirm;
        private Action onCancel;
        private Coroutine countdownCoroutine;

        private void Start()
        {
            popup = uiDocument.rootVisualElement.Q<VisualElement>("Root_Confirmation");
            confirmButton = popup.Q<Button>("ConfirmButton");
            cancelButton = popup.Q<Button>("CancelButton");
            _confirmationTimePopup = popup.Q<Label>("ConfirmationTime");

            popup.style.display = DisplayStyle.None;

            confirmButton.clicked += ConfirmClicked;
            cancelButton.clicked += CancelClicked;
        }

        public void Show(Action confirmAction, Action cancelAction, float timeOutAction = 15.0f)
        {
            onConfirm = confirmAction;
            onCancel = cancelAction;
            popup.style.display = DisplayStyle.Flex;

            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
            countdownCoroutine = StartCoroutine(AutoCancelCoroutine(timeOutAction));
        }

        public void Hide()
        {
            popup.style.display = DisplayStyle.None;

            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }
        }

        private void ConfirmClicked()
        {
            onConfirm?.Invoke();
            Hide();
        }

        private void CancelClicked()
        {
            onCancel?.Invoke();
            Hide();
        }

        private IEnumerator AutoCancelCoroutine(float timeoutSeconds)
        {
            float timer = timeoutSeconds;
            while (timer > 0)
            {
                _confirmationTimePopup.text = (uint)timer + "s";
                timer -= Time.deltaTime;
                yield return null;
            }

            CancelClicked();
        }
    }
}
