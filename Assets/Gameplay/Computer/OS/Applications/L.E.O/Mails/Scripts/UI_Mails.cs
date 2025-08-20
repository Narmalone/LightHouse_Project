using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Game.Computer.LEO.Mails
{
    public class UI_Mails : LEOWindow
    {
        [SerializeField] private RectTransform _mailParent;
        [SerializeField] private UI_MailElement _mailPrefab;

        public List<UI_MailElement> AllMails = new List<UI_MailElement>();

        public void GenerateMail(MailDatas datas)
        {
            UI_MailElement newInstance = Instantiate(_mailPrefab, _mailParent);
            newInstance.Initialize(datas);
            AllMails.Add(newInstance);
        }
    }

}
