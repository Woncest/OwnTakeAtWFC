using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputFieldHandler : MonoBehaviour
{
    private TMP_InputField myInputField;  // Reference to your TMP InputField
    public int defaultValue = 25;        // Default value to set if input is invalid

    public TileGridGenerator tileGridGenerator;

    void Start()
    {
        myInputField = GetComponent<TMP_InputField>();

        // Set the initial value of the input field to the default value
        myInputField.text = tileGridGenerator.gridSize.ToString();

        // Add a listener to validate input when editing ends
        myInputField.onEndEdit.AddListener(OnInputFieldSubmitted);
    }

    // Method called when the InputField is submitted
    void OnInputFieldSubmitted(string inputText)
    {
        if (IsPositiveInteger(inputText))
        {
            int parsedValue = int.Parse(inputText);
            tileGridGenerator.newGridSize = parsedValue;
            // You can proceed to use parsedValue in any way you need here
        }
        else
        {
            // Set the field to the default value if input is invalid
            myInputField.text = defaultValue.ToString();
        }
    }

    // Helper method to check if the input text is a positive integer
    private bool IsPositiveInteger(string inputText)
    {
        return int.TryParse(inputText, out int result) && result > 0;
    }
}
