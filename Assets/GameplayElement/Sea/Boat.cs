using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour, SeaReportedObject
{
    private int _id;
    public int ID { get => _id; set => _id = value; }
    public GameObject This { get => gameObject; }
}
