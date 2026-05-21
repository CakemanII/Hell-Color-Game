using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

enum CheatCode
{
    SetPlayerHealth,
    SpawnCollectable,
    SpawnEnemy,
    StartRound,
    EndRound,
    NextLevel,
    Quicksilver
}

public class CheatsManager : MonoBehaviour
{    
    public static CheatsManager Instance { get; private set; }
    private List<string> commandLines = new List<string>();

    private UnityEvent onCommandLinesChange = new UnityEvent();


    private void Awake()
    {
        // Ensure singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        Instance = this;

        
    }

    public void CheatInput(string input)
    {
        // Trim and lower
        string newInput = input.Trim().ToLower();

        // Remove any spacing more than 2+ between words
        while (newInput.Contains("  "))
        {
            newInput = newInput.Replace("  ", " ");
        }

        // Append the input to the command lines
        AppendInputToCMD(newInput);

        // Check if the command exists
        CheatCode? code = GetCodeFromCommandLine(newInput);

        // If the code exists, execute it
        if (code != null)
        {
            List<string> parameters = new List<string>(newInput.Split(' '));
            parameters.RemoveAt(0); // Remove the command itself

            ExecuteCommand((CheatCode)code, parameters);
        }
        else
        {
            // Invalid Command
            commandLines.Add($"Command: '{input.Split(' ')[0]}' is not a valid command...");
        }
    }

    private CheatCode? GetCodeFromCommandLine(string input)
    {
        string[] strings = input.Split(' ');

        switch (strings[0].ToLower())
        {
            case "set_health":
                return CheatCode.SetPlayerHealth;
            case "spawn_collectable":
                return CheatCode.SpawnCollectable;
            case "spawn_enemy":
                return CheatCode.SpawnEnemy;
            case "start_round":
                return CheatCode.StartRound;
            case "end_round":
                return CheatCode.EndRound;
            case "next_level":
                return CheatCode.NextLevel;
            case "quicksilver":
                return CheatCode.Quicksilver;
            default:
                return null;
        }
    }

    private void AppendInputToCMD(string input)
    {
        commandLines.Add(input);
        onCommandLinesChange.Invoke();
    }

    public void SubscribeToCommandLineChange(UnityAction listener)
    { onCommandLinesChange.AddListener(listener); }
    public void UnsubscribeToCommandLineChange(UnityAction listener)
    { onCommandLinesChange.RemoveListener(listener); }

    public string GetMostRecentLine()
    {
        return commandLines[commandLines.Count - 1];
    }

    private void ExecuteCommand(CheatCode code, List<string> parameters)
    {
        switch (code)
        {
            case CheatCode.SetPlayerHealth:
                EntityHealth health = GameManager.Instance.Player.GetComponent<EntityHealth>();
                List<object> parsedParameters = ParseParameters(parameters, new List<Type> { typeof(float) });
                if (parsedParameters != null)
                {
                    health.SetHealth((float)parsedParameters[0]);
                    health.SetMaxHealth(parsedParameters.Count > 1 ? (float)parsedParameters[1] : (float)parsedParameters[0]);
                }
                else
                    commandLines.Add($"Failed to execute command: Invalid parameters for '{code}'");
                break;

            case CheatCode.SpawnCollectable:
                break;

            case CheatCode.SpawnEnemy:
                break;

            case CheatCode.StartRound:
                break;

            case CheatCode.EndRound:
                break;

            case CheatCode.NextLevel:
                break;

            case CheatCode.Quicksilver:
                break;

            default:
                commandLines.Add("An unexpected error occured...");
                break;
        }
    }

    private List<object> ParseParameters(List<string> parameters, List<Type> types)
    {
        List<object> parsedParameters = new List<object>();

        for (int i = 0; i < parameters.Count; i++)
        {
            string parameter = parameters[i];
            Type type = types[i];

            if (type == typeof(int))
            {
                if (int.TryParse(parameter, out int intValue))
                {
                    parsedParameters.Add(intValue);
                }
                else
                {
                    Debug.LogError($"Parameter '{parameter}' is not a valid integer.");
                    return null;
                }
            }
            else if (type == typeof(float))
            {
                if (float.TryParse(parameter, out float floatValue))
                {
                    parsedParameters.Add(floatValue);
                }
                else
                {
                    Debug.LogError($"Parameter '{parameter}' is not a valid float.");
                    return null;
                }
            }
            else if (type == typeof(string))
            {
                parsedParameters.Add(parameter);
            }
            else
            {
                Debug.LogError($"Unsupported parameter type: {type}");
                return null;
            }
        }

        return parsedParameters;
    }
}
