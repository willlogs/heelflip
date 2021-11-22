using DB.HeelFlip;
using Knife.Portal;
using PT.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour, IPortalTransient
{
    public bool UseThreshold {
        get
        {
            return true;
        }
    }

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    public void Teleport(Vector3 position, Quaternion rotation, Transform entry, Transform exit)
    {
        if (_canTeleport)
        {
            _gonnaTeleport = false;
            _canTeleport = false;

            Vector3 up = transform.up;
            transform.position = position;
            transform.rotation = rotation;
            Quaternion ftr = Quaternion.FromToRotation(up, transform.up);
            _offset = ftr * _offset;
            //SetOffset();

            TimeManager.Instance.DoWithDelay(1, () =>
            {
                _canTeleport = true;
            });
        }
    }

    [SerializeField] private PortalRagdollTransit _target;
    [SerializeField] private float _lerpSpeed, _offsetSize = 2;

    private Vector3 _offset, _telDiff, _forw;
    private bool _canTeleport = true, _gonnaTeleport;

    private void Awake()
    {
        _target.BeforeTeleport += BeforeTargetTeleport;
        SetOffset();
    }

    private void SetOffset()
    {
        _offset = transform.position - _target.transform.position;
        _offset = _offset.normalized * _offsetSize;
        _forw = transform.forward = _target.transform.position - transform.position;
    }

    private void FixedUpdate()
    {
        Vector3 goalPos = _target.transform.position + _offset;
        if (_gonnaTeleport)
        {
            goalPos = transform.position + _telDiff;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            goalPos,
            Time.fixedDeltaTime * _lerpSpeed
        );

        //transform.forward = Vector3.Lerp(transform.forward, _forw, Time.deltaTime);
    }

    private void BeforeTargetTeleport(Transform entry)
    {
        _gonnaTeleport = true;

        Vector3 diff = transform.position - entry.position;
        float dis = Vector3.Dot(diff, entry.up);

        _telDiff = transform.position - entry.up * dis;
        _telDiff = ((_telDiff - entry.position).normalized) + entry.position - transform.position;
        _telDiff = _telDiff.normalized * 2f;
    }
}
