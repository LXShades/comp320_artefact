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
    private const int numFrameTimestamps = 60;
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

        fpsCounter.text = "FPS: " + (1 / (totalFrameTimes / frameTimestamps.Length)).ToString("0.0");

        frameTimestamps[frameTimestampIndex] = Time.deltaTime;
        frameTimestampIndex = (frameTimestampIndex + 1) % numFrameTimestamps;
    }
}
