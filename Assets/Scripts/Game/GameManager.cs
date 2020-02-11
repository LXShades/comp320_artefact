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
                _data = new DataFile();
            }

            return _data;
        }
    }
    private DataFile _data;

    // File name of the data file
    public string dataName = "data.csv";

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
            return ((System.Char)('A' + impostorConfigurationIndex)).ToString();
        }
    }

    // The current impostor configuration
    public int impostorConfigurationIndex = 0;

    // The current index in the random impostor configuration list
    public int currentRound = 0;

    // Returns the total number of rounds - equivalent to the length of the impostor configuration-by-round sequence
    public int numRounds
    {
        get
        {
            return impostorConfigurationRounds.Length;
        }
    }

    // The impostor indexes to go through each round, randomised
    public int[] impostorConfigurationRounds = new int[0];

    // Game-defined impostor configurations
    public ImpostorConfiguration[] impostorConfigurations = new ImpostorConfiguration[0];

    // Duration of each game round
    public float levelTimeLimit = 120;

    // Time.time when the level was started
    private float levelStartTime;

    // Name of the main level
    public string levelName = "SunTemple";

    /// <summary>
    /// Sets up the random impostor configuration order
    /// </summary>
    void Awake()
    {
        StartSequence();
    }

    void Start()
    {
        // Register the scene load callback
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneLoad;

        // Don't destory this object when we change scenes
        DontDestroyOnLoad(this);
    }

    /// <summary>
    /// Generates and starts the impostor sequence from the list of configurations provided
    /// </summary>
    public void StartSequence()
    {
        // Randomise the impostor configuration order using the following strategy
        // Make a list of random values
        int numConfigurations = impostorConfigurations.Length;

        if (numConfigurations > 0)
        {
            float[] randomValues = new float[numConfigurations];

            for (int i = 0; i < numConfigurations; i++)
            {
                randomValues[i] = Random.value;
            }

            // Add the impostor configurations in the same order as these values
            impostorConfigurationRounds = new int[numConfigurations];

            for (int configIndex = 0; configIndex < numConfigurations; configIndex++)
            {
                float smallestValue = 1.0f;
                int smallestValueIndex = -1;

                for (int i = 0; i < randomValues.Length; i++)
                {
                    if (randomValues[i] < smallestValue)
                    {
                        smallestValueIndex = i;
                        smallestValue = randomValues[i];
                    }
                }

                impostorConfigurationRounds[configIndex] = smallestValueIndex;
                randomValues[smallestValueIndex] = float.MaxValue;
            }

            impostorConfigurationIndex = impostorConfigurationRounds[0];
            currentRound = 0;
        }
        else
        {
            impostorConfigurationIndex = 0;
            currentRound = 0;
        }
    }

    /// <summary>
    /// Continues to the next round or the end screen
    /// </summary>
    public void StartNextRound()
    {
        if (currentRound + 1 < impostorConfigurationRounds.Length)
        {
            // Use next impostor configuration in the sequence
            currentRound++;
            impostorConfigurationIndex = impostorConfigurationRounds[currentRound];

            // Load/reload main level
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
        else
        {
            // Load the end screen
            UnityEngine.SceneManagement.SceneManager.LoadScene(2);
        }
    }

    /// <summary>
    /// Sets the currently active ImpMan impostor configuration to the one specified in configurationIndex
    /// </summary>
    public void SetImpostorConfiguration(int configurationIndex)
    {
        ImpMan.singleton.SetConfiguration(impostorConfigurations[configurationIndex]);
    }

    /// <summary>
    /// Resets key gameplay variables upon scene change
    /// </summary>
    void OnSceneLoad(UnityEngine.SceneManagement.Scene last, UnityEngine.SceneManagement.Scene next)
    {
        numTotalBalloons = 0;
        numPoppedBalloons = 0;

        levelStartTime = Time.time;

        if (impostorConfigurationIndex < impostorConfigurations.Length)
        {
            // Initialise impman impostor configuration
            SetImpostorConfiguration(impostorConfigurationIndex);
        }
    }
}
