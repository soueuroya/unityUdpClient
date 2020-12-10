using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour
{
    public NetworkMan netMan;
    public float speed;
    public Vector2 startPosition;
    public TrailRenderer trail;

    //public GameObject cube;
    Vector3 positionVector3;
    Vector3 rotationVector3;

    public Rigidbody2D rb;

    public float x = 1f, y = 1f;

    // Start is called before the first frame update
    void Start()
    {
        x = 1f;
        y = x;
        positionVector3 = gameObject.transform.position;
        rotationVector3 = gameObject.transform.rotation.eulerAngles;

        startPosition = transform.position;
        Launch();

        SendPos();
        InvokeRepeating("SendPos", 1, 0.033f);
    }

    // Update is called once per frame
    void Update()
    {
        //gameObject.transform.position = gameObject.transform.position + new Vector3(x, y, 0) * Time.deltaTime;
    }

    public void SendPos()
    {
        //netMan.SendPosition(rotationVector3, positionVector3);
    }

    public void Launch()
    {
        //float x = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
        //float y = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
        trail.enabled = true;
    }

    public void Reset()
    {
        trail.enabled = false;
        transform.position = startPosition;
        Invoke("Launch", 1);
        Launch();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("collision");
        if (collision.gameObject.CompareTag("wall"))
        {
            y = -y;
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            x = -x;
        }
        else if (collision.gameObject.CompareTag("goal1"))
        {
            x = -x;
        }
        else if (collision.gameObject.CompareTag("goal2"))
        {
            x = -x;
        }
    }
}
