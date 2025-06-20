using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoatsNationalities_", menuName = "LightHouse/Boats/BoatDabatase")]
public class BoatsNationalitiesManager : ScriptableObject
{
    public BoatsNationalitiesConfig[] PossibleConfigs;

    public List<BoatData> PossibleBoatDatas = new();
    public List<BoatData> CurrentUsedBoatDatas = new();

    private void OnValidate()
    {
        PossibleBoatDatas = new List<BoatData>();
        CurrentUsedBoatDatas = new List<BoatData>();

        foreach (var config in PossibleConfigs)
        {
            if (config == null || config.BoatsDatas == null) continue;

            foreach (var boat in config.BoatsDatas)
            {
                if (!PossibleBoatDatas.Contains(boat))
                    PossibleBoatDatas.Add(boat);
            }
        }
    }

    public BoatData Register()
    {
        if (PossibleBoatDatas.Count == 0)
        {
            Debug.LogWarning("Tous les bateaux ont été utilisés.");
            return null;
        }

        int index = Random.Range(0, PossibleBoatDatas.Count);
        BoatData selected = PossibleBoatDatas[index];

        // Déplacer vers Used
        PossibleBoatDatas.RemoveAt(index);
        CurrentUsedBoatDatas.Add(selected);

        return selected;
    }

    public void Unregister(BoatData data)
    {
        if (CurrentUsedBoatDatas.Remove(data))
        {
            if (!PossibleBoatDatas.Contains(data)) // Sécurité
                PossibleBoatDatas.Add(data);
        }
    }

    public BoatData FindName(string name)
    {
        foreach (var boat in PossibleBoatDatas)
        {
            if (boat.Name == name)
                return boat;
        }

        foreach (var boat in CurrentUsedBoatDatas)
        {
            if (boat.Name == name)
                return boat;
        }

        return null;
    }
}
