using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NodesGrid : MonoBehaviour
{
    Node[,] gridNodes;
    List<Node> openNodes;
    List<Node> closedNodes;

    public List<Node> path;

    [SerializeField]
    int gridWidth = 30;
    [SerializeField]
    int gridHeight = 30;
    [SerializeField]
    float nodeSize = 2f;
    [SerializeField]
    public LayerMask obstacleLayers;
    public LayerMask roomLayers;

    private void Awake()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        gridNodes = new Node[gridWidth, gridHeight];

        for (int i = 0; i < gridWidth; i++)
        {
            for(int j = 0; j < gridHeight; j++)
            {
                Vector3 nodePosition = new Vector3(i * nodeSize, 0, j * nodeSize);
                Node currentNode = gridNodes[i, j] = new Node(i, j, nodePosition);

                if(!Physics.CheckBox(nodePosition, Vector3.one * nodeSize / 2, Quaternion.identity, obstacleLayers))
                {
                    currentNode.walkable = true;

                    if (i > 0) 
                        LinkNodes(currentNode, gridNodes[i - 1, j]);
                    if (j > 0)
                        LinkNodes(currentNode, gridNodes[i, j - 1]);
                    if (i > 0 && j > 0)
                        LinkNodes(currentNode, gridNodes[i - 1, j - 1]);
                    if (i > 0 && j < gridHeight - 1)
                        LinkNodes(currentNode, gridNodes[i - 1, j + 1]);

                    Collider[] roomsCollider = Physics.OverlapBox(nodePosition, Vector3.one * nodeSize / 2, Quaternion.identity, roomLayers);
                    foreach(Collider roomCollider in roomsCollider)
                    {
                        Room room = roomCollider.GetComponent<Room>();
                        if(room != null)
                        {
                            currentNode.room = room;
                            room.nodes.Add(currentNode);
                            break;
                        }
                    }
                }
            }
        }
    }

    void LinkNodes(Node nodeA, Node nodeB)
    {
        if (nodeB.walkable)
        {
            nodeA.nearestNodes.Add(nodeB);
            nodeB.nearestNodes.Add(nodeA);
        }
    }

    void OnDrawGizmos()
    {
        if (gridNodes == null)
            return;
        
        foreach(Node node in gridNodes)
        {
            if(node.walkable)
                Gizmos.color = Color.yellow;
            else
                Gizmos.color = Color.red;

            if(path != null && path.Contains(node))
                Gizmos.color = Color.green;

            Gizmos.DrawWireCube(node.position, (new Vector3(nodeSize, 0, nodeSize)) * 0.9f);
            Handles.Label(node.position + new Vector3(-nodeSize * 0.4f, 0 , -nodeSize * 0.4f), node.gCost.ToString());
            Handles.Label(node.position + new Vector3(-nodeSize * 0.4f, 0, nodeSize * 0.4f), node.fCost.ToString());
            Handles.Label(node.position + new Vector3(nodeSize * 0.4f, 0, -nodeSize * 0.4f), node.hCost.ToString());
        }
    }

    // A* Search Algorithm
    public void FindPath(Vector3 start, Vector3 end)
    {
        Node startNode = NodeFromWorldPoint(start);
        Node endNode = NodeFromWorldPoint(end);

        // Initialize the open list
        openNodes = new List<Node>();
        // Initialize the closed list
        closedNodes = new List<Node>();
        // put the starting node on the open list
        openNodes.Add(startNode);

        while(openNodes.Count > 0)
        {
            Node currentNode = getNodeWithMinF();

            if(currentNode == endNode)
            {
                Debug.Log("Path found");

                TracePath(startNode, endNode);

                break;
            }

            foreach(Node node in currentNode.nearestNodes)
            {
                if (!node.walkable || closedNodes.Contains(node))
                    continue;

                int nearGCost = (Mathf.Abs(currentNode.x - node.x) + Mathf.Abs(currentNode.y - node.y)) == 1 ? 10 : 14;
                int tmpGCost = currentNode.gCost + nearGCost;

                if (node.gCost > tmpGCost || !openNodes.Contains(node))
                {
                    node.parentNode = currentNode;
                    node.gCost = tmpGCost;
                    node.hCost = CountHCost(node, endNode);

                    if(!openNodes.Contains(node))
                    {
                        openNodes.Add(node);
                    }
                }
            }

            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);
        }
    }

    void TracePath(Node startNode, Node endNode)
    {
        path = new List<Node>();

        Node node = endNode;

        while(node != startNode)
        {
            path.Add(node);
            node = node.parentNode;
        }
        
        path.Add(startNode);
        path.Reverse();
    }

    public Node NodeFromWorldPoint(Vector3 worldPositon)
    {
        float percentX = worldPositon.x / gridWidth * nodeSize;
        float percentY = worldPositon.z / gridHeight * nodeSize;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt(percentX * (gridWidth));
        int y = Mathf.RoundToInt(percentY * (gridHeight));

        return gridNodes[x, y];
    }

    Node getNodeWithMinF()
    {
        Node minFNode = null;

        foreach(Node node in openNodes)
        {
            if (minFNode == null)
            {
                minFNode = node;
                continue;
            }

            if (node.fCost < minFNode.fCost)
                minFNode = node;
        }

        return minFNode;
    }

    int CountHCost(Node nodeA, Node nodeB)
    { 
        return (Mathf.Abs(nodeB.x - nodeA.x) + Mathf.Abs(nodeB.y - nodeA.y)) * 10;
    }

    public List<Node> GetNearestNodesInRange2(Node currentNode, int range = 3)
    {
        List<Node> nearestNodes = new List<Node>();

        if (range == 0)
        {
            nearestNodes.Add(currentNode);
        }
        else
        {
            foreach (Node node in currentNode.nearestNodes)
            {
                foreach (Node node2 in GetNearestNodesInRange2(node, range - 1))
                {
                    if (!nearestNodes.Contains(node2))
                        nearestNodes.Add(node2);
                }
            }
        }

        return nearestNodes;
    }

    public List<Node> GetNearestNodesInRange(Node currentNode, int range = 1)
    {
        List<Node> nearestNodes = new List<Node>();

        for(int k = 1; k <= range; k++)
        {
            for (int i = -k; i <= k; i++)
            {
                for (int j = -k; j <= k; j++)
                {
                    if (j >= 1 - k && j <= k - 1 && i >= 1 - k && i <= k - 1)
                        continue;

                    int checkX = currentNode.x + i;
                    int checkY = currentNode.y + j;

                    if (checkX >= 0 && checkY >= 0 && checkX < gridWidth && checkY < gridHeight)
                        nearestNodes.Add(gridNodes[checkX, checkY]);
                }
            }
        }

        return nearestNodes;
    }
}
