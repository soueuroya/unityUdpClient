using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateButtonScript : MonoBehaviour
{
    public NetworkMan nm;
    public void CreateRoom()
    {
        nm = GameObject.Find("NetworkMan").GetComponent<NetworkMan>();
        nm.CreateClicked();
    }
}
