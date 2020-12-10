using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //public GameObject cube;
    public NetworkMan netMan;
    Vector3 positionVector3;

    public float walkSpeed = 5;
    public Rigidbody2D rb;

    public bool isPlayer1 = true;
    public bool canControl = true;

    // Start is called before the first frame update
    void Start()
    {
        if (isPlayer1)
        {
            positionVector3 = new Vector3(-1.2f, 0, 0);
        }
        else
        {
            positionVector3 = new Vector3(1.2f, 0, 0);
        }
        rb = gameObject.GetComponent<Rigidbody2D>();
        SendPos();
        InvokeRepeating("SendPos", 1, 0.033f);

    }

    // Update is called once per frame
    void Update()
    {
        if (canControl)
        {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                positionVector3 += transform.TransformVector(Vector3.up) * Time.deltaTime;
                //gameObject.transform.position += (Vector3.up * walkSpeed * Time.deltaTime);
                //rb.velocity = (Vector3.up * walkSpeed);
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                positionVector3 += transform.TransformVector(Vector3.up) * -Time.deltaTime;
                //gameObject.transform.position += (Vector3.up * -walkSpeed * Time.deltaTime);
                //rb.velocity = (Vector3.down * walkSpeed);
            }
            else
            {
                //rb.velocity = Vector2.zero;
            }
        }
    }

    public void SendPos()
    {
        //positionVector3 = gameObject.transform.position;
        //rotationVector3 = gameObject.transform.rotation.eulerAngles;

        netMan.SendPosition(positionVector3);
    }
}
