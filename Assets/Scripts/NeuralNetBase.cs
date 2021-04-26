using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Xml.Schema;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.WSA;
using Random = System.Random;

/// <summary>
/// The basic class of a neural network, allows for any number and size of hidden layers, inputs, and outputs
/// </summary>
public class NeuralNetBase : MonoBehaviour
{
    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// The functions used for activating a neuron within a neural network
    /// </summary>
    public static class Activation
    {
        public static double Sigmoid(double value)
        {
            return 1.0f / (1.0f + math.exp(-value));
        }

        public static double Heaviside(double value)
        {
            return value > 0.5d ? 0.0d : 1.0d;
        }
    }

    /// <summary>
    /// Copies from the given index bounds of an array of doubles output = vec[start:end)
    /// works like python array indexing
    /// </summary>
    /// <param name="vec"> the array to copy from </param>
    /// <param name="start"> starting index </param>
    /// /// <param name="end"> the index to end before </param>
    /// <returns> the copied array of doubles </returns>
    private static double[] CopyFrom(double[] vec, int start, int end)
    {
        if (end == -1) end = vec.Length;
        if (start == -1) start = vec.Length;
        var copy = new double[end-start];
        var o = 0;
        for (var i = start; i < end; ++i, ++o)
            copy[o] = vec[i];
        return copy;
    }
    
    /// <summary>
    /// Copies from the given index of an array of doubles
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="idx"></param>
    /// <returns></returns>
    private static double CopyFrom(IReadOnlyList<double> vec, int idx)
    {
        if (idx == -1) idx = vec.Count;
        return vec[idx];
    }

    /// <summary>
    /// A struct that defines a Neuron within the confines of a neural network
    /// </summary>
    private struct Neuron
    {
        /// <summary>
        /// the weights of the neuron
        /// </summary>
        public double[] Weights;
        
        /// <summary>
        /// the last output of this neuron
        /// </summary>
        public double Output;
        
        /// <summary>
        /// the expected change in this neuron's weights and bias to reach the correct answer of the previous inputs
        /// </summary>
        public double Delta;
    }

    /// <summary>
    /// The learning rate of the Neural Net (i.e. how much of the difference of the expected output to change weights and biases by)
    /// </summary>
    public double LearningRate;
    
    /// <summary>
    /// The body of the neural net
    /// </summary>
    private readonly List<List<Neuron>> Network;
    
    /// <summary>
    /// The function used to activate a given neuron
    /// </summary>
    private readonly Func<double, double> TransferFunc;
    
    /// <summary>
    /// The number of inputs and number of outputs for this network
    /// </summary>
    private uint NumInputs, NumOutputs;
    
    /// <summary>
    /// The previous inputs crunched by this neural network
    /// </summary>
    private double[] PreviousInput;
    
    /// <summary>
    /// Generates a Neural Network from the construction information
    /// </summary>
    /// <param name="inputs"> the number of inputs this neuron expects to receive </param>
    /// <param name="outputs"> the number of neurons in the output layer </param>
    /// <param name="layers"> a list of neurons per hidden layers (i.e. [1,2] results in two hidden layers with 1 and 2 neurons in them, respectively </param>
    /// <param name="transferFunc"> the function to activate each neuron (defaults to sigmoid) </param>
    public NeuralNetBase(uint inputs, uint outputs, [NotNull] IReadOnlyList<uint> layers, Func<double, double> transferFunc=null)
    {
        Network = new List<List<Neuron>>();
        TransferFunc = transferFunc ?? Activation.Sigmoid;
        NumInputs = inputs; NumOutputs = outputs;
        var numRows = 1 + layers?.Count ?? 1;
        
        for (var layer_idx = 0; layer_idx < numRows; ++layer_idx)
        {
            Network[layer_idx] = new List<Neuron>();
            var neuron_ins = inputs;
            if (layer_idx > 0) neuron_ins = layers[layer_idx - 1];
            for (var neuron_idx = 0; neuron_idx < layers[layer_idx]; ++layer_idx)
            {
                var neuron = new Neuron();
                var num_weights = neuron_ins + 1;
                neuron.Weights = new double[num_weights]; 
                for (var w = 0; w < num_weights; ++w)
                {
                    neuron.Weights[w] = UnityEngine.Random.value;
                }
            } 
        }
    }

    /// <summary>
    /// Activates a neuron, 
    /// </summary>
    /// <param name="weights"> the weights (and bias) of the neuron to activate</param>
    /// <param name="inputs"> the inputs to feed into the neuron </param>
    /// <returns> the output of dot(neuron.Weights, inputs) + neuron.Bias </returns>
    private static double Activate(IReadOnlyList<double> weights, IReadOnlyList<double> inputs)
    {
        var total = CopyFrom(weights, -1);
        for (var i = 0; i < weights.Count - 1; ++i)
        {
            total += weights[i] * inputs[i];
        }

        return total;
    }

    /// <summary>
    /// produces a network output from a given input 
    /// </summary>
    /// <param name="row"> the inputs for the network to crunch </param>
    /// <returns> the output of the network </returns>
    public double[] ForwardPropagation(double[] row)
    {
        var inputs = CopyFrom(row, 0, -1);
        foreach (var layer in Network)
        {
            var new_inputs = new double[layer.Count];
            var input_idx = 0;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var neuron_idx = 0; neuron_idx < layer.Count; ++neuron_idx)
            {
                var neuron = layer[neuron_idx];
                var val = Activate(neuron.Weights, inputs);
                neuron.Output = TransferFunc(val);
                new_inputs[input_idx] = neuron.Output;
                input_idx++;
            }
            inputs = new_inputs;
        }
        return inputs;
    }

    /// <summary>
    /// "derives" a given output
    /// </summary>
    /// <param name="output"> the output </param>
    /// <returns> the derivation </returns>
    private static double Derive(double output)
    {
        return output * (1.0 - output);
    }

    /// <summary>
    /// Runs through and calculates the error / impact of each neuron in the network
    /// </summary>
    /// <param name="expected"> the expected output from the last ForwardPropagation </param>
    public void BackwardPropagation(double[] expected)
    {
        for (var layer_idx = Network.Count-1; layer_idx >= 0; --layer_idx)
        {
            var layer = Network[layer_idx];
            var errors = new List<double>();
            if (layer_idx != Network.Count - 1)
                for (var neuron_idx = 0; neuron_idx < layer.Count; ++neuron_idx)
                {
                    var error = Network[layer_idx + 1].Sum(neuron => neuron.Weights[neuron_idx] * neuron.Delta);
                    errors.Append(error);
                }
            else
                for (var neuron_idx = 0; neuron_idx < layer.Count; ++neuron_idx)
                    errors.Append(expected[neuron_idx] - layer[neuron_idx].Delta);

            for (var neuron_idx = 0; neuron_idx < layer.Count; ++neuron_idx)
            {
                var neuron = layer[neuron_idx];
                neuron.Delta = errors[neuron_idx] * Derive(neuron.Output);
            }
        } 
    }

    /// <summary>
    /// Changes the weights of the network based on the inputs and delta from backwards propogation
    /// </summary>
    /// <param name="row"> the inputs from this round of BackwardPropagation </param>
    private void UpdateWeights(double[] row)
    {
        for (var layer_idx = 0; layer_idx < Network.Count; ++layer_idx)
        {
            var inputs = CopyFrom(row, 0, -1);
            if (layer_idx != 0)
            {
                var prev_layer = Network[layer_idx - 1];
                var temp = new double[prev_layer.Count];
                foreach (var neuron in prev_layer)
                    temp.Append(neuron.Output);
            }
            foreach (var neuron in Network[layer_idx])
            {
                for (var weight_idx = 0; weight_idx < neuron.Weights.Length; ++weight_idx)
                    neuron.Weights[weight_idx] += LearningRate * neuron.Delta * inputs[weight_idx];
                neuron.Weights[neuron.Weights.Length - 1] += LearningRate * neuron.Delta;
            }
        }
    }

    /// <summary>
    /// Runs the network with backwards propagation
    /// </summary>
    /// <param name="data"> a list of inputs arrays </param>
    /// <param name="answers"> a list of the expected outputs </param>
    /// <returns> the error rate </returns>
    public double Train(double[][] data, double[][] answers)
    {
        var sum_error = 0.0d;
        for (var data_idx = 0; data_idx < data.Length; ++data_idx)
        {
            var row = data[data_idx];
            var expected = answers[data_idx];
            PreviousInput = CopyFrom(row, 0, -1);
            var outputs = ForwardPropagation(row);
            sum_error += outputs.Select((t, idx) => math.pow(expected[idx] - t, 2)).Sum();
            BackwardPropagation(expected);
            UpdateWeights(row);
        }
        return sum_error;
    }

    /// <summary>
    /// Runs the network without backwards propagation
    /// </summary>
    /// <param name="data"> a list of inputs arrays </param>
    /// <param name="answers"> a list of the expected outputs </param>
    /// <returns> the error rate </returns>
    public double Predict(double[][] data, double[][] answers)
    {
        var sum_error = 0.0d;
        for (var data_idx = 0; data_idx < data.Length; ++data_idx)
        {
            var row = data[data_idx];
            var expected = answers[data_idx];
            PreviousInput = CopyFrom(row, 0, -1);
            var outputs = ForwardPropagation(row);
            sum_error += outputs.Select((t, idx) => math.pow(expected[idx] - t, 2)).Sum();
        }
        return sum_error;
    }
    
    /* for drawing */
    // def setupDrawing(self, window):
    //     num_rows = len(self.Network)
    //     x_inc = window.Size[0] // (num_rows + 2)
    //     for row_idx in range(num_rows):
    //         num_neurons = len(self.Network[row_idx])
    //         y_inc = window.Size[1] // (num_neurons + 1)
    //         x = x_inc*2 + row_idx * x_inc
    //         for neuron_idx in range(num_neurons):
    //             y = y_inc + neuron_idx * y_inc
    //             neuron = self.Network[row_idx][neuron_idx]
    //             neuron['position'] = (x, y)
    //             rad = y_inc // 3
    //             if x_inc < y_inc:
    //                 rad = x_inc // 3
    //             neuron['radius'] = rad
    
    // def drawNetwork(self, window) -> None:
    //     x_inc = window.Size[0] // (len(self.Network) + 2)
    //     y_inc = window.Size[1] // (self.NumInput+1)
    //     rad = y_inc // 3
    //     if x_inc < y_inc:
    //         rad = x_inc // 3
    //     input_pos = []
    //     for idx in range(len(self.PreviousInput)):
    //         input_pos.append((x_inc, y_inc + y_inc * idx))
    //         pg.draw.circle(window.Screen, BLACK, input_pos[idx], rad+4)
    //         pg.draw.circle(window.Screen, (255,50,50), input_pos[idx], rad)
    //     for row_idx in range(len(self.Network)):
    //         for neuron in self.Network[row_idx]:
    //             n1r = int(neuron['radius'])
    //             n1p = neuron['position']
    //             if row_idx == 0:
    //                 for idx in range(len(self.PreviousInput)):
    //                     pg.draw.line(window.Screen, BLACK, input_pos[idx], n1p, 9)
    //                     pg.draw.line(window.Screen, MakeGray(self.PreviousInput[idx]), input_pos[idx], n1p, 5)
    //             else:
    //                 for n2 in self.Network[row_idx - 1]:
    //                     pg.draw.line(window.Screen, BLACK, n2['position'], n1p, 9)
    //                     if 'output' in n2.keys():
    //                         pg.draw.line(window.Screen, MakeGray(n2['output']), n2['position'],
    //                                      n1p, 5)
    //             pg.draw.circle(window.Screen, BLACK, n1p, n1r+4)
    //             if row_idx == len(self.Network)-1:
    //                 pg.draw.circle(window.Screen, (50,250,50), n1p, n1r)
    //             elif 'output' in neuron.keys():
    //                 pg.draw.circle(window.Screen, MakeGray(neuron['output']), n1p, n1r)

}
