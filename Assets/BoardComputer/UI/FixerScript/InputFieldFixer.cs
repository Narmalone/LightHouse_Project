using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputFieldFixer : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    [SerializeField] private RectTransform text;

    private TMP_SelectionCaret caret;

    private bool EnableFix = true;

    private Vector2 caretFix;
    private Vector2 init;
    private void Awake()
    {
        init = text.anchoredPosition;
        inputField.onSelect.AddListener(OnSelect);
        inputField.onDeselect.AddListener(OnDeselect);
    }

    private void Start()
    {
        caret = GetComponentInChildren<TMP_SelectionCaret>(); //wait for this on start when unity instantiate the caret
        caretFix = caret.rectTransform.anchoredPosition;
    }

    private void Update()
    {
        if (EnableFix)
            ResetAnchoredPosition();
    }

    private void OnDestroy()
    {
        inputField.onSelect.RemoveListener(OnSelect);
        inputField.onDeselect.RemoveListener(OnDeselect);
    }

    private void ResetAnchoredPosition()
    {
        text.anchoredPosition = init;
        caret.rectTransform.anchoredPosition = caretFix;
    }

    public void OnSelect(string arg0)
    {
        EnableFix = true;
    }

    private void OnDeselect(string arg0)
    {
        EnableFix = false;
    }
}
