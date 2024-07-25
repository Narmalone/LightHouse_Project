using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BouleDePetanque : ItemBase
{
    public GrabOptionBase h = new GrabOptionBase();
    public HoldOptionBase hold = new HoldOptionBase();
    public UseOptionBase use = new UseOptionBase();

    public override List<IOption> GetOptions()
    {
        return new List<IOption>
        {
            new GrabOptionBase(),
            new HoldOptionBase(),
            new UseOptionBase { UseAction = () => {
                    Use();
                } 
            }
        };
    }

    public void Use()
    {
        Debug.Log("Key used!");
    }
}
