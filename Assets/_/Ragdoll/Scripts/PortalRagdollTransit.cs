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
            puppetMaster.mode = PuppetMaster.Mode.Kinematic;

            Vector3 up = transform.up;
            transform.position = position;
            transform.rotation = rotation;

            Quaternion ftr = Quaternion.FromToRotation(up, transform.up);
            rb.velocity = ftr * rb.velocity.normalized * rb.velocity.magnitude;

            puppetMaster.mode = PuppetMaster.Mode.Active;
            puppetMaster.FixMusclePositionsAndRotations();
            puppetMaster.Teleport(position, rotation, true);
        }

        [SerializeField] private PuppetMaster puppetMaster;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }
    }
}
