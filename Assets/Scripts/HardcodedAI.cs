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
        outputList[6] = 1;
        return outputList;
    }
    public override List<float> ProcessWeightsNetwork(List<float> inputs, int expectedOutputs)
    {
        List<float> outputList = new List<float>();
        for (int i = 0; i < expectedOutputs; i++) outputList.Add(0);
        //hardcoded throw units at neutral or hostile territory
        float randomMultiplier = Random.Range(0.5f, 1f);
        if (inputs[1] == 1)
        {
            outputList[5] = 1*randomMultiplier;
            outputList[6] = 1*randomMultiplier;
        }
        //else
        {
            outputList[1] = 1*randomMultiplier;
            outputList[2] = 1*randomMultiplier;
            outputList[3] = 1*randomMultiplier;
            outputList[4] = 1*randomMultiplier;
        }
        return outputList;
    }
}
