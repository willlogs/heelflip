using DB.HeelFlip;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeelUpgrade : MonoBehaviour
{
    [SerializeField] private Transform _leftShoe, _rightShoe;
    private PMFlipper _flipper;
    private bool used = false;

    public void Enter(Collider other)
    {
        if (!used)
        {
            used = true;
            _flipper.shoes.LevelUp();
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _flipper = FindObjectOfType<PMFlipper>();
    }
}
