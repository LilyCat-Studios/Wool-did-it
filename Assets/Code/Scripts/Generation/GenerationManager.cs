using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GenerationManager : MonoBehaviour
{
    private Node[] _nodes;
    private NodePath[] _paths;

    private Node _bodyLocation;

    [SerializeField] private int minPathLength;
    [SerializeField] private int maxPathLength;

    private void Start()
    {
        _nodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
        _paths = FindObjectsByType<NodePath>(FindObjectsSortMode.None);

        _bodyLocation = _nodes[Random.Range(0, _nodes.Length)];

        Debug.Log(_bodyLocation.name);

        if ( /* Random.Range(0, 1) == 0 */ true)
        {
            GeneratePathFromMiddle(_bodyLocation);
        }
        else
        {
            GeneratePathFromStart(_bodyLocation);
        }
    }

    private void GeneratePathFromStart(Node startLocation)
    {
        Debug.Log(startLocation.connectedPaths.Length);
    }

    private void GeneratePathFromMiddle(Node startLocation)
    {
        Debug.Log(startLocation.connectedPaths.Length);
    }
}
