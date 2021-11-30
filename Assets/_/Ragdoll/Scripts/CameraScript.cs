using DB.HeelFlip;
using Knife.Portal;
using PT.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour, IPortalTransient
{
    public void Freeze()
    {
        _follow = false;
    }

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

            //Quaternion ftr = Quaternion.FromToRotation(up, transform.up);
            //_targetTransform.parent.rotation = ftr * _targetTransform.parent.rotation;

            TimeManager.Instance.DoWithDelay(1, () =>
            {
                _canTeleport = true;
            });
        }
    }

    [SerializeField] private PortalRagdollTransit _target;
    [SerializeField] private float _lerpSpeed, _offsetSize = 2;
    [SerializeField] private Transform _targetTransform, _targetLookTransform, _tempLookT;

    private Vector3 _offset, _telDiff, _forw;
    private bool _canTeleport = true, _gonnaTeleport, _follow = true;

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
        if (!_follow)
            return;

        Vector3 goalPos = _targetTransform.position;
        Vector3 forw = _targetLookTransform.position - transform.position;

        if (_gonnaTeleport)
        {
            midway = Vector3.Lerp(midway, _tempLookT.position, Time.deltaTime * _lerpSpeed);
            goalPos = midway;
            forw = _tempLookT.position - transform.position;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            goalPos,
            Time.fixedDeltaTime * _lerpSpeed
        );

        transform.forward = Vector3.Lerp(transform.forward, forw, Time.deltaTime);
    }

    Vector3 midway;
    private void BeforeTargetTeleport(Transform entry)
    {
        _gonnaTeleport = true;

        Vector3 diff = transform.position - entry.position;
        float dis = Vector3.Dot(diff, entry.forward);

        midway = entry.position + Vector3.Dot(entry.forward, transform.position - entry.position) * entry.forward;

        _telDiff = transform.position - entry.forward * dis;
        _telDiff = ((_telDiff - entry.position).normalized) + entry.position - transform.position;
        _telDiff = -diff.normalized;
        _tempLookT = entry;
    }
}
