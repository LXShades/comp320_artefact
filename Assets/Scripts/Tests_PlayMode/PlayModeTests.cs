using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PlayModeTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void PlayModeTestsSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        /// <summary>
        /// Formatting tests for data files
        /// </summary>
        [UnityTest]
        public IEnumerator TestCSVDataFile()
        {
            string testFilePath = $"{Application.dataPath}/CSVTest.csv";

            // Test data files
            DataFile file = new DataFile();

            // Fill with basic data
            file.sessionData["impostorsA"] = "didn't notice";
            file.sessionData["impostorsB"] = "kinda noticed";
            file.sessionData["impostorsC"] = "pretty obvious mate";
            file.sessionData["impostorsD"] = "missed it like it was the battle of britain";

            // Ensure file format matches the data inserted
            Assert.AreEqual(file.GenerateDataFormat(), new string[] { "impostorsA", "impostorsB", "impostorsC", "impostorsD" });

            // Try writing the file as new
            System.IO.File.Delete(testFilePath);
            file.WriteToFile(testFilePath);

            // Check the file format still matches
            Assert.AreEqual(file.ReadDataFormat(testFilePath), new string[] { "impostorsA", "impostorsB", "impostorsC", "impostorsD" });

            // Attempt to continue writing to the file (writing additional lines)
            file.WriteToFile(testFilePath);

            // Ensure the format still matches
            Assert.AreEqual(file.ReadDataFormat(testFilePath), new string[] { "impostorsA", "impostorsB", "impostorsC", "impostorsD" });

            // Ensure the file data matches
            string fileText = System.IO.File.ReadAllText(testFilePath);

            Assert.AreEqual(fileText, "impostorsA,impostorsB,impostorsC,impostorsD\r\ndidn't notice,kinda noticed,pretty obvious mate,missed it like it was the battle of britain\r\ndidn't notice,kinda noticed,pretty obvious mate,missed it like it was the battle of britain\r\n");

            // Test partial writing of data
            file.sessionData.Remove("impostorsA");

            Assert.AreEqual(file.WriteToFile(testFilePath, "blank"), true);

            fileText = System.IO.File.ReadAllText(testFilePath);
            Assert.AreEqual(fileText, "impostorsA,impostorsB,impostorsC,impostorsD\r\ndidn't notice,kinda noticed,pretty obvious mate,missed it like it was the battle of britain\r\ndidn't notice,kinda noticed,pretty obvious mate,missed it like it was the battle of britain\r\nblank,kinda noticed,pretty obvious mate,missed it like it was the battle of britain\r\n");

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
