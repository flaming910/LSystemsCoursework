using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

public class LSystemGenerator : MonoBehaviour
{
    private string CurrentString;
    private Stack<Vector3> SavedPositions;
    private Stack<Quaternion> SavedRotations;

    private Dictionary<char, List<ActionTypes>> ActionMapping;

    [SerializeField]
    private Transform TurtleTransform;
    [SerializeField]
    private GameObject LineRendererPrefab;
    private LineRenderer CurrentLineRenderer;
    
    private float Length;
    private float Angle;
    
    private IEnumerator ProcessActions()
    {
        foreach (Transform child in transform)
        {
            if(!child.CompareTag("Player")) Destroy(child.gameObject);
        }
        CurrentLineRenderer = Instantiate(LineRendererPrefab, transform).GetComponent<LineRenderer>();
        CurrentLineRenderer.positionCount = 1;
        CurrentLineRenderer.SetPosition(0, transform.position);
        TurtleTransform.position = transform.position;
        TurtleTransform.rotation = Quaternion.identity;
        SavedPositions.Push(transform.position);
        SavedRotations.Push(Quaternion.identity);
        int iterationsTillNextFrame = 100;
        foreach (var character in CurrentString)
        {
            List<ActionTypes> actions = ActionMapping[character];
            foreach (var action in actions)
            {
                ProcessAction(action);
            }
            iterationsTillNextFrame--;
            if (iterationsTillNextFrame <= 0)
            {
                iterationsTillNextFrame = 100;
                yield return null;
            }
        }
    }
    
    private void ProcessAction(ActionTypes action)
    {
        switch (action)
        {
            case ActionTypes.PushPosition:
                SavedPositions.Push(TurtleTransform.position);
                SavedRotations.Push(TurtleTransform.rotation);
                break;
            case ActionTypes.PopPosition:
                TurtleTransform.position = SavedPositions.Pop();
                TurtleTransform.rotation = SavedRotations.Pop();
                CurrentLineRenderer = Instantiate(LineRendererPrefab, transform).GetComponent<LineRenderer>();
                CurrentLineRenderer.positionCount = 1;
                CurrentLineRenderer.SetPosition(0, TurtleTransform.position);
                break;
            case ActionTypes.RotateAnticlock:
                TurtleTransform.Rotate(Vector3.forward, -Angle);
                break;
            case ActionTypes.RotateClock:
                TurtleTransform.Rotate(Vector3.forward, Angle);
                break;
            case ActionTypes.DrawLine:
                TurtleTransform.Translate(Vector3.up * Length);
                CurrentLineRenderer.positionCount++;
                CurrentLineRenderer.SetPosition(CurrentLineRenderer.positionCount - 1, TurtleTransform.position);
                break;
            case ActionTypes.DrawLeaf:
                TurtleTransform.Translate(Vector3.up * Length);
                CurrentLineRenderer.positionCount++;
                CurrentLineRenderer.SetPosition(CurrentLineRenderer.positionCount - 1, TurtleTransform.position);
                //Draw a leaf (aka a little diamond shape) at the current position (trying out other stuff for now)
                // TurtleTransform.Rotate(Vector3.forward, 45);
                // TurtleTransform.Translate(Vector3.up * Length/2);
                // CurrentLineRenderer.positionCount++;
                // CurrentLineRenderer.SetPosition(CurrentLineRenderer.positionCount - 1, TurtleTransform.position);
                // for (int i = 0; i < 3; i++)
                // {
                //     TurtleTransform.Rotate(Vector3.forward, -90);
                //     TurtleTransform.Translate(Vector3.up * Length/2);
                //     CurrentLineRenderer.positionCount++;
                //     CurrentLineRenderer.SetPosition(CurrentLineRenderer.positionCount - 1, TurtleTransform.position);
                //
                // }
                //Draw 2 smaller lines to represent a leaf
                SavedPositions.Push(TurtleTransform.position);
                SavedRotations.Push(TurtleTransform.rotation);
                TurtleTransform.Rotate(Vector3.forward, Angle);
                TurtleTransform.Translate(Vector3.up * Length / 2);
                CurrentLineRenderer.positionCount++;
                CurrentLineRenderer.SetPosition(CurrentLineRenderer.positionCount - 1, TurtleTransform.position);
                TurtleTransform.position = SavedPositions.Pop();
                TurtleTransform.rotation = SavedRotations.Pop();
                TurtleTransform.Rotate(Vector3.forward, -Angle);
                TurtleTransform.Translate(Vector3.up * Length / 2);
                CurrentLineRenderer.positionCount++;
                CurrentLineRenderer.SetPosition(CurrentLineRenderer.positionCount - 1, TurtleTransform.position);
                break;
        }
    }

    public void Initialise(
        string currentString,
        Dictionary<char, List<ActionTypes>> actionMapping,
        float length,
        float angle
    )
    {
        SavedPositions = new Stack<Vector3>();
        SavedRotations = new Stack<Quaternion>();
        CurrentString = currentString;
        ActionMapping = actionMapping;
        Length = length;
        Angle = angle;
        StartCoroutine(ProcessActions());
    }
    
}
