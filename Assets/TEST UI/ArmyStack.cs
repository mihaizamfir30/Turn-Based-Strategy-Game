using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class ArmyStack
{

    public int Type;
    public int Size;

    public ArmyStack(int Type, int Size)
    {
        this.Type = Type;
        this.Size = Size;
    }

    public void Merge(ArmyStack other)
    {
        this.Size += other.Size;
    }

    public void Print(int index)
    {
        Debug.Log(index  + ". " + Type + " - " + Size);
    }


    // 

}
