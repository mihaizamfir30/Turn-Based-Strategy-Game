using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HexTroop
{

    public HexArmy Army;

    public int Type;
    public int Quantity;

    public HexTroop(HexArmy army, int type, int quantity)
    {
        Army = army;
        Type = type;
        Quantity = quantity;
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(Type);
        writer.Write(Quantity);
    }

    public static void Load(BinaryReader reader, HexArmy army, int index)
    {
        int type = reader.ReadInt32();
        int quantity = reader.ReadInt32();

        army.EditTroop(index, type, quantity);
    }
}
