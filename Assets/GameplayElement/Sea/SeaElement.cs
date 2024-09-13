using UnityEngine;

public class SeaElement : MonoBehaviour
{
    public string acac;
    public int _idHash;
    [SerializeField] public string _id;
    public int ID { get => _id.GetHashCode(); }
    public GameObject This { get => gameObject; }

    private void Awake()
    {
        _idHash = _id.GetHashCode();
    }

    public int GetId()
    {
        Debug.Log(_id);
        return _id.GetHashCode();
    }
}