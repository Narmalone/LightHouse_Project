using LightHouse.Game.Computer.OS;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Game.Computer.LEO
{
    public abstract class LEOWindow : MonoBehaviour, ILEOWindow
    {
        [SerializeField] private ELEOWindow _type;
        [SerializeField] private CanvasGroup _canvasGroup;
        public Button CloseButton;
        public CanvasGroup CanvasGroup => _canvasGroup;
        public ELEOWindow Type => _type;
        public OS.OS OSSystem { get; set; }

        public virtual void Open()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public virtual void Close()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }

}
