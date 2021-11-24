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

        public void Teleport(Vector3 position, Quaternion rotation, Transform entry, Transform exit)
        {
            BeforeTeleport?.Invoke(entry);
            Quaternion before = transform.rotation;
            Vector3 up = transform.up;

            // x is for right, y is for up, z is for forward
            float velMag, camPosMag;
            Vector3 velCoord, camPosCoord;
            ToRelativeCoord(rb.velocity, out velMag, out velCoord);
            ToRelativeCoord(_camTarget.position - transform.position, out camPosMag, out camPosCoord);

            // change position and rotation
            transform.position = position;
            transform.rotation = rotation;

            // do misc
            Quaternion ftr = Quaternion.Inverse(before) * rotation;
            Quaternion ftr2 = Quaternion.FromToRotation(up, transform.up);

            rb.velocity = (transform.right * velCoord.x + transform.up * velCoord.y + transform.forward * velCoord.z).normalized * velMag;

            _camTarget.parent.position = transform.position;
            Vector3 relativePos = (transform.right * camPosCoord.x + transform.up * camPosCoord.y + transform.forward * camPosCoord.z).normalized * camPosMag;
            _camTarget.transform.position = transform.position + relativePos;

            _flipper.Rotate(ftr, ftr2);
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
        [SerializeField] private Transform _camTarget;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }
    }
}
