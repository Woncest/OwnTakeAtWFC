using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ProbabilityInputFieldManager : MonoBehaviour
{
    [Header("Input Fields")]
    public List<TMP_InputField> inputFields; // List of TMP_InputFields, assigned in the inspector

    [Header("Values")]
    public int fourWay;
    public int threeWay;
    public int curve;
    public int empty;
    public int streetStraight;

    private void Start()
    {
        // Ensure we have exactly 5 InputFields
        if (inputFields.Count != 5)
        {
            Debug.LogError("Please assign exactly 5 TMP_InputFields to the list.");
            return;
        }

        // Initialize variables with the default values in the TMP_InputFields
        InitializeVariables();

        // Assign listeners to each TMP_InputField
        for (int i = 0; i < inputFields.Count; i++)
        {
            int index = i; // Capture the current index to use in the lambda
            inputFields[index].onEndEdit.AddListener((value) => OnInputFieldChanged(index, value));
        }
    }

    private void InitializeVariables()
    {
        for (int i = 0; i < inputFields.Count; i++)
        {
            if (int.TryParse(inputFields[i].text, out int parsedValue) && parsedValue >= 1)
            {
                // Set the variable based on the index
                switch (i)
                {
                    case 0:
                        fourWay = parsedValue;
                        break;
                    case 1:
                        threeWay = parsedValue;
                        break;
                    case 2:
                        curve = parsedValue;
                        break;
                    case 3:
                        empty = parsedValue;
                        break;
                    case 4:
                        streetStraight = parsedValue;
                        break;
                }
            }
            else
            {
                // If the TMP_InputField contains invalid or empty input, reset it to a default value (e.g., 1)
                inputFields[i].text = "1";
                switch (i)
                {
                    case 0:
                        fourWay = 1;
                        break;
                    case 1:
                        threeWay = 1;
                        break;
                    case 2:
                        curve = 1;
                        break;
                    case 3:
                        empty = 1;
                        break;
                    case 4:
                        streetStraight = 1;
                        break;
                }
            }
        }
    }

    private void OnInputFieldChanged(int index, string value)
    {
        // Parse the input value and ensure it is a valid positive integer
        if (int.TryParse(value, out int parsedValue) && parsedValue >= 1)
        {
            // Update the corresponding variable
            switch (index)
            {
                case 0:
                    fourWay = parsedValue;
                    break;
                case 1:
                    threeWay = parsedValue;
                    break;
                case 2:
                    curve = parsedValue;
                    break;
                case 3:
                    empty = parsedValue;
                    break;
                case 4:
                    streetStraight = parsedValue;
                    break;
            }
        }
        else
        {
            // Reset the TMP_InputField to the last valid value if the input is invalid
            ResetInputField(index);
        }
    }

    private void ResetInputField(int index)
    {
        switch (index)
        {
            case 0:
                inputFields[index].text = fourWay.ToString();
                break;
            case 1:
                inputFields[index].text = threeWay.ToString();
                break;
            case 2:
                inputFields[index].text = curve.ToString();
                break;
            case 3:
                inputFields[index].text = empty.ToString();
                break;
            case 4:
                inputFields[index].text = streetStraight.ToString();
                break;
        }
    }
}
