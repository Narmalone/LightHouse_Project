using MPUIKIT;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Messagerie : ContentWindow
{
    [Header(" -- CONTROLLERS -- ")]
    [SerializeField] private MailController _mailPrefab;
    [SerializeField] private ScrollWindowController _mailScrollWindow;
    [SerializeField] public DialogWindowController _dialogWindow;
    [SerializeField] private CustomEvent_Mail _onMailSelectedChanged;
    public List<MailController> Controllers;

    [Header(" -- REFERENCES -- ")]
    [Header(" - TEXTS - ")]
    public TextMeshProUGUI ReceptionBoxText;
    public TextMeshProUGUI UnreadsText;
    public TextMeshProUGUI NowPlayingText;
    public TextMeshProUGUI ReadsText;
    public LanguageText[] AllTextsLanguages;

    [Header("- BACKGROUNDS - ")]
    public MPImageBasic UnreadHelperBackground;
    public MPImageBasic ReadingHelperBackground;
    public MPImageBasic ReadedHelperBackground;

    [Header("- COLORS - ")]
    public Color UnreadColor;
    public Color ReadingColor;
    public Color ReadedColor;
   
    [Header(" - DEBUG - ")]
    public Mail CurrentOpenedMail;

    private void Awake()
    {
        _onMailSelectedChanged.handle += _onMailSelectedChanged_handle;
    }
    private void Start()
    {
        UnreadHelperBackground.color = UnreadColor;
        ReadingHelperBackground.color = ReadingColor;
        ReadedHelperBackground.color = ReadedColor;
    }

    private void OnDestroy()
    {
        _onMailSelectedChanged.handle -= _onMailSelectedChanged_handle;
    }

    private void _onMailSelectedChanged_handle(Mail obj)
    {
        CurrentOpenedMail = obj;
        _dialogWindow.UpdateMail(obj);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            GenerateMail(new Mail("Coucou", "Salutation_" + Random.Range(0, 300), $"J{Random.Range(0, 31)}-{Random.Range(0, 23)}h{Random.Range(1, 60)}", $"Je m'appelle kaka {Random.Range(0, 2000)}"));
        }
    }

    public void GenerateMail(Mail mail)
    {
        MailController newMail = Instantiate(_mailPrefab, _mailScrollWindow.Content);
        newMail.SetMail(mail);
        newMail.SetColors(UnreadColor, ReadingColor, ReadedColor);
        Controllers.Add(newMail);
        if (Controllers.Count >= 10)
        {
            _mailScrollWindow.UpdateContentTransform();
        }
    }

    //Specific Mails ?
    //Very old mails ?
    public void DeleteMail()
    {

    }
}
