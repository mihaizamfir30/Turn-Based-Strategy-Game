using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditArmyPanel : MonoBehaviour
{

    Button[][] incButtons;
    Button[][] decButtons;

    Button[] prevButtons;
    Button[] nextButtons;

    Image[] icons;

    int[] types;
    int[][] numbers;
    Text[] quantities;

    int noStacks = 6;

    Button cancelButton;
    Button editButton;

    HexMapEditor editor;
    HexArmy army;

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

        incButtons = new Button[noStacks][];
        decButtons = new Button[noStacks][];

        prevButtons = new Button[noStacks];
        nextButtons = new Button[noStacks];

        icons = new Image[noStacks];

        types = new int[noStacks];
        numbers = new int[noStacks][];
        quantities = new Text[noStacks];

        for (int i = 0; i < quantities.Length; i++)
        {

            int x = i;

            Transform parent = gameObject.transform.GetChild(0).GetChild(0).GetChild(i);

            incButtons[i] = new Button[4];
            decButtons[i] = new Button[4];

            for (int j = 0; j < 4; j++)
            {

                int y = j;

                incButtons[i][j] = parent.GetChild(4 + j).GetComponent<Button>();
                decButtons[i][j] = parent.GetChild(8 + j).GetComponent<Button>();


                incButtons[i][j].onClick.AddListener(() => ChangeNumber(x, y, 1));
                decButtons[i][j].onClick.AddListener(() => ChangeNumber(x, y, -1));

            }

            prevButtons[i] = parent.GetChild(2).GetComponent<Button>();
            nextButtons[i] = parent.GetChild(3).GetComponent<Button>();

            prevButtons[i].onClick.AddListener(() => ChangeType(x, -1));
            nextButtons[i].onClick.AddListener(() => ChangeType(x, +1));

            icons[i] = parent.GetChild(0).GetChild(0).GetComponent<Image>();

            numbers[i] = new int[4];
            quantities[i] = parent.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>();
        }

        cancelButton = gameObject.transform.GetChild(0).GetChild(0).GetChild(6).GetComponent<Button>();
        editButton = gameObject.transform.GetChild(0).GetChild(0).GetChild(7).GetComponent<Button>();

        
        cancelButton.onClick.AddListener(
            () =>
            {
                editor.OnEditArmyFinished(army, null, true);
                Close();
            }
        );

        editButton.onClick.AddListener(
            () =>
            {
                editor.OnEditArmyFinished(army, GetArmy());
                Close();
            }
        );

    }

    int[][] GetArmy()
    {

        int[][] army = new int[6][];

        for (int i = 0; i < noStacks; i++)
        {
            army[i] = new int[2];

            army[i][0] = types[i];
            army[i][1] = GetNumber(i);
        }

        return army;
    }

    void ChangeType(int index, int v)
    {
        types[index] += v;

        if (types[index] == GameplayMetrics.TroopNames.Length)
        {
            types[index] = -1;
        } else if (types[index] == -2)
        {
            types[index] = GameplayMetrics.TroopNames.Length - 1;
        }

        if (types[index] == -1)
        {
            numbers[index][0] = 0;
            numbers[index][1] = 0;
            numbers[index][2] = 0;
            numbers[index][3] = 1;
        }

        UpdateUI();
    }

    void ChangeNumber(int i, int j, int value)
    {

        if (types[i] == -1)
        {
            return;
        }

        numbers[i][j] = Mathf.Clamp(numbers[i][j] + value, 0, 9);
        UpdateUI();
    }

    void SetNumber(int index)
    {

        if (army.Troops[index] == null)
        {
            numbers[index][0] = -1;
            return;
        }

        int quantity = army.Troops[index].Quantity;

        numbers[index][0] = (int)(quantity / 1000);
        numbers[index][1] = (int)((quantity % 1000) / 100);
        numbers[index][2] = (int)((quantity % 100) / 10);
        numbers[index][3] = quantity % 10;
    }

    int GetNumber(int index)
    {

        if (numbers[index][0] == -1)
        {
            return -1;
        }

        int number = numbers[index][0] * 1000 + numbers[index][1] * 100 + numbers[index][2] * 10 + numbers[index][3];
        if (number == 0)
        {
            numbers[index][3] = 1;
            number = 1;
        }
        return number;
    }

    void UpdateUI()
    {
        for (int i = 0; i < noStacks; i++)
        {
            if (types[i] != -1)
            {
                icons[i].sprite = GameplayMetrics.GetTroopSprite75(types[i]);
                icons[i].color = Color.white;
            } else
            {
                icons[i].color = new Color(0, 0, 0, 0);
            }

            if (types[i] != -1)
            {
                quantities[i].text = GetNumber(i).ToString();
            } else
            {
                quantities[i].text = "";
            }
        }
    }

    public void Open(HexArmy army)
    {
        this.army = army;

        for (int i = 0; i < noStacks; i++)
        {
            types[i] = army.Troops[i].Type;
            SetNumber(i);
        }

        UpdateUI();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
