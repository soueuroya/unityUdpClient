using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinButtonScript : MonoBehaviour
{
    public NetworkMan nm;
    public void JoinRoom()
    {
        nm = GameObject.Find("NetworkMan").GetComponent<NetworkMan>();
        nm.JoinClicked();
    }
}
