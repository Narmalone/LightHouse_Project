using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    public List<TabBtn> tabButtons = new List<TabBtn>();

    public TabBtn CurrentSelectedTab;
    public void Subscribe(TabBtn btn)
    {
        if(tabButtons == null)
        {
            tabButtons = new List<TabBtn>();
        }
        tabButtons.Add(btn);
    }

    public virtual void OnTabEnter(TabBtn btn)
    {
        ResetTab();
    }

    public virtual void OnTabExit(TabBtn btn)
    {
        ResetTab();
    }

    public virtual void OnTabSelected(TabBtn btn)
    {
        if(CurrentSelectedTab != null)
        {
            CurrentSelectedTab.DeSelect();
        }
        CurrentSelectedTab = btn;
        CurrentSelectedTab.Select();
        ResetTab();
    }

    public virtual void ResetTab()
    {
        foreach(TabBtn btn in tabButtons)
        {
            if(CurrentSelectedTab != null && CurrentSelectedTab == btn)
            {
                continue;
            }
        }
    }
}
