using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HexArmy : MonoBehaviour
{

    public HexGrid Grid { get; set; }

	public HexTroop[] Troops;

	int noStacks = 6;

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
				location.Army = null;
			}

			location = value;
			value.Army = this;
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
		}
	}

	HexCell location;

	HexDirection orientation;

	void Awake()
    {
		if (Troops == null)
		{
			Troops = new HexTroop[noStacks];

			Troops[0] = new HexTroop(this, 0, 1);

            for (int i = 1; i < noStacks; i++)
            {
				Troops[i] = new HexTroop(this, -1, 0);
            }
		}

	}

	public void EditTroop(int index, HexTroop troop)
    {
		Troops[index] = troop;
    }

	public void EditTroop(int index, int type, int quantity)
    {
		Troops[index].Type = type;
		Troops[index].Quantity = quantity;
    }

	public void ValidateLocation()
	{
		transform.localPosition = location.Position;
	}

	public void Die()
	{
		location.Army = null;
		Destroy(gameObject);
	}

	public void Save(BinaryWriter writer)
	{
		location.coordinates.Save(writer);
		writer.Write((int)orientation);

		writer.Write(Troops.Length);
        for (int i = 0; i < Troops.Length; i++)
        {
			Debug.Log("SAVE " + Troops[i].Type + " " + Troops[i].Quantity);
			writer.Write(Troops[i].Type);
			writer.Write(Troops[i].Quantity);
        }
	}

	public static void Load(BinaryReader reader, HexGrid grid)
	{
		HexCoordinates coordinates = HexCoordinates.Load(reader);
		HexDirection orientation = (HexDirection)reader.ReadInt32();

		int troopsLength = reader.ReadInt32();
		int[] troopInfo = new int[2 * troopsLength];

		for (int i = 0; i < troopsLength; i++)
        {
			troopInfo[2 * i + 0] = reader.ReadInt32();
			troopInfo[2 * i + 1] = reader.ReadInt32();
		}

		grid.AddArmy(grid.GetCell(coordinates), orientation, troopInfo);
	}

}
