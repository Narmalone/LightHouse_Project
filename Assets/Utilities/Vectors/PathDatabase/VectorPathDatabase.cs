using UnityEngine;

[CreateAssetMenu(fileName = "New Vector Path Database", menuName = "Utilities/Vectors/Vector Path Database")]
public class VectorPathDatabase : ScriptableObject
{
    public VectorPath[] Paths;

    public VectorPath GetRandomPath()
    {
        int rdm = Random.Range(0, Paths.Length);
        return Paths[rdm];
    }
}
