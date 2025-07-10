using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CostLogger : MonoBehaviour
{
    private StringBuilder csvContent = new StringBuilder();
    private string filePath;

    void Awake()
    {
        filePath = Path.Combine(Application.dataPath, "cost_log.csv");
        //csvContent.AppendLine("Iteration,Total,Clearance,Circulation,PairwiseDistance,PairwiseAngle,ConversationDistance,ConversationAngle,Balance,Alignment,WallAlignment,Symmetry,Emphasis");
        csvContent.AppendLine("Iteration,Cost,Acceptance Probability");
        File.WriteAllText(filePath, csvContent.ToString());  // initialize
    }

    public void Log(int iteration, Dictionary<string, float> values)
    {
        // string line = $"{iteration},{values["Total"]},{values["Clearance"]},{values["Circulation"]},{values["PairwiseDistance"]},{values["PairwiseAngle"]},{values["ConversationDistance"]},{values["ConversationAngle"]},{values["Balance"]},{values["Alignment"]},{values["WallAlignment"]},{values["Symmetry"]},{values["Emphasis"]}";
        // File.AppendAllText(filePath, line + "\n");
        
        string line = $"{iteration},{values["Cost"]},{values["Acceptance Probability"]}";
        File.AppendAllText(filePath, line + "\n");



    }
}

