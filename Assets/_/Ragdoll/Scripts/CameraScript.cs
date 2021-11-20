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

            transform.position = position;
            transform.rotation = rotation;
            SetOffset();

            TimeManager.Instance.DoWithDelay(1, () =>
            {
                _canTeleport = true;
            });
        }
    }

    [SerializeField] private PortalRagdollTransit _target;
    [SerializeField] private float _lerpSpeed;

    private Vector3 _offset, _lastDiff;
    private bool _canTeleport = true, _gonnaTeleport;

    private void Awake()
    {
        _target.BeforeTeleport += BeforeTargetTeleport;
        SetOffset();
    }

    private void SetOffset()
    {
        _offset = transform.position - _target.transform.position;
    }

    private void Update()
    {
        Vector3 goalPos = _target.transform.position + _offset;
        if (_gonnaTeleport)
        {
            goalPos = transform.position + _lastDiff;
        }
        else
        {
            _lastDiff = goalPos - transform.position;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            goalPos,
            Time.deltaTime * _lerpSpeed
        );
    }

    private void BeforeTargetTeleport(Transform entry)
    {
        _gonnaTeleport = true;
    }
}
