using Cinemachine;
using LightHouse.Game.Computer;
using LightHouse.Handlers;
using LightHouse.KinematicCharacterController;
using UnityEngine;

public class EnterComputerTestScript : MonoBehaviour
{
    public LightHouseComputer computer;
    public bool Enabled = false;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) 
        {
            if (!Enabled)
            {
                Enabled = true;
                computer.ComputerEnter();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Player.ForceChangePlayerState?.Invoke(PlayerState.CameraMode);
            }
            else
            {
                Enabled = false;
                computer.ComputerExit();
                Player.ForceChangePlayerState?.Invoke(PlayerState.Normal);
            }
        }
    }
}
