using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Features.Boats.Nationalities
{
    [CreateAssetMenu(fileName = "BoatsNationalities_", menuName = "LightHouse/Boats/BoatDabatase")]
    public class BoatsNationalitiesManager : ScriptableObject
    {
        public SO_BoatsNationalitiesConfig[] PossibleConfigs;

        public List<BoatNationalityDatas> PossibleBoatDatas = new();
        public List<BoatNationalityDatas> CurrentUsedBoatDatas = new();

        private void OnValidate()
        {
            PossibleBoatDatas = new List<BoatNationalityDatas>();
            CurrentUsedBoatDatas = new List<BoatNationalityDatas>();

            foreach (var config in PossibleConfigs)
            {
                if (config == null || config.BoatsDatas == null) continue;

                foreach (var boat in config.BoatsDatas)
                {
                    if (!PossibleBoatDatas.Contains(boat))
                        PossibleBoatDatas.Add(boat);
                    boat.NationalityFlag = config.Flag;
                }
            }
        }

        public BoatNationalityDatas Register()
        {
            if (PossibleBoatDatas.Count == 0)
            {
                Debug.LogWarning("Tous les bateaux ont été utilisés.");
                return null;
            }

            int index = Random.Range(0, PossibleBoatDatas.Count);
            BoatNationalityDatas selected = PossibleBoatDatas[index];

            // Déplacer vers Used
            PossibleBoatDatas.RemoveAt(index);
            CurrentUsedBoatDatas.Add(selected);

            return selected;
        }

        public void Unregister(BoatNationalityDatas data)
        {
            if (CurrentUsedBoatDatas.Remove(data))
            {
                if (!PossibleBoatDatas.Contains(data)) // Sécurité
                    PossibleBoatDatas.Add(data);
            }
        }

        public bool FindName(string name, out BoatNationalityDatas datas)
        {
            datas = null;
            foreach (var boat in CurrentUsedBoatDatas)
            {
                if (boat.Name == name)
                {
                    datas = boat;
                    return true;
                }
            }

            return false;
        }
    }
}