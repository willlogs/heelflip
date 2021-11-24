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
                BoxCollider bc = _leftRenderer.GetComponent<BoxCollider>();
                _bottom = bc.bounds.center;
                _bottom.y -= (bc.bounds.size / 2).y;

                // raycast and replace _bottom to matching position

                RaycastHit hit;
                Physics.Raycast(
                    new Ray(_leftRenderer.transform.position, Vector2.down),
                    out hit,
                    3f,
                    _layerMask,
                    QueryTriggerInteraction.Ignore
                );
                Vector3 pivot = hit.collider != null ? hit.point : _bottom;                

                _rb.transform.position = pivot + _rb.transform.position - _bottom;

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
            if(_level < _shoeMeshes.Length - 1)
            {
                _level++;
                SetShoeMesh();
            }
        }

        public void LevelDown()
        {
            if(_level > 0)
            {
                _level--;
                SetShoeMesh();
            }
        }

        [SerializeField] private Rigidbody _rb;
        [SerializeField] private PuppetMaster _puppet;
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private MeshFilter _leftRenderer, _rightRenderer;
        [SerializeField] private Mesh[] _shoeMeshes;
        [SerializeField] private int _level = 0;

        private bool _canAttach = true;
        [SerializeField] private Vector3 _bottom;

        private void Start()
        {
            SetShoeMesh();
        }

        private void SetShoeMesh()
        {
            SetFilter(_leftRenderer);
            SetFilter(_rightRenderer);
        }

        private void SetFilter(MeshFilter filter)
        {
            filter.mesh = _shoeMeshes[_level];
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