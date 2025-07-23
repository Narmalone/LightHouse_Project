using TMPro;
using UnityEngine;

public class PopUp : MonoBehaviour, IDisplayable
{
    [SerializeField] TextMeshProUGUI _timerText;
    [SerializeField] float _timerValue = 10f;

    CanvasGroup _canvasGroup;
    Timer _timer;

    private void Awake()
    {
        _timer = new Timer(_timerValue);
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        Hide();
        _canvasGroup.blocksRaycasts = false;
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
        SetCanvaGroup(1f, true, true);
    }

    // le pop up disparait et arrête le timer
    public void Hide()
    {
        _timer.StopTimer();
        SetCanvaGroup(0f, false, false);
    }

    // change les valeurs du canva group
    void SetCanvaGroup(float alpha, bool interactable, bool blocksRaycasts)
    {
        _canvasGroup.alpha = alpha;
        _canvasGroup.interactable = interactable;
        _canvasGroup.blocksRaycasts = blocksRaycasts;
    }
}
