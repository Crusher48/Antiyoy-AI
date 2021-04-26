using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsScript : MonoBehaviour
{
    public float moveSpeed = 5;
    public GameObject selectionHexPrefab;
    //selected objects
    public GameObject newUnit = null;
    public UnitScript selectedUnit = null;
    public ProvinceManagerScript hoveredProvince = null;
    //for movement
    public HashSet<GameObject> selectionHexagons;
    public HashSet<Vector3Int> validMovePositions;
    [SerializeField] Button placeUnitButton;
    [SerializeField] Button placeTowerButton;
    [SerializeField] Button placeTownButton;
    [SerializeField] Button endTurnButton;
    [SerializeField] Button endGameButton;
    [SerializeField] Text currentTeamText;
    [SerializeField] Text incomeText;
    // Start is called before the first frame update
    void Start()
    {
        selectionHexagons = new HashSet<GameObject>();
        placeUnitButton.onClick.AddListener(SpawnGuy);
        placeTowerButton.onClick.AddListener(SpawnTower);
        placeTownButton.onClick.AddListener(SpawnTown);
        endTurnButton.onClick.AddListener(EndTurn);
        endGameButton.onClick.AddListener(EndGame);
    }

    // Update is called once per frame
    void Update()
    {
        //camera movement
        Vector2 moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        transform.position += (Vector3)moveDirection * moveSpeed * Time.deltaTime;
        //get the mouse position
        Vector2 mousePos = GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPos = GridManager.GetGridPosition(mousePos);
        TileScript hoveredHex = GridManager.GetHexAtGridPoint(gridPos);
        //get the hovered province
        if (hoveredHex == null) hoveredProvince = null; else hoveredProvince = GridManager.GetHexAtGridPoint(gridPos).owner;
        //update text
        if (hoveredProvince == null)
        {
            incomeText.text = "";
        }
        else
        {
            incomeText.text = string.Format("${0} (+${1})", hoveredProvince.money, hoveredProvince.GetIncome());
        }
        currentTeamText.text = "Current Team: " + GameManager.Main.activeTeam;
        //all functionality is on left click
        if (Input.GetMouseButtonDown(0))
        {
            //if new unit is not null, we're placing a unit
            if (newUnit != null)
            {
                if (hoveredHex != null && hoveredHex.team == GameManager.Main.activeTeam) //basic spawning
                {
                    hoveredHex.owner.SpawnUnit(newUnit, gridPos);
                }
                else //advanced spawning, look for an adjacent province to spawn from
                {
                    List<Vector3Int> adjacentPositions = GridManager.GetAllGridPointsInRange(gridPos, 1);
                    foreach (var adjacentPosition in adjacentPositions)
                    {
                        TileScript adjacentHex = GridManager.GetHexAtGridPoint(adjacentPosition);
                        if (adjacentHex != null && adjacentHex.team == GameManager.Main.activeTeam) //will arbitrarilly pick one province to do it, but if the move is valid this will merge provinces
                        {
                            adjacentHex.owner.SpawnUnit(newUnit, gridPos);
                            break;
                        }
                    }
                }
                //either way, destroy the selection hexagons afterwards
                newUnit = null;
                foreach (var obj in selectionHexagons)
                    Destroy(obj);
                selectionHexagons.Clear();
            }
            //if selected unit is null, attempt to get the selected unit
            else if (selectedUnit == null)
            {
                selectedUnit = GridManager.GetUnitAtGridPoint(gridPos);
                //if the unit was successfully selected, fill in the movement range
                if (selectedUnit != null && selectedUnit.mobile && selectedUnit.canMove && selectedUnit.GetTeam() == GameManager.Main.activeTeam)
                {
                    HashSet<Vector3Int> movePositions = selectedUnit.GetAllValidMovePositions();
                    foreach (Vector3Int position in movePositions)
                    {
                        GameObject newObject = Instantiate(selectionHexPrefab, GridManager.GetWorldPosition(position), Quaternion.identity);
                        selectionHexagons.Add(newObject);
                    }
                    validMovePositions = movePositions;
                }
                else
                    selectedUnit = null;
            }
            //if selected unit is not null, attempt to move the unit
            else
            {
                if (validMovePositions.Contains(gridPos))
                {
                    selectedUnit.MoveUnit(gridPos);
                }
                selectedUnit = null;
                foreach (var obj in selectionHexagons)
                    Destroy(obj);
                selectionHexagons.Clear();
            }
        }
    }
    //starts to spawn a unit
    void EnterSpawningMode(GameObject unitToSpawn)
    {
        newUnit = unitToSpawn;
        HashSet<Vector3Int> movePositions = new HashSet<Vector3Int>();
        foreach (ProvinceManagerScript activeProvince in GameManager.Main.activeProvinces)
        {
            foreach (Vector3Int position in GridManager.GetAllConnectedTiles(GridManager.GetGridPosition(activeProvince.transform.position),99) )
            {
                TileScript tile = GridManager.GetHexAtGridPoint(position);
                movePositions.Add(GridManager.GetGridPosition(tile.transform.position));
            }
        }
        foreach (Vector3Int position in movePositions)
        {
            GameObject newObject = Instantiate(selectionHexPrefab, GridManager.GetWorldPosition(position), Quaternion.identity);
            selectionHexagons.Add(newObject);
        }
    }
    void SpawnGuy()
    {
        EnterSpawningMode(GameManager.Main.tier1Unit);
    }
    void SpawnTower()
    {
        EnterSpawningMode(GameManager.Main.tower);
    }
    void SpawnTown()
    {
        EnterSpawningMode(GameManager.Main.town);
    }
    //ends the turn
    void EndTurn()
    {
        GameManager.Main.EndTurn();
    }
    //ends the game, returning to the menu
    void EndGame()
    {
        GameManager.Main.EndGame();
    }
}
