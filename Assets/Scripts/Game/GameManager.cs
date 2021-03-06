﻿using System.Collections;
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
    /// Whether this is a development/debug build. Used to supply debug info.
    /// </summary>
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static bool isDebugBuild = true;
#else
    public static bool isDebugBuild = false;
#endif

    /// <summary>
    /// Retrieves a reference to the player
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

    [Tooltip("Game-defined impostor configurations")]
    public ImpostorConfiguration[] impostorConfigurations = new ImpostorConfiguration[0];

    [Tooltip("Duration of each game round")]
    public float levelTimeLimit = 120;

    [Tooltip("Name of the .csv data file")]
    public string dataName = "participantData.csv";

    [Tooltip("Name of the main scene")]
    public string levelName = "SunTemple";

    /// <summary>
    /// Number of seconds remaining until time runs out
    /// </summary>
    public float timeRemaining
    {
        get
        {
            return Mathf.Max(levelTimeLimit - (Time.time - levelStartTime), 0);
        }
    }

    /// <summary>
    /// Retrieves the symbol (letter identifier) of the current impostor configuration
    /// </summary>
    public string activeImpostorConfigurationSymbol
    {
        get
        {
            return ((System.Char)('A' + activeImpostorConfiguration)).ToString();
        }
    }

    /// <summary>
    /// Retrieves the full name of the current impostor configuration (for debugging)
    /// </summary>
    public string activeImpostorConfigurationName
    {
        get
        {
            return impostorConfigurations[activeImpostorConfiguration].name;
        }
    }

    /// <summary>
    /// Retrieves the index of the currently active impostor configuration
    /// </summary>
    public int activeImpostorConfiguration
    {
        get
        {
            return currentRound < numRounds ? impostorConfigByRound[currentRound] : 0;
        }
    }

    /// <summary>
    /// The current round index. This is independent to impostor configuration index as the latter is picked in random order.
    /// </summary>
    [HideInInspector] public int currentRound = 0;

    /// <summary>
    /// Returns the total number of rounds - equivalent to the length of the impostor configuration-by-round sequence
    /// </summary>
    public int numRounds
    {
        get
        {
            return impostorConfigByRound.Length;
        }
    }

    /// <summary>
    /// The impostor indexes to go through each round, randomised
    /// </summary>
    [HideInInspector] public int[] impostorConfigByRound = new int[0];

    /// <summary>
    /// Returns whether the game is running. Currently this is only true when time is up
    /// </summary>
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
    /// Total number of balloons seen in this round
    /// </summary>
    [HideInInspector] public int numTotalBalloons;

    /// <summary>
    /// Number of balloons that have been popped by the player
    /// </summary>
    [HideInInspector] public int numPoppedBalloons;

    /// <summary>
    /// List of lifetimes of balloons when popped
    /// </summary>
    [HideInInspector] public List<float> balloonPopLifetimes = new List<float>();

    /// <summary>
    /// Whether the timer has started tickintg
    /// </summary>
    [HideInInspector] public bool hasTimerStarted;

    /// <summary>
    /// Tracks frame rate and stuff
    /// </summary>
    [HideInInspector] public FpsSampler fpsSampler = new FpsSampler();

    /// <summary>
    /// Time.time when the level was started
    /// </summary>
    private float levelStartTime;

    /// <summary>
    /// Sets up the random impostor configuration order
    /// </summary>
    void Start()
    {
        // only allow one instance
        if (singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        // Register the scene load callback
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneLoad;

        // Don't destroy this object when we change scenes
        DontDestroyOnLoad(this);

        // Allow immediate testing in the SunTemple scnee
#if UNITY_EDITOR
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == levelName)
        {
            StartFirstRound();
        }
#endif

        // Call the missed OnSceneLoad
        OnSceneLoad(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    /// <summary>
    /// Provides some debug functionality
    /// </summary>
    private void Update()
    {
        if (!hasTimerStarted)
        {
            levelStartTime = Time.time;
        }

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

        // Update FPS tracking
        fpsSampler.Update();
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

            Debug.Log($"StartSequence (first: {impostorConfigByRound[0]})");

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

        data.sessionData[$"fpsAvg{activeImpostorConfigurationSymbol}"] = fpsSampler.GetAverageFps().ToString();
        data.sessionData[$"fpsLow{activeImpostorConfigurationSymbol}"] = fpsSampler.GetFpsAtPercentile(1f).ToString();
        data.sessionData[$"fpsMed{activeImpostorConfigurationSymbol}"] = fpsSampler.GetFpsAtPercentile(50f).ToString();
        data.sessionData[$"fpsHigh{activeImpostorConfigurationSymbol}"] = fpsSampler.GetFpsAtPercentile(99f).ToString();
        data.sessionData[$"travelled{activeImpostorConfigurationSymbol}"] = player.distanceTravelled.ToString();
        data.sessionData[$"shotsFired{activeImpostorConfigurationSymbol}"] = player.slingshot.numShotsFired.ToString();
        data.sessionData[$"balloonLifetime{activeImpostorConfigurationSymbol}"] = (totalLifetime / balloonPopLifetimes.Count).ToString();
        data.sessionData[$"balloonsPopped{activeImpostorConfigurationSymbol}"] = numPoppedBalloons.ToString();
        data.sessionData[$"balloonsSeen{activeImpostorConfigurationSymbol}"] = numTotalBalloons.ToString();

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
        Debug.Log($"Setting impostor config to {configurationIndex}/{impostorConfigurations[configurationIndex].name}");

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

        hasTimerStarted = false;

        levelStartTime = Time.time;

        if (activeImpostorConfiguration < impostorConfigurations.Length)
        {
            // Initialise ImpMan impostor configuration
            SetImpostorConfiguration(activeImpostorConfiguration);
        }
    }
}
