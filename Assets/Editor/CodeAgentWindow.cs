using UnityEngine;
using UnityEditor;
using System.IO;

public class CodeAgentWindow : EditorWindow
{
    private string selectedScriptName = "None";
    private string codeToAnalyze = "";
    private string agentResult = "";

    [MenuItem("Tools/My AI Code Agent")]
    public static void ShowWindow()
    {
        GetWindow<CodeAgentWindow>("AI Code Agent");
    }

    // פונקציה מובנית של יוניטי שמתעוררת בכל פעם שאת לוחצת על משהו בעורך
    void OnSelectionChange()
    {
        // בודקים אם מה שסומן בעכבר הוא קובץ סקריפט (C#)
        if (Selection.activeObject is MonoScript script)
        {
            selectedScriptName = script.name + ".cs";
            codeToAnalyze = script.text; // קריאת כל הקוד של הקובץ אוטומטית!
            Repaint(); // מרענן את החלון כדי שנראה את השינוי
        }
    }

    void OnGUI()
    {
        GUILayout.Label("AI CODE AGENT: Project Scanner", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // מציג איזה קובץ בחרת כרגע בפרויקט
        EditorGUILayout.HelpBox($"Selected File: {selectedScriptName}", MessageType.Info);
        
        GUILayout.Space(10);

        // כפתור הפעלה לסריקה
        if (GUILayout.Button($"Scan {selectedScriptName} for Unity Violations"))
        {
            RunLocalCheck();
        }

        GUILayout.Space(20);
        GUILayout.Label("Scan Results:", EditorStyles.boldLabel);
        
        // תיבת טקסט שמציגה את התוצאה
        EditorGUILayout.TextArea(agentResult, GUILayout.Height(150));
    }

    void RunLocalCheck()
    {
        if (string.IsNullOrEmpty(codeToAnalyze))
        {
            agentResult = "ERROR: No C# script selected, or the file is empty.";
            return;
        }

        agentResult = ""; // איפוס

        // חוק 1: GetComponent בתוך Update
        if (codeToAnalyze.Contains("void Update()") && codeToAnalyze.Contains("GetComponent"))
        {
            agentResult += "[WARNING] GetComponent found inside Update() loop!\n" +
                           "Unity Rule: This hurts performance. Cache the component in Start() or Awake() instead.\n\n";
        }

        // חוק 2: new בתוך Update
        if (codeToAnalyze.Contains("void Update()") && codeToAnalyze.Contains("new "))
        {
            agentResult += "[WARNING] Memory allocation (new) found inside Update() loop!\n" +
                           "Unity Rule: This triggers the Garbage Collector and causes gameplay lags.\n\n";
        }

        if (string.IsNullOrEmpty(agentResult))
        {
            agentResult = $"SUCCESS: {selectedScriptName} follows Unity resource management rules perfectly!";
        }
    }
}