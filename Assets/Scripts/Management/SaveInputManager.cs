using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveInputManager : MonoBehaviour
{
    public InputField field;

    public string SaveName => field.text;

    string DefaultValue => System.DateTime.Now.ToString();

    public static SaveInputManager instance;
    
    void OnEnable()
    {
        instance = this;
        field.text = DefaultValue;
    }

    public void OnTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) field.text = DefaultValue;
    }

    public void Save()
    {
        GameManager.instance.SaveBoard(SaveName);
        Close();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
