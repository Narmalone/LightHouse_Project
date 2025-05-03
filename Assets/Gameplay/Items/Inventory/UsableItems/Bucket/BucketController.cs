using LightHouse.Items.Interactable;
using LightHouse.Items.Inventory;
using UnityEngine;

public class BucketController : MonoBehaviour
{
    [SerializeField] private Bucket _bucketKey;
    [SerializeField] private MopBucketWetItemTracker _bucketWetItemTracker;

    private void Awake()
    {
        
    }

    private void Start()
    {
        _bucketWetItemTracker.gameObject.SetActive(false);
    }
}
