using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI for impostor and performance debugging
/// </summary>
public class DebugUI : MonoBehaviour
{
    /// <summary>
    /// The FPS counter text
    /// </summary>
    public Text fpsCounter;

    /// <summary>
    /// The debug texture images in the UI hiearchy
    /// </summary>
    public Image[] debugTextures;

    /// <summary>
    /// A list of debug toggles used for wow
    /// </summary>
    public Text debugToggles;

    // Frame logging is done in a circular array
    /// <summary>
    /// Current frame timestamp index (where the processing time of the next frame will be recorded)
    /// </summary>
    private int frameTimeIndex = 0;

    /// <summary>
    /// Maximum number of frame timestamps in the circular array
    /// </summary>
    private const int numFrameTimes = 60;
    
    /// <summary>
    /// Circular array of frame timestamps
    /// </summary>
    private float[] frameTimes = new float[numFrameTimes];

    private float lastRealTime;

    void Awake()
    {
        lastRealTime = Time.realtimeSinceStartup;
    }

    void Update()
    {
        // Refresh the impostor texture display
        for (int i = 0; i < ImpMan.singleton.impostorTextures.Count && i < debugTextures.Length; i++)
        {
            debugTextures[i].material.mainTexture = ImpMan.singleton.impostorTextures[i];
        }

        // Refresh the frame rate 
        // Count total frame times to get the average
        float totalFrameTimes = 0;

        foreach (float f in frameTimes)
        {
            totalFrameTimes += f;
        }

        if (frameTimes.Length > 0)
        {
            // Sort the timestamps so we can get a lower quartile, median and upper quartile amount
            float[] sortedTimestamps = new float[numFrameTimes];

            System.Array.Copy(frameTimes, sortedTimestamps, numFrameTimes);
            System.Array.Sort(sortedTimestamps);
            
            fpsCounter.text =
                $"FPS: {(1 / (totalFrameTimes / frameTimes.Length)).ToString("0000.0")}\n" +
                $"Low: {(1 / sortedTimestamps[sortedTimestamps.Length - 1]).ToString("0000.0")}\n" +
                $"Med: {(1 / sortedTimestamps[(int)(sortedTimestamps.Length * 0.5f)]).ToString("0000.0")}\n" + 
                $"High: {(1 / sortedTimestamps[0]).ToString("0000.0")}\n";
        }

        // Record this frame into the array
        frameTimes[frameTimeIndex] = Time.realtimeSinceStartup - lastRealTime;
        frameTimeIndex = (frameTimeIndex + 1) % numFrameTimes;
        lastRealTime = Time.realtimeSinceStartup;

        // Show debug toggles
        debugToggles.text = "";
        debugToggles.text += $"EnableImpostors (Space): {ImpMan.singleton.enableImpostors}\n";
        debugToggles.text += $"ActivateCam (C): {ImpMan.singleton.activateImpostorCamera}\n";
        debugToggles.text += $"FreezeImpostors (F): {ImpMan.singleton.freezeImpostors}\n";
        debugToggles.text += $"Backgrounds (B): Unknown, toggle them\n";
        debugToggles.text += $"Impostor config (+/-): {GameManager.singleton.impostorConfigurationName}";

        // Process debug toggles
        //ImpMan.singleton.enableImpostors ^= Input.GetKeyDown(KeyCode.Space);
        ImpMan.singleton.activateImpostorCamera ^= Input.GetKeyDown(KeyCode.C);
        ImpMan.singleton.freezeImpostors ^= Input.GetKeyDown(KeyCode.F);

        if (Input.GetKeyDown(KeyCode.B))
        {
            foreach (ImpostorLayer layer in ImpMan.singleton.impostorLayers)
            {
                layer.debugFillBackground = !layer.debugFillBackground;
            }
        }
    }
}