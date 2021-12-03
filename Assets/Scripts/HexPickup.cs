using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HexPickup : MonoBehaviour
{

    public int PickupType;

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
                location.Pickup = null;
            }

            location = value;
            value.Pickup = this;
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
            location.Pickup = null;
        }

        Destroy(gameObject);
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(PickupType);
        location.coordinates.Save(writer);
    }

    public static void Load(BinaryReader reader, HexGrid grid)
    {
        int pickupType = reader.ReadInt32();
        HexCoordinates coordinates = HexCoordinates.Load(reader);

        grid.AddPickup(pickupType, grid.GetCell(coordinates));
    }

}
