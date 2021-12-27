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
using UnityEngine.SceneManagement;

namespace DB.HeelFlip
{
    public class PMFlipper : MonoBehaviour
    {
        public UnityEvent OnJump, OnSlide, OnStopSlide, OnDeath;
        public BoolCondition isFeetAttached;
        public LayerMask layerMask;
        public Vector3 jump;
        public StickyShoes shoes;

        public void Restart()
        {
            TimeManager.Instance.DoWithDelay(3, () =>
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
        }

        public void DieQuestionMark()
        {
            if (!isFeetAttached.value)
            {
                OnDeath?.Invoke();
                _puppet.mode = PuppetMaster.Mode.Active;
                _rb.isKinematic = true;
                Restart();
                /*TimeManager.Instance.AddLayer(0);
                TimeManager.Instance.DoWithDelay(1, () =>
                {
                    TimeManager.Instance.GoNormal();
                });*/
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
            _puppet.mode = PuppetMaster.Mode.Active;
        }

        public void Die()
        {
            _puppet.state = PuppetMaster.State.Dead;
            _rb.isKinematic = true;
        }

        public void SetStruggleTrue()
        {
            _animator.SetBool("Struggle", true);
        }

        public void SetStruggleFalse()
        {
            _animator.SetBool("Struggle", false);
        }

        public UnityEvent<Collision> OnDidntDie;
        public void CheckAngle(Collision c)
        {
            float dot = Vector3.Dot(_bodyT.up, c.GetContact(0).normal);
            if (dot < 0.9f)
            {
                DieQuestionMark();
            }
            else
            {
                _animator.SetBool("Struggle", true);
                OnDidntDie?.Invoke(c);
            }
        }

        [SerializeField] private PuppetMaster _puppet;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private bool _isClinched = false;
        [SerializeField] private Animator _animator;
        [SerializeField] private float _angularVel, _angularLimit, _angularAcceleration, _angularDamper;
        [SerializeField] public Transform _feetT, _bodyT;
        [SerializeField] private LayerMask _slideLayer;
        [SerializeField] private SplinePositioner _positioner;
        [SerializeField] private Collider _collider;

        private Vector3 _rotationPivot;
        private float _distance;
        bool _jumpCue = false, _jumping = false, _resetingRotation = false, _grounded, _sliding;

        private void Awake()
        {
            _rb.centerOfMass = Vector3.zero;
            _puppet.mode = PuppetMaster.Mode.Kinematic;
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButtonUp(0))
            {
                ToggleClinch();
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
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

            if (!isFeetAttached.value && !_sliding)
                _bodyT.rotation = Quaternion.AngleAxis(_angularVel, _bodyT.right) * _bodyT.rotation;

            if (_sliding)
            {
                _distance += Time.deltaTime * 20;
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
                    Time.deltaTime * 5
                );

                if((transform.position - _positioner.transform.position).magnitude < 5)
                    transform.position = Vector3.Lerp(
                        transform.position,
                        _positioner.transform.position + bodyOffset * 0.8f,
                        Time.deltaTime * 10
                    );

                if (_sample.percent >= 0.98f)
                {
                    print("Shoot");
                    Vector3 shootDir = angle1 > angle2 ? _bodyT.up : -_bodyT.up;
                    _canSlide = false;
                    DeactivateSlide();
                    _rb.velocity = shootDir * 10f;
                    _collider.enabled = false;
                    TimeManager.Instance.DoWithDelay(1f, () =>
                    {
                        _canSlide = true;
                        _collider.enabled = true;
                    });
                }
            }

            if (_resetingRotation)
            {
                _bodyT.rotation = Quaternion.Slerp(
                    _bodyT.rotation,
                    transform.rotation,
                    Time.deltaTime * 5
                );
                float angle = Quaternion.Angle(_bodyT.rotation, transform.rotation);
                if(angle <= 0.1)
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

                if (isFeetAttached.value && !_resetingRotation && !_sliding)
                {
                    _puppet.mode = PuppetMaster.Mode.Kinematic;
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

                slideNormal = collision.GetContact(0).normal;
                float dot = Vector3.Dot(slideNormal, _bodyT.up);
                
                if (dot < -0.9)
                {
                    DieQuestionMark();
                    return;
                }

                if (!_sliding && _canSlide)
                {
                    Slide s = collision.gameObject.GetComponent<Slide>();
                    if (s != null && !s._used)
                    {
                        s._used = true;

                        _rb.isKinematic = true;
                        _rb.useGravity = false;
                        //_collider.enabled = false;
                        OnSlide?.Invoke();
                        _beforeSlideRotation = transform.rotation;
                        _positioner.transform.parent = null;
                        SplineComputer spline = collision.gameObject.GetComponent<SplineComputer>();
                        SplineSample sample = spline.Project(transform.position);
                        _distance = (float)sample.percent * spline.CalculateLength();
                        _positioner.spline = spline;
                        _positioner.transform.position = sample.position;
                        _positioner.SetDistance(_distance);

                        _sliding = true;
                    }
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
            _collider.enabled = false;
            OnStopSlide?.Invoke();
            _sliding = false;
            _puppet.mode = PuppetMaster.Mode.Kinematic;
            _rb.useGravity = true;
            _rb.isKinematic = false;
        }
    }
}