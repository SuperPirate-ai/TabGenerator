using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;

public class BPM : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;

    private void Start()
    {
        inputField.onValueChanged.AddListener(delegate { BPMChange(); });
    }
    public void BPMChange()
    {
      EventManager.TriggerEvent("BPMChanged", new Dictionary<string, object> { {"BPM",inputField.text } });
    }
}
