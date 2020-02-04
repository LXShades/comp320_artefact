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

    public float timeRemaining
    {
        get
        {
            return 120;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneLoad;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Resets key gameplay variables upon scene change
    /// </summary>
    void OnSceneLoad(UnityEngine.SceneManagement.Scene last, UnityEngine.SceneManagement.Scene next)
    {
        numTotalBalloons = 0;
        numPoppedBalloons = 0;
    }
}
