using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using QuikGraph;
using QuikGraph.Graphviz;

public class TestScript : MonoBehaviour
{
    [SerializeField]
    private int testProperty;

    private AdjacencyGraph<int, Edge<int>> testGraph;

    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("test log");

        testGraph = new AdjacencyGraph<int, Edge<int>>();
        testGraph.AddVertex(1);
        testGraph.AddVertex(2);
        testGraph.AddVertex(3);
        testGraph.AddEdge(new Edge<int>(1, 2));
        testGraph.AddEdge(new Edge<int>(3, 1));
        testGraph.AddEdge(new Edge<int>(3, 2));

        /*/
        foreach (var edge in testGraph.Edges)
        {
            Debug.Log(edge.Source + "->" + edge.Target);
        }
        //*/

        var graphviz = new GraphvizAlgorithm<int, Edge<int>>(testGraph);

        string dotGraphSource = graphviz.Generate();
        Debug.Log(dotGraphSource);

        string outputFilePath = graphviz.Generate(new FileDotEngine(), "dotOutputGraph");
        Debug.Log(outputFilePath);
    }
}