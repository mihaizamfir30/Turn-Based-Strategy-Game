using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIArmySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{

    Transform stackHolder;
    Image stackImage;
    TMP_Text stackSize;

    AdventureMap.AdventureHeroScreen adventureHeroUI;
    ArmyStack armyStack;

    int id;

    bool isMouseOver;

    public bool IsSplittingWhileMouseOver => isSplittingWhileMouseOver;
    bool isSplittingWhileMouseOver;

    bool isDragging;

    public bool IsSplitting => isSplitting;
    bool isSplitting;

    // Start is called before the first frame update
    void Start()
    {
        CacheComponents();
    }

    void Update()
    {

        if (isDragging == false)
        {
            return;
        }

        if (Input.GetKeyDown(GameplayMetrics.SplitKeyCode))
        {
            isSplitting = true;
        }


        stackHolder.position = Input.mousePosition;

        return;

        if (isMouseOver)
        {
            if (Input.GetKeyDown(GameplayMetrics.SplitKeyCode))
            {
                isSplittingWhileMouseOver = true;
            }
        }

        if (isDragging)
        {
            if (Input.GetKeyDown(GameplayMetrics.SplitKeyCode))
            {
                isSplitting = true;
            }

            stackHolder.position = Input.mousePosition;
        }

        //stackHolder.position = eventData.position;
    }

    public void Init(AdventureMap.AdventureHeroScreen avdentureHeroScreen, int id)
    {
        this.adventureHeroUI = avdentureHeroScreen;
        this.id = id;

        stackHolder.gameObject.name = id.ToString();
    }

    void CacheComponents()
    {
        stackHolder = gameObject.transform.GetChild(0);
        stackImage = stackHolder.GetChild(0).GetComponent<Image>();
        stackSize = stackHolder.GetChild(1).GetComponent<TMP_Text>();
    }

    public void SetStack(ArmyStack armyStack)
    {
        this.armyStack = armyStack;
        UpdateUI();
    }

    void UpdateUI()
    {

        if (armyStack == null)
        {
            stackSize.text = "";
            stackImage.color = new Color(0, 0, 0, 0);
            return;
        }

        stackSize.text = GameUI.Utils.IntToString(armyStack.Size);

        if (armyStack.Type == 0)
        {
            stackImage.color = Color.red;
        }

        if (armyStack.Type == 1)
        {
            stackImage.color = Color.blue;
        }

        if (armyStack.Type == 2)
        {
            stackImage.color = Color.green;
        }

        if (armyStack.Type == 3)
        {
            stackImage.color = Color.gray;
        }
    }

    bool ShouldSplit(UIArmySlot otherUIArmySlot)
    {

        return otherUIArmySlot.IsSplitting == true;

        bool ret = otherUIArmySlot.IsSplitting == true || otherUIArmySlot.IsSplittingWhileMouseOver == true;
        otherUIArmySlot.OnDropped();
        return ret;
    }

    public void OnDropped()
    {
        isSplitting = false;
        isSplittingWhileMouseOver = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        UIArmySlot otherUIArmySlot = eventData.pointerDrag.GetComponent<UIArmySlot>();
        if (otherUIArmySlot == null)
        {
            return;
        }

        if (2 == 3)
        {
            return;
        }

        if (ShouldSplit(otherUIArmySlot))
        {
            adventureHeroUI.BeginSplit(otherUIArmySlot.id, id);
        }
        else
        {
            adventureHeroUI.MoveSlotOnSlot(otherUIArmySlot.id, id);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        stackHolder.SetParent(this.transform.parent.parent);
        isDragging = true;
        isSplitting = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //stackHolder.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        stackHolder.SetParent(transform);
        stackHolder.localPosition = Vector3.zero;
        isDragging = false;
        isSplitting = false;
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        isMouseOver = true;

        if (isDragging == false)
        {
            //IsSplitting = false;
            //isSplittingWhileMouseOver = false;
        }

    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        isMouseOver = false;

        if (isDragging == false)
        {
            isSplittingWhileMouseOver = false;
        }

    }


}


//https://www.facebook.com/photo/?fbid=10225247694047738&set=gm.1267760240286744