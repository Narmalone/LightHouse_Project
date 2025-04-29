using UnityEngine;

namespace LightHouse.Localization
{
    [System.Serializable]
    public enum InteractionsObjectsType
    {
        None,
        Switch,
        OpenClose,
        Grabable,
    }

    public enum InteractableObjectsName
    {
        Required,
        EnableGeneratorFirst
    }

    public class LocalizationManager : MonoBehaviour
    {
        void OnGUI()
        {

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}

