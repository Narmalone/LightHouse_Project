using UnityEngine;

public class TestQuality : MonoBehaviour
{
    private int currentIndex = 0;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            QualitySettings.IncreaseLevel(true);
        }
    }
}
