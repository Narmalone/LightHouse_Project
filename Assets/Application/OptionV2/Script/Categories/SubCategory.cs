using LightHouse.Interactions;
using System.Collections.Generic;
using UnityEngine;
using static PopUp;

public class SubCategory : MonoBehaviour, IDisplayable
{
    public List<IConfigurable> _settings = new List<IConfigurable>();
    public static event PopUpDelagate popUpDelagate;

    CanvasGroup _canvasGroup;

    void OnEnable()
    {
        PopUp.popUpApply += ApplyChangedSettings;
        PopUp.popUpReset += ResetChangedSettings;
    }

    void OnDisable()
    {
        PopUp.popUpApply -= ApplyChangedSettings;
        PopUp.popUpReset -= ResetChangedSettings;
    }

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        // Récupère le CanvasGroup
        GetAllSetting();
    }

    void Update()
    {
        // en fait c'est "A" mais pour une raison que j'ignore c'est en QWERTY
        if (Input.GetKeyDown(KeyCode.F))
        {
            ApplyChangedSettings();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            ResetChangedSettings();
        }
    }

    // récupère tout les paramétres
    void GetAllSetting()
    {
        // Parcours des enfants directs
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            IConfigurable configurable = child.GetComponent<IConfigurable>();

            if (configurable != null)
            {
                _settings.Add(configurable);
            }
        }
    }

    // applique tout les param de la sous-catégorie
    public void ApplyChangedSettings()
    {
        OnClic(true);
    }

    // réinitialise tout les param de la sous-catégorie
    public void ResetChangedSettings()
    {
        OnClic(false);
    }

    void OnClic(bool apply)
    {
        foreach (IConfigurable configurable in _settings)
        {
            if (apply)
            {
                configurable.Apply();
            }
            else
            {
                configurable.Reset();
            }
        }
    }

    public bool HasAnyAppliedSetting()
    {
        foreach (var setting in _settings)
        {
            if (setting.HasBeenApplied())
            {
                return true;
            }
        }
        return false;
    }

    // la sous-catégorie apparait
    public void Show()
    {
        SetCanvaGroup(1f, true, true);
    }

    // la sous-catégorie disparait
    public void Hide()
    {
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