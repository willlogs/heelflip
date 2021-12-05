using DB.HeelFlip.Ragdoll;
using DB.Utils;
using Dreamteck.Splines;
using PT.Utils;
using RootMotion.Dynamics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DB.HeelFlip
{
    public class PMFlipper : MonoBehaviour
    {
        public UnityEvent OnJump, OnSlide, OnStopSlide, OnDeath;
        public BoolCondition isFeetAttached;
        public LayerMask layerMask;
        public Vector3 jump;
        public StickyShoes shoes;

        public void DieQuestionMark()
        {
            if (!isFeetAttached.value)
            {
                OnDeath?.Invoke();
            }
        }

        public void Rotate(Quaternion ftr, Quaternion ftr2)
        {
            //_angularVel = -Quaternion.AngleAxis(1, transform.right).eulerAngles.normalized * _angularVel.magnitude;
            //goalRotation = ftr * goalRotation;
        }

        public void FeetAttached(Vector3 pivot)
        {
            _resetingRotation = true;
            _grounded = true;
            _rotationPivot = pivot;
        }

        public void Die()
        {
            _puppet.state = PuppetMaster.State.Dead;
            _rb.isKinematic = true;
        }

        [SerializeField] private PuppetMaster _puppet;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private bool _isClinched = false;
        [SerializeField] private Animator _animator;
        [SerializeField] private float _angularVel, _angularLimit, _angularAcceleration, _angularDamper;
        [SerializeField] private Transform _feetT, _bodyT;
        [SerializeField] private LayerMask _slideLayer;
        [SerializeField] private SplinePositioner _positioner;
        [SerializeField] private Collider _collider;

        private Vector3 _rotationPivot;
        private float _distance;
        bool _jumpCue = false, _jumping = false, _resetingRotation = false, _grounded, _sliding;

        private void Awake()
        {
            _rb.centerOfMass = Vector3.zero;
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                ToggleClinch();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ToggleClinch();
            }            
        }

        private void FixedUpdate()
        {
            if (!isFeetAttached.value && _isClinched && !_sliding)
            {
                _animator.SetBool("Spin", true);
                _angularVel += _angularAcceleration * Time.fixedDeltaTime;
                _angularVel = Mathf.Clamp(_angularVel, 0, _angularLimit);
            }
            else
            {
                _angularVel *= _angularDamper;
                _animator.SetBool("Spin", false);
            }

            if (!isFeetAttached.value && !_sliding)
                _bodyT.rotation = Quaternion.AngleAxis(_angularVel, _bodyT.right) * _bodyT.rotation;

            // manual sliding - now works with spline positioner
            /*if (_sliding)
            {
                _bodyT.rotation = Quaternion.Slerp(
                    _bodyT.rotation,
                    Quaternion.LookRotation(transform.forward, transform.up),
                    Time.fixedDeltaTime * 10
                );
                
                if(slideNormal.magnitude > 0)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(-slideNormal),
                        Time.fixedDeltaTime * 10
                    );
                    _rb.velocity = -transform.up * 5;
                }
            }*/

            if (_sliding)
            {
                _distance += Time.fixedDeltaTime * 20;
                _positioner.SetDistance(_distance);

                SplineSample _sample = _positioner.spline.Project(transform.position);
                Quaternion targetBodyRotation1 = Quaternion.LookRotation(-_positioner.transform.up, -_positioner.transform.forward);
                Quaternion targetBodyRotation2 = Quaternion.LookRotation(_positioner.transform.up, _positioner.transform.forward);
                
                float angle1 = Quaternion.Angle(_bodyT.rotation, targetBodyRotation1);
                float angle2 = Quaternion.Angle(_bodyT.rotation, targetBodyRotation2);

                Quaternion targetBodyRotation = angle1 < angle2 ? targetBodyRotation1 : targetBodyRotation2;
                Vector3 bodyOffset = angle1 < angle2 ? -_bodyT.forward : _bodyT.forward;

                _bodyT.rotation = Quaternion.Lerp(
                    _bodyT.rotation,
                    targetBodyRotation,
                    Time.fixedDeltaTime * 5
                );
                    
                transform.position = Vector3.Lerp(
                    transform.position,
                    _positioner.transform.position + bodyOffset / 2,
                    Time.fixedDeltaTime * 5
                );

                if (_sample.percent >= 0.98f)
                {
                    Vector3 shootDir = angle1 > angle2 ? _bodyT.up : -_bodyT.up;
                    _canSlide = false;
                    TimeManager.Instance.DoWithDelay(0.1f, () =>
                    {
                        _canSlide = true;
                        _collider.enabled = true;
                    });
                    DeactivateSlide();
                    _rb.velocity = shootDir * 20f;
                }
            }

            if (_resetingRotation)
            {
                _bodyT.rotation = Quaternion.Slerp(
                    _bodyT.rotation,
                    Quaternion.identity,
                    Time.deltaTime * 5
                );
            }
        }

        private void ToggleClinch()
        {
            if (_isClinched)
            {
                _isClinched = false;
                _animator.SetBool("Clinch", false);
                _jumpCue = false;

                if (isFeetAttached.value && !_resetingRotation && !_sliding)
                {
                    OnJump?.Invoke();
                    _jumping = true;
                    _rb.velocity = jump.y * transform.up + jump.x * -transform.forward;
                }
            }
            else
            {
                _isClinched = true;
                _jumpCue = true;
                _animator.SetBool("Clinch", true);
            }
        }

        Quaternion _beforeSlideRotation;
        private void OnCollisionEnter(Collision collision)
        {
            int layerTest = _slideLayer.value & (1 << collision.gameObject.layer);
            if (layerTest > 0)
            {
                CancelInvoke("DeactivateSlide");

                if (!_sliding && _canSlide)
                {
                    _sliding = true;
                    _collider.enabled = false;
                    _rb.isKinematic = true;
                    OnSlide?.Invoke();
                    _beforeSlideRotation = transform.rotation;
                    _positioner.transform.parent = null;
                    SplineComputer spline = collision.gameObject.GetComponent<SplineComputer>();
                    _positioner.transform.position = spline.Project(transform.position).position;
                    _positioner.spline = spline;
                    _distance = 0;
                }
            }
        }

        Vector3 slideNormal;
        bool _canSlide = true;
        private void OnCollisionStay(Collision collision)
        {
            int layerTest = _slideLayer.value & (1 << collision.gameObject.layer);
            if (layerTest > 0)
            {
                slideNormal = collision.GetContact(0).normal;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            int layerTest = _slideLayer.value & (1 << collision.gameObject.layer);
            if (layerTest > 0)
            {
                //Invoke("DeactivateSlide", 0.5f);
            }
        }

        private void DeactivateSlide()
        {
            OnStopSlide?.Invoke();
            _sliding = false;
            _rb.isKinematic = false;
        }
    }
}