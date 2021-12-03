using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DB.HeelFlip
{
    public class CameraTrigger : MonoBehaviour
    {
        public void ActivateCamTransition(Collider other)
        {
            PortalRagdollTransit transit = other.gameObject.GetComponent<PortalRagdollTransit>();
            if(transit != null)
            {
                transit.BeforeTel(_entryPlane);
            }
        }

        [SerializeField] private Transform _entryPlane;
    }
}