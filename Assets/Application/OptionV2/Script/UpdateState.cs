using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateState : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI DisplayText;
    [SerializeField] private int Index;
    [SerializeField] private Quality CurrentQuality;
    [SerializeField] private string[] Qualities = { "Low", "Medium", "High", "VeryHigh", "Epic" };
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // la valeur de Index est égale à l'effectif du tableau Name
        //Index = Qualities.Length;
    }

    // Update is called once per frame
    void Update()
    {
        /*switch (CurrentQuality)
        {
            case Quality.Low:
                //définir une méthode
                break;
            case Quality.Medium:
                //définir une méthode
                break;
            case Quality.High:
                //définir une méthode
                break;
            case Quality.VeryHigh:
                //définir une méthode
                break;
            case Quality.Epic:
                //définir une méthode
                break;
        }*/
    }

    // Incremente Index lorsque le bouton positif est cliqué
    public void Increment(int _Index,int _MinValue, int _MaxValue) 
    {
        print(_Index);
        /*_Index++;
        _Index = Mathf.Clamp(0, _MinValue, _MaxValue);*/
    }

    // Décremente Index lorsque le bouton positif est cliqué
    public void Decrement(int _Index, int _MinValue, int _MaxValue)
    {
        _Index--;
        /*print(_Index);
        _Index = Mathf.Clamp(0, _MinValue, _MaxValue);*/
    }

    /*enum Quality
    {
        Low, Medium, High, VeryHigh, Epic
    }*/
}
