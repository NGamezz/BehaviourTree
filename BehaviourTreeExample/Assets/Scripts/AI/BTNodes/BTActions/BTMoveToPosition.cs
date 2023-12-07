using UnityEngine;
using UnityEngine.AI;

public class BTMoveToPosition : BTBaseNode
{
    private NavMeshAgent agent;
    private float moveSpeed;
    private float keepDistance;
    private Vector3 targetPosition;
    private string BBtargetPosition;

    public BTMoveToPosition ( NavMeshAgent agent, float moveSpeed, string BBtargetPosition, float keepDistance )
    {
        this.agent = agent;
        this.moveSpeed = moveSpeed;
        this.BBtargetPosition = BBtargetPosition;
        this.keepDistance = keepDistance;
    }

    protected override void OnEnter ()
    {
        agent.speed = moveSpeed;
        agent.stoppingDistance = keepDistance;
        targetPosition = blackboard.GetVariable<Vector3>(BBtargetPosition);
    }

    protected override TaskStatus OnUpdate ()
    {
        if ( agent == null )
        { return TaskStatus.Failed; }
        if ( agent.pathPending )
        { return TaskStatus.Running; }
        if ( agent.hasPath && agent.path.status == NavMeshPathStatus.PathInvalid )
        { return TaskStatus.Failed; }
        if ( agent.pathEndPosition != targetPosition )
        {
            agent.SetDestination(targetPosition);
        }

        if ( Vector3.Distance(agent.transform.position, targetPosition) <= keepDistance )
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }
}

public class BTGetNextPatrolPosition : BTBaseNode
{
    private Transform[] wayPoints;
    private string conditionVariableNameForSkip;
    private bool skipSupport;

    public BTGetNextPatrolPosition ( Transform[] wayPoints, string conditionVariableName = "", bool skipSupport = false )
    {
        this.wayPoints = wayPoints;
        this.conditionVariableNameForSkip = conditionVariableName;
        this.skipSupport = skipSupport;
    }

    protected override void OnEnter ()
    {
        int currentIndex = blackboard.GetVariable<int>(VariableNames.CURRENT_PATROL_INDEX);

        if ( skipSupport )
        {
            if ( !blackboard.GetVariable<bool>(conditionVariableNameForSkip) )
            {
                currentIndex++;
                if ( currentIndex >= wayPoints.Length )
                {
                    currentIndex = 0;
                }
            }
            else
            {
                blackboard.SetVariable(conditionVariableNameForSkip, false);
            }
        }
        else
        {
            currentIndex++;
            if ( currentIndex >= wayPoints.Length )
            {
                currentIndex = 0;
            }
        }

        blackboard.SetVariable(VariableNames.CURRENT_PATROL_INDEX, currentIndex);
        blackboard.SetVariable(VariableNames.TARGET_POSITION, wayPoints[currentIndex].position);
    }

    protected override TaskStatus OnUpdate ()
    {
        return TaskStatus.Success;
    }
}

//Uses a transform to set the target position.
public class BTGetPosition : BTBaseNode
{
    private string variableName;
    private Blackboard currentBlackBoard;

    public BTGetPosition ( string variableName, Blackboard currentBlackBoard )
    {
        this.variableName = variableName;
        this.currentBlackBoard = currentBlackBoard;
    }

    protected override void OnEnter ()
    {
        Vector3 position = currentBlackBoard.GetVariable<Transform>(variableName).position;
        blackboard.SetVariable(VariableNames.TARGET_POSITION, position);
    }

    protected override TaskStatus OnUpdate ()
    {
        return TaskStatus.Success;
    }
}