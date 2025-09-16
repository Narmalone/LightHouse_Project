using LightHouse.Game.Boats;
using LightHouse.Handlers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaceHolder_BoatsUI : MonoBehaviour
{
    [SerializeField] private Boat _thisBoat;
    [SerializeField] private TextMeshProUGUI _nameTxt;
    [SerializeField] private TextMeshProUGUI _frequencyTxt;
    [SerializeField] private Image _flagImg;

    private void Awake()
    {
        _thisBoat.OnBoatInitialized += ThisBoat_OnBoatInitialized;
    }

    private void ThisBoat_OnBoatInitialized()
    {
        _nameTxt.text = "Name: " + _thisBoat.Data.Name;
        _frequencyTxt.text = "Frequency: " + _thisBoat.RadioFrequency + "Mhz";
        _flagImg.sprite = _thisBoat.Data.NationalityFlag;
    }

    private void OnDestroy()
    {
        _thisBoat.OnBoatInitialized -= ThisBoat_OnBoatInitialized;
    }

    private void LateUpdate()
    {
        //Make the orientation right to the player
        var direction = PlayerHandlerData.MainPlayer.Character.transform.position - this.transform.position;
        var targetRotation = Quaternion.LookRotation(-direction, Vector3.up);
        targetRotation.eulerAngles = new Vector3(0f, targetRotation.eulerAngles.y, 0f);
        transform.rotation = targetRotation;
    }
}
