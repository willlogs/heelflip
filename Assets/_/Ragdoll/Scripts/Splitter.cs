using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splitter : MonoBehaviour
{
    public AudioSource _as;
    public static float pitch = 1f;

    public void PlayEffect()
    {
        _as.pitch = pitch;
        pitch += 0.005f;
        _as.Play();
    }

    private void Start()
    {
        Renderer r = GetComponent<Renderer>();
        r.materials[1].mainTextureOffset = new Vector2(Random.Range(0f, 1f), 0);
    }
}
