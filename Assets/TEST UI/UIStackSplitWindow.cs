using UnityEngine;
using UnityEngine.UI;

using TMPro;

using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class UIStackSplitWindow : MonoBehaviour
{

    ArmyStack armyStackFrom;
    ArmyStack armyStackTo;

    Image[] stackIcons;
    TMP_Text[] stackSizes;
    Slider slider;

    Button cancelButton;
    Button splitButton;


    // Start is called before the first frame update
    void Start()
    {
        CacheComponents();
    }

    void CacheComponents()
    {
        stackIcons = new Image[2];
        stackSizes = new TMP_Text[2];

        stackIcons[0] = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();
        stackIcons[1] = transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>();

        stackSizes[0] = transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<TMP_Text>();
        stackSizes[1] = transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<TMP_Text>();

        slider = transform.GetChild(0).GetChild(2).GetComponent<Slider>();

        cancelButton = transform.GetChild(0).GetChild(3).GetComponent<Button>();
        splitButton = transform.GetChild(0).GetChild(4).GetComponent<Button>();

        // Register callbacks
        slider.onValueChanged.AddListener(delegate { OnValueChanged(); });
        cancelButton.onClick.AddListener(delegate { OnCancelButtonClicked(); });
        splitButton.onClick.AddListener(delegate { OnSplitButtonClicked(); });

    }

    public void OnValueChanged()
    {
    }

    void OnCancelButtonClicked()
    {
        Close();
    }

    void OnSplitButtonClicked()
    {
        Debug.Log("I am closing " + slider.value);
        Close();
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void UpdateUI()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
