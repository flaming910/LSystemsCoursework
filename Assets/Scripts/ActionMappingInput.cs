using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ActionMappingInput : MonoBehaviour
{
    public TMP_Text Label;
    public List<TMP_Dropdown> Dropdowns;

    public void Initialise(int option)
    {
        var options = new List<string>();
        foreach (ActionTypes action in Enum.GetValues(typeof(ActionTypes)))
        {
            options.Add(action.ToString());
        }
        Dropdowns[0].ClearOptions();
        Dropdowns[0].AddOptions(options);
        Dropdowns[0].value = option;
    }

    public void AddDropdown()
    {
        var dropdownObject = Dropdowns[0].gameObject.transform.parent.gameObject;
        var dropdownParent = dropdownObject.transform.parent;
        var dropdown = Instantiate(dropdownObject, dropdownParent).GetComponentInChildren<TMP_Dropdown>();
        Dropdowns.Add(dropdown);
    }

    public void RemoveDropdown(TMP_Dropdown dropdown)
    {
        if (Dropdowns.Count == 1) return;
        Destroy(dropdown.gameObject.transform.parent.gameObject);
        Dropdowns.Remove(dropdown);
        
    }
}
