using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BouleDePetanque : ItemBase
{
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
