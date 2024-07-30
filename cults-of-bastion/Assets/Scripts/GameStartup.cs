using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartup : MonoBehaviour
{
    private void Start()
    {
        for (int i = 1; i < 4; i++)
        {
            SceneManager.LoadSceneAsync(i, LoadSceneMode.Additive);
        }
    }
}
