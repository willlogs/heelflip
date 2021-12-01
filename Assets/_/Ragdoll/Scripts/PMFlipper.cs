using DB.HeelFlip.Ragdoll;
using DB.Utils;
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
        public UnityEvent OnJump;
        public BoolCondition isFeetAttached;
        public LayerMask layerMask;
        public Vector3 jump;
        public StickyShoes shoes;

        public void Rotate(Quaternion ftr, Quaternion ftr2)
        {
            //_angularVel = -Quaternion.AngleAxis(1, transform.right).eulerAngles.normalized * _angularVel.magnitude;
            //goalRotation = ftr * goalRotation;
        }

        public void FeetAttached(Vector3 pivot)
        {
            //_resetingRotation = true;
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

        private Vector3 _rotationPivot;
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

            if (!isFeetAttached.value && _isClinched && !_sliding)
            {
                _animator.SetBool("Spin", true);
                _angularVel += _angularAcceleration * Time.deltaTime;
                _angularVel = Mathf.Clamp(_angularVel, 0, _angularLimit);
            }
            else
            {
                _angularVel *= _angularDamper;
                _animator.SetBool("Spin", false);
            }

            if(!isFeetAttached.value && !_sliding)
                _bodyT.rotation = Quaternion.AngleAxis(_angularVel, _bodyT.right) * _bodyT.rotation;

            if (_sliding)
            {
                _bodyT.rotation = Quaternion.Slerp(
                    _bodyT.rotation,
                    Quaternion.LookRotation(transform.forward, transform.up),
                    Time.deltaTime * 10
                );
                
                if(slideNormal.magnitude > 0)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(-slideNormal),
                        Time.deltaTime * 10
                    );
                    _rb.velocity = -transform.up * 5;
                }
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

        private void OnCollisionEnter(Collision collision)
        {
            int layerTest = _slideLayer.value & (1 << collision.gameObject.layer);
            if (layerTest > 0)
            {
                _sliding = true;
            }
        }

        Vector3 slideNormal;
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
                _sliding = false;
            }
        }
    }
}