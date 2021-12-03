using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HexPlayer
{

    public int Id;
    public Color Color;

    public int[] Resources;

    // Volatile Data

    // Visibility Data
    public bool[] cellVisibilityData;

    public void Initialize(int x, int z)
    {
        if (cellVisibilityData == null || cellVisibilityData.Length != x * z)
        {
            cellVisibilityData = new bool[2 * x * z];
        }

        Resources = new int[GameplayMetrics.ResourceNames.Length];
    }

    public void OnPlayerTurnEnded()
    {
        Debug.Log("HexPlayer::OnPlayerTurnEnded");
    }

    public void OnTurnEnded()
    {
        Debug.Log("HexPlayer::OnTurnEnded");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Save(BinaryWriter writer)
    {

    }

    public static void Load(BinaryReader reader, HexGrid grid)
    {

    }

    /*
    public void Save(BinaryWriter writer)
    {
        location.coordinates.Save(writer);
        writer.Write(orientation);
    }

    public static void Load(BinaryReader reader, HexGrid grid)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        float orientation = reader.ReadSingle();
        grid.AddUnit(grid.GetCell(coordinates), orientation);
    }
    */
}
