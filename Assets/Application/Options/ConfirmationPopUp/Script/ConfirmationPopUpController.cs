using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace LightHouse.Game.Options
{
    public class ConfirmationPopupController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI _confirmationTimerText;

        private Action onConfirm;
        private Action onCancel;
        private Coroutine countdownCoroutine;

        private void Start()
        {
            Hide();
            confirmButton.onClick.AddListener(ConfirmClicked);
            cancelButton.onClick.AddListener(CancelClicked);
        }

        public void Show(Action confirmAction, Action cancelAction, float timeOutAction = 15.0f)
        {
            onConfirm = confirmAction;
            onCancel = cancelAction;
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;

            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
            countdownCoroutine = StartCoroutine(AutoCancelCoroutine(timeOutAction));
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;

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
                _confirmationTimerText.text = (uint)timer + "s";
                timer -= Time.deltaTime;
                yield return null;
            }

            CancelClicked();
        }
    }
}
