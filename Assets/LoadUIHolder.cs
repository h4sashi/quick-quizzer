using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadUIHolder : MonoBehaviour
{
    public GameObject LoaderUI;
    public GameObject playfabInstance;

    // Start is called before the first frame update
    void Start()
    {
        // Check if there's an existing PlayFabLogin instance and handle accordingly
        if (PlayFabLogin.Instance != null)
        {
            // Destroy the existing instance to avoid duplication
            Destroy(PlayFabLogin.Instance.gameObject);
        }

        GameObject playfabhttp = GameObject.Find("PlayFabHttp");
        if (playfabhttp != null)
        {
            Destroy(playfabhttp);
        }

        StartCoroutine(InstantiatePlayFab());
    }

    IEnumerator InstantiatePlayFab()
    {
        yield return new WaitForSeconds(2f);
        // Instantiate PlayFabManager (PlayFabLogin instance) and mark it as persistent across scenes
        GameObject playfabManager = Instantiate(playfabInstance, transform.position, Quaternion.identity);
        playfabManager.name = "PlayfabManager";

        // Make sure PlayFabManager persists across scenes
        DontDestroyOnLoad(playfabManager);
    }
    // Update is called once per frame
    void Update()
    {
        // Handle any necessary updates for the UI or instance management
    }
}
