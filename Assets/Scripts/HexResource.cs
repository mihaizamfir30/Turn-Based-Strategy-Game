using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HexResource : MonoBehaviour
{

    public int Type;
    public int Quantity { get; set; }

    public HexGrid Grid { get; set; }

    public HexCell Location
    {
        get
        {
            return location;
        }
        set
        {
            if (location)
            {
                location.Resource = null;
            }

            location = value;
            value.Resource = this;
            transform.localPosition = value.Position;
            Grid.MakeChildOfColumn(transform, value.ColumnIndex);
        }
    }

    HexCell location;

    public void ValidateLocation()
    {
        transform.localPosition = location.Position;
    }

    public void Die()
    {
        if (location)
        {
            location.Resource = null;
        }

        Destroy(gameObject);
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(Type);
        location.coordinates.Save(writer);
        writer.Write(Quantity);
    }

    public static void Load(BinaryReader reader, HexGrid grid)
    {
        int resourceType = reader.ReadInt32();
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        int quantity = reader.ReadInt32();

        grid.AddResource(resourceType, grid.GetCell(coordinates), quantity);
    }
}
