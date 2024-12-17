using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using UnityEngine.UI;

public class PlayFabLogin : MonoBehaviour
{
    public static PlayFabLogin Instance { get; private set; } // Singleton instance

    public TMP_InputField userInput; // Use TextMeshProUGUI for the input field
    private TextMeshProUGUI ScoreText;

    public TextMeshProUGUI prompt;

    public GameObject LoadingUI;

    private string displayName;
    private string sessionID;

    public GameObject rowPrefab;
    public Transform rowsParent;

    private string loginPlayfabID;

    private void Awake()
    {
        // Ensure only one instance of PlayFabLogin exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make this instance persistent across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }

        Button signInButton = GameObject.Find("SetUserButton").GetComponent<Button>();
        signInButton.onClick.AddListener(LoginWithUsername);

        LoadingUI = GameObject.Find("LoadUIHolder").GetComponent<LoadUIHolder>().LoaderUI;

        prompt = GameObject.Find("PromptText").GetComponent<TextMeshProUGUI>();

        userInput = GameObject.Find("InputField").GetComponent<TMP_InputField>();

        // Register the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Unregister the sceneLoaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main")
        {
            TextMeshProUGUI _displayNameText = GameObject.Find("UsernameText").GetComponent<TextMeshProUGUI>();
            _displayNameText.text = displayName;
            ScoreText = GameObject.Find("MenuScoreText").GetComponent<TextMeshProUGUI>();
            rowsParent = GameObject.Find("Table").GetComponent<Transform>();

            MarkUsernameAsUsed(displayName);

            GetPlayerLeaderboardScore();
        }
        else if (scene.name == "Menu")
        {
            // // Destroy the current PlayFabLogin instance when returning to the Menu scene
            // if (Instance != null)
            // {
            //     Destroy(Instance);
            //     gameObject.AddComponent<PlayFabLogin>(); // Reset the singleton reference to allow for a new instance

            // }

            Button signInButton = GameObject.Find("SetUserButton").GetComponent<Button>();
            signInButton.onClick.AddListener(LoginWithUsername);

            LoadingUI = GameObject.Find("LoadUIHolder").GetComponent<LoadUIHolder>().LoaderUI;

            prompt = GameObject.Find("PromptText").GetComponent<TextMeshProUGUI>();

            userInput = GameObject.Find("InputField").GetComponent<TMP_InputField>();


        }
    }


    public void LoginWithUsername()
    {
        LoadingUI.SetActive(true);
        displayName = userInput.text;

        if (string.IsNullOrEmpty(displayName))
        {
            Debug.LogError("Display Name is required");
            LoadingUI.SetActive(false);
            return;
        }

        PerformLogin(displayName);
    }

    private void PerformLogin(string username)
    {
        sessionID = System.Guid.NewGuid().ToString(); // Generate a unique session ID

        var request = new LoginWithCustomIDRequest
        {
            CustomId = username, // Treat the displayName as the custom ID
            CreateAccount = true, // Automatically create an account if it doesn't exist
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserData = true // Request user data to check for existing sessions
            }
        };

        PlayFabClientAPI.LoginWithCustomID(request, result =>
        {
            Debug.Log("Login Successful");
            loginPlayfabID = result.PlayFabId;

            // Check if the username has already been marked as used
            if (result.InfoResultPayload.UserData != null && result.InfoResultPayload.UserData.ContainsKey("usernameUsed") && result.InfoResultPayload.UserData["usernameUsed"].Value == "true")
            {
                prompt.text = "Username has already been used. Login denied.";
                LogoutUser(); // Log the player out immediately
                return;
            }

            // Username is not marked as used, mark it as used now
            displayName = username;

            // Set the Display Name for this user
            var updateDisplayNameRequest = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = username
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(updateDisplayNameRequest, displayNameResult =>
            {
                Debug.Log("Display Name updated successfully");
                StartCoroutine(LoadNextScene(2.5f));
            },
            error =>
            {
                Debug.LogError("Failed to update Display Name: " + error.GenerateErrorReport());
                 prompt.text = "Username has already been used try a fresh name";
            });

        },
        OnLoginFailure);
    }

    private void MarkUsernameAsUsed(string username)
    {
        // Update the user data to mark the username as used
        var data = new Dictionary<string, string>
        {
            { "displayName", username },
            { "usernameUsed", "true" }
        };

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
        {
            Data = data
        }, result =>
        {
            Debug.Log("Username marked as used successfully.");
        }, error =>
        {
            Debug.LogError("Failed to mark username as used: " + error.GenerateErrorReport());
        });
    }

    public void LogoutUser()
    {
        PlayFabClientAPI.ForgetAllCredentials(); // Clear PlayFab credentials
                                                 // LoadingUI.SetActive(false); // Stop the loading UI
                                                 // You can also add any additional logic for user feedback, e.g., a message that the user is logged out
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.Log("Login failed: " + error.GenerateErrorReport());
        LoadingUI.SetActive(false); // Stop the loading UI
    }

    private void GetPlayerLeaderboardScore()
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = "Leaderboard",
            MaxResultsCount = 1, // We only need the player's own score
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, result =>
        {
            if (result.Leaderboard.Count >= 0)
            {
                var playerEntry = result.Leaderboard[0];
                int playerScore = playerEntry.StatValue;

                ScoreText.text = playerScore.ToString() + "/15";
            }
            else
            {
                Debug.LogWarning("No leaderboard entry found for this player.");
                ScoreText.text = "Score: 0"; // Default to 0 if no entry found
            }
        }, error =>
        {
            Debug.LogError("Failed to get leaderboard score: " + error.GenerateErrorReport());
            ScoreText.text = "Score: 0"; // Default to 0 in case of an error
        });
    }

    IEnumerator LoadNextScene(float t)
    {
        yield return new WaitForSeconds(t);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void SendLeaderboard(int score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>{
                new StatisticUpdate{
                    StatisticName = "Leaderboard",
                    Value = score
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("Error: " + error.GenerateErrorReport());
    }

    private void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Leaderboard Score Sent");
    }

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "Leaderboard",
            StartPosition = 0,
            MaxResultsCount = 10,
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
    }

    private void OnLeaderboardGet(GetLeaderboardResult result)
    {
        Debug.Log("Leaderboard Pulled");

        // Clear the leaderboard rows before populating with new data
        foreach (Transform child in rowsParent)
        {
            Destroy(child.gameObject);
        }

        // Loop through each leaderboard entry (already sorted in descending order by PlayFab)
        foreach (var item in result.Leaderboard)
        {
            // Store the current leaderboard item locally to avoid issues in the callback
            var leaderboardItem = item;

            // Fetch the Display Name for each PlayFabId
            PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest
            {
                PlayFabId = leaderboardItem.PlayFabId,
                ProfileConstraints = new PlayerProfileViewConstraints
                {
                    ShowDisplayName = true
                }
            }, userProfileResult =>
            {
                string playerDisplayName = userProfileResult.PlayerProfile.DisplayName ?? "Unknown User";

                // Instantiate a new row for the leaderboard entry
                GameObject newGO = Instantiate(rowPrefab, rowsParent);
                TextMeshProUGUI[] texts = newGO.GetComponentsInChildren<TextMeshProUGUI>();

                // Set leaderboard entry details
                texts[0].text = (leaderboardItem.Position + 1).ToString(); // Leaderboard position
                texts[1].text = playerDisplayName;                        // Display Name
                texts[2].text = leaderboardItem.StatValue.ToString();     // Player score

            }, error =>
            {
                Debug.LogError($"Failed to get Display Name for PlayFabId {leaderboardItem.PlayFabId}: {error.GenerateErrorReport()}");

                // Handle error by displaying default information
                GameObject newGO = Instantiate(rowPrefab, rowsParent);
                TextMeshProUGUI[] texts = newGO.GetComponentsInChildren<TextMeshProUGUI>();
                texts[0].text = (leaderboardItem.Position + 1).ToString(); // Leaderboard position
                texts[1].text = "Error";                                 // Default to "Error"
                texts[2].text = leaderboardItem.StatValue.ToString();   // Player score
            });
        }
    }


    public void GetLeaderboardAroundPlayer()
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = "Leaderboard",
            MaxResultsCount = 7,
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnLeaderboardAroundPlayerGet, OnError);
    }

    private void OnLeaderboardAroundPlayerGet(GetLeaderboardAroundPlayerResult result)
    {
        Debug.Log("Leaderboard Pulled");

        // Clear the leaderboard rows only once
        foreach (Transform child in rowsParent)
        {
            Destroy(child.gameObject);
        }

        // Loop through each leaderboard entry and fetch the Display Name
        foreach (var item in result.Leaderboard)
        {
            // Store the current leaderboard item locally to avoid issues in the callback
            var leaderboardItem = item;

            // Call GetPlayerProfileRequest for each PlayFabId to retrieve the Display Name
            PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest
            {
                PlayFabId = leaderboardItem.PlayFabId,
                ProfileConstraints = new PlayerProfileViewConstraints
                {
                    ShowDisplayName = true
                }
            }, userProfileResult =>
            {
                string playerDisplayName = userProfileResult.PlayerProfile.DisplayName ?? "Unknown User";

                // Populate leaderboard row with Display Name and score
                GameObject newGO = Instantiate(rowPrefab, rowsParent);
                TextMeshProUGUI[] texts = newGO.GetComponentsInChildren<TextMeshProUGUI>();

                // Set leaderboard entry details
                texts[0].text = (leaderboardItem.Position + 1).ToString(); // Leaderboard position
                texts[1].text = playerDisplayName;                        // Display Name
                texts[2].text = leaderboardItem.StatValue.ToString();     // Player score

                if (playerDisplayName == displayName)
                {
                    texts[0].color = Color.red;
                    texts[1].color = Color.red;
                    texts[2].color = Color.red;
                }

            }, error =>
            {
                Debug.LogError($"Failed to get Display Name for PlayFabId {leaderboardItem.PlayFabId}: {error.GenerateErrorReport()}");

                // Handle error
                GameObject newGO = Instantiate(rowPrefab, rowsParent);
                TextMeshProUGUI[] texts = newGO.GetComponentsInChildren<TextMeshProUGUI>();
                texts[0].text = (leaderboardItem.Position + 1).ToString(); // Leaderboard position
                texts[1].text = "Error";                                 // Default to "Error"
                texts[2].text = leaderboardItem.StatValue.ToString();   // Player score
            });
        }
    }
}
