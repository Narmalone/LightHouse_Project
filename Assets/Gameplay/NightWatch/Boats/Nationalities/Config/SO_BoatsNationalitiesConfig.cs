using UnityEngine;

namespace LightHouse.Features.Boats.Nationalities
{
    public enum NationalitiesBoats
    {
        EN,
        FR,
        RU,
        DE,
        SPA,
        CA
    }

    [System.Serializable]
    public class BoatNationalityDatas
    {
        public string Name;
        public Sprite NationalityFlag;
    }

    [CreateAssetMenu(fileName = "SO_BoatNationalitiesDatabase_", menuName = GlobalAssetsMenuPaths.BoatsAssetsMenuPath + "New Nationality Database Config")]
    public class SO_BoatsNationalitiesConfig : ScriptableObject
    {
        public NationalitiesBoats Country;
        public string NationalityName;
        public Sprite Flag;
        public BoatNationalityDatas[] BoatsDatas;

        private void OnValidate()
        {
            foreach (var boatData in BoatsDatas)
            {
                boatData.NationalityFlag = Flag;
            }
        }
    }

}
