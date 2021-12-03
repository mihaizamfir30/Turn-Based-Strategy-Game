using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour {

	public int cellCountX = 20, cellCountZ = 15;

	public bool wrapping;

	public HexCell cellPrefab;
	public Text cellLabelPrefab;
	public HexGridChunk chunkPrefab;

	public HexUnit unitPrefab;
	public HexBuilding[] buildingPrefabs;
	public HexPickup[] pickupPrefabs;
	public HexResource[] resourcePrefabs;
	public HexArmy armyPrefab;

	public Texture2D noiseSource;

	public Texture2D[] troopIcons;

	public int seed;

	public bool HasPath {
		get {
			return currentPathExists;
		}
	}

	Transform[] columns;
	HexGridChunk[] chunks;
	HexCell[] cells;
	HexPlayer[] players;
	int currentPlayerId;
	int turnId;

	int chunkCountX, chunkCountZ;

	HexCellPriorityQueue searchFrontier;

	int searchFrontierPhase;

	HexCell currentPathFrom, currentPathTo;
	bool currentPathExists;

	int currentCenterColumnIndex = -1;

	List<HexUnit> units = new List<HexUnit>();
	List<HexBuilding> buildings = new List<HexBuilding>();
	List<HexPickup> pickups = new List<HexPickup>();
	List<HexResource> resources = new List<HexResource>();
	List<HexArmy> armies = new List<HexArmy>();

	HexCellShaderData cellShaderData;

	void Awake() {
		HexMetrics.noiseSource = noiseSource;
		GameplayMetrics.TroopIcons = troopIcons;
		HexMetrics.InitializeHashGrid(seed);
		HexUnit.unitPrefab = unitPrefab;
		cellShaderData = gameObject.AddComponent<HexCellShaderData>();
		cellShaderData.Grid = this;
		CreateMap(cellCountX, cellCountZ, wrapping);
	}

	public void OnPlayerEndedTurn()
    {

		Debug.Log("HexGrid::Player " + currentPlayerId + " has ended his turn!");
		players[currentPlayerId].OnPlayerTurnEnded();

		currentPlayerId += 1;

		if (currentPlayerId == players.Length)
        {
			currentPlayerId = 0;
			OnTurnEnded();

            for (int i = 0; i < players.Length; i++)
            {
				players[i].OnTurnEnded();
            }
		}
	}

	public void OnTurnEnded()
    {
		Debug.Log("TURN ENDED");

        for (int i = 0; i < buildings.Count; i++)
        {
			buildings[i].OnTurnEnded();
		}

		turnId++;
		if (turnId % 7 == 0)
        {
			OnWeekEnded();
        }
	}

	public void OnWeekEnded()
    {
    }

	public void AddUnit(HexCell location, float orientation) {

		if (location.Unit != null || location.Building != null || location.Pickup != null || location.Resource != null || location.Army != null)
		{
			return;
		}

		HexUnit unit = Instantiate(unitPrefab);
		units.Add(unit);
		unit.Grid = this;
		unit.Location = location;
		unit.Orientation = orientation;
	}

	public void RemoveUnit(HexUnit unit) {
		units.Remove(unit);
		unit.Die();
	}

	public void AddBuilding(int buildingTypeIndex, HexCell location, HexDirection buildingOrientation)
	{

		if (location.Unit != null || location.Building != null || location.Pickup != null || location.Resource != null || location.Army != null)
        {
			return;
        }

		HexBuilding building = Instantiate(buildingPrefabs[buildingTypeIndex]);
		building.Grid = this;
		building.Location = location;
		building.Orientation = buildingOrientation;

		buildings.Add(building);
	}

	public void EditBuilding(HexBuilding building, HexDirection orientation)
	{
		building.Orientation = orientation;
	}

	public void RemoveBuilding(HexBuilding building)
    {
		buildings.Remove(building);
		building.Die();
    }

	public void AddPickup(int pickupTypeIndex, HexCell location)
    {

		if (location.Unit != null || location.Building != null || location.Pickup != null || location.Resource != null || location.Army != null)
		{
			return;
		}

		HexPickup pickup = Instantiate(pickupPrefabs[pickupTypeIndex]);
		pickup.Grid = this;
		pickup.Location = location;

		pickups.Add(pickup);
	}

	public void RemovePickup(HexPickup pickup)
    {
		pickups.Remove(pickup);
		pickup.Die();
	}

	public void AddResource(int resourceTypeIndex, HexCell location, int quantity = 1)
    {

		if (location.Unit != null || location.Building != null || location.Pickup != null || location.Resource != null || location.Army != null)
		{
			return;
		}

		HexResource resource = Instantiate(resourcePrefabs[resourceTypeIndex]);
		resource.Grid = this;
		resource.Location = location;
		resource.Quantity = quantity;

		resources.Add(resource);
	}

	public void EditResource(HexResource resource, int quantity)
    {
		resource.Quantity = quantity;
    }

	public void RemoveResource(HexResource resource)
    {
		resources.Remove(resource);
		resource.Die();
	}

	public void AddArmy(HexCell location, HexDirection armyOrientation, int[] troopInfo = null)
    {

		if (location.Unit != null || location.Building != null || location.Pickup != null || location.Resource != null || location.Army != null)
		{
			return;
		}

		HexArmy army = Instantiate(armyPrefab);
		army.Grid = this;
		army.Location = location;
		army.Orientation = armyOrientation;

		if (troopInfo != null)
        {
			army.Troops = new HexTroop[troopInfo.Length / 2];

			for (int i = 0; i < troopInfo.Length; i += 2)
            {
				army.Troops[i / 2] = new HexTroop(army, troopInfo[i], troopInfo[i + 1]);
				Debug.Log("TROOP " + i / 2 + " - " + army.Troops[i / 2].Type + " " + army.Troops[i / 2].Quantity);
            }
        }

		armies.Add(army);
	}

	public void EditArmy(HexArmy army, int[][] serializedArmy)
	{
        for (int i = 0; i < serializedArmy.Length; i++)
        {
			army.Troops[i].Type = serializedArmy[i][0];
			army.Troops[i].Quantity = serializedArmy[i][1];
		}
	}

	public void RemoveArmy(HexArmy army)
    {
		armies.Remove(army);
		army.Die();
	}

	public void MakeChildOfColumn (Transform child, int columnIndex) {
		child.SetParent(columns[columnIndex], false);
	}

	public bool CreateMap (int x, int z, bool wrapping) {
		if (
			x <= 0 || x % HexMetrics.chunkSizeX != 0 ||
			z <= 0 || z % HexMetrics.chunkSizeZ != 0
		) {
			Debug.LogError("Unsupported map size.");
			return false;
		}

		ClearPath();
		ClearUnits();
		if (columns != null) {
			for (int i = 0; i < columns.Length; i++) {
				Destroy(columns[i].gameObject);
			}
		}

		cellCountX = x;
		cellCountZ = z;
		this.wrapping = wrapping;
		currentCenterColumnIndex = -1;
		HexMetrics.wrapSize = wrapping ? cellCountX : 0;
		chunkCountX = cellCountX / HexMetrics.chunkSizeX;
		chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;
		cellShaderData.Initialize(cellCountX, cellCountZ);
		CreateChunks();
		CreateCells();
		CreatePlayers();
		return true;
	}

	void CreateChunks () {
		columns = new Transform[chunkCountX];
		for (int x = 0; x < chunkCountX; x++) {
			columns[x] = new GameObject("Column").transform;
			columns[x].SetParent(transform, false);
		}

		chunks = new HexGridChunk[chunkCountX * chunkCountZ];
		for (int z = 0, i = 0; z < chunkCountZ; z++) {
			for (int x = 0; x < chunkCountX; x++) {
				HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
				chunk.transform.SetParent(columns[x], false);
			}
		}
	}

	void CreateCells () {
		cells = new HexCell[cellCountZ * cellCountX];

		for (int z = 0, i = 0; z < cellCountZ; z++) {
			for (int x = 0; x < cellCountX; x++) {
				CreateCell(x, z, i++);
			}
		}
	}

	void CreatePlayers()
    {
		players = new HexPlayer[2];

        for (int i = 0; i < players.Length; i++)
        {
			players[i] = new HexPlayer();
			players[i].Initialize(cellCountX, cellCountZ);

		}
	}

	void ClearUnits () {
		for (int i = 0; i < units.Count; i++) {
			units[i].Die();
		}
		units.Clear();
	}

	void OnEnable () {
		if (!HexMetrics.noiseSource) {
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.InitializeHashGrid(seed);
			HexUnit.unitPrefab = unitPrefab;
			HexMetrics.wrapSize = wrapping ? cellCountX : 0;
			ResetVisibility();
		}
	}

	public HexCell GetCell (Ray ray) {
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			return GetCell(hit.point);
		}
		return null;
	}

	public HexCell GetCell (Vector3 position) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		return GetCell(coordinates);
	}

	public HexCell GetCell (HexCoordinates coordinates) {
		int z = coordinates.Z;
		if (z < 0 || z >= cellCountZ) {
			return null;
		}
		int x = coordinates.X + z / 2;
		if (x < 0 || x >= cellCountX) {
			return null;
		}
		return cells[x + z * cellCountX];
	}

	public HexCell GetCell (int xOffset, int zOffset) {
		return cells[xOffset + zOffset * cellCountX];
	}

	public HexCell GetCell (int cellIndex) {
		return cells[cellIndex];
	}

	public void ShowUI (bool visible) {
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].ShowUI(visible);
		}
	}

	void CreateCell (int x, int z, int i) {
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * HexMetrics.innerDiameter;
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.Index = i;
		cell.ColumnIndex = x / HexMetrics.chunkSizeX;
		cell.ShaderData = cellShaderData;

		if (wrapping) {
			cell.Explorable = z > 0 && z < cellCountZ - 1;
		}
		else {
			cell.Explorable =
				x > 0 && z > 0 && x < cellCountX - 1 && z < cellCountZ - 1;
		}

		if (x > 0) {
			cell.SetNeighbor(HexDirection.W, cells[i - 1]);
			if (wrapping && x == cellCountX - 1) {
				cell.SetNeighbor(HexDirection.E, cells[i - x]);
			}
		}
		if (z > 0) {
			if ((z & 1) == 0) {
				cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
				if (x > 0) {
					cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
				}
				else if (wrapping) {
					cell.SetNeighbor(HexDirection.SW, cells[i - 1]);
				}
			}
			else {
				cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
				if (x < cellCountX - 1) {
					cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
				}
				else if (wrapping) {
					cell.SetNeighbor(
						HexDirection.SE, cells[i - cellCountX * 2 + 1]
					);
				}
			}
		}

		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		cell.uiRect = label.rectTransform;

		cell.Elevation = 0;

		AddCellToChunk(x, z, cell);
	}

	void AddCellToChunk (int x, int z, HexCell cell) {
		int chunkX = x / HexMetrics.chunkSizeX;
		int chunkZ = z / HexMetrics.chunkSizeZ;
		HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
		chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
	}

	public void Save (BinaryWriter writer) {
		writer.Write(cellCountX);
		writer.Write(cellCountZ);
		writer.Write(wrapping);

		for (int i = 0; i < cells.Length; i++) {
			cells[i].Save(writer);
		}

		// Save units
		writer.Write(units.Count);
		for (int i = 0; i < units.Count; i++) {
			units[i].Save(writer);
		}

		// Save buildings
		writer.Write(buildings.Count);
        for (int i = 0; i < buildings.Count; i++)
        {
			buildings[i].Save(writer);
        }

		// Save pickups
		writer.Write(pickups.Count);
		for (int i = 0; i < pickups.Count; i++)
		{
			pickups[i].Save(writer);
		}

		// Save resources
		writer.Write(resources.Count);
		for (int i = 0; i < resources.Count; i++)
		{
			resources[i].Save(writer);
		}

		// Save armies
		writer.Write(armies.Count);
		for (int i = 0; i < armies.Count; i++)
		{
			armies[i].Save(writer);
		}

	}

	public void Load (BinaryReader reader, int header) {
		ClearPath();
		ClearUnits();
		int x = 20, z = 15;
		if (header >= 1) {
			x = reader.ReadInt32();
			z = reader.ReadInt32();
		}
		bool wrapping = header >= 5 ? reader.ReadBoolean() : false;
		if (x != cellCountX || z != cellCountZ || this.wrapping != wrapping) {
			if (!CreateMap(x, z, wrapping)) {
				return;
			}
		}

		bool originalImmediateMode = cellShaderData.ImmediateMode;
		cellShaderData.ImmediateMode = true;

		for (int i = 0; i < cells.Length; i++) {
			cells[i].Load(reader, header);
		}
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].Refresh();
		}

		// Load units
		if (header >= 2) {
			int unitCount = reader.ReadInt32();
			for (int i = 0; i < unitCount; i++) {
				HexUnit.Load(reader, this);
			}
		}

		// Load buildings, pickups, resources and armies
		if (header >= 6)
        {
			//lovers_love99
			//flexair
			int buildingsCount = reader.ReadInt32();
            for (int i = 0; i < buildingsCount; i++)
            {
				HexBuilding.Load(reader, this);
            }

			int pickupsCount = reader.ReadInt32();
			for (int i = 0; i < pickupsCount; i++)
			{
				HexPickup.Load(reader, this);
			}

			int resourcesCount = reader.ReadInt32();
			for (int i = 0; i < resourcesCount; i++)
			{
				HexResource.Load(reader, this);
			}

			int armiesCount = reader.ReadInt32();
			for (int i = 0; i < armiesCount; i++)
			{
				HexArmy.Load(reader, this);
			}

		}

		cellShaderData.ImmediateMode = originalImmediateMode;
	}

	public List<HexCell> GetPath () {
		if (!currentPathExists) {
			return null;
		}
		List<HexCell> path = ListPool<HexCell>.Get();
		for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathFrom) {
			path.Add(c);
		}
		path.Add(currentPathFrom);
		path.Reverse();
		return path;
	}

	public void ClearPath () {
		if (currentPathExists) {
			HexCell current = currentPathTo;
			while (current != currentPathFrom) {
				current.SetLabel(null);
				current.DisableHighlight();
				current = current.PathFrom;
			}
			current.DisableHighlight();
			currentPathExists = false;
		}
		else if (currentPathFrom) {
			currentPathFrom.DisableHighlight();
			currentPathTo.DisableHighlight();
		}
		currentPathFrom = currentPathTo = null;
	}

	void ShowPath (int speed) {
		if (currentPathExists) {
			HexCell current = currentPathTo;
			while (current != currentPathFrom) {
				int turn = (current.Distance - 1) / speed;
				current.SetLabel(turn.ToString());
				current.EnableHighlight(Color.white);
				current = current.PathFrom;
			}
		}
		currentPathFrom.EnableHighlight(Color.blue);
		currentPathTo.EnableHighlight(Color.red);
	}

	public void FindPath (HexCell fromCell, HexCell toCell, HexUnit unit) {
		ClearPath();
		currentPathFrom = fromCell;
		currentPathTo = toCell;
		currentPathExists = Search(fromCell, toCell, unit);
		ShowPath(unit.Speed);
	}

	bool Search (HexCell fromCell, HexCell toCell, HexUnit unit) {
		int speed = unit.Speed;
		searchFrontierPhase += 2;
		if (searchFrontier == null) {
			searchFrontier = new HexCellPriorityQueue();
		}
		else {
			searchFrontier.Clear();
		}

		fromCell.SearchPhase = searchFrontierPhase;
		fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);
		while (searchFrontier.Count > 0) {
			HexCell current = searchFrontier.Dequeue();
			current.SearchPhase += 1;

			if (current == toCell) {
				return true;
			}

			int currentTurn = (current.Distance - 1) / speed;

			HexCell[] neighbors = current.GetNeighborsForPathfinding();


			/*
			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
				HexCell neighbor = current.GetNeighbor(d);
			}
			*/

			for (int i = 0; i < neighbors.Length; i++)
            {
				HexCell neighbor = neighbors[i];
				HexDirection d = (HexDirection)i;

				if (neighbor == null)
                {
					continue;
                }

				if (neighbor.SearchPhase > searchFrontierPhase) {
					continue;
				}
				if (!unit.IsValidDestination(neighbor)) {
					continue;
				}
				int moveCost = unit.GetMoveCost(current, neighbor, d);
				if (moveCost < 0) {
					continue;
				}

				int distance = current.Distance + moveCost;
				int turn = (distance - 1) / speed;
				if (turn > currentTurn) {
					distance = turn * speed + moveCost;
				}

				if (neighbor.SearchPhase < searchFrontierPhase) {
					neighbor.SearchPhase = searchFrontierPhase;
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					neighbor.SearchHeuristic =
						neighbor.coordinates.DistanceTo(toCell.coordinates);
					searchFrontier.Enqueue(neighbor);
				}
				else if (distance < neighbor.Distance) {
					int oldPriority = neighbor.SearchPriority;
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					searchFrontier.Change(neighbor, oldPriority);
				}
			}
		}
		return false;
	}

	public void IncreaseVisibility (HexCell fromCell, int range, int playerId) {
		List<HexCell> cells = GetVisibleCells(fromCell, range);
		for (int i = 0; i < cells.Count; i++) {
			cells[i].IncreaseVisibility(playerId);
		}
		ListPool<HexCell>.Add(cells);
	}

	public void DecreaseVisibility (HexCell fromCell, int range, int playerId) {
		List<HexCell> cells = GetVisibleCells(fromCell, range);
		for (int i = 0; i < cells.Count; i++) {
			cells[i].DecreaseVisibility(playerId);
		}
		ListPool<HexCell>.Add(cells);
	}

	public void ResetVisibility () {
		for (int i = 0; i < cells.Length; i++) {
			cells[i].ResetVisibility();
		}
		// TODO: Reset Visibility for ALL players
		for (int i = 0; i < units.Count; i++) {
			HexUnit unit = units[i];
			IncreaseVisibility(unit.Location, unit.VisionRange, 0);
		}
	}

	List<HexCell> GetVisibleCells (HexCell fromCell, int range) {
		List<HexCell> visibleCells = ListPool<HexCell>.Get();

		searchFrontierPhase += 2;
		if (searchFrontier == null) {
			searchFrontier = new HexCellPriorityQueue();
		}
		else {
			searchFrontier.Clear();
		}

		range += fromCell.ViewElevation;
		fromCell.SearchPhase = searchFrontierPhase;
		fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);
		HexCoordinates fromCoordinates = fromCell.coordinates;
		while (searchFrontier.Count > 0) {
			HexCell current = searchFrontier.Dequeue();
			current.SearchPhase += 1;
			visibleCells.Add(current);

			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
				HexCell neighbor = current.GetNeighbor(d);
				if (
					neighbor == null ||
					neighbor.SearchPhase > searchFrontierPhase ||
					!neighbor.Explorable
				) {
					continue;
				}

				int distance = current.Distance + 1;
				if (distance + neighbor.ViewElevation > range ||
					distance > fromCoordinates.DistanceTo(neighbor.coordinates)
				) {
					continue;
				}

				if (neighbor.SearchPhase < searchFrontierPhase) {
					neighbor.SearchPhase = searchFrontierPhase;
					neighbor.Distance = distance;
					neighbor.SearchHeuristic = 0;
					searchFrontier.Enqueue(neighbor);
				}
				else if (distance < neighbor.Distance) {
					int oldPriority = neighbor.SearchPriority;
					neighbor.Distance = distance;
					searchFrontier.Change(neighbor, oldPriority);
				}
			}
		}
		return visibleCells;
	}

	public void CenterMap (float xPosition) {
		int centerColumnIndex = (int)
			(xPosition / (HexMetrics.innerDiameter * HexMetrics.chunkSizeX));
		
		if (centerColumnIndex == currentCenterColumnIndex) {
			return;
		}
		currentCenterColumnIndex = centerColumnIndex;

		int minColumnIndex = centerColumnIndex - chunkCountX / 2;
		int maxColumnIndex = centerColumnIndex + chunkCountX / 2;

		Vector3 position;
		position.y = position.z = 0f;
		for (int i = 0; i < columns.Length; i++) {
			if (i < minColumnIndex) {
				position.x = chunkCountX *
					(HexMetrics.innerDiameter * HexMetrics.chunkSizeX);
			}
			else if (i > maxColumnIndex) {
				position.x = chunkCountX *
					-(HexMetrics.innerDiameter * HexMetrics.chunkSizeX);
			}
			else {
				position.x = 0f;
			}
			columns[i].localPosition = position;
		}
	}
}