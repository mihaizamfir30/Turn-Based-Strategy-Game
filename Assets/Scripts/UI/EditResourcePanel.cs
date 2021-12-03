using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditResourcePanel : MonoBehaviour
{

    Button[] incButtons;
    Button[] decButtons;
    
    int[] numbers;
    Text type;
    Text quantity;

    Button cancelButton;
    Button editButton;

    HexMapEditor editor;
    HexResource resource;

    public void Init(HexMapEditor editor)
    {
        this.editor = editor;
    }

    // Start is called before the first frame update
    void Start()
    {
        CacheComponents();
    }

    void CacheComponents()
    {
        incButtons = transform.GetChild(0).GetChild(1).GetComponentsInChildren<Button>();
        decButtons = transform.GetChild(0).GetChild(2).GetComponentsInChildren<Button>();

        for (int i = 0; i < incButtons.Length; i++)
        {
            int v = i;
            incButtons[i].onClick.AddListener(() => ChangeNumber(v, 1));
            decButtons[i].onClick.AddListener(() => ChangeNumber(v, -1));
        }

        quantity = transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>();
        quantity.text = "";

        type = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>();
        type.text = "";

        numbers = new int[incButtons.Length];

        cancelButton = transform.GetChild(0).GetChild(3).GetComponent<Button>();
        editButton = transform.GetChild(0).GetChild(4).GetComponent<Button>();

        cancelButton.onClick.AddListener(
            () => 
            { 
                editor.OnEditResourceFinished(resource, -1); 
                Close(); 
            }
        );

        editButton.onClick.AddListener(
            () =>
            {
                editor.OnEditResourceFinished(resource, GetNumber());
                Close();
            }
        );

    }

    void ChangeNumber(int index, int value)
    {
        numbers[index] = Mathf.Clamp(numbers[index] + value, 0, 9);
        UpdateUI();
    }

    void SetNumber()
    {
        numbers[0] = (int)(resource.Quantity / 1000);
        numbers[1] = (int)((resource.Quantity % 1000) / 100);
        numbers[2] = (int)((resource.Quantity % 100) / 10);
        numbers[3] = resource.Quantity % 10;
    }

    int GetNumber()
    {
        int number = numbers[0] * 1000 + numbers[1] * 100 + numbers[2] * 10 + numbers[3];
        if (number == 0)
        {
            numbers[3] = 1;
            number = 1;
        }
        return number;
    }

    void UpdateUI()
    {
        quantity.text = GetNumber().ToString();
    }

    public void Open(HexResource resource)
    {
        this.resource = resource;

        gameObject.SetActive(true);

        type.text = GameplayMetrics.ResourceNames[resource.Type];
        SetNumber();
        UpdateUI();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
