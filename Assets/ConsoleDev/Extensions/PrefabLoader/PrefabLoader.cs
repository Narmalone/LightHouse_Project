using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrefabLoader : MonoBehaviour
{
    public List<GameObject> SpawnablePrefabs = new List<GameObject>();
    public Dictionary<string, GameObject> DictionnaryObjects;

    private void Awake()
    {
        DictionnaryObjects = SpawnablePrefabs.ToDictionary(x => x.name, x => x);
    }

    public GameObject GetObjectFromDictionnaryByName(string objName)
    {
        if (DictionnaryObjects.ContainsKey(objName))
        {
            return DictionnaryObjects[objName];
        }

        ChatController.Instance.SendChatMessage($"No object was found to generate with name: {objName}", ChatTabs.Dev, logLevel: LogLevel.Error);
        return null;
    }
}