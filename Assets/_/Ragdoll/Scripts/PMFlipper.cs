using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DB.HeelFlip
{
    public class PMFlipper : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private bool _isClinched = false;
        [SerializeField] private Animator _animator;
        [SerializeField] private Vector3 _angularVel;
        [SerializeField] private Vector3 _jump;

        bool _jumpCue = false, _jumping = false;

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

            if (_jumping && !_isClinched)
            {
                _rb.angularVelocity = _angularVel;
            }
        }

        private void ToggleClinch()
        {
            if (_isClinched)
            {
                _isClinched = false;
                _animator.SetBool("Clinch", false);
                _jumpCue = false;
                _jumping = true;
                _rb.velocity = _jump;
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