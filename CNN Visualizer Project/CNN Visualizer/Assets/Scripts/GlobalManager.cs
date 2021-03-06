﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Singleton for handling global behaviour.
/// </summary>
public class GlobalManager : MonoBehaviour
{
    static GlobalManager instance;

    List<Layer> layers = new List<Layer>();

    public Dictionary<int, Dictionary<int, string>> predPerSamplePerEpoch = new Dictionary<int, Dictionary<int, string>>();
    public Dictionary<int, string> groundtruthPerSample = new Dictionary<int, string>();

    public List<string> classNames = new List<string>();

    public int testSample = 0;
    public int epoch = 0;

    public bool multWeightsByActivations = false;

    /// <summary>
    /// Return singleton instance
    /// </summary>
    public static GlobalManager Instance { 
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GlobalManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.hideFlags = HideFlags.HideAndDontSave;
                    instance = obj.AddComponent<GlobalManager>();
                }
            }
            return instance;
        }
    }

    public void SetPointBrightness(float value)
    {
        List<Layer> layers = GetAllLayersUnOrdered();

        foreach (Layer l in layers)
        {

            l.pointBrightness = value;
            l.UpdateMesh();
        }
    }

    public void SetWeightBrightness(float value)
    {
        List<Layer> layers = GetAllLayersUnOrdered();

        foreach (Layer l in layers)
        {
            if (!l.GetType().Equals(typeof(ImageLayer)))
            {
                l.weightBrightness = value;
                l.UpdateMesh();
            }
        }
    }

    public void SetExpansionLevel(float value)
    {
        List<Layer> layers = GetAllLayersUnOrdered();

        foreach (Layer l in layers)
        {
            if (l.GetType().IsSubclassOf(typeof(InputAcceptingLayer)))
            {
                InputAcceptingLayer cast_l = (InputAcceptingLayer)l;
                cast_l.SetExpansionLevel(value);
                cast_l.UpdateMesh();
            }
        }
    }

    public void UpdateMeshes()
    {
        List<Layer> layers = GetAllLayersUnOrdered();

        foreach (Layer l in layers)
        {
            l.UpdateMesh();
        }
    }

    public void SetEpoch(int epoch)
    {
        this.epoch = epoch;
        List<Layer> orderedLayers = GetAllLayersOrdered();

        foreach (Layer l in orderedLayers)
        {
            if (l.GetType().Equals(typeof(ConvLayer))){
                ((ConvLayer)l).SetEpoch(epoch);
            } else if (l.GetType().Equals(typeof(FCLayer)))
            {
                ((FCLayer)l).SetEpoch(epoch);
            }
            else if (l.GetType().Equals(typeof(MaxPoolLayer)))
            {
                ((MaxPoolLayer)l).SetEpoch(epoch);
            }
        }
    }

    public void SetSample(float value)
    {
        testSample = (int)value;

        List<Layer> orderedLayers = GetAllLayersOrdered();

        foreach (Layer l in orderedLayers)
        {
            l.UpdateMesh();
        }
    }

    /// <summary>
    /// Returns all layers in scene ordered.
    /// </summary>
    /// <returns></returns>
    public List<Layer> GetAllLayersOrdered()
    {
        Layer[] layers = FindObjectsOfType<Layer>();

        HashSet<Layer> inputLayers = new HashSet<Layer>();

        Layer outputLayer = null;

        foreach(Layer l in layers)
        {
            inputLayers.Add(l.GetInputLayer());
        }

        foreach(Layer l in layers)
        {
            if (!inputLayers.Contains(l))
                outputLayer = l;
        }

        List<Layer> list = new List<Layer>();

        Layer current = outputLayer;

        list.Add(current);
        DfsAddInputs(list, current);
        list.Reverse();

        return list;
    }

    /// <summary>
    /// Returns Layers in scene unordered.
    /// </summary>
    /// <returns></returns>
    public List<Layer> GetAllLayersUnOrdered()
    {
        Layer[] layers = FindObjectsOfType<Layer>();

        return new List<Layer>(layers);
    }

    /// <summary>
    /// Gets all convolutional layers in the scene.
    /// </summary>
    /// <returns></returns>
    public List<ConvLayer> GetAllConvLayers()
    {
        List<Layer> allLayers = this.GetAllLayersUnOrdered();
        List<ConvLayer> outList = new List<ConvLayer>();

        foreach(Layer l in allLayers)
        {
             if (l.GetType().Equals(typeof(ConvLayer)))
            {
                outList.Add((ConvLayer)l);
            }
        }

        return outList;
    }

    /// <summary>
    /// Recursively adds layers to output list.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    private Layer DfsAddInputs(List<Layer> list, Layer parent)
    {
        Layer input = parent.GetInputLayer();
        if (input == null)
            return null;
        else
            list.Add(input);
            return DfsAddInputs(list, input);
    }

    /// <summary>
    /// Globally sets the y position of the full res filter view.
    /// </summary>
    /// <param name="height"></param>
    public void SetFullResHeight(float height)
    {
        foreach(ConvLayer l in GetAllConvLayers())
        {
            l.fullResHeight = height;
            l.UpdateMesh();
        }
    }

    public void SetFullResDisplay(bool value)
    {
        if(value)
        {
            foreach(Layer l in GetAllLayersUnOrdered())
            {
                if (l.GetType().Equals(typeof(ConvLayer)))
                {
                    ((ConvLayer)l).showOriginalDepth = true;
                }
                else if (l.GetType().Equals(typeof(FCLayer)))
                {
                    ((FCLayer)l).showOriginalDepth = true;
                }
                else if (l.GetType().Equals(typeof(ImageLayer)))
                {
                    ((ImageLayer)l).showOriginalResolution = true;
                }
                l.CalcMesh();
            }
        }
    }

}
