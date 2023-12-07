using System;
using UnityEngine;
using UnityEngine.AI;

public class BTFindHidePosition : BTBaseNode
{
    private Collider[] colliders;
    private NavMeshAgent agent;
    private string transformToHideFromVariableName;
    private string variableName;
    private Blackboard sharedBlackBoard;
    private Transform transformToHideFrom;
    private Transform transformToStayCloseTo;
    private float maxDistanceTo;

    public BTFindHidePosition ( string variableName, NavMeshAgent agent, string transformToHideFromVariableName, Blackboard sharedBlackBoard, Transform transformToStayCloseTo, float maxDistanceTo )
    {
        this.variableName = variableName;
        this.agent = agent;
        this.transformToHideFromVariableName = transformToHideFromVariableName;
        this.sharedBlackBoard = sharedBlackBoard;
        this.transformToStayCloseTo = transformToStayCloseTo;
        this.maxDistanceTo = maxDistanceTo;
    }

    protected override void OnEnter ()
    {
        colliders = blackboard.GetVariable<Collider[]>(variableName);
        transformToHideFrom = sharedBlackBoard.GetVariable<Transform>(transformToHideFromVariableName).transform;

        Array.Sort(colliders, CompareCloserOne);
    }

    protected override TaskStatus OnUpdate ()
    {
        for ( int i = 0; i < colliders.Length; i++ )
        {
            if ( colliders[i] == null )
            { continue; }

            if ( Vector3.Distance(transformToStayCloseTo.position, colliders[i].transform.position) > maxDistanceTo )
            { continue; }

            if ( NavMesh.SamplePosition(colliders[i].transform.position, out NavMeshHit hit, 2.0f, agent.areaMask) )
            {
                //To obtain the normal of the hit.
                if ( !NavMesh.FindClosestEdge(hit.position, out hit, agent.areaMask) )
                {
                    Debug.LogWarning("Failed to find closest Edge.");
                }

                if ( Vector3.Dot(hit.normal, (transformToHideFrom.position - hit.position).normalized) < 0.0f )
                {
                    blackboard.SetVariable(VariableNames.TARGET_POSITION, hit.position);
                    return TaskStatus.Success;
                }
                else
                {
                    //Try to get a position with an offset.
                    if ( NavMesh.SamplePosition(colliders[i].transform.position - (transformToHideFrom.position - hit.position) * 2.0f, out NavMeshHit hit2, 2.0f, agent.areaMask) )
                    {
                        if ( !NavMesh.FindClosestEdge(hit2.position, out hit2, agent.areaMask) )
                        {
                            Debug.LogWarning("Failed to find closest Edge.");
                        }

                        if ( Vector3.Dot(hit2.normal, (transformToHideFrom.position - hit2.position).normalized) < 0.0f )
                        {
                            blackboard.SetVariable(VariableNames.TARGET_POSITION, hit2.position);
                            return TaskStatus.Success;
                        }
                    }
                }
            }
        }
        return TaskStatus.Failed;
    }

    //For the sorting of the colliders Array.
    private int CompareCloserOne ( Collider a, Collider b )
    {
        if ( a == null && b != null )
        {
            return 1;
        }
        else if ( a != null && b == null )
        {
            return -1;
        }
        else if ( a == null && b == null )
        {
            return 0;
        }
        else
        {
            return Vector3.Distance(transformToStayCloseTo.position, a.transform.position).CompareTo(Vector3.Distance(transformToStayCloseTo.position, b.transform.position));
        }
    }
}