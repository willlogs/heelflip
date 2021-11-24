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
        public Quaternion goalRotation;
        public LayerMask layerMask;
        public Vector3 jump;

        public void Rotate(Quaternion ftr, Quaternion ftr2)
        {
            _angularVel = -Quaternion.AngleAxis(1, transform.right).eulerAngles.normalized * _angularVel.magnitude;
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
        [SerializeField] private Vector3 _angularVel;
        [SerializeField] private Transform _feetT;

        private Vector3 _rotationPivot;
        bool _jumpCue = false, _jumping = false, _resetingRotation = false, _grounded;

        private void Awake()
        {
            _rb.centerOfMass = Vector3.zero;
            goalRotation = transform.rotation;
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

            if (!isFeetAttached.value && _isClinched)
            {
                _animator.SetBool("Spin", true);
                _rb.angularVelocity = _angularVel;
            }
            else
            {
                _animator.SetBool("Spin", false);
            }

            if (_resetingRotation)
            {
                Vector3 feetPos = _feetT.position;
                Vector3 feetOffset = transform.position - _feetT.transform.position;

                Vector3 before = transform.up;

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    goalRotation,
                    Time.deltaTime * 8f
                );

                Vector3 after = transform.up;

                Quaternion ftr = Quaternion.FromToRotation(before, after);
                feetOffset = ftr * feetOffset;
                transform.position = feetPos + feetOffset;

                if(Quaternion.Angle(goalRotation, transform.rotation) < 1f)
                {
                    _resetingRotation = false;
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

                if (isFeetAttached.value && !_resetingRotation)
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
    }
}