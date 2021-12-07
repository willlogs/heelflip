using DB.HeelFlip;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeelUpgrade : MonoBehaviour
{
    [SerializeField] private Transform _leftShoe, _rightShoe;
    private void OnTriggerEnter(Collider other)
    {
        PMFlipper flipper = other.GetComponent<PMFlipper>();
        if (flipper != null)
        {
            float dot = Vector3.Dot(flipper._bodyT.up, transform.up);
            if (dot > 0.9)
            {
                flipper.shoes.LevelUp();
            }
            else
            {
                _leftShoe.parent = null;
                _leftShoe.gameObject.AddComponent<BoxCollider>();
                _leftShoe.gameObject.AddComponent<Rigidbody>();

                _rightShoe.parent = null;
                _rightShoe.gameObject.AddComponent<BoxCollider>();
                _rightShoe.gameObject.AddComponent<Rigidbody>();
            }
        }
    }
}
