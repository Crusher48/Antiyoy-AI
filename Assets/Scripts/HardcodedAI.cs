using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardcodedAI : AIInterface
{
    public override List<float> ProcessStrategicNetwork(List<float> inputs, int expectedOutputs)
    {
        List<float> outputList = new List<float>();
        for (int i = 0; i < expectedOutputs; i++) outputList.Add(0);
        //hardcoded unit spamming
        outputList[1] = 1;
        return outputList;
    }
    public override List<float> ProcessWeightsNetwork(List<float> inputs, int expectedOutputs)
    {
        List<float> outputList = new List<float>();
        for (int i = 0; i < expectedOutputs; i++) outputList.Add(0);
        //hardcoded throw units at neutral or hostile territory
        float moveWeight = 1 * Random.Range(0f, 1f);
        if (inputs[1] != 1)
        {
            outputList[1] = moveWeight;
            outputList[2] = moveWeight;
            outputList[3] = moveWeight;
            outputList[4] = moveWeight;
        }    
        return outputList;
    }
}
