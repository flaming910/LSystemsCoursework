using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LSystemSettings : MonoBehaviour
{
    [SerializeField] private LSystemController Controller;
    
    [SerializeField] private TMP_InputField Iterations;
    [SerializeField] private TMP_InputField Length;
    [SerializeField] private TMP_InputField Angle;
    
    [SerializeField] private TMP_InputField Axiom;

    [Header("Variables")]
    [SerializeField] private Transform VariableInputPanel;
    private Transform VariableInputParent;
    [SerializeField] private List<TMP_InputField> VariableInputs;
    [SerializeField] private GameObject VariableInputPrefab;
    
    [Header("Constants")]
    [SerializeField] private Transform ConstantInputPanel;
    private Transform ConstantInputParent;
    [SerializeField] private List<TMP_InputField> ConstantInputs;
    [SerializeField] private GameObject ConstantInputPrefab;
    
    [Header("Variable Rules")]
    [SerializeField] private Transform VariableRulesInputPanel;
    [SerializeField] private Transform VariableRulesInputParent;
    private List<VariableRuleInput> VariableRulesInputs;
    [SerializeField] private GameObject VariableRulesInputPrefab;
    
    [Header("Action Mapping")]
    [SerializeField] private Transform ActionMappingInputPanel;
    [SerializeField] private Transform ActionMappingInputParent;
    private List<ActionMappingInput> ActionMappingInputs;
    [SerializeField] private GameObject ActionMappingInputPrefab;

    [SerializeField] private TMP_InputField XOffsetInput;
    [SerializeField] private TMP_InputField YOffsetInput;
    
    public List<char> Variables;
    public List<char> Constants;
    private Dictionary<char, string> VariableRules;
    private Dictionary<char, List<ActionTypes>> ActionMapping;
    private Vector3 OffsetPosition = Vector3.zero;
    
    private string PresetFilePath = "Assets/LSystemPresets.txt";

    private void Start()
    {
        Variables = new List<char>();
        Constants = new List<char>();
        VariableRules = new Dictionary<char, string>();
        ActionMapping = new Dictionary<char, List<ActionTypes>>();
        VariableInputParent = VariableInputs[0].transform.parent.parent;
        VariableInputs[0].onValueChanged.AddListener(VariableInputChanged);
        VariableInputs[0].transform.parent.
            GetComponentInChildren<Button>().onClick.
            AddListener(() => RemoveVariableInput(VariableInputs[0]));
        
        ConstantInputParent = ConstantInputs[0].transform.parent.parent;
        ConstantInputs[0].onValueChanged.AddListener(ConstantInputChanged);
        ConstantInputs[0].transform.parent.
            GetComponentInChildren<Button>().onClick.
            AddListener(() => RemoveConstantInput(ConstantInputs[0]));
    }

    private void Update()
    {
        //Shift + 1-8 key to load presets
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                LoadFromFile(0);
                ApplySettings();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                LoadFromFile(1);
                ApplySettings();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                LoadFromFile(2);
                ApplySettings();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                LoadFromFile(3);
                ApplySettings();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                LoadFromFile(4);
                ApplySettings();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                LoadFromFile(5);
                ApplySettings();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                LoadFromFile(6);
                ApplySettings();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                LoadFromFile(7);
                ApplySettings();
            }
        }
    }

    public void ApplySettings()
    {
        var length = float.Parse(Length.text);
        var angle = float.Parse(Angle.text);
        var iter = int.Parse(Iterations.text);
        var axiom = Axiom.text;
        if(XOffsetInput.text == "") XOffsetInput.text = "0";
        if(YOffsetInput.text == "") YOffsetInput.text = "0";
        OffsetPosition.x = float.Parse(XOffsetInput.text);
        OffsetPosition.y = float.Parse(YOffsetInput.text);
        Controller.SetParameters(length, angle, iter, axiom, VariableRules, Variables, ActionMapping, OffsetPosition);
    }

    public void ApplyIterations()
    {
        var iter = int.Parse(Iterations.text);
        if(iter < 1) iter = 1;
        Controller.SetIterations(iter);
        Iterations.text = iter.ToString();
    }
    
    /*
     * Take an input string and parse it to set the system parameters
     * Example input:
     * n=5; axiom = F; angle=25.7; length=2.0; rules={F: F[+F]F[-F]F}; variables={F}; constants={+, -, [, ]}; actions={F: DL, +: RC, -: RA, [: Push, ]: Pop}
     * Output:
     * iteration: 5
     * axiom: F
     * angle: 25.7
     * length: 2.0
     * Variables: F
     * Constants: +, -, [, ]
     * Rules: F -> F[+F]F[-F]F
     * Action Mapping: F -> DrawLine, + -> RotateClock, - -> RotateAntiClock, [ -> PushPosition, ] -> PopPosition
     */
    private void SystemStringParser(string input)
    {
        //Go through string by splitting by ';'
        var parameters = input.Split(';');
        Variables = new List<char>();
        VariableRules = new Dictionary<char, string>();
        ActionMapping = new Dictionary<char, List<ActionTypes>>();
        OffsetPosition = Vector3.zero;
        
        foreach (var param in parameters)
        {
            var keyValue = param.Split('=');
            if(keyValue.Length != 2) continue;
            var key = keyValue[0].Replace(" ", "");
            var value = keyValue[1].Replace(" ", "");
            switch(key) {
                case "n":
                    Iterations.text = int.Parse(value).ToString();
                    break;
                case "angle":
                    Angle.text = float.Parse(value).ToString();
                    break;
                case "length":
                    Length.text = float.Parse(value).ToString();
                    break;
                case "axiom":
                    Axiom.text = value;
                    break;
                case "rules":
                    value = value.Trim('{', '}');
                    var rulePairs = value.Split(',');
                    foreach (var rule in rulePairs)
                    {
                        var ruleKeyValue = rule.Split(':');
                        VariableRules.Add(ruleKeyValue[0].Trim()[0], ruleKeyValue[1].Trim());
                    }
                    break;
                case "variables":
                    value = value.Trim('{', '}');
                    var vars = value.Split(',');
                    foreach (var v in vars)
                    {
                        Variables.Add(v.Trim()[0]);
                    }
                    LoadVariableInputs();
                    break;
                case "constants":
                    value = value.Trim('{', '}');
                    var consts = value.Split(',');
                    foreach (var c in consts)
                    {
                        Constants.Add(c.Trim()[0]);
                    }
                    LoadConstantInputs();
                    break;
                case "actions":
                    value = value.Trim('{', '}');
                    var actionPairs = value.Split(',');
                    foreach (var actionPair in actionPairs)
                    {
                        var actionKeyValue = actionPair.Split(':');
                        var actionList = new List<ActionTypes>();
                        var actions = actionKeyValue[1].Split(',');
                        foreach (var action in actions)
                        {
                            switch (action.Trim())
                            {
                                case "DLi":
                                    actionList.Add(ActionTypes.DrawLine);
                                    break;
                                case "DLe":
                                    actionList.Add(ActionTypes.DrawLeaf);
                                    break;
                                case "RC":
                                    actionList.Add(ActionTypes.RotateClock);
                                    break;
                                case "RA":
                                    actionList.Add(ActionTypes.RotateAnticlock);
                                    break;
                                case "Push":
                                    actionList.Add(ActionTypes.PushPosition);
                                    break;
                                case "Pop":
                                    actionList.Add(ActionTypes.PopPosition);
                                    break;
                                case "DN":
                                    actionList.Add(ActionTypes.DoNothing);
                                    break;
                                //Add more cases as needed
                            }
                        }
                        ActionMapping.Add(actionKeyValue[0].Trim()[0], actionList);
                    }
                    break;
                case "offset":
                    value = value.Trim('{', '}');
                    var offsetValues = value.Split(',');
                    OffsetPosition.x = float.Parse(offsetValues[0]);
                    OffsetPosition.y = float.Parse(offsetValues[1]);
                    XOffsetInput.text = OffsetPosition.x.ToString("F1");
                    YOffsetInput.text = OffsetPosition.y.ToString("F1");
                    break;
            }
        }   
    }

    private void LoadFromFile(int presetIndex)
    {
        //Load presets from a text file located at PresetFilePath and then choose line based on presetIndex
        var allPresets = System.IO.File.ReadAllLines(PresetFilePath);
        if(presetIndex < 0 || presetIndex >= allPresets.Length) return;
        var presetString = allPresets[presetIndex];
        SystemStringParser(presetString);
    }

    public void IncreaseIteration()
    {
        var iter = int.Parse(Iterations.text);
        iter++;
        Controller.SetIterations(iter);
        Iterations.text = iter.ToString();
    }
    
    public void DecreaseIteration()
    {
        var iter = int.Parse(Iterations.text);
        if(iter > 1) iter--;
        Controller.SetIterations(iter);
        Iterations.text = iter.ToString();
    }

    #region Visiblity Handlers
    public void OpenVariablesPanel()
    {
        VariableInputPanel.gameObject.SetActive(true);
        ConstantInputPanel.gameObject.SetActive(false);
        VariableRulesInputPanel.gameObject.SetActive(false);
        ActionMappingInputPanel.gameObject.SetActive(false);
    }
    
    public void OpenConstantsPanel()
    {
        VariableInputPanel.gameObject.SetActive(false);
        ConstantInputPanel.gameObject.SetActive(true);
        VariableRulesInputPanel.gameObject.SetActive(false);
        ActionMappingInputPanel.gameObject.SetActive(false);
    }

    public void OpenVariableRulesPanel()
    {
        VariableInputPanel.gameObject.SetActive(false);
        ConstantInputPanel.gameObject.SetActive(false);
        VariableRulesInputPanel.gameObject.SetActive(true);
        ActionMappingInputPanel.gameObject.SetActive(false);
        SetVariableRuleInputLabels();
    }
    
    public void OpenActionMappingPanel()
    {
        VariableInputPanel.gameObject.SetActive(false);
        ConstantInputPanel.gameObject.SetActive(false);
        VariableRulesInputPanel.gameObject.SetActive(false);
        ActionMappingInputPanel.gameObject.SetActive(true);
        SetActionMappingInputLabels();
    }
    #endregion
    
    #region Variable Input Handlers

    private void VariableInputChanged(string input)
    {
        List<char> newVariables = new List<char>();
        foreach (var varInput in VariableInputs)
        {
            var varString = varInput.text.Trim();
            if(varString != "") newVariables.Add(varString[0]);
        }
        Variables = newVariables;
    }

    public void AddVariableInputButton()
    {
        AddVariableInput();
    }
    
    private void AddVariableInput(char value = '\0')
    {
        var newInput = Instantiate(VariableInputPrefab, VariableInputParent).GetComponentInChildren<TMP_InputField>();
        newInput.transform.parent.SetSiblingIndex(VariableInputParent.childCount - 2);
        newInput.text = value.ToString();
        newInput.onValueChanged.AddListener(VariableInputChanged);
        newInput.transform.parent.
            GetComponentInChildren<Button>().onClick.
            AddListener(() => RemoveVariableInput(newInput));
        VariableInputs.Add(newInput);
    }

    public void RemoveVariableInput(TMP_InputField input)
    {
        if (VariableInputs.Count == 1) return;
        Destroy(input.transform.parent.gameObject);
        VariableInputs.Remove(input);
        List<char> newVariables = new List<char>();
        foreach (var varInput in VariableInputs)
        {
            var varString = varInput.text.Trim();
            if(varString != "") newVariables.Add(varString[0]);
        }
        Variables = newVariables;
    }

    private void LoadVariableInputs()
    {
        var tempStore = new List<char>(Variables);
        if (VariableInputs.Count == tempStore.Count)
        {
            for (int i = 0; i < tempStore.Count; i++)
            {
                VariableInputs[i].text = tempStore[i].ToString();
            }
        } 
        else if(VariableInputs.Count < tempStore.Count)
        {
            for (int i = 0; i < tempStore.Count; i++)
            {
                if (i < VariableInputs.Count)
                {
                    VariableInputs[i].text = tempStore[i].ToString();
                }
                else
                {
                    AddVariableInput(tempStore[i]);
                }
            }
        }
        else if(VariableInputs.Count > tempStore.Count)
        {
            for (int i = VariableInputs.Count - 1; i >= tempStore.Count; i--)
            {
                RemoveVariableInput(VariableInputs[i]);
            }
            for (int i = 0; i < tempStore.Count; i++)
            {
                VariableInputs[i].text = tempStore[i].ToString();
            }
        }
        Variables = tempStore;
    }
    #endregion
    
    #region Constant Input Handlers

    private void ConstantInputChanged(string input)
    {
        List<char> newConstants = new List<char>();
        foreach (var conInput in ConstantInputs)
        {
            var conString = conInput.text.Trim();
            if(conString != "") newConstants.Add(conString[0]);
        }
        Constants = newConstants;
    }
    
    public void AddConstantInputButton()
    {
        AddConstantInput();
    }
    
    private void AddConstantInput(char value = '\0')
    {
        var newInput = Instantiate(ConstantInputPrefab, ConstantInputParent).GetComponentInChildren<TMP_InputField>();
        newInput.transform.parent.SetSiblingIndex(ConstantInputParent.childCount - 2);
        newInput.text = value.ToString();
        newInput.onValueChanged.AddListener(ConstantInputChanged);
        newInput.transform.parent.
            GetComponentInChildren<Button>().onClick.
            AddListener(() => RemoveConstantInput(newInput));
        ConstantInputs.Add(newInput);
    }

    public void RemoveConstantInput(TMP_InputField input)
    {
        if (ConstantInputs.Count == 1) return;
        Destroy(input.transform.parent.gameObject);
        ConstantInputs.Remove(input);
        List<char> newConstants = new List<char>();
        foreach (var conInput in ConstantInputs)
        {
            var conString = conInput.text.Trim();
            if(conString != "") newConstants.Add(conString[0]);
        }
        Constants = newConstants;
    }
    
    private void LoadConstantInputs()
    {
        var tempStore = new List<char>(Constants);
        if (ConstantInputs.Count == tempStore.Count)
        {
            for (int i = 0; i < tempStore.Count; i++)
            {
                ConstantInputs[i].text = tempStore[i].ToString();
            }
        } 
        else if(ConstantInputs.Count < tempStore.Count)
        {
            for (int i = 0; i < tempStore.Count; i++)
            {
                if (i < ConstantInputs.Count)
                {
                    ConstantInputs[i].text = tempStore[i].ToString();
                }
                else
                {
                    AddConstantInput(tempStore[i]);
                }
            }
        }
        else if(ConstantInputs.Count > tempStore.Count)
        {
            for (int i = ConstantInputs.Count - 1; i >= tempStore.Count; i--)
            {
                RemoveConstantInput(ConstantInputs[i]);
            }
            for (int i = 0; i < tempStore.Count; i++)
            {
                ConstantInputs[i].text = tempStore[i].ToString();
            }
        }
        Constants = tempStore;
    }
    
    #endregion

    #region Variable Rules Input Handlers

    private void SetVariableRuleInputLabels()
    {
        foreach (Transform child in VariableRulesInputParent.transform)
        {
            if(child.name != "Title") Destroy(child.gameObject);
        }
        
        foreach (var var in Variables)
        {
            var newRule = Instantiate(VariableRulesInputPrefab, VariableRulesInputParent).GetComponent<VariableRuleInput>();
            newRule.VariableLabel.text = var.ToString();
            newRule.RuleInputField.onValueChanged.AddListener((rule) => VariableRuleInputChanged(var, rule));
            if(VariableRules.ContainsKey(var))
            {
                newRule.RuleInputField.text = VariableRules[var];
            }
        }
    }
    
    public void VariableRuleInputChanged(char variable, string rule)
    {
        VariableRules[variable] = rule.Trim();
    }
    #endregion
    
    #region Action Mapping Input Handlers

    private void SetActionMappingInputLabels()
    {
        foreach (Transform child in ActionMappingInputParent.transform)
        {
            if(child.name != "Title") Destroy(child.gameObject);
        }
        
        foreach (var symbol in Variables)
        {
            var actionMap = Instantiate(ActionMappingInputPrefab, ActionMappingInputParent).GetComponent<ActionMappingInput>();
            actionMap.Label.text = symbol.ToString();
            actionMap.Dropdowns[0].onValueChanged.AddListener((index) => ActionMappingInputChanged(symbol, actionMap.Dropdowns));
            if (ActionMapping.ContainsKey(symbol))
            {
                for (int i = 0; i < ActionMapping[symbol].Count; i++)
                {
                    var action = ActionMapping[symbol][i];
                    if(i == 0) actionMap.Initialise((int)action);
                    else actionMap.Dropdowns[i].value = (int)action;
                }
            }
            else
            {
                ActionMapping[symbol] = new List<ActionTypes> { ActionTypes.DrawLine };
                actionMap.Initialise(0);
            }
        }

        foreach (var symbol in Constants)
        {
            var actionMap = Instantiate(ActionMappingInputPrefab, ActionMappingInputParent).GetComponent<ActionMappingInput>();
            actionMap.Label.text = symbol.ToString();
            actionMap.Dropdowns[0].onValueChanged.AddListener((index) => ActionMappingInputChanged(symbol, actionMap.Dropdowns));
            if (ActionMapping.ContainsKey(symbol))
            {
                for (int i = 0; i < ActionMapping[symbol].Count; i++)
                {
                    var action = ActionMapping[symbol][i];
                    if(i == 0) actionMap.Initialise((int)action);
                    else actionMap.Dropdowns[i].value = (int)action;
                }
            }
            else
            {
                actionMap.Initialise(0);
            }
        }
    }
    
    private void ActionMappingInputChanged(char symbol, List<TMP_Dropdown> dropdowns)
    {
        List<ActionTypes> actions = new List<ActionTypes>();
        foreach (var dropdown in dropdowns)
        {
            var actionString = dropdown.options[dropdown.value].text;
            if(Enum.TryParse(actionString, out ActionTypes action))
            {
                actions.Add(action);
            }
        }
        ActionMapping[symbol] = actions;
    }
    
    #endregion
}
