using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpreadSheetPageName : Attribute
{
    public string name;

    public SpreadSheetPageName(string Name)
    {
        name = Name;
    }
}

[System.Serializable]
public class BeaufortDescription
{
    public int ID;
    public Languages Language;
    public string Description;

    public BeaufortDescription(int id,  Languages language, string description)
    {
        ID = id;
        Language = language;
        Description = description;
    }
}

// Le DataContainer peut maintenant gérer plusieurs feuilles avec des données variées
public class DataContainer : ScriptableObject
{
    // Stocke les données par feuille dans un dictionnaire
    private Dictionary<string, List<Dictionary<string, string>>> sheetsData = new Dictionary<string, List<Dictionary<string, string>>>();

    // Fonction pour stocker les données d'une feuille
    public void StoreSheetData(string sheetName, List<Dictionary<string, string>> data)
    {
        if (!sheetsData.ContainsKey(sheetName))
        {
            sheetsData[sheetName] = new List<Dictionary<string, string>>();
        }

        sheetsData[sheetName].AddRange(data);
        Debug.Log($"Data for sheet {sheetName} successfully stored.");
    }

    // Récupérer les données d'une feuille spécifique
    public List<Dictionary<string, string>> GetSheetData(string sheetName)
    {
        if (sheetsData.ContainsKey(sheetName))
        {
            return sheetsData[sheetName];
        }

        Debug.LogWarning($"Sheet {sheetName} not found.");
        return null;
    }
}
