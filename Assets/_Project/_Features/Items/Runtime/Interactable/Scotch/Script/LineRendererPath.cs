using UnityEngine;

namespace LightHouse.Items.Interactable.Scotch
{
    public class LineRendererPath : MonoBehaviour
    {
        [SerializeField] private Transform[] _linePos;

        public Transform[] LinePos
        {
            get => _linePos;
            set => _linePos = value;
        }
    }
}
