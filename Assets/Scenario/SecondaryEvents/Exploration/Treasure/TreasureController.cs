using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureController : MonoBehaviour
{
    [SerializeField] private ScenarioEvent _treasureEvent;
    //Liste des objets à spawn possibles
    //Liste de spawns possibles
    //Nombre d'objet à spawn < au spawns count
    //équilibrer en fonction de l'acte

    private void Awake()
    {
        _treasureEvent.EventAction += OnEvent;
    }

    private void OnEvent()
    {
        
    }
}
