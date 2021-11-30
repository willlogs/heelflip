using DB.HeelFlip;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeelUpgrade : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PMFlipper flipper = other.GetComponent<PMFlipper>();
        if (flipper != null)
        {
            flipper.shoes.LevelUp();
            Destroy(gameObject);
        }
    }
}
