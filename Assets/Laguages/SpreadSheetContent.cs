using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpreadSheetContent", menuName = "SpreadSheetContent")]
public class SpreadSheetContent : ScriptableObject
{
    [SpreadSheetPageName("Feuille 1")]
    public List<BeaufortDescription> beaufortDescriptions;
}
