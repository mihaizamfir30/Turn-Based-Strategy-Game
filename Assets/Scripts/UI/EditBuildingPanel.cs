using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditBuildingPanel : MonoBehaviour
{

    Text type;
    Toggle[] toggles;

    Button cancelButton;
    Button editButton;

    HexMapEditor editor;
    HexBuilding building;

    int orientation;

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
        type = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>();
        toggles = GetComponentsInChildren<Toggle>();

        //cancelButton
        cancelButton = transform.GetChild(0).GetChild(0).GetChild(7).GetComponent<Button>();
        editButton = transform.GetChild(0).GetChild(0).GetChild(8).GetComponent<Button>();

        cancelButton.onClick.AddListener(
            () =>
            {
                editor.OnEditBuildingFinished(building, -1);
                Close();
            }
        );

        editButton.onClick.AddListener(
            () =>
            {
                editor.OnEditBuildingFinished(building, orientation);
                Close();
            }
        );
    }

    public void SetBuildingOrientation(int value)
    {
        orientation = value;
    }

    public void Open(HexBuilding building)
    {
        this.building = building;
        gameObject.SetActive(true);

        type.text = GameplayMetrics.BuildingNames[building.Type];
        toggles[(int)building.Orientation].isOn = true;

    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
