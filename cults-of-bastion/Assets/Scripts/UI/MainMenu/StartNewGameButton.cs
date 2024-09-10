using System;
using UnityEngine;
using UnityEngine.UI;

public class StartNewGameButton : MonoBehaviour
{
    public static event Action OnStartNewGameButtonClicked;
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => OnStartNewGameButtonClicked?.Invoke());
    }
}
