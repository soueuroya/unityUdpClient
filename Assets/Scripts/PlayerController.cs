using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public NetworkMan network;

    [SerializeField]
    float speed;

    [SerializeField]
    float rotationSpeed;

    Vector3 pos;
    Vector3 rot;

    void Start()
    {
        speed = 1;
        rotationSpeed = 90;
        InvokeRepeating("UpdatePosition", 1, 0.033f);
    }

    public PlayerController(NetworkMan _network)
    {
        network = _network;
    }
    
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            pos += transform.TransformVector(Vector3.forward) * Time.deltaTime * speed;
            //UpdatePosition();
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            pos -= transform.TransformVector(Vector3.forward) * Time.deltaTime * speed;
            //UpdatePosition();
        }
        
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rot -= Vector3.up * Time.deltaTime * rotationSpeed;
            //UpdatePosition();
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            rot += Vector3.up * Time.deltaTime * rotationSpeed;
            //UpdatePosition();
        }
    }

    void UpdatePosition()
    {
        network.SendPosition(pos, rot);
    }
}
