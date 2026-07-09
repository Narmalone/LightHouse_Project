using System.Collections;
using TMPro;
using UnityEngine;

public class UI_ObjectiveController : MonoBehaviour
{
    [Header("Typing")]
    [SerializeField] private float _charactersPerSecond = 30f;
    [SerializeField] private float _startDelay = 0f;
    [SerializeField] private float _endDelay = 0f;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _titleObjText;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [SerializeField] private Color _defaultColor = Color.white;
    [SerializeField] private Color _completedColor = new Color(0.3f, 1f, 0.3f);

    [SerializeField] private float _completeAnimDuration = 0.35f;
    [SerializeField] private float _completeScale = 1.08f;
    [SerializeField] private RectTransform _content;

    private Coroutine _writingCoroutine;

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void WriteDescription(string text)
    {
        if (!this.gameObject.activeInHierarchy) this.Show();

        if (_writingCoroutine != null)
            StopCoroutine(_writingCoroutine);

        _writingCoroutine = StartCoroutine(WriteObjective(_descriptionText, text));
    }

    private IEnumerator WriteObjective(TextMeshProUGUI textUI, string text)
    {
        if (_startDelay > 0f)
            yield return new WaitForSeconds(_startDelay);

        textUI.text = string.Empty;
        textUI.color = _defaultColor;

        float delay = 1f / Mathf.Max(_charactersPerSecond, 1f);

        for (int i = 0; i < text.Length; i++)
        {
            textUI.text += text[i];
            yield return new WaitForSeconds(delay);
        }

        if (_endDelay > 0f)
            yield return new WaitForSeconds(_endDelay);

        _writingCoroutine = null;
    }

    public void CompleteObjective(System.Action onEnd)
    {
        StartCoroutine(CompleteAnimation(onEnd));
    }

    private IEnumerator CompleteAnimation(System.Action onEnd)
    {
        Vector3 startScale = _content.localScale;
        Vector3 targetScale = startScale * _completeScale;

        Color startColor = _descriptionText.color;

        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / _completeAnimDuration;

            float ease = Mathf.Sin(t * Mathf.PI);

            _content.localScale = Vector3.Lerp(startScale, targetScale, ease);
            _descriptionText.color = Color.Lerp(startColor, _completedColor, t);

            yield return null;
        }

        _content.localScale = startScale;
        _descriptionText.color = _completedColor;

        onEnd?.Invoke();
    }
}