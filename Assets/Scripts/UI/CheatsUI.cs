using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CheatsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private TMP_InputField inputField;

    [SerializeField] private int mostLines = 10;

    private readonly List<string> currentlyDisplayingLines = new List<string>();

    private void Awake()
    {
        CheatsManager.Instance.SubscribeToCommandLineChange(OnCommandLinesChange);
        RefreshDisplay();
    }

    public void EnterCommand()
    {
        string input = inputField.text;
        CheatsManager.Instance.CheatInput(input);
    }

    private void OnCommandLinesChange()
    {
        string mostRecentLine = CheatsManager.Instance.GetMostRecentLine();

        if (string.IsNullOrEmpty(mostRecentLine))
            return;

        AddLine(mostRecentLine);
        RefreshDisplay();
    }

    private void AddLine(string line)
    {
        currentlyDisplayingLines.Add(line);

        // Keep only the most recent X lines
        if (currentlyDisplayingLines.Count > mostLines)
        {
            currentlyDisplayingLines.RemoveAt(0);
        }
    }

    private void RefreshDisplay()
    {
        outputText.text = string.Join("\n", currentlyDisplayingLines);
    }
}