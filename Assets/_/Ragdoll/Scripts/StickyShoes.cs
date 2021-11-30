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
        public UnityEvent<Vector3> OnAttach;
        public BoolCondition isAttachedBool;

        public void OnCollisionE(Collider other)
        {
            if (_canAttach && !other.isTrigger)
            {
                Vector3 pivot = transform.position;
                if (_level > 1)
                {
                    pivot = _leftStack[_leftStack.Count - 1]._bottomT.position;
                }
                else
                {
                    pivot = _leftHeel._bottomT.position;
                }

                OnAttach?.Invoke(pivot);
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

        public void LevelUp()
        {
            if(_level < 1)
            {
                _rightHeel.gameObject.SetActive(true);
                _leftHeel.gameObject.SetActive(true);
            }
            else
            {
                Transform rlp = _rightHeel._bottomT;
                Transform llp = _leftHeel._bottomT;

                if (_level > 1)
                {
                    llp = _leftStack[_leftStack.Count - 1]._bottomT;
                    rlp = _rightStack[_rightStack.Count - 1]._bottomT;
                }

                HeelPart rhp = Instantiate(_rightHeel.gameObject).GetComponent<HeelPart>();
                HeelPart lhp = Instantiate(_leftHeel.gameObject).GetComponent<HeelPart>();

                CloneTransportFromTo(_rightHeel ,rhp);
                CloneTransportFromTo(_leftHeel ,lhp);

                _rightStack.Add(rhp);
                _leftStack.Add(lhp);

                rhp.transform.position = rlp.position;
                lhp.transform.position = llp.position;
            }
            _level++;
        }

        private void CloneTransportFromTo(HeelPart from_, HeelPart to_)
        {
            to_.transform.parent = from_.transform.parent;
            to_.transform.localScale = from_.transform.localScale;
            to_.transform.rotation = from_.transform.rotation;
        }

        public void LevelDown()
        {

        }

        [SerializeField] private Rigidbody _rb;
        [SerializeField] private PuppetMaster _puppet;
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private HeelPart _leftHeel, _rightHeel;
        [SerializeField] private List<HeelPart> _leftStack, _rightStack;
        [SerializeField] private int _level = 0;

        private bool _canAttach = true;
        [SerializeField] private Vector3 _bottom;

        private void Start()
        {
            if(_level == 0)
            {
                _rightHeel.gameObject.SetActive(false);
                _leftHeel.gameObject.SetActive(false);
            }
        }

        private void SetFilter(MeshFilter filter)
        {
            Destroy(filter.GetComponent<BoxCollider>());
            filter.gameObject.AddComponent<BoxCollider>().isTrigger = true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                LevelUp();
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                LevelDown();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_bottom, 0.1f);
        }
    }
}