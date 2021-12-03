using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AdventureMap
{
    public class AdventureHeroScreen : MonoBehaviour
    {

        //http://liga1defotbal.blogspot.com/

        public AdventureHero targetHero;

        TMP_Text heroName;
        TMP_Text heroLevel;

        UIArmySlot[] uiArmySlots;

        // Stack Split Window
        UIStackSplitWindow stackSplitWindow;
        Image[] stackSplitIcons;
        TMP_Text[] stackSplitSizes;
        Slider splitSlider;

        // Start is called before the first frame update
        void Start()
        {
            CacheComponents();
            Open(targetHero);
        }

        void CacheComponents()
        {
            heroName = gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMP_Text>();
            heroLevel = gameObject.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<TMP_Text>();

            uiArmySlots = new UIArmySlot[GameplayMetrics.StacksPerArmy];

            Transform stacksParent = gameObject.transform.GetChild(0).GetChild(1).GetChild(0);

            for (int i = 0; i < uiArmySlots.Length; i++)
            {
                uiArmySlots[i] = stacksParent.GetChild(i).GetComponent<UIArmySlot>();
                uiArmySlots[i].Init(this, i);
            }

            // Stack Split Window
            stackSplitWindow = gameObject.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<UIStackSplitWindow>();
            stackSplitWindow.Close();
        }

        public void Open(AdventureHero targetHero)
        {
            this.targetHero = targetHero;
            UpdateUI();
        }

        public void Close()
        {
            targetHero = null;
        }

        void UpdateUI()
        {
            heroName.text = targetHero.HeroName;
            heroLevel.text = "Level " + targetHero.Level.ToString() + " " + targetHero.Class;

            for (int i = 0; i < GameplayMetrics.StacksPerArmy; i++)
            {
                uiArmySlots[i].SetStack(targetHero.Army[i]);
            }
        }

        public void BeginSplit(int slotFrom, int slotTo)
        {

            Debug.Log("AdventureHeroScreen::BeginSplit " + slotFrom + " " + slotTo);

            int[] ret = targetHero.BeginSplit(slotFrom, slotTo);

            if (ret[0] == -1 && ret[1] == -1)
            {
                return;
            }

            stackSplitWindow.Open();
        }

        public void MoveSlotOnSlot(int slotFrom, int slotTo)
        {
            targetHero.MoveSlotOnSlot(slotFrom, slotTo);
            UpdateUI();
        }
    }
}
