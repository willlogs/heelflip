using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Portal;
using System;

namespace DB.HeelFlip
{
    [RequireComponent(typeof(Rigidbody))]
    public class PortalRBTransit : MonoBehaviour, IPortalTransient
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

        public Vector3 Position {
            get
            {
                return transform.position;
            }
        }

        public void Teleport(Vector3 position, Quaternion rotation, Transform entry, Transform exit)
        {
            BeforeTeleport?.Invoke(entry);

            Vector3 up = transform.up;
            transform.position = position;
            transform.rotation = rotation;

            rb.velocity = Quaternion.FromToRotation(up, transform.up) * rb.velocity;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }
    }
}