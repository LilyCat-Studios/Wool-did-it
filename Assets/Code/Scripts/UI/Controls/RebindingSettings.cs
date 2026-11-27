using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class RebindingSettings : MonoBehaviour
{
    private const string PrefsKey = "InputBindings";

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string[] actionNames = { "Move", "Look", "SelectorWheel", "Sprint",
        "Interact", "Hold", "Jump", "OpenNotebook", "OpenMap", "OpenMenu" };

    [Header("UI")]
    [SerializeField] private Transform bindingContainer;
    [SerializeField] private RebindRow rowPrefab;
    [SerializeField] private Button resetButton;
    [SerializeField] private TMP_Text statusText;

    private class Entry
    {
        public InputAction action;
        public int bindingIndex;
        public RebindRow row;
    }

    private readonly List<Entry> entries = new();
    private InputActionRebindingExtensions.RebindingOperation currentOperation;

    private void Awake()
    {
        LoadOverrides(inputActions);
        BuildUI();

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetAll);
        foreach (InputAction a in inputActions)
        {
            Debug.Log($"Action: {a.name}");
            for (int i = 0; i < a.bindings.Count; i++)
            {
                InputBinding b = a.bindings[i];
                Debug.Log(
                    $"   [{i}] name='{b.name}' path='{b.path}' composite={b.isComposite} part={b.isPartOfComposite}");
            }
        }
    }

    private void OnDisable()
    {
        currentOperation?.Cancel();
    }
    

    public static void LoadOverrides(InputActionAsset asset)
    {
        string json = PlayerPrefs.GetString(PrefsKey, string.Empty);
        if (!string.IsNullOrEmpty(json))
            asset.LoadBindingOverridesFromJson(json);
    }

    private static void SaveOverrides(InputActionAsset asset)
    {
        PlayerPrefs.SetString(PrefsKey, asset.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }
    

    private void BuildUI()
    {
        foreach (string actionName in actionNames)
        {
            InputAction action = inputActions.FindAction(actionName);
            if (action == null)
            {
                Debug.LogWarning("No action with name: " + actionName);
                continue;
            }

            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];

                
                if (binding.isComposite) continue;

                
                if (!binding.path.StartsWith("<Keyboard>")) continue;

                CreateRow(action, i);
            }
        }
    }

    private void CreateRow(InputAction action, int bindingIndex)
    {
        InputBinding binding = action.bindings[bindingIndex];

        RebindRow row = Instantiate(rowPrefab, bindingContainer);

        string label = binding.isPartOfComposite
            ? $"{action.name} - {Capitalize(binding.name)}"
            : action.name;

        row.SetLabel(label);

        Entry entry = new Entry { action = action, bindingIndex = bindingIndex, row = row };
        entries.Add(entry);

        RefreshRow(entry);
        row.Button.onClick.AddListener(() => StartRebinding(entry));
    }

    private void RefreshRow(Entry entry)
    {
        entry.row.SetKey(entry.action.GetBindingDisplayString(entry.bindingIndex));
    }

    private void RefreshAll()
    {
        foreach (Entry entry in entries)
            RefreshRow(entry);
    }
    

    private void StartRebinding(Entry entry)
    {
        currentOperation?.Cancel();

        InputAction action = entry.action;
        bool wasEnabled = action.enabled;
        action.Disable();

        entry.row.SetKey("Press a button");
        SetStatus("Escape = cancel");

        currentOperation = action.PerformInteractiveRebinding(entry.bindingIndex)
            .WithControlsHavingToMatchPath("<Keyboard>")
            .WithControlsExcluding("<Mouse>")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(op =>
            {
                if (IsDuplicate(entry))
                {
                    action.RemoveBindingOverride(entry.bindingIndex);
                    SaveOverrides(inputActions);
                    SetStatus("Key is in use");
                }
                else
                {
                    SaveOverrides(inputActions);
                    SetStatus("");
                }

                FinishRebinding(entry, wasEnabled, op);
            })
            .OnCancel(op =>
            {
                SetStatus("");
                FinishRebinding(entry, wasEnabled, op);
            })
            .Start();
    }

    private void FinishRebinding(Entry entry, bool wasEnabled,
        InputActionRebindingExtensions.RebindingOperation op)
    {
        op.Dispose();
        if (currentOperation == op) currentOperation = null;

        if (wasEnabled) entry.action.Enable();
        RefreshRow(entry);
    }

    private bool IsDuplicate(Entry entry)
    {
        InputBinding newBinding = entry.action.bindings[entry.bindingIndex];

        foreach (InputBinding other in entry.action.actionMap.bindings)
        {
            if (other.id == newBinding.id) continue;      
            if (other.isComposite) continue;
            if (!other.path.StartsWith("<Keyboard>")) continue;

            if (other.effectivePath == newBinding.effectivePath)
                return true;
        }
        return false;
    }
    

    private void ResetAll()
    {
        currentOperation?.Cancel();
        inputActions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(PrefsKey);
        PlayerPrefs.Save();
        RefreshAll();
        SetStatus("Default settings recovered");
    }
    

    private void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);
   
    
}
