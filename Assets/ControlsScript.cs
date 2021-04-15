using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsScript : MonoBehaviour
{
    public float moveSpeed = 5;
    public UnitScript selectedUnit = null;
    public GameObject selectionHexPrefab;
    public HashSet<GameObject> selectionHexagons;
    public HashSet<Vector3Int> validMovePositions;
    // Start is called before the first frame update
    void Start()
    {
        selectionHexagons = new HashSet<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        //camera movement
        Vector2 moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        transform.position += (Vector3)moveDirection * moveSpeed * Time.deltaTime;
        //get the mouse position
        Vector2 mousePos = GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
        Vector3Int snappedPos = GridManager.GetGridPosition(mousePos);
        //all functionality is on left click
        if (Input.GetMouseButtonDown(0))
        {
            //if selected unit is null, attempt to get the selected unit
            if (selectedUnit == null)
            {
                selectedUnit = GridManager.GetUnitAtGridPoint(snappedPos);
                //if the unit was successfully selected, fill in the movement range
                if (selectedUnit != null && selectedUnit.canMove)
                {
                    HashSet<Vector3Int> movePositions = selectedUnit.GetAllValidMovePositions();
                    foreach (Vector3Int position in movePositions)
                    {
                        GameObject newObject = Instantiate(selectionHexPrefab, GridManager.GetWorldPosition(position), Quaternion.identity);
                        selectionHexagons.Add(newObject);
                    }
                    validMovePositions = movePositions;
                }
            }
            //if selected unit is not null, attempt to move the unit
            else
            {
                if (validMovePositions.Contains(snappedPos))
                {
                    selectedUnit.transform.position = GridManager.GetWorldPosition(snappedPos);
                }
                selectedUnit = null;
                foreach (var obj in selectionHexagons)
                    Destroy(obj);
                selectionHexagons.Clear();
            }
        }
    }
}
