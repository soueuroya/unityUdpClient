using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchButtonScript : MonoBehaviour
{
    public void Matchmaking()
    {
        SceneManager.LoadScene("Game");
    }
}
