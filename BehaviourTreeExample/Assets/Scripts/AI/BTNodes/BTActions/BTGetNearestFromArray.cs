using UnityEngine;

public class BTGetNearestFromArray : BTBaseNode
{
    private Transform[] transforms;
    private Transform currentTransform;

    private string variableName;
    private Blackboard board;

    public BTGetNearestFromArray ( Transform[] transforms, Transform currentTransform, Blackboard board, string variableName )
    {
        this.transforms = transforms;
        this.currentTransform = currentTransform;
        this.board = board;
        this.variableName = variableName;
    }

    protected override TaskStatus OnUpdate ()
    {
        var currentShortesDistance = float.MaxValue;
        Transform currentShortestTransform = transforms[0];

        foreach ( Transform t in transforms )
        {
            if ( Vector3.Distance(t.position, currentTransform.position) < currentShortesDistance )
            {
                currentShortestTransform = t;
            }
        }

        board.SetVariable(variableName, currentShortestTransform.position);

        return TaskStatus.Success;
    }
}
