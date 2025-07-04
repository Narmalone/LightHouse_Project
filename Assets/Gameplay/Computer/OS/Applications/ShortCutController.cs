using UnityEngine;

public abstract class ShortCutController : MonoBehaviour
{
    [SerializeField] private ComputerApp _prefab;
    public virtual void OnExecute()
    {
        Instantiate(_prefab);
    }
}
