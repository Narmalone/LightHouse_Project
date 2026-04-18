using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : PersistentSingleton<LoadingScreen>
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI subLabel;

    public void Show() => root.SetActive(true);
    public void Hide() => root.SetActive(false);

    protected override void Awake()
    {
        base.Awake();
        progressBar.type = Image.Type.Filled;
        progressBar.fillMethod = Image.FillMethod.Horizontal;
    }

    public void SetProgress(float value)
    {
        progressBar.fillAmount = value;
    }

    public void SetLabel(string text)
    {
        label.text = text;
    }

    public void SetSubLabel(string text)
    {
        subLabel.text = text;
    }

    
}