﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Maintains the file containing the recorded gameplay data
/// </summary>
public class DataFile
{
    // Variable-value associative data
    public Dictionary<string, string> sessionData = new Dictionary<string, string>();

    // The name of the file to be used
    public string filename;

    public DataFile(string filename)
    {
        this.filename = filename;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    /// <summary>
    /// Attempts to write to a data file and returns whether successful
    /// If this class contains data not available by the existing file's format, this function fails
    /// The file is created if it does not exist
    /// </summary>
    public bool WriteToFile()
    {
        // Read the current file, if one exists
        string[] format = null;
        bool isNewFile = true;

        if (File.Exists(filename))
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                if (reader.BaseStream.Length > 0)
                {
                    format = GetDataFormat(filename);

                    // Verify that each of our keys is in the format
                    foreach (KeyValuePair<string, string> key in sessionData)
                    {
                        if (!System.Array.Exists<string>(format, item => item == key.Key))
                        {
                            Debug.LogWarning($"WriteToFile failed: Incompatible format (key: {key.Key})");
                            return false;
                        }
                    }

                    isNewFile = false;
                }
            }
        }

        // Generate the format if it doesn't already exist in the file
        if (isNewFile)
        {
            format = CreateDataFormat();
        }

        // Write to the file
        using (StreamWriter writer = new StreamWriter(filename, true))
        {
            if (isNewFile)
            {
                // Write the format to the file first
                for (int i = 0; i < format.Length; i++)
                {
                    if (i < format.Length - 1)
                    {
                        writer.Write($"{format[i]},");
                    }
                    else
                    {
                        writer.WriteLine(format[i]);
                    }
                }
            }

            // Write the values to the file
            for (int i = 0; i < format.Length; i++)
            {
                string data = sessionData.ContainsKey(format[i]) ? sessionData[format[i]] : "0";
                if (i < format.Length - 1)
                {
                    writer.Write($"{data},");
                }
                else
                {
                    writer.WriteLine(data);
                }
            }
        }

        Debug.Log("File successfully written");
        return true;
    }

    /// <summary>
    /// Reads the data format in a csv file
    /// </summary>
    public string[] GetDataFormat(string filename)
    {
        using (StreamReader reader = new StreamReader(filename))
        {
            string dataLine = reader.ReadLine();

            if (dataLine.Length > 0)
            {
                return dataLine.Split(',', '\n', '\r');
            }
        }

        Debug.Log($"Error: Comma-separated data line not found in {filename}");
        return null;
    }

    /// <summary>
    /// Generates the data format (column headers)
    /// </summary>
    public string[] CreateDataFormat()
    {
        int index = 0;
        string[] format = new string[sessionData.Count];

        foreach (KeyValuePair<string, string> item in sessionData)
        {
            format[index++] = item.Key;
        }

        System.Array.Sort(format);
        return format;
    }
}