using System;
using UnityEngine;

public class GrabableObject : ItemBase
{
    [SerializeField] private SpringJoint _joint;
    [SerializeField] private CustomEvent_String _updateName;
    [SerializeField] private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        Name = "Active Grab";
    }

    public override void Use()
    {
        base.Use();

        if (_joint.enablePreprocessing == false)
        {
            Name = "Cancel Grab";
            _joint.connectedBody = PlayerManager.Instance._data._rb;
            _joint.enablePreprocessing = true;
            _joint.anchor = Vector3.zero;
            _joint.connectedAnchor = Vector3.zero;
            _updateName.Raise(Name);
            return;
        }
        Name = "Active Grab";
        _joint.connectedBody = null;
        _joint.enablePreprocessing = false;
        _updateName.Raise(Name);

    }
    // button active grab
    // set joint from player to center object
    // display cancel E to ungrab
}