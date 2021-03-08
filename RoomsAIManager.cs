using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsAIManager : MonoBehaviour
{
    public GameObject player;
    public EnemyAI enemy;
    public NodesGrid grid;

    void Update()
    {
        if(enemy.enemyMode == EnemyFSM.Stopped)
        {
            enemy.room = grid.NodeFromWorldPoint(player.transform.position).room;
            grid.FindPath(enemy.transform.position, player.transform.position);
            enemy.nextNode = grid.path[0];
            enemy.enemyMode = EnemyFSM.ChangingRoom;
        }
    }
}
