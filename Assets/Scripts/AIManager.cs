using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Main;
    public List<AIInterface> currentAIs; //the AIs that are currently loaded in, index corresponds to team
    private void Awake()
    {
        Main = this;
    }
    //run the AI turn for a province
    public void RunAITurn(ProvinceManagerScript province)
    {
        if (province == null)
        {
            //print("Province is null!");
            return;
        }
        //get the AI running this province
        AIInterface currentAI = currentAIs[province.team-1];
        //create strategic input list
        List<float> strategicInputs = new List<float>();
        //assign values to the strategic inputs
        strategicInputs.Add((float)province.money); //input 0: current province money
        strategicInputs.Add((float)province.GetIncome()); //input 1: current province income
        strategicInputs.Add((float)province.GetUnitCount(0)); //input 2: total unit count
        strategicInputs.Add((float)province.GetUnitCount(1)); //input 3: tier 1 units
        strategicInputs.Add((float)province.GetUnitCount(2)); //input 4: tier 2 units
        strategicInputs.Add((float)province.GetUnitCount(3)); //input 5: tier 3 units
        strategicInputs.Add((float)province.GetUnitCount(4)); //input 6: tier 4 units
        strategicInputs.Add((float)province.GetTownCount()); //input 7: amount of towns in the province
        strategicInputs.Add((float)GridManager.GetRangeToOtherTeam(GridManager.GetGridPosition(transform.position), 99)); //input 8: distance to closest enemy province
        //TODO: enemy unit counts?
        //get the strategic outputs
        List<float> strategicOutputs = currentAI.ProcessStrategicNetwork(strategicInputs,7);
        float unitMode = strategicOutputs[0]; //output 0: strategic mode passed to weight AI
        float constructPeasant = strategicOutputs[1]; //output 1: amount of peasants to recruit
        float constructSpearman = strategicOutputs[2]; //output 2: amount of spearment to recruit
        float constructBaron = strategicOutputs[3]; //output 3: amount of barons to recruit
        float constructKnight = strategicOutputs[4]; //output 4: amount of knights to recruit
        float constructTower = strategicOutputs[5]; //output 2: amount of towers to construct
        float constructTown = strategicOutputs[6]; //output 3: amount of towns to construct
        //get the weight list for each tile
        Dictionary<Vector3Int, List<float>> tileWeightList = new Dictionary<Vector3Int, List<float>>();
        //iterate through each tile to build the weight set
        var allConnectedPositions = GridManager.GetAllConnectedTiles(GridManager.GetGridPosition(province.transform.position), 99);
        foreach (Vector3Int position in allConnectedPositions)
        {
            tileWeightList.Add(position, GetTileWeightOutputs(currentAI, unitMode, position, province.team));
        }
        //perform unit moves
        List<UnitScript> moveableUnits = new List<UnitScript>(province.controlledUnits);
        foreach (UnitScript unit in moveableUnits)
        {
            if (unit == null || !unit.mobile) continue; //skip over units that can't move
            float bestWeight = -9999;
            Vector3Int bestPosition = Vector3Int.zero;
            foreach (Vector3Int position in unit.GetAllValidMovePositions())
            {
                if (!tileWeightList.ContainsKey(position)) //we likely merged, ignore the tiles we just merged into
                {
                    tileWeightList.Add(position, GetTileWeightOutputs(currentAI, unitMode, position, province.team));
                }
                if (tileWeightList[position][unit.powerLevel] > bestWeight)
                {
                    bestWeight = tileWeightList[position][1];
                    bestPosition = position;
                }
            }
            //move the unit
            unit.MoveUnit(bestPosition);
            //update adjacent weights
            foreach (var position in GridManager.GetAllGridPointsInRange(bestPosition,1))
            {
                if (GridManager.GetHexAtGridPoint(position) == null) continue; //skip if it's a hole
                if (tileWeightList.ContainsKey(position))
                    tileWeightList[position] = GetTileWeightOutputs(currentAI, unitMode, position, province.team);
                else
                    tileWeightList.Add(position,GetTileWeightOutputs(currentAI, unitMode, position, province.team));
            }
        }
        //spawn units
        for (float x = 1; x <= constructPeasant; x += 1)
        {
            float bestWeight = -9999;
            Vector3Int bestPosition = Vector3Int.zero;
            foreach (Vector3Int position in allConnectedPositions)
            {
                if (!tileWeightList.ContainsKey(position)) //we likely merged, ignore the tiles we just merged into
                {
                    tileWeightList.Add(position, GetTileWeightOutputs(currentAI, unitMode, position, province.team));
                }
                if (tileWeightList[position][1] > bestWeight)
                {
                    bestWeight = tileWeightList[position][1];
                    bestPosition = position;
                }
            }
            province.SpawnUnit(GameManager.Main.tier1Unit,bestPosition);
            //update adjacent weights
            foreach (var position in GridManager.GetAllGridPointsInRange(bestPosition, 1))
            {
                if (GridManager.GetHexAtGridPoint(position) == null) continue; //skip if it's a hole
                if (tileWeightList.ContainsKey(position))
                    tileWeightList[position] = GetTileWeightOutputs(currentAI, unitMode, position, province.team);
                else
                    tileWeightList.Add(position, GetTileWeightOutputs(currentAI, unitMode, position, province.team));
            }
        }
    }
    List<float> GetTileWeightOutputs(AIInterface currentAI, float unitMode, Vector3Int position, int team)
    {
        TileScript tile = GridManager.GetHexAtGridPoint(position);
        UnitScript unit = GridManager.GetUnitAtGridPoint(position);
        //create tile weight input list
        List<float> weightInputs = new List<float>();
        //assign values to the weight inputs
        weightInputs.Add(unitMode); //input 0: strategic mode from the strategic AI
        weightInputs.Add((float)((tile.team == team) ? 1 : (tile.team == 0) ? 0 : -1)); //input 1: team attribute (1 if friendly, 0 if neutral, -1 if hostile
        weightInputs.Add((float)GridManager.GetDefenseLevel(position)); //input 2: defense level of the tile
        weightInputs.Add((float)((unit != null) ? unit.powerLevel : 0)); //input 3: power level of the unit on the tile, or 0 if there is no unit
        weightInputs.Add((float)((unit != null) ? ((unit.mobile) ? 1 : 0) : 0)); //input 4: whether the unit on the tile can move, or 0 if there is no unit
        weightInputs.Add((float)GridManager.GetRangeToOtherTeam(position, 99)); //input 5: distance to closest enemy tile
        weightInputs.Add((float)GridManager.GetRangeToOtherTeam(position, 99, true)); //input 6: distance to closest enemy or neutral tile
        //TODO: distance to closets neutral/enemy tile
        //get the weight output
        List<float> weightOutputs = currentAI.ProcessWeightsNetwork(weightInputs, 7);
        float activationWeight = weightOutputs[0]; //output 0: priority for moving this unit (new spawns always have priority 1?)
        float peasantWeight = weightOutputs[1]; //output 1: move weight for peasants
        float spearmanWeight = weightOutputs[2]; //output 2: move weight for spearmen
        float baronWeight = weightOutputs[3]; //output 3: move weight for barons
        float knightWeight = weightOutputs[4]; //output 4: move weight for knights
        float towerWeight = weightOutputs[5]; //output 5, build weight for towers
        float townWeight = weightOutputs[6]; //output 6, build weight for towns
        //add to the tile weight list
        return weightOutputs;
    }
}
