using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FretboardCalibrationRangeScript : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI textbox;

    public static FretboardCalibrationRangeScript Instance;

    public int min = 0;
    public int max = 4;

    private void Awake()
    {
        if (Instance != null) Destroy(this);

        Instance = this;
    }

    void Start()
    {
        slider.onValueChanged.AddListener((v) => {
            min = (int)v;
            max = min + 4;
            textbox.text = $"{min} - {max}";
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
