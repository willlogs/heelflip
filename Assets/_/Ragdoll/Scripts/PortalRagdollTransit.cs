using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Knife.Portal;
using RootMotion.Dynamics;

namespace DB.HeelFlip
{
    [RequireComponent(typeof(Rigidbody))]
    public class PortalRagdollTransit : MonoBehaviour, IPortalTransient
    {
        public event Action<Transform> BeforeTeleport;
        Rigidbody rb;

        public bool UseThreshold
        {
            get
            {
                return true;
            }
        }

        public Vector3 Position
        {
            get
            {
                return transform.position;
            }
        }

        bool _wentIn = true;
        public void BeforeTel(Transform entry)
        {
            if (_wentIn)
            {
                _wentIn = false;
                BeforeTeleport?.Invoke(entry);
            }
        }

        public void Teleport(Vector3 position, Quaternion rotation, Transform entry, Transform exit)
        {
            //BeforeTeleport?.Invoke(entry);
            Quaternion before = transform.rotation;
            Vector3 up = transform.up;

            // x is for right, y is for up, z is for forward
            float velMag;
            Vector3 velCoord;
            ToRelativeCoord(rb.velocity, out velMag, out velCoord);
            Quaternion bodyTR = Quaternion.AngleAxis(180, exit.up) * (exit.rotation * Quaternion.Inverse(entry.rotation) * _bodyT.rotation);

            // change position and rotation
            transform.position = position;
            transform.rotation = rotation;

            rb.velocity = (transform.right * velCoord.x + transform.up * velCoord.y + transform.forward * velCoord.z).normalized * velMag;

            Vector3 diff = exit.position - transform.position;
            diff.y = 0;
            transform.rotation = Quaternion.LookRotation(diff.normalized, Vector3.up);

            _bodyT.parent = null;
            _bodyT.rotation = bodyTR;
            _bodyT.transform.parent = transform;
            _wentIn = true;
        }

        private void ToRelativeCoord(Vector3 input, out float mag, out Vector3 coord)
        {
            Vector3 temp = input.normalized;
            mag = input.magnitude;
            coord = new Vector3(
                Vector3.Dot(temp, transform.right),
                Vector3.Dot(temp, transform.up),
                Vector3.Dot(temp, transform.forward)
            );
        }

        [SerializeField] private PMFlipper _flipper;
        [SerializeField] private Transform _camTarget, _bodyT, _pelvisT;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }
    }
}
