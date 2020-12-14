using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using UnityEngine.UI;

public class NetworkMan : MonoBehaviour
{
    public UdpClient udp;
    public GameObject player;
    public GameObject player2;
    //public Transform playerSpawn;
    //public Transform player2Spawn;
    //public Transform ballSpawn;
    public GameObject ball;
    public GameObject btemp;
    //public Transform canvas;
    List<GameObject> players;
    Stack<string> newID;
    Stack<string> dropID;
    int spawnCounter;
    string ownedID;
    public bool spawningPlayer2 = false;
    public GameObject canvas;
    public Button create, join;

    void Start()
    {
        join.interactable = false;
        spawnCounter = 0;
        players = new List<GameObject>();
        newID = new Stack<string>();
        dropID = new Stack<string>();

        udp = new UdpClient();
        
        udp.Connect("18.191.250.230", 12345);

        Byte[] sendBytes = Encoding.ASCII.GetBytes("connect");
      
        udp.Send(sendBytes, sendBytes.Length);

        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        InvokeRepeating("HeartBeat", 1, 0.033f);

    }

    void OnDestroy(){
        udp.Dispose();
    }

  
    public enum commands{
        NEW_CLIENT,
        UPDATE,
        CLIENT_LIST,
        DROP,
        OWN_ID
    };
    
    [Serializable]
    public class Message{
        public commands cmd;
    }
    
    [Serializable]
    public class Player{
        public string id;
        [Serializable]
        public struct receivedColor{
            public float R;
            public float G;
            public float B;
        }
        public receivedColor color;

        public Vector3 position;
        public Vector3 rotation;
    }
    [Serializable]
    class BallInfo
    {
        public Vector3 rotation;
        public Vector3 position;
    }
    [Serializable]
    class PlayerInfo
    {
        public Vector3 position;
    }
    [Serializable]
    public class OwnID
    {
        public Player ownID;
    }
    [Serializable]
    public class NewPlayer{
        public Player[] player;
    }

    [Serializable]
    public class GameState{
        public Player[] players;
    }

    public Message latestMessage;
    public GameState lastestGameState;
    void OnReceived(IAsyncResult result){
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;
        
        // points towards whoever had sent the message:
        IPEndPoint source = new IPEndPoint(0, 0);

        // get the actual message and fill out the source:
        byte[] message = socket.EndReceive(result, ref source);
        
        // do what you'd like with `message` here:
        string returnData = Encoding.ASCII.GetString(message);
        //Debug.Log("Got this: " + returnData);
        
        latestMessage = JsonUtility.FromJson<Message>(returnData);
        try{
            switch(latestMessage.cmd){
                
                case commands.NEW_CLIENT:
                    NewPlayer p = JsonUtility.FromJson<NewPlayer>(returnData);
                    foreach(var it in p.player)
                    {
                        newID.Push(it.id);
                    }
                    break;
                case commands.UPDATE:
                    lastestGameState = JsonUtility.FromJson<GameState>(returnData);
                    break;
                case commands.CLIENT_LIST:
                    NewPlayer np = JsonUtility.FromJson<NewPlayer>(returnData);
                    foreach (var it in np.player)
                    {
                        newID.Push(it.id);
                    }
                    //Debug.Log("CLIENT_LIST");
                    break;
                case commands.DROP:
                    NewPlayer dp = JsonUtility.FromJson<NewPlayer>(returnData);
                    foreach (var it in dp.player)
                    {
                        dropID.Push(it.id);
                    }
                    //Debug.Log("DROP");
                    break;
                case commands.OWN_ID:
                    OwnID oID = JsonUtility.FromJson<OwnID>(returnData);
                    ownedID = oID.ownID.id;
                    //Debug.Log("OWNID");
                    break;
                default:
                    //Debug.Log("Error");
                    break;
            }
        }
        catch (Exception e){
            //Debug.Log(e.ToString());
        }
        
        // schedule the next receive operation once reading is done:
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }

    void SpawnPlayers(string id){

        GameObject ptemp;

        foreach (var it in players)
        {
            if (it.GetComponent<NetworkID>().id == id)
            {
                //Debug.Log("RETURNING --11111111111111111111111" + id);
                return;
            }
        }

        if (!spawningPlayer2)
        {
            //Debug.Log("Inst player 1 --77777777777777777" + id);
            ptemp = Instantiate(player, new Vector3(-1.2f, 0, 0), player.transform.rotation);

            ptemp.GetComponent<NetworkID>().id = id; // this is this player's id
            if (id == ownedID)
            {
                //Debug.Log("OWNEDID --888888888888888" + id);
                ptemp.AddComponent<PlayerController>();
                ptemp.GetComponent<PlayerController>().netMan = this;
            }
            players.Add(ptemp);
            spawnCounter++;
            spawningPlayer2 = true;
        }
        else
        {
            ptemp = Instantiate(player2, new Vector3(1.2f, 0, 0), player2.transform.rotation);

            ptemp.GetComponent<NetworkID>().id = id; // this is this player's id
            if (id == ownedID)
            {
                ptemp.AddComponent<PlayerController>();
                ptemp.GetComponent<PlayerController>().netMan = this;
                ptemp.GetComponent<PlayerController>().isPlayer1 = false;
                join.interactable = true;
                create.interactable = false;
                ptemp.GetComponent<PlayerController>().SendPos();
            }

            players.Add(ptemp);
            spawnCounter++;

            btemp = Instantiate(ball, new Vector3(0, 0, 0), ball.transform.rotation);
            btemp.GetComponent<NetworkID>().id = "ball";
            btemp.GetComponent<BallScript>().netMan = this;
            players.Add(btemp);
            spawningPlayer2 = false;
            canvas.SetActive(false);
        }
    }

    void UpdatePlayers(){
        if (btemp == null)
        {
            btemp = GameObject.Find("Ball");
            if (btemp == null)
            {
                btemp = GameObject.Find("Ball(Clone)");
            }
        }
        foreach (var it in players)
        {
            foreach (var p in lastestGameState.players)
            {
                if (it.GetComponent<NetworkID>().id == p.id && p.id != "ball")
                {
                    Debug.Log("ID: " + it.GetComponent<NetworkID>().id + "   ------   " + p.id + " < P ID");
                    // change it to position & rotation
                    Debug.Log(p.position + " <<<<<<< " + p.id);
                    if (p.position.x == 1.2f || p.position.x == -1.2f)
                    {
                        it.transform.position = p.position;
                    }
                    else
                    {
                        btemp.transform.position = p.position;
                    }
                    //it.transform.eulerAngles = p.rotation;

                    //Color c = new Color(p.color.R, p.color.G, p.color.B);
                    //it.GetComponent<Renderer>().material.SetColor("_Color", c);
                }
                else if (it.GetComponent<NetworkID>().id == "ball" && p.id == "ball")
                {
                    if (p.position.x != 1.2f && p.position.x != -1.2f)
                    {
                        btemp.transform.position = p.position;
                    }
                    Debug.Log("its the ball");
                }
                Debug.Log("FOREACH" + it.name + "  -----  " + p.id);
            }
        }
    }

    void DestroyPlayers(string id){
        for(int i = 0; i < players.Count; i++)
        {
            var it = players[i];
            if (it.GetComponent<NetworkID>().id == id)
            {
                var temp = it;
                players.Remove(it);
                Destroy(temp);
                spawnCounter--;
            }
        }

    }

    void HeartBeat(){
        Byte[] sendBytes = Encoding.ASCII.GetBytes("heartbeat");
        udp.Send(sendBytes, sendBytes.Length);
    }

    public void SendPosition(Vector3 rot, Vector3 pos)
    {
        BallInfo info = new BallInfo();
        info.rotation = rot;
        info.position = pos;
        string jsonString = JsonUtility.ToJson(info);
        //Debug.Log(jsonString);
        Byte[] sendBytes = Encoding.ASCII.GetBytes(jsonString);
        udp.Send(sendBytes, sendBytes.Length);
    }

    public void SendPosition(Vector3 pos)
    {
        PlayerInfo info = new PlayerInfo();
        info.position = pos;
        string jsonString = JsonUtility.ToJson(info);
        //Debug.Log(jsonString);
        Byte[] sendBytes = Encoding.ASCII.GetBytes(jsonString);
        udp.Send(sendBytes, sendBytes.Length);
    }

    void Update(){
        //Debug.Log("COUNT ------- " + newID.Count);
        if (newID.Count > 0)
        {
            if (newID.Count > 2)
            {
                spawningPlayer2 = true;
            }

            for(int i = 0; i < newID.Count; i++)
            {
                var it = newID.Pop();
                SpawnPlayers(it);
            }
        }

        UpdatePlayers();

        if (dropID.Count > 0)
        {
            for (int i = 0; i < dropID.Count; i++)
            {
                var it = dropID.Pop();
                DestroyPlayers(it);
            }
        }
    }

    public void CreateClicked()
    {
        create.interactable = false;
    }

    public void JoinClicked()
    {
        canvas.SetActive(false);
    }
}