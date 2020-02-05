using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global game manager handling player scores and key object references (camera, player, etc)
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Retrieves the Game Manager
    /// </summary>
    public static GameManager singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = GameObject.FindObjectOfType<GameManager>();

                if (_singleton == null)
                {
                    _singleton = new GameObject("_GameManager_", typeof(GameManager)).GetComponent<GameManager>();
                }

                DontDestroyOnLoad(_singleton);
            }

            return _singleton;
        }
    }
    private static GameManager _singleton;

    /// <summary>
    /// Total number of balloons in this scene
    /// </summary>
    public int numTotalBalloons;

    /// <summary>
    /// Number of balloons that have been popped by the player
    /// </summary>
    public int numPoppedBalloons;

    public bool isPaused
    {
        get
        {
            return (timeRemaining == 0);
        }
    }

    /// <summary>
    /// The recorded data file
    /// </summary>
    public DataFile data
    {
        get
        {
            if (_data == null)
            {
                _data = new DataFile("data.csv");
            }

            return _data;
        }
    }
    private DataFile _data;

    public float timeRemaining
    {
        get
        {
            return Mathf.Max(levelTimeLimit - (Time.time - levelStartTime), 0);
        }
    }

    // Retrieves the name (letter) of the current impostor configuration
    public string impostorConfigurationName
    {
        get
        {
            return impostorConfigurations[impostorConfigurationIndex];
        }
    }

    // The current impostor configuration
    public int impostorConfigurationIndex = 0;

    public string[] impostorConfigurations = new string[] { "A", "B", "C", "D" };

    // Duration of each game round
    public float levelTimeLimit = 120;

    // Time.time when the level was started
    private float levelStartTime;

    // Start is called before the first frame update
    void Start()
    {
        // Register the scene load callback
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneLoad;
    }

    /// <summary>
    /// Continues to the next round or the end screen
    /// </summary>
    public void StartNextRound()
    {
        impostorConfigurationIndex = (impostorConfigurationIndex + 1) % impostorConfigurations.Length;

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Resets key gameplay variables upon scene change
    /// </summary>
    void OnSceneLoad(UnityEngine.SceneManagement.Scene last, UnityEngine.SceneManagement.Scene next)
    {
        numTotalBalloons = 0;
        numPoppedBalloons = 0;

        levelStartTime = Time.time;
    }
}
