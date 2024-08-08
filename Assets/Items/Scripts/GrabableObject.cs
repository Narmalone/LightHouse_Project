using System;
using UnityEngine;

public class GrabableObject : ItemBase
{
    [SerializeField] private SpringJoint _joint;
    [SerializeField] private Rigidbody _rb;



    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public override void Use()
    {
        base.Use();

        if (_joint.enablePreprocessing == false)
        {
            Name = "Cancel Grab";
            _joint.connectedBody = PlayerManager.Instance._data._rb;
            _joint.enablePreprocessing = true;
        }
        else
        {
            Name = "Active Grab";
            _joint.connectedBody = null;
            _joint.enablePreprocessing = false;
        }
    }
    // button active grab
    // set joint from player to center object
    // display cancel E to ungrab
}