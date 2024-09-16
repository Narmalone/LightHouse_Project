using UnityEngine;

public class SeaElement : MonoBehaviour
{
    [SerializeField] public string _id;
    public int ID { get => _id.GetHashCode(); }
    public GameObject This { get => gameObject; }
}