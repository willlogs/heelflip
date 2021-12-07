using DB.HeelFlip;
using Knife.Portal;
using PT.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

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

    public void ActivateOffsetMode()
    {
        if (!_offsetMode)
        {
            _offsetMode = true;
        }
    }

    public void DeactivateOffsetMode()
    {
        _brain.enabled = false;
        _offsetMode = false;
        _realOffsetMode = false;
    }

    public void Teleport(Vector3 position, Quaternion rotation, Transform entry, Transform exit)
    {
        if (_canTeleport)
        {
            _gonnaTeleport = false;
            _canTeleport = false;

            transform.position = position;
            transform.rotation = rotation;

            TimeManager.Instance.DoWithDelay(1, () =>
            {
                _canTeleport = true;
            });
        }
    }

    [SerializeField] private PortalRagdollTransit _target;
    [SerializeField] private float _lerpSpeed, _transitLerpSpeed, _transitLerpFactor, _offsetSize = 2;
    [SerializeField] private Transform _targetTransform, _targetLookTransform, _tempLookT;
    [SerializeField] private CinemachineBrain _brain;
    [SerializeField] private CinemachineVirtualCamera _mainVCam;
    [SerializeField] private Rigidbody _flowerRB;

    private Vector3 _offset, _telDiff, _forw;
    private bool _canTeleport = true, _gonnaTeleport, _follow = true, _offsetMode = false, _realOffsetMode = false;

    private void Awake()
    {
        _target.BeforeTeleport += BeforeTargetTeleport;
        SetOffset();
        Application.targetFrameRate = 60;
    }

    private void SetOffset()
    {
        _offset = transform.position - _target.transform.position;
        //_offset = _offset.normalized * _offsetSize;
        _forw = transform.forward = _target.transform.position - transform.position;
    }

    private void FixedUpdate()
    {
        if (_realOffsetMode)
        {
            _brain.ManualUpdate();
            return;
        }

        float _tempLerpSpeed = _lerpSpeed;
        Vector3 goalPos = _targetTransform.position;
        Vector3 forw = _targetLookTransform.position - transform.position;
        if (_offsetMode && !_realOffsetMode && !_gonnaTeleport)
        {
            transform.position = Vector3.MoveTowards(transform.position, _mainVCam.transform.position, Time.fixedDeltaTime * 50);
            transform.rotation = Quaternion.Slerp(transform.rotation, _mainVCam.transform.rotation, Time.fixedDeltaTime * 5);
            return;
        }

        if (_gonnaTeleport)
        {
            _curMidway = Vector3.Lerp(midway, _tempLookT.position, distance);
            goalPos = _curMidway;
            forw = _tempLookT.position - transform.position;
        }

        if (_follow || _gonnaTeleport)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                goalPos,
                Time.fixedDeltaTime * _tempLerpSpeed
            );
        }
        else if(!_gonnaTeleport)
        {
            forw = _targetLookTransform.position - transform.position;
        }

        transform.forward = Vector3.Lerp(transform.forward, forw, Time.fixedDeltaTime);
    }

    private void Update()
    {
        _mainVCam.UpdateCameraState(Vector3.up, Time.deltaTime);
        if (_gonnaTeleport)
        {
            distance += Time.deltaTime * _transitLerpSpeed;
        }
    }

    Vector3 midway, _curMidway;
    float distance;
    private void BeforeTargetTeleport(Transform entry)
    {
        _gonnaTeleport = true;
        _transitLerpSpeed = (transform.position - entry.position).magnitude * _transitLerpFactor / _flowerRB.velocity.magnitude;

        Vector3 diff = transform.position - entry.position;
        float dis = Vector3.Dot(diff, entry.forward);
        distance = 0;

        _curMidway = midway = entry.position + Vector3.Dot(entry.forward, transform.position - entry.position) * entry.forward;

        _telDiff = transform.position - entry.forward * dis;
        _telDiff = ((_telDiff - entry.position).normalized) + entry.position - transform.position;
        _telDiff = -diff.normalized;
        _tempLookT = entry;
    }
}
