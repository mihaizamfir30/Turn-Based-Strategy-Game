using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HexBuilding : MonoBehaviour
{

    public int PlayerId = 0;
    public int Type;

    public int VisionRange
    {
        get
        {
            if (Type == 0)
            {
                return 3;
            } else
            {
                return -1;
            }
        }
    }

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

                if (VisionRange != -1 && PlayerId != -1)
                {
                    Grid.DecreaseVisibility(location, VisionRange, PlayerId);
                }
                
                location.Building = null;
            }

            location = value;
            value.Building = this;

            if (VisionRange != -1 && PlayerId != -1)
            {
                Grid.IncreaseVisibility(value, VisionRange, PlayerId);
            }

            transform.localPosition = value.Position;
            Grid.MakeChildOfColumn(transform, value.ColumnIndex);

        }
    }

    public HexDirection Orientation
    {
        get
        {
            return orientation;
        }
        set
        {
            orientation = value;
            transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 330 - 60 * (int)orientation, 0.0f));
            //transform.localRotation = Quaternion.Euler(0f, value, 0f);
        }
    }

    HexCell location;
    HexDirection orientation;

    public void Die()
    {
        
        if (location)
        {
            if (VisionRange != -1 && PlayerId != -1)
            {
                Grid.DecreaseVisibility(location, VisionRange, PlayerId);
            }
            location.Building = null;
        }

        Destroy(gameObject);
    }

    public void ValidateLocation()
    {
        transform.localPosition = location.Position;
    }

    public void OnTurnEnded()
    {

    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(Type);
        location.coordinates.Save(writer);
        writer.Write((int)orientation);
    }

    public static void Load(BinaryReader reader, HexGrid grid)
    {

        int buildingType = reader.ReadInt32();
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        HexDirection orientation = (HexDirection)reader.ReadInt32();

        grid.AddBuilding(buildingType, grid.GetCell(coordinates), orientation);
    }

}
