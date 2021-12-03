using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class UIArmyStack : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    Transform container;
    Transform parentToReturnTo;

    Image image;

    ArmyStack armyStack;

    TMP_Text stackSize;

    public Sprite sprite;
    public int id;


    // Start is called before the first frame update
    void Start()
    {

        CacheComponents();

        container = this.transform.parent.parent;

    }


    public void Init(int id)
    {
        this.id = id;
    }

    void CacheComponents()
    {
        stackSize = gameObject.transform.GetChild(0).GetComponent<TMP_Text>();
        image = GetComponent<Image>();
        parentToReturnTo = this.transform.parent;
    }

    public void SetStack(ArmyStack armyStack)
    {
        this.armyStack = armyStack;
        UpdateUI();
    }

    public void UpdateUI()
    {

        if (armyStack == null)
        {
            stackSize.text = "";
            image.color = new Color(0, 0, 0, 0);
            return;
        }

        stackSize.text = armyStack.Size.ToString();

        if (armyStack.Type == 0)
        {
            image.color = Color.red;
        }

        if (armyStack.Type == 1)
        {
            image.color = Color.blue;
        }
        
        if (armyStack.Type == 2)
        {
            image.color = Color.green;
        }

        if (armyStack.Type == 3)
        {
            image.color = Color.gray;
        }

    }

    public void SetRaycastTargetState(bool state)
    {
        if (image != null)
        {
            image.raycastTarget = state;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        SetRaycastTargetState(false);
        
        this.transform.SetParent(this.transform.parent.parent.parent);

    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("OnDrag");

        /*
        Debug.Log(transform.localPosition + " - " + container.localPosition + " - " + container.name + " - " +
            (this.transform.parent == container.parent).ToString());
        */

        this.transform.position = eventData.position;


    }

    public void OnEndDrag(PointerEventData eventData)
    {

        this.transform.parent = parentToReturnTo;
        this.transform.localPosition = Vector3.zero;

        image.raycastTarget = true;

    }

}