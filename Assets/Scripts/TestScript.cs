using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

        var graphvizAlgorithm = new GraphvizAlgorithm<int, Edge<int>>(testGraph);
        graphvizAlgorithm.CommonVertexFormat.Shape = GraphvizVertexShape.Diamond;
        graphvizAlgorithm.CommonEdgeFormat.ToolTip = "Edge tooltip";
        graphvizAlgorithm.FormatVertex += (sender, args) =>
        {
            args.VertexFormat.Label = $"Vertex {args.Vertex}";
        };

        string dotGraphSource = graphvizAlgorithm.Generate();
        Debug.Log(dotGraphSource);

        string outputFilePath = graphvizAlgorithm.Generate(new FileDotEngine(), "dotOutputGraph");
        Debug.Log(outputFilePath);
    }
}