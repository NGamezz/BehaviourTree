using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class Guard : MonoBehaviour
{
    public float moveSpeed = 3;
    public float keepDistance = 1f;
    public Transform[] wayPoints;
    private BTBaseNode tree;
    private BTBaseNode treePlayerChase;
    private NavMeshAgent agent;
    private Animator animator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        //Create your Behaviour Tree here!
        Blackboard blackboard = new();
        blackboard.SetVariable(VariableNames.ENEMY_HEALTH, 100);
        blackboard.SetVariable(VariableNames.TARGET_POSITION, new Vector3(0, 0, 0));
        blackboard.SetVariable(VariableNames.CURRENT_PATROL_INDEX, -1);
        blackboard.SetVariable(VariableNames.PLAYER_TRANSFORM, FindObjectOfType<Player>().transform.position);

        tree =
            new BTRepeater(wayPoints.Length,
                new BTSequence(
                    new BTGetNextPatrolPosition(wayPoints),
                    new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, keepDistance)
                   )
            );

        treePlayerChase =
            new BTSequence(
                new BTConditionNode(() => { return Vector3.Distance(blackboard.GetVariable<Vector3>(VariableNames.PLAYER_TRANSFORM), transform.position) < 10.0f; }),
                new BTGetPlayerPosition(VariableNames.PLAYER_TRANSFORM),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, keepDistance)
                );

        tree.SetupBlackboard(blackboard);
        treePlayerChase.SetupBlackboard(blackboard);
    }

    private void FixedUpdate()
    {
        tree.Tick();
        treePlayerChase.Tick();
    }
}
