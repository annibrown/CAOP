using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CostLogger : MonoBehaviour
{
    private StringBuilder csvContent = new StringBuilder();
    private string filePath;
    private bool headerWritten = false;

    void Awake()
    {
        filePath = Path.Combine(Application.dataPath, "cost_log.csv");

        // This clears the file initially (optional)
        File.WriteAllText(filePath, string.Empty);
    }

    public void Log(int iteration, Dictionary<string, float> values)
    {
        // If header hasn't been written yet, do it now
        if (!headerWritten)
        {
            List<string> headers = new List<string>(values.Keys);
            csvContent.AppendLine(string.Join(",", headers));
            headerWritten = true;
        }

        // Convert values to CSV line
        List<string> lineValues = new List<string>();
        foreach (var key in values.Keys)
        {
            lineValues.Add(values[key].ToString("F6"));  // 6 decimal places
        }

        csvContent.AppendLine(string.Join(",", lineValues));

        // Write to file after each entry
        File.WriteAllText(filePath, csvContent.ToString());
    }
}

