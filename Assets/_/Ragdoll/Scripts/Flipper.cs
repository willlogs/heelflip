using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using PT.Utils;

namespace DB.HeelFlip.Ragdoll
{
    public class Flipper : MonoBehaviour
    {
        public void AddJoint()
        {
            _hipJoint = _pelvisRB.gameObject.AddComponent<ConfigurableJoint>();
            _hipJoint.slerpDrive = hipS0;
        }

        public void RemovePelvisJoint()
        {
            try
            {
                hipS0 = _hipJoint.slerpDrive;
                Destroy(_hipJoint);
                foreach(ConfigurableJoint cj in GetComponentsInChildren<ConfigurableJoint>())
                {
                    cj.angularXDrive = new JointDrive();
                    cj.angularYZDrive = new JointDrive();
                }
            }
            catch { }
        }

        public void StickFoot()
        {
            if (!_footLock) { 
                _rightFoot = _rightFootRB.gameObject.AddComponent<ConfigurableJoint>();
                LockJoint(_rightFoot);
                _leftFoot = _leftFootRB.gameObject.AddComponent<ConfigurableJoint>();
                LockJoint(_leftFoot);
                _footLock = true;
            }
        }

        [FoldoutGroup("Joints")]
        [SerializeField] private ConfigurableJoint _leftKnee, _rightKnee, _leftHip, _rightHip;
        [FoldoutGroup("Joints")]
        [SerializeField] private ConfigurableJoint _leftFoot, _rightFoot;
        [FoldoutGroup("Joints")]
        [SerializeField] private ConfigurableJoint _hipJoint;

        [FoldoutGroup("targetRotations")]
        [SerializeField] private Vector4 _leftHipGoal0, _rightHipGoal0;
        [FoldoutGroup("targetRotations")]
        [SerializeField] private Vector4 _leftKneeGoal0, _rightKneeGoal0;

        [SerializeField] private Rigidbody _pelvisRB, _rightFootRB, _leftFootRB;
        [SerializeField] private float _rotSpeed = 7, _stickyFeetDelay = 1, _hipsJointPower = 1000;
        [SerializeField] private bool _footLock = true;
        [SerializeField] private Vector3 _jump;
        [SerializeField] private Vector3 _angularVelocity;

        private bool _isClinched = false, _pressed = false;

        private JointDrive hipS0, hipS1;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Toggle();
            }
            if(!_isClinched && _pressed)
            {
                /*_hipJoint.targetRotation = 
                    Quaternion.AngleAxis(Time.deltaTime * _rotSpeed, Vector3.forward) *
                    _hipJoint.targetRotation;*/
                _pelvisRB.angularVelocity = _angularVelocity;
                //_pelvisRB.AddTorque(_angularVelocity, ForceMode.Impulse);
                /*_pelvisRB.MoveRotation(
                    Quaternion.AngleAxis(_rotSpeed * Time.deltaTime, Vector3.forward) *
                    _pelvisRB.rotation
                );*/
                /*_hipJoint.targetRotation = 
                    Quaternion.AngleAxis(_rotSpeed * Time.deltaTime, Vector3.forward) *
                    _hipJoint.targetRotation;*/
            }
        }

        private void Toggle()
        {
            if (!_isClinched) {
                _pressed = true;
                _isClinched = true;

                _leftHip.targetRotation = new Quaternion(
                    _leftHipGoal0.x,
                    _leftHipGoal0.y,
                    _leftHipGoal0.z,
                    _leftHipGoal0.w
                );

                _rightHip.targetRotation = new Quaternion(
                    _rightHipGoal0.x,
                    _rightHipGoal0.y,
                    _rightHipGoal0.z,
                    _rightHipGoal0.w
                );

                _leftKnee.targetRotation = new Quaternion(
                    _leftKneeGoal0.x,
                    _leftKneeGoal0.y,
                    _leftKneeGoal0.z,
                    _leftKneeGoal0.w
                );

                _rightKnee.targetRotation = new Quaternion(
                    _rightKneeGoal0.x,
                    _rightKneeGoal0.y,
                    _rightKneeGoal0.z,
                    _rightKneeGoal0.w
                );
            }
            else
            {
                _isClinched = false;
                try
                {
                    Destroy(_rightFoot);
                    Destroy(_leftFoot);
                }
                catch { }

                TimeManager.Instance.DoWithDelay(_stickyFeetDelay, () =>
                {
                    _footLock = false;
                });

                _pelvisRB.velocity = _jump;
                JointDrive jd = new JointDrive();
                jd.maximumForce = 1000000;
                jd.positionSpring = _hipsJointPower;
                _hipJoint.slerpDrive = jd;

                _leftHip.targetRotation = new Quaternion(
                    0,
                    0,
                    0,
                    0
                );

                _rightHip.targetRotation = new Quaternion(
                    0,
                    0,
                    0,
                    0
                );

                _leftKnee.targetRotation = new Quaternion(
                    0,
                    0,
                    0,
                    0
                );

                _rightKnee.targetRotation = new Quaternion(
                    0,
                    0,
                    0,
                    0
                );
            }
        }

        private void FreeUpJoint(ConfigurableJoint cj)
        {
            cj.xMotion = ConfigurableJointMotion.Free;
            cj.yMotion = ConfigurableJointMotion.Free;
            cj.zMotion = ConfigurableJointMotion.Free;
            cj.angularXMotion = ConfigurableJointMotion.Free;
            cj.angularYMotion = ConfigurableJointMotion.Free;
            cj.angularZMotion = ConfigurableJointMotion.Free;
        }

        private void LockJoint(ConfigurableJoint cj)
        {
            cj.xMotion = ConfigurableJointMotion.Locked;
            cj.yMotion = ConfigurableJointMotion.Locked;
            cj.zMotion = ConfigurableJointMotion.Locked;
            cj.angularXMotion = ConfigurableJointMotion.Locked;
            cj.angularYMotion = ConfigurableJointMotion.Locked;
            cj.angularZMotion = ConfigurableJointMotion.Locked;
        }
    }
}