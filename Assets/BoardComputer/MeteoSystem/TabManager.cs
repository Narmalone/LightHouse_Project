using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabManager : MonoBehaviour
{
    public List<TabController> tabs = new List<TabController>();


    public virtual void TabSelectedChanged()
    {

    }
}
