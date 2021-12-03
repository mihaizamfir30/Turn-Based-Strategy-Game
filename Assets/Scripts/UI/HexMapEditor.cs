using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

	// Root GO that contains the Canvas for the In Game UI. All the scripts
	// used for the In Game UI are attached to this object. We reference it here
	// so we can enable/disable when entering edit mode.
	public GameObject inGameUI;

	public GameObject testPrefab = null;

	public HexGrid hexGrid;

	public Material terrainMaterial;

	int activeElevation;
	int activeWaterLevel;

	int activeUrbanLevel, activeFarmLevel, activePlantLevel, activeSpecialIndex, activeBuildingIndex, activePickupIndex, activeResourceIndex;
	HexDirection activeDirection;

	int activeTerrainTypeIndex;

	int brushSize;

	bool applyElevation = true;
	bool applyWaterLevel = true;

	bool applyUrbanLevel, applyFarmLevel, applyPlantLevel, applySpecialIndex;

	enum OptionalToggle {
		Ignore, Yes, No
	}

	OptionalToggle riverMode, roadMode, walledMode;

	bool isDrag;
	HexDirection dragDirection;
	HexCell previousCell;

	Dropdown directionDropdown;
	Dropdown buildingDropdown;
	Dropdown pickupDropdown;
	Dropdown resourceDropdown;

	EditUnitPanelUI editUnitPanel;
	EditBuildingPanel editBuildingPanel;
	EditPickupPanel editPickupPanel;
	EditResourcePanel editResourcePanel;
	EditArmyPanel editArmyPanel;

	bool isEditPanelOpen;

	void Start()
    {
		CacheComponents();
    }
	void CacheComponents()
    {
		directionDropdown = transform.GetChild(1).GetChild(8).gameObject.GetComponent<Dropdown>();
		buildingDropdown = transform.GetChild(1).GetChild(9).gameObject.GetComponent<Dropdown>();
		pickupDropdown = transform.GetChild(1).GetChild(10).gameObject.GetComponent<Dropdown>();
		resourceDropdown = transform.GetChild(1).GetChild(11).gameObject.GetComponent<Dropdown>();

		editUnitPanel = transform.GetComponentInChildren<EditUnitPanelUI>();
		editBuildingPanel = transform.GetComponentInChildren<EditBuildingPanel>();
		editPickupPanel = transform.GetComponentInChildren<EditPickupPanel>();
		editResourcePanel = transform.GetComponentInChildren<EditResourcePanel>();
		editArmyPanel = transform.GetComponentInChildren<EditArmyPanel>();

		editUnitPanel.Init(this);
		editBuildingPanel.Init(this);
		editPickupPanel.Init(this);
		editResourcePanel.Init(this);
		editArmyPanel.Init(this);

		//editUnitPanel.Close();
		editBuildingPanel.Close();
		//editPickupPanel.Close();
		editResourcePanel.Close();
		editArmyPanel.Close();
	}

	public void SetTerrainTypeIndex (int index) {
		activeTerrainTypeIndex = index;
	}

	public void SetApplyElevation (bool toggle) {
		applyElevation = toggle;
	}

	public void SetElevation (float elevation) {
		activeElevation = (int)elevation;
	}

	public void SetApplyWaterLevel (bool toggle) {
		applyWaterLevel = toggle;
	}

	public void SetWaterLevel (float level) {
		activeWaterLevel = (int)level;
	}

	public void SetApplyUrbanLevel (bool toggle) {
		applyUrbanLevel = toggle;
	}

	public void SetUrbanLevel (float level) {
		activeUrbanLevel = (int)level;
	}

	public void SetApplyFarmLevel (bool toggle) {
		applyFarmLevel = toggle;
	}

	public void SetFarmLevel (float level) {
		activeFarmLevel = (int)level;
	}

	public void SetApplyPlantLevel (bool toggle) {
		applyPlantLevel = toggle;
	}

	public void SetPlantLevel (float level) {
		activePlantLevel = (int)level;
	}

	public void SetApplySpecialIndex (bool toggle) {
		applySpecialIndex = toggle;
	}

	public void SetSpecialIndex (float index) {
		activeSpecialIndex = (int)index;
	}

	public void SetDirection(float index)
	{
		activeDirection = (HexDirection)((int)directionDropdown.value);
	}

	public void SetBuildingIndex(float index)
    {
		activeBuildingIndex = (int)buildingDropdown.value;
	}

	public void SetPickupIndex(float index)
    {
		activePickupIndex = (int)pickupDropdown.value;
    }

	public void SetResourceIndex(float index)
    {
		activeResourceIndex = (int)resourceDropdown.value;
    }

	public void SetBrushSize (float size) {
		brushSize = (int)size;
	}

	public void SetRiverMode (int mode) {
		riverMode = (OptionalToggle)mode;
	}

	public void SetRoadMode (int mode) {
		roadMode = (OptionalToggle)mode;
	}

	public void SetWalledMode (int mode) {
		walledMode = (OptionalToggle)mode;
	}

	public void SetEditMode (bool toggle) {
		//inGameUI.SetActive(!toggle);
		
		enabled = toggle;

		return;

		if (toggle == true)
		{
			Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
		} else
        {
			Shader.DisableKeyword("HEX_MAP_EDIT_MODE");
		}

		for (int i = 0; i < 2; i++)
        {
			transform.GetChild(i).gameObject.SetActive(toggle);
		}

	}

	public void ShowGrid (bool visible) {
		if (visible) {
			terrainMaterial.EnableKeyword("GRID_ON");
		}
		else {
			terrainMaterial.DisableKeyword("GRID_ON");
		}
	}

	void Awake () {
		terrainMaterial.DisableKeyword("GRID_ON");
		Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
		SetEditMode(true);
	}

	void Update () {
		if (!EventSystem.current.IsPointerOverGameObject() && isEditPanelOpen == false) {
			if (Input.GetMouseButton(0)) {
				HandleInput();
				return;
			}
			if (Input.GetMouseButtonDown(1))
            {
				HandleEditHexObject();
				return;
            }

			// Units
			if (Input.GetKeyDown(KeyCode.U)) {
				if (Input.GetKey(KeyCode.LeftShift)) {
					DestroyUnit();
				}
				else {
					CreateUnit();
				}
				return;
			}

			// Buildings
			if (Input.GetKeyDown(KeyCode.B))
            {
				if (Input.GetKey(KeyCode.LeftShift))
                {
					DestroyBuilding();
				}
				else
                {
					CreateBuilding();
				}
				return;
            }

			// Pickups
			if (Input.GetKeyDown(KeyCode.I))
			{
				if (Input.GetKey(KeyCode.LeftShift))
				{
					DestroyPickup();
				}
				else
				{
					CreatePickup();
				}
				return;
			}

			// Resources
			if (Input.GetKeyDown(KeyCode.O))
			{
				if (Input.GetKey(KeyCode.LeftShift))
				{
					DestroyResource();
				}
				else
				{
					CreateResource();
				}
				return;
			}

			// Armies
			if (Input.GetKeyDown(KeyCode.P))
			{
				if (Input.GetKey(KeyCode.LeftShift))
				{
					DestroyArmy();
				}
				else
				{
					CreateArmy();
				}
				return;
			}

		}
		previousCell = null;
	}

	HexCell GetCellUnderCursor () {
		return
			hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
	}

	void CreateUnit () {
		HexCell cell = GetCellUnderCursor();
		if (cell && !cell.Unit) {
			hexGrid.AddUnit(cell, Random.Range(0f, 360f));
		}
	}

	void DestroyUnit () {
		HexCell cell = GetCellUnderCursor();
		if (cell && cell.Unit) {
			hexGrid.RemoveUnit(cell.Unit);
		}
	}

	void CreateBuilding()
	{
		HexCell cell = GetCellUnderCursor();
		if (cell)
		{
			hexGrid.AddBuilding(activeBuildingIndex, cell, activeDirection);
		}
	}

	void DestroyBuilding()
	{
		HexCell cell = GetCellUnderCursor();
		if (cell && cell.Building)
		{
			hexGrid.RemoveBuilding(cell.Building);
		}
	}

	void CreatePickup()
    {
		HexCell cell = GetCellUnderCursor();
		if (cell)
		{
			hexGrid.AddPickup(activePickupIndex, cell);
		}
	}

	void DestroyPickup()
    {
		HexCell cell = GetCellUnderCursor();
		if (cell && cell.Pickup)
		{
			hexGrid.RemovePickup(cell.Pickup);
		}
	}

	void CreateResource()
    {
		HexCell cell = GetCellUnderCursor();
		if (cell)
		{
			hexGrid.AddResource(activeResourceIndex, cell);
		}
	}

	void DestroyResource()
    {
		HexCell cell = GetCellUnderCursor();
		if (cell && cell.Resource)
		{
			hexGrid.RemoveResource(cell.Resource);
		}
	}

	void CreateArmy()
	{
		HexCell cell = GetCellUnderCursor();
		if (cell)
		{
			//urfirstluv
			//http://www.rsssf.com/tablesr/roem97.html
			hexGrid.AddArmy(cell, activeDirection);
		}
	}

	void DestroyArmy()
    {
		HexCell cell = GetCellUnderCursor();
		if (cell && cell.Army)
        {
			hexGrid.RemoveArmy(cell.Army);
        }
    }

	void HandleInput () {
		HexCell currentCell = GetCellUnderCursor();
		if (currentCell) {
			if (previousCell && previousCell != currentCell) {
				ValidateDrag(currentCell);
			}
			else {
				isDrag = false;
			}
			EditCells(currentCell);
			previousCell = currentCell;
		}
		else {
			previousCell = null;
		}
	}

	void ValidateDrag (HexCell currentCell) {
		for (
			dragDirection = HexDirection.NE;
			dragDirection <= HexDirection.NW;
			dragDirection++
		) {
			if (previousCell.GetNeighbor(dragDirection) == currentCell) {
				isDrag = true;
				return;
			}
		}
		isDrag = false;
	}

	void EditCells (HexCell center) {
		int centerX = center.coordinates.X;
		int centerZ = center.coordinates.Z;

		for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
			for (int x = centerX - r; x <= centerX + brushSize; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
		for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
			for (int x = centerX - brushSize; x <= centerX + r; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
	}

	void EditCell (HexCell cell) {
		if (cell) {
			if (activeTerrainTypeIndex >= 0) {
				cell.TerrainTypeIndex = activeTerrainTypeIndex;
			}
			if (applyElevation) {
				cell.Elevation = activeElevation;
			}
			if (applyWaterLevel) {
				cell.WaterLevel = activeWaterLevel;
			}
			if (applySpecialIndex) {
				cell.SpecialIndex = activeSpecialIndex;
			}
			if (applyUrbanLevel) {
				cell.UrbanLevel = activeUrbanLevel;
			}
			if (applyFarmLevel) {
				cell.FarmLevel = activeFarmLevel;
			}
			if (applyPlantLevel) {
				cell.PlantLevel = activePlantLevel;
			}
			if (riverMode == OptionalToggle.No) {
				cell.RemoveRiver();
			}
			if (roadMode == OptionalToggle.No) {
				cell.RemoveRoads();
			}
			if (walledMode != OptionalToggle.Ignore) {
				cell.Walled = walledMode == OptionalToggle.Yes;
			}
			if (isDrag) {
				HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
				if (otherCell) {
					if (riverMode == OptionalToggle.Yes) {
						otherCell.SetOutgoingRiver(dragDirection);
					}
					if (roadMode == OptionalToggle.Yes) {
						otherCell.AddRoad(dragDirection);
					}
				}
			}
		}
	}

	void HandleEditHexObject()
	{
		HexCell currentCell = GetCellUnderCursor();
		if (currentCell == null)
		{
			return;
		}

		if (currentCell.Unit)
        {
			EditUnit(currentCell.Unit);
			return;
        }
		if (currentCell.Building)
		{
			EditBuilding(currentCell.Building);
			return;
		}
		if (currentCell.Pickup)
		{
			EditPickup(currentCell.Pickup);
			return;
		}
		if (currentCell.Resource)
		{
			EditResource(currentCell.Resource);
			return;
		}
		if (currentCell.Army)
		{
			EditArmy(currentCell.Army);
			return;
		}
	}

	void EditUnit(HexUnit unit)
    {
		return;
		Debug.Log("EditUnit");
		isEditPanelOpen = true;
	}

	void EditBuilding(HexBuilding building)
    {
		editBuildingPanel.Open(building);

		isEditPanelOpen = true;
		HexMapCamera.Locked = true;
	}

	public void OnEditBuildingFinished(HexBuilding building, int direction)
	{

		if (direction != -1)
        {
			hexGrid.EditBuilding(building, (HexDirection)direction);
		}

		isEditPanelOpen = false;
		HexMapCamera.Locked = false;
	}

	void EditPickup(HexPickup pickup)
    {
		return;
		Debug.Log("EditPickup");
		isEditPanelOpen = true;
	}

	void EditResource(HexResource resource)
    {
		editResourcePanel.Open(resource);

		isEditPanelOpen = true;
		HexMapCamera.Locked = true;
	}

	public void OnEditResourceFinished(HexResource resource, int quantity)
    {
		if (quantity != -1)
		{
			hexGrid.EditResource(resource, quantity);
		}

        isEditPanelOpen = false;
		HexMapCamera.Locked = false;
	}

	void EditArmy(HexArmy army)
    {
		editArmyPanel.Open(army);

		isEditPanelOpen = true;
		HexMapCamera.Locked = true;
	}

	public void OnEditArmyFinished(HexArmy army, int[][] serializedArmy, bool isCancelled = false)
    {

		if (isCancelled == false)
        {
			hexGrid.EditArmy(army, serializedArmy);

			//return;
		}


		isEditPanelOpen = false;
		HexMapCamera.Locked = false;
	}

}