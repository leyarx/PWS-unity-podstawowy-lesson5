using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EnemyFSM
{
    Stopped,
    ChangingRoom,
    Patrolling,
    PlayerCatched
}

public class EnemyAI : MonoBehaviour
{
    // Default state
    [SerializeField]
    public EnemyFSM enemyMode = EnemyFSM.Stopped;
    public Room room;
    public Node nextNode;
    public List<Node> waypoints;
    public float speed = 1.0f;
    public NodesGrid grid;

    // Start is called before the first frame update
    void Start()
    {
        waypoints = new List<Node>();
    }

    void Update()
    {
        switch (enemyMode)
        {
            case EnemyFSM.Stopped:
            case EnemyFSM.PlayerCatched:
                break;
            case EnemyFSM.ChangingRoom:
            case EnemyFSM.Patrolling:
                MoveEnemy();
                break;
        }
    }

   void MoveEnemy()
   {     
        if(grid.path.Count > 0)
        {
            float dist = Vector3.Distance(transform.position, nextNode.position);

            if(dist< 0.001f)
            {
                nextNode = grid.path[0];
                grid.path.RemoveAt(0);
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, nextNode.position) < 0.001f)
            {
                switch (enemyMode)
                {
                    case EnemyFSM.ChangingRoom:
                        enemyMode = EnemyFSM.Patrolling;
                        waypoints.AddRange(room.waypoints);
                        break;
                    case EnemyFSM.Patrolling:
                        if (waypoints.Count > 0)
                        {
                            grid.FindPath(transform.position, waypoints[0].position);
                            waypoints.RemoveAt(0);
                            nextNode = grid.path[0];
                        }
                        else
                        {
                            enemyMode = EnemyFSM.Stopped;
                        }
                        break;
                }
            }
        }

        float step = speed * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, nextNode.position, step);
   }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Intersected with player");
            enemyMode = EnemyFSM.PlayerCatched;
        }
    }

}
