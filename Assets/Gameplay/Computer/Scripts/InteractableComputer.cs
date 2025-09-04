using LightHouse.Interactions;
using LightHouse.Items.Interactable;
using UnityEngine;

namespace LightHouse.Game.Computer
{
    public class InteractableComputer : InteractableItemBase, IInteractable
    {
        public bool Enabled = false;
        public Collider Collider => _detectionCollider;
        public override string GetInteractionName()
        {
            return "Press to enter in the computer";
        }

        public override void Interact()
        {
            InvokeObjectInteracted();
        }


        // Update is called once per frame
        void Update()
        {
            /*if (Input.GetKeyDown(KeyCode.K))
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
            }*/
        }
    }

}
