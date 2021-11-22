using DB.Utils;
using PT.Utils;
using RootMotion.Dynamics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DB.HeelFlip.Ragdoll {
    public class StickyShoes : MonoBehaviour
    {
        public UnityEvent OnAttach;
        public BoolCondition isAttachedBool;

        public void OnCollisionE(Collider other)
        {
            if (_canAttach && !other.isTrigger)
            {
                OnAttach?.Invoke();
                _rb.angularVelocity = Vector3.zero;
                _rb.velocity = Vector3.zero;
                _rb.isKinematic = true;
                isAttachedBool.value = true;

                _canAttach = false;
            }
        }

        public void Release()
        {
            _rb.isKinematic = false;
            isAttachedBool.value = false;
            TimeManager.Instance.DoWithDelay(1f, () =>
            {
                _canAttach = true;
            });
        }

        [SerializeField] private Rigidbody _rb;
        [SerializeField] private PuppetMaster _puppet;

        private bool _canAttach = true;
    }
}