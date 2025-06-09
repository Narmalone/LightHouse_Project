using Cinemachine;
using UnityEngine;

public class EnterComputerTestScript : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCam;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) 
        {
            virtualCam.Priority = 100;
        }
    }
}
