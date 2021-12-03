using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AdventureMapStructure : MonoBehaviour
{

    public HexCell location;
    public HexDirection orientation;
    public string BuildingName;

    void Start()
    {
        //Debug.Log("Hello from " + BuildingName);
    }

    //public virtual void OnVisited(AdventureMap.AdventureHero hero)

    public virtual void OnVisited()
    {
        Debug.Log("Hello, I am building");
    }

    public virtual void OnLeft()
    {

    }

    public int MovementCostToEnterBuilding(HexCell fromCell, HexCell toCell, HexDirection direction)
    {

        return 0;

        // Can only enter from a direction that matches orientation

        if (direction == orientation)
        {
            return 0;
        }

        return -1;

    }

    public void Save(BinaryWriter writer)
    {

    }

    public void Load(BinaryReader reader, int header)
    {

    }
}