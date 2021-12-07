using System.Collections.Generic;
using UnityEngine;

public class Runner : MonoBehaviour
{
    public float speed;
    public float acceleration;
    private Camera cam;
    private AudioSource aud;
    public List<GameObject> Tiles = new List<GameObject>();
    private float zPos = 0;
    private int starTiles = 2;
    private int prevRand = 0;
    private int num = 0;
    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        aud = GetComponent<AudioSource>();
        for (int i = 0; i < Tiles.Count; i++)
        {
            PoolManager.Instance.CreatePool(Tiles[i], 2);
        }
        for (int i = 0; i < starTiles; i++)
        {
            while (prevRand == num)
            {
                num = Tiles[Random.Range(0, Tiles.Count)].GetInstanceID();
            }
            PoolManager.Instance.Reuse(num, new Vector3(0, 0, zPos), Quaternion.identity);
            prevRand = num;
            zPos += 50;
        }

    }

    void Update()
    {
        if (speed <= 1.2f)
        {
            if (Input.GetMouseButton(0))
            {
                acceleration = 0.15f;
            }
            else
            {
                acceleration = -Mathf.Sqrt(speed);
            }
            speed += acceleration * Time.deltaTime;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView + acceleration * Time.deltaTime * 10, 60, 80);
        }

        speed = Mathf.Clamp(speed, 0, 1);
        aud.pitch = Mathf.Clamp(Mathf.Sqrt(speed * 10), 1.25f, 3);
        aud.volume = Mathf.Clamp(speed * 0.3f, 0.5f, 0.7f);


        if (transform.position.z + 100 > zPos)
        {
            while (prevRand == num)
            {
                num = Tiles[Random.Range(0, Tiles.Count)].GetInstanceID();
            }
            PoolManager.Instance.Reuse(num, new Vector3(0, 0, zPos), Quaternion.identity);
            zPos += 50;
            prevRand = num;
        }
        transform.position += Vector3.forward * speed;
    }
}
