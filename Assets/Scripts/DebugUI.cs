using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    /** The FPS counter text */
    public Text fpsCounter;

    /** The debug texture images in the UI hiearchy */
    public Image[] debugTextures;

    /** Circular array for frame rate logging */
    private const int numFrameTimestamps = 10;
    private int frameTimestampIndex = 0;
    private float[] frameTimestamps = new float[numFrameTimestamps];

    // Update is called once per frame
    void Update()
    {
        // Refresh the impostor texture display
        for (int i = 0; i < ImpMan.singleton.impostorTextures.Count && i < debugTextures.Length; i++)
        {
            debugTextures[i].material.mainTexture = ImpMan.singleton.impostorTextures[i];
        }

        // Refresh the frame rate counter
        float totalFrameTimes = 0;

        foreach (float f in frameTimestamps)
        {
            totalFrameTimes += f;
        }

        float[] sortedTimestamps = new float[numFrameTimestamps];

        if (frameTimestamps.Length > 0)
        {
            System.Array.Copy(frameTimestamps, sortedTimestamps, numFrameTimestamps);
            System.Array.Sort(sortedTimestamps);
            
            fpsCounter.text =
                $"FPS: {(1 / (totalFrameTimes / frameTimestamps.Length)).ToString("0000.0")}" +
                $" (Low: {(1 / sortedTimestamps[sortedTimestamps.Length - 1]).ToString("0000.0")})" +
                $" (Med: {(1 / sortedTimestamps[(int)(sortedTimestamps.Length * 0.5f)]).ToString("0000.0")})" + 
                $" (High: {(1 / sortedTimestamps[0]).ToString("0000.0")})";
        }

        frameTimestamps[frameTimestampIndex] = Time.deltaTime;
        frameTimestampIndex = (frameTimestampIndex + 1) % numFrameTimestamps;
    }
}
