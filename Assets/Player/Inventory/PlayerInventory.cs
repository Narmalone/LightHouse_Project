using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private byte slots = 4;
    public List<GameObject> objectsInInventory = new List<GameObject>();
}
