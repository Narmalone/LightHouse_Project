using UnityEngine;

public class BootTerminalView : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private BootLineView linePrefab;

    public IBootLineView CreateLine()
    {
        return Instantiate(linePrefab, container);
    }

    public void Clear()
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}