using TMPro;
using UnityEngine;

public class PopUp : MonoBehaviour, IDisplayable
{
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private float _timerValue = 10f;

    private CanvasGroup _canvasGroup;
    private Timer _timer;

    private void Awake()
    {
        _timer = new Timer(_timerValue);
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        Hide();
    }

    void Update()
    {
        DebugInput();

        // fps => secondes
        _timer.Tick(_timerValue =+ Time.deltaTime);

        SetTimerText();

        if (_timer.GetTimeRemaining() <= 0)
        {
            Hide();
        }

    }

    // pour tester
    void DebugInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Show();
        }
    }

    // met à jour le texte
    void SetTimerText()
    {
        // affiche le temps restant arrondi à l'unité
        _timerText.text = Mathf.Round(_timer.GetTimeRemaining()).ToString();
    }

    // le pop up apparait, reset et lance le timer
    public void Show()
    {
        _timer.ResetTimer();
        _timer.StartTimer();
        _canvasGroup.alpha = 1.0f;
        _canvasGroup.interactable = true;
    }

    // le pop up disparait et arrête le timer
    public void Hide()
    {
        _timer.StopTimer();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
    }
}
