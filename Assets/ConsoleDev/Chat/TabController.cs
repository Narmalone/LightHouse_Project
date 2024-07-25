using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public int maxMessagesOnTab = 30;
    public Button tabBtn;
    public List<GameObject> messages = new List<GameObject>();
    public Transform contentTransform;

    public void OnAddMessage(TextMeshProUGUI prefab, string msg, Transform parentTarget, bool enableOnGenerate = true, LogLevel textColor = LogLevel.Normal)
    {
        if(messages.Count >= maxMessagesOnTab)
        {
            OnRemoveLastMessage();
        }
        TextMeshProUGUI obj = Instantiate(prefab);
        obj.text = msg;
        obj.gameObject.SetActive(enableOnGenerate);
        obj.transform.SetParent(parentTarget, false);
        messages.Add(obj.gameObject);

        switch(textColor)
        {
            case LogLevel.Normal:
                break;
            case LogLevel.Warning:
                obj.color = Color.yellow;
                break;
            case LogLevel.Error:
                obj.color = Color.red;
                break;
        }
    }

    public void OnAddMessage(Button prefab, Action act, string msg, Transform parentTarget, bool enableOnGenerate = true, LogLevel textColor = LogLevel.Normal)
    {
        if (messages.Count >= maxMessagesOnTab)
        {
            OnRemoveLastMessage();
        }
        Button btn = Instantiate(prefab);
        TextMeshProUGUI obj = btn.GetComponentInChildren<TextMeshProUGUI>();
        btn.onClick.AddListener(() =>
        {
            act?.Invoke();
        });
        obj.text = msg;
        btn.gameObject.SetActive(enableOnGenerate);
        btn.transform.SetParent(parentTarget, false);
        messages.Add(btn.gameObject);

        switch (textColor)
        {
            case LogLevel.Normal:
                break;
            case LogLevel.Warning:
                obj.color = Color.yellow;
                break;
            case LogLevel.Error:
                obj.color = Color.red;
                break;
        }
    }

    public void ClearTab()
    {
        List<GameObject> messagesToRemove = new List<GameObject>(messages);

        foreach (GameObject obj in messagesToRemove)
        {
            Destroy(obj);
            messages.Remove(obj);
        }
    }

    public void OnRemoveLastMessage()
    {
        if (messages.Count > 0)
        {
            GameObject oldestMessage = messages[0];
            Destroy(oldestMessage);
            messages.RemoveAt(0);
        }
    }

    public void EnableButton()
    {
        tabBtn.interactable = false;
    }

    public void DisableButton()
    {
        tabBtn.interactable = false;
    }

    public void HideAll()
    {
        tabBtn.gameObject.SetActive(false);
        HideTabMessages();
    }

    public void ShowAll()
    {
        tabBtn.gameObject.SetActive(true);
        ShowTabMessages();
    }

    public void ShowTabMessages()
    {
        foreach(var go in messages)
        {
            go.SetActive(true);
            go.transform.SetParent(contentTransform);
        }
    }

    public void HideTabMessages()
    {
        if (messages.Count == 0) return;
        foreach (var go in messages)
        {
            go.transform.SetParent(this.transform);
            go.SetActive(false);
        }
    }
}
