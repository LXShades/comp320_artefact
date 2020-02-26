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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static bool isDebugBuild = true;
#else
    public static bool isDebugBuild = false;
#endif

    /// <summary>
    /// Retrieves the player
    /// </summary>
    public Player player
    {
        get
        {
            if (_player == null)
            {
                _player = FindObjectOfType<Player>();
            }

            return _player;
        }
    }
    private Player _player;

    /// <summary>
    /// Total number of balloons in this scene
    /// </summary>
    public int numTotalBalloons;

    /// <summary>
    /// Number of balloons that have been popped by the player
    /// </summary>
    public int numPoppedBalloons;

    // List of lifetimes of balloons when popped
    public List<float> balloonPopLifetimes = new List<float>();

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

    /// <summary>
    /// Tracks frame rate and stuff
    /// </summary>
    public FpsSampler fpsSampler = new FpsSampler();

    // File name of the data file
    public string dataName = "participantData.csv";

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
            return ((System.Char)('A' + activeImpostorConfiguration)).ToString();
        }
    }

    // The currently active impostor configuration
    public int activeImpostorConfiguration
    {
        get
        {
            return currentRound < numRounds ? impostorConfigByRound[currentRound] : 0;
        }
    }

    // The current index in the random impostor configuration list
    public int currentRound = 0;

    // Returns the total number of rounds - equivalent to the length of the impostor configuration-by-round sequence
    public int numRounds
    {
        get
        {
            return impostorConfigByRound.Length;
        }
    }

    // The impostor indexes to go through each round, randomised
    public int[] impostorConfigByRound = new int[0];

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
    void Start()
    {
        // Register the scene load callback
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneLoad;

        // Don't destroy this object when we change scenes
        DontDestroyOnLoad(this);

        // Call the missed OnSceneLoad
        OnSceneLoad(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        // Allow immediate testing in the SunTemple scnee
#if UNITY_EDITOR
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == levelName)
        {
            StartSequence();
        }
#endif
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// Provides some debug functionality
    /// </summary>
    private void Update()
    {
        if (isDebugBuild)
        {
            // Take screenshots
            if (Input.GetKeyDown(KeyCode.F12))
            {
                ScreenCapture.CaptureScreenshot($"Screenshot {System.DateTime.Now.ToLongTimeString().Replace(":", "-")}.png");
            }

            // Restart level
            if (Input.GetKeyDown(KeyCode.ScrollLock))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }

            // Changes impostor configuration
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                Debug.Log("Changing impostor config");

                impostorConfigByRound[currentRound] = (impostorConfigByRound[currentRound] + 1) % impostorConfigurations.Length;
                SetImpostorConfiguration(impostorConfigByRound[currentRound]);
            }

            if (Input.GetKeyDown(KeyCode.Minus))
            {
                Debug.Log("Changing impostor config");

                impostorConfigByRound[currentRound] = ((impostorConfigByRound[currentRound] - 1) + impostorConfigurations.Length) % impostorConfigurations.Length;
                SetImpostorConfiguration(impostorConfigByRound[currentRound]);
            }
        }
    }
#endif

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
            impostorConfigByRound = new int[numConfigurations];

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

                impostorConfigByRound[configIndex] = smallestValueIndex;
                randomValues[smallestValueIndex] = float.MaxValue;
            }

            currentRound = 0;
        }
        else
        {
            currentRound = 0;
        }
    }

    /// <summary>
    /// Begins the first round and teleports to main level
    /// </summary>
    public void StartFirstRound()
    {
        StartSequence();

        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Continues to the next round or the end screen
    /// </summary>
    public void StartNextRound()
    {
        // Record uncollated data
        float totalLifetime = 0;
        balloonPopLifetimes.ForEach(b => totalLifetime += b);

        data.sessionData[$"fps{impostorConfigurationName}"] = fpsSampler.GetAverageFps().ToString();
        data.sessionData[$"travelled{impostorConfigurationName}"] = player.distanceTravelled.ToString();
        data.sessionData[$"balloonLifetime{impostorConfigurationName}"] = (totalLifetime / balloonPopLifetimes.Count).ToString();
        data.sessionData[$"balloonsPopped{impostorConfigurationName}"] = numPoppedBalloons.ToString();
        data.sessionData[$"balloonsSeen{impostorConfigurationName}"] = numTotalBalloons.ToString();

        if (currentRound + 1 < impostorConfigByRound.Length)
        {
            // Use next impostor configuration in the sequence
            currentRound++;

            // Load/reload main level
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
        else
        {
            // Save the data!
            data.WriteToFile($"{Application.dataPath}/{dataName}");

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

        balloonPopLifetimes.Clear();
        fpsSampler.Reset();

        levelStartTime = Time.time;

        if (activeImpostorConfiguration < impostorConfigurations.Length)
        {
            // Initialise ImpMan impostor configuration
            SetImpostorConfiguration(activeImpostorConfiguration);
        }
    }
}
