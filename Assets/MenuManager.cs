using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MenuManager : MonoBehaviour
{
    public UnityEvent onPlay;
    public UnityEvent onGetLeaderboard;

    private void Start()
    {
        Time.timeScale = 1f;
    }

    public void Play()
    {
        onPlay?.Invoke();
    }

    public void Leaderboard()
    {
        PlayFabLogin.Instance.GetLeaderboard();
        onGetLeaderboard?.Invoke();
    }

    public void LeaderboardAroundPlayer()
    {
        PlayFabLogin.Instance.GetLeaderboardAroundPlayer();
    }


    public void Quit()
    {
        PlayFabLogin.Instance.LogoutUser();
        Application.Quit();
    }
}
