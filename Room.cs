using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<Node> nodes;
    public List<Node> waypoints;

    public int numberOfWaypoints = 3;

    private void Awake()
    {
        nodes = new List<Node>();
        waypoints = new List<Node>();
    }

    void Start()
    {
        GenerateWaypoints();
    }

    void GenerateWaypoints()
    {
        var random = new System.Random();

        int index;

        for(int i = 0; i < numberOfWaypoints; i++)
        {
            index = random.Next(nodes.Count);
            waypoints.Add(nodes[index]);
        }
    }
}
