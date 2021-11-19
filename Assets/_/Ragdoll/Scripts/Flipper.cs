using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DB.HeelFlip.Ragdoll
{
    public class Flipper : MonoBehaviour
    {
        public void AddJoint()
        {
            _hipJoint = _pelvisRB.gameObject.AddComponent<ConfigurableJoint>();
            _hipJoint.angularXDrive = hipS0;
            _hipJoint.angularYZDrive = hipS1;
        }

        [SerializeField] private ConfigurableJoint _leftKnee, _rightKnee, _leftHip, _rightHip;
        [SerializeField] private ConfigurableJoint _leftFoot, _rightFoot;
        [SerializeField] private ConfigurableJoint _hipJoint;
        [SerializeField] private Vector4 _leftHipGoal0, _rightHipGoal0;
        [SerializeField] private Vector4 _leftKneeGoal0, _rightKneeGoal0;
        [SerializeField] private Rigidbody _pelvisRB;
        [SerializeField] private Vector3 _angularVelocity;

        private bool _isClinched = false, _pressed = false;
        private float _rot = 0;

        private JointDrive hipS0, hipS1;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Toggle();
            }
            if(!_isClinched && _pressed)
            {
                
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
                Destroy(_leftFoot);
                Destroy(_rightFoot);

                hipS0 = _hipJoint.angularXDrive;
                hipS1 = _hipJoint.angularYZDrive;
                Destroy(_hipJoint);

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
    }
}