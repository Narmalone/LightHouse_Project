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
        _joint.enablePreprocessing = false;
    }

    public override bool Use()
    {
        base.Use();

        if (_joint.enablePreprocessing == false)
        {
            Name = "Cancel Grab";
            _joint.connectedBody = PlayerManager.Instance._data._rb;
            _joint.enablePreprocessing = true;
            _joint.spring = _rb.mass * 10;
            _joint.anchor = Vector3.zero;
            _joint.connectedAnchor = Vector3.zero;
            _updateName.Raise(Name);
            EditPlayerMoveSpeed(_rb.mass);
            return false;
        }
        Name = "Active Grab";
        _joint.connectedBody = null;
        _joint.enablePreprocessing = false;
        _joint.spring = 0;
        _updateName.Raise(Name);
        EditPlayerMoveSpeed(0);
        return false;

    }

    private void EditPlayerMoveSpeed(float mass)
    {
        if(mass == 0)
        {
            PlayerManager.Instance.SetSpeedWhenIsGrabbing(1);
            return;
        }
        PlayerManager.Instance.SetSpeedWhenIsGrabbing(1- _rb.mass / 5);
    }
}