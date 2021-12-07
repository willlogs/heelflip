using DB.HeelFlip;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [SerializeField] private Vector3 shootSpeed;
    public void Shoot(Collision c)
    {
        PMFlipper flipper = c.gameObject.GetComponent<PMFlipper>();
        if (flipper != null)
        {
            if (Vector3.Dot(flipper._bodyT.up, transform.up) < 0.9)
            {
                flipper.DieQuestionMark();
            }
            else
            {
                Vector3 vel = transform.right * shootSpeed.x + transform.up * shootSpeed.y + transform.forward * shootSpeed.z;
                Rigidbody rb = c.gameObject.GetComponent<Rigidbody>();
                rb.velocity = shootSpeed;
            }
        }
    }
}
