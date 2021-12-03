using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdventureMap
{
    public class AdventureHero : MonoBehaviour
    {

        public string HeroName => heroName;

        public string Class => className;

        public int Level => level;


        [SerializeField]
        string heroName;

        [SerializeField]
        int level;

        [SerializeField]
        string className;

        [SerializeField]
        public ArmyStack[] Army;

        [SerializeField]
        int[] types;

        [SerializeField]
        int[] sizes;

        // Start is called before the first frame update
        void Start()
        {

            Army = new ArmyStack[7];

            for (int i = 0; i < sizes.Length; i++)
            {

                if (types[i] == -1)
                {
                    continue;
                }

                Army[i] = new ArmyStack(types[i], sizes[i]);
            }

        }

        // Merge Stacks
        // Swap Stacks
        // Split Stack
        // Move Stack
        // Delete Stack

        public void MoveSlotOnSlot(int slotFrom, int slotTo)
        {
            //Debug.Log("MoveSlotOnSlot " + slotFrom + " to " + slotTo);

            // If starting slot is same as destination slot
            if (slotFrom == slotTo)
            {
                return;
            }

            // If starting slot is empty
            if (Army[slotFrom] == null)
            {
                return;
            }

            // Move to empty slot
            if (Army[slotTo] == null)
            {
                MoveStack(slotFrom, slotTo);
                return;
            }

            // The destination slot is not empty

            // The two slots contain stacks of different types, so swap them
            if (Army[slotFrom].Type != Army[slotTo].Type)
            {
                SwapStacks(slotFrom, slotTo);
                return;
            }

            MergeStacks(slotFrom, slotTo);

            // The two stacks are of the same type
            //Debug.Assert(Army[slotFrom].Type == Army[slotTo].Type, "WTF");

        }

        // Returns the minimum number of units that have to remain in each stack
        public int[] BeginSplit(int slotFrom, int slotTo)
        {

            int[] ret = new int[2];
            ret[0] = -1;
            ret[1] = -1;

            // Starting slot and destination slot are the same. Do nothing
            if (slotFrom == slotTo)
            {
                return ret;
            }

            // The starting slot is empty. Do nothing
            if (Army[slotFrom] == null)
            {
                return ret;
            }

            // The starting slot is not empty.

            // The destination slot is not empty and of a different type. Can't split
            if (Army[slotTo] != null && Army[slotFrom].Type != Army[slotTo].Type)
            {
                return ret;
            }

            // The destination slot is empty, or of the same type. 
            // TODO: Check if we are moving to a different army, because we can't lose our final stack.
            ret[0] = 0;
            ret[1] = 0;

            return ret;
        }

        public void MoveStack(int slotFrom, int slotTo)
        {

            Debug.Log("MoveStack");

            Army[slotTo] = Army[slotFrom];
            Army[slotFrom] = null;
        }

        public void SwapStacks(int slotFrom, int slotTo)
        {
            Debug.Log("SwapStacks");


            ArmyStack slotRef = Army[slotFrom];
            Army[slotFrom] = Army[slotTo];
            Army[slotTo] = slotRef;
        }

        public void MergeStacks(int slotFrom, int slotTo)
        {

            Debug.Log("MergeStacks");


            Army[slotTo].Merge(Army[slotFrom]);
            Army[slotFrom] = null;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Print();
            }
        }

        void Print()
        {
            for (int i = 0; i < 7; i++)
            {

                /*
                [SerializeField]
                public ArmyStack[] Army;
                */

                if (Army[i] == null)
                {
                    Debug.Log(i + ". -1 -1");
                    continue;
                }

                Army[i].Print(i);

            }
        }
    }
}
