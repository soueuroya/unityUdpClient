using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

public class NetworkMan : MonoBehaviour
{
    public UdpClient udp;
    public GameObject playerPrefab;
    List<GameObject> playerList;
    int playerCount;

    Stack<string> newID;
    Stack<string> dropID;

    string ownedID;

    void Start()
    {
        playerCount = 0;
        playerList = new List<GameObject>();
        newID = new Stack<string>();
        dropID = new Stack<string>();

        udp = new UdpClient();

        udp.Connect("18.191.250.230", 12345);

        Byte[] sendBytes = Encoding.ASCII.GetBytes("connect");

        udp.Send(sendBytes, sendBytes.Length);

        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        InvokeRepeating("HeartBeat", 1, 0.033f);
    }

    void OnDestroy()
    {
        udp.Dispose();
    }


    public enum commands
    {
        NEW_CLIENT,
        UPDATE,
        CLIENT_LIST,
        DROP,
        OWN_ID
    };

    [Serializable]
    public class Message
    {
        public commands cmd;
    }

    [Serializable]
    public class Player
    {
        public string id;
        [Serializable]
        public struct receivedColor
        {
            public float R;
            public float G;
            public float B;
        }
        public receivedColor color;

        public Vector3 position;
        public Vector3 rotation;
    }
    [Serializable]
    class PlayerInfo
    {
        public Vector3 position;
        public Vector3 rotation;
    }
    [Serializable]
    public class OwnID
    {
        public Player ownID;
    }
    [Serializable]
    public class PlayerList
    {
        public Player[] player;
    }

    [Serializable]
    public class GameState
    {
        public Player[] players;
    }

    public Message latestMessage;
    public GameState lastestGameState;
    void OnReceived(IAsyncResult result)
    {
        UdpClient socket = result.AsyncState as UdpClient;
        IPEndPoint source = new IPEndPoint(0, 0);
        byte[] message = socket.EndReceive(result, ref source);
        string returnData = Encoding.ASCII.GetString(message);
        //Debug.Log("Got this: " + returnData);
        latestMessage = JsonUtility.FromJson<Message>(returnData);
        try
        {
            switch (latestMessage.cmd)
            {
                case commands.NEW_CLIENT:
                    PlayerList pl_nc = JsonUtility.FromJson<PlayerList>(returnData);
                    foreach (var player in pl_nc.player)
                    {
                        newID.Push(player.id);
                    }
                    break;
                case commands.UPDATE:
                    lastestGameState = JsonUtility.FromJson<GameState>(returnData);
                    break;
                case commands.CLIENT_LIST:
                    PlayerList pl_cl = JsonUtility.FromJson<PlayerList>(returnData);
                    foreach (var player in pl_cl.player)
                    {
                        newID.Push(player.id);
                    }
                    //Debug.Log("CLIENT_LIST");
                    break;
                case commands.DROP:
                    PlayerList pl_d = JsonUtility.FromJson<PlayerList>(returnData);
                    foreach (var player in pl_d.player)
                    {
                        dropID.Push(player.id);
                    }
                    //Debug.Log("DROP");
                    break;
                case commands.OWN_ID:
                    OwnID oID = JsonUtility.FromJson<OwnID>(returnData);
                    ownedID = oID.ownID.id;
                    //Debug.Log("OWNID");
                    break;
                default:
                    Debug.LogError("Error");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }

    void SpawnPlayers(string id)
    {
        foreach (var it in playerList)
        {
            if (it.GetComponent<NetworkID>().id == id)
                return;
        }

        GameObject temp = Instantiate(playerPrefab, new Vector3(-5 + playerCount, 0, 0), playerPrefab.transform.rotation);
        temp.GetComponent<NetworkID>().id = id;
        if (id == ownedID)
        {
            temp.AddComponent<PlayerController>();
            temp.GetComponent<PlayerController>().network = this;
        }
        playerList.Add(temp);
        playerCount++;
    }

    void UpdatePlayers()
    {
        foreach (var it in playerList)
        {
            foreach (var p in lastestGameState.players)
            {
                if (it.GetComponent<NetworkID>().id == p.id)
                {
                    Color c = new Color(p.color.R, p.color.G, p.color.B);
                    Debug.Log(c);
                    it.GetComponent<Renderer>().material.SetColor("_Color", c);
                    it.transform.position = p.position;
                    it.transform.eulerAngles = p.rotation;
                }
            }
        }
    }

    void DestroyPlayers(string id)
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            var it = playerList[i];
            if (it.GetComponent<NetworkID>().id == id)
            {
                var temp = it;
                playerList.Remove(it);
                Destroy(temp);
            }
        }
    }

    void HeartBeat()
    {
        Byte[] sendBytes = Encoding.ASCII.GetBytes("heartbeat");
        udp.Send(sendBytes, sendBytes.Length);
    }

    public void SendPosition(Vector3 pos, Vector3 rot)
    {
        PlayerInfo info = new PlayerInfo();
        info.position = pos;
        info.rotation = rot;
        string jsonString = JsonUtility.ToJson(info);
        //Debug.Log(jsonString);
        Byte[] sendBytes = Encoding.ASCII.GetBytes(jsonString);
        udp.Send(sendBytes, sendBytes.Length);
    }

    void Update()
    {
        CheckNewPlayer();
        UpdatePlayers();
        CheckDropPlayer();
    }

    void CheckNewPlayer()
    {
        if (newID.Count > 0)
        {
            for (int i = 0; i < newID.Count; i++)
            {
                var it = newID.Pop();
                SpawnPlayers(it);
            }
        }
    }

    void CheckDropPlayer()
    {
        if (dropID.Count > 0)
        {
            for (int i = 0; i < dropID.Count; i++)
            {
                var it = dropID.Pop();
                DestroyPlayers(it);
            }
        }
    }
}