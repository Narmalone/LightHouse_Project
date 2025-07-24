using TMPro;
using UnityEngine;

public class PopUp : MonoBehaviour, IDisplayable
{
    public delegate void PopUpDelagate();
    public static event PopUpDelagate popUpApply;
    public static event PopUpDelagate popUpReset;

    [SerializeField] TextMeshProUGUI _timerText;
    [SerializeField] float _timerValue = 10f;

    CanvasGroup _canvasGroup;
    Timer _timer;
    bool _doOnce;

    private void Awake()
    {
        _timer = new Timer(_timerValue);
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        _canvasGroup.blocksRaycasts = false;
        Hide();
    }

    void Update()
    {

        SetTimerText();
        Countdown();
    }


    public void Onclic(bool apply)
    {
        if (apply)
        {
            popUpApply?.Invoke();
            Hide();
        }
        else
        {
            ResetSetting();
        }
    }

    // Décompte du chrono
    void Countdown()
    {
        // fps => secondes
        _timer.Tick(_timerValue =+ Time.deltaTime);

        if (_timer.GetTimeRemaining() <= 0 && !_doOnce) //  
        {
            ResetSetting();
            _doOnce = true;
        }
    }

    void ResetSetting()
    {
        popUpReset?.Invoke();
        Hide();
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
        _doOnce = false;
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
