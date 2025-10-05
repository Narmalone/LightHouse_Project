using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ShortcutButtonsManager : MonoBehaviour
{
    [field: SerializeField] public List<ShortcutButton> Buttons { get; private set; }

    private ShortcutButton _lastCliquedButton;

    private void Awake()
    {
        foreach(var button in Buttons)
        {
            button.OnClick += Btn_OnClick;
        }
    }

    private void OnValidate()
    {
        Buttons = GetComponentsInChildren<ShortcutButton>().ToList();
    }

    public void Register(ShortcutButton btn)
    {
        if(Buttons.Contains(btn))
        {
            Debug.LogWarning("Le bouton existait déjà");
            return;
        }
        Buttons.Add(btn);
        btn.OnClick += Btn_OnClick;
    }

    public void Unregister(ShortcutButton btn)
    {
        
        btn.OnClick -= Btn_OnClick;
        Buttons.Remove(btn);
    }

    private void Btn_OnClick(ShortcutButton cliquedBtn)
    {
        SwitchSelectedButton(cliquedBtn);
    }

    /// <summary>
    /// The button you want to be selected
    /// </summary>
    /// <param name="selectButton"> if "null" it just unselect the last, or put the ref of the shortcut button you want to select </param>
    public void SwitchSelectedButton(ShortcutButton selectButton)
    {
        if (_lastCliquedButton != null)
            _lastCliquedButton.Unselect();

        if (selectButton == null) return;

        _lastCliquedButton = selectButton;
        _lastCliquedButton.Select();
    }

    private void OnDestroy()
    {
        for (int i = Buttons.Count - 1; i >= 0; i--)
        {
            var btn = Buttons[i];
            if (btn != null) btn.OnClick -= Btn_OnClick;
        }
        Buttons.Clear();
        _lastCliquedButton = null;
    }
}
