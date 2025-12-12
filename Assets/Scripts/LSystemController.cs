using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LSystemController : MonoBehaviour
{
    private string CurrentString;
    private string Axiom;

    private int Iterations;
    private List<char> Variables;
    private Dictionary<char, string> VariableRules;
    private Dictionary<char, List<ActionTypes>> ActionMapping;
    private Dictionary<int, string> SavedIterations;
    private Vector3 OffsetPosition;

    [SerializeField] 
    private GameObject LSystemGenPrefab;

    private GameObject CurrentGenerator;
    
    [SerializeField] private float Length;
    [SerializeField] private float Angle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActionMapping = new Dictionary<char, List<ActionTypes>>();
        VariableRules = new Dictionary<char, string>();
        Variables = new List<char>();
        SavedIterations = new Dictionary<int, string>();
        // ActionMapping = new Dictionary<char, List<ActionTypes>>
        // {
        //     { '0', new List<ActionTypes> { ActionTypes.DrawLeaf } },
        //     { '1', new List<ActionTypes> { ActionTypes.DrawLine } },
        //     { '[', new List<ActionTypes> { ActionTypes.PushPosition, ActionTypes.RotateAnticlock } },
        //     { ']', new List<ActionTypes> { ActionTypes.PopPosition, ActionTypes.RotateClock } }
        // };
        // Axiom = "0";
        // Variables = new List<char>{ '0', '1' };
        // VariableRules = new Dictionary<char, string>
        // {
        //     { '1', "11" },
        //     { '0', "1[0]0" }
        // };
        // CurrentString = Axiom;
        // recursionText.text = CurrentString;
        // SavedIterations = new Dictionary<int, string>
        // {
        //     [0] = CurrentString
        // };
        // CurrentGenerator = CreateGen();
    }

    private GameObject CreateGen()
    {
        if(CurrentGenerator) DestroyImmediate(CurrentGenerator);
        var lsystemGen = Instantiate(LSystemGenPrefab, OffsetPosition, Quaternion.identity).GetComponent<LSystemGenerator>();
        lsystemGen.Initialise(CurrentString, ActionMapping, Length, Angle);
        return lsystemGen.gameObject;
    }

    private void ProcessIterations(bool processActions)
    {
        var newString = "";
        foreach (var character in CurrentString)
        {
            if (Variables.Contains(character))
            {
                newString += VariableRules[character];
            }
            else
            {
                newString += character.ToString();
            }
        }
        CurrentString = newString;
        SavedIterations[Iterations] = CurrentString;
        if(processActions) CurrentGenerator = CreateGen();
    }

    
    // Update is called once per frame
    void Update()
    {

    }
    
    public void SetIterations(int iterations)
    {
        if (SavedIterations.ContainsKey(iterations))
        {
            Iterations = iterations;
            CurrentString = SavedIterations[iterations];
            CurrentGenerator = CreateGen();
            return;
        }
        if (iterations > Iterations)
        {
            for (int i = Iterations + 1; i <= iterations; i++)
            {
                Iterations = i;
                ProcessIterations(i == iterations); 
            }
            return;
        }

        if (iterations < Iterations)
        {
            //Find the closest saved iteration less than the target iterations
            var closestIteration = SavedIterations.Keys.Where(x => x < iterations).DefaultIfEmpty(0).Max();
            Iterations = closestIteration;
            CurrentString = SavedIterations[closestIteration];
            for (int i = closestIteration + 1; i <= iterations; i++)
            {
                Iterations = i;
                ProcessIterations(i == iterations);
            }
        }
    }

    public void SetParameters(float length, float angle, int iterations, string axiom,
        Dictionary<char, string> variableRules,
        List<char> variables,
        Dictionary<char, List<ActionTypes>> actionMapping,
        Vector3 offsetPosition)
    {
        Length = length;
        Angle = angle;
        Axiom = axiom;
        VariableRules = variableRules;
        Variables = variables;
        ActionMapping = actionMapping;
        OffsetPosition = offsetPosition;
        CurrentString = Axiom;
        Iterations = 0;
        SavedIterations = new Dictionary<int, string>
        {
            [0] = CurrentString
        };
        SetIterations(iterations);
    }
}
