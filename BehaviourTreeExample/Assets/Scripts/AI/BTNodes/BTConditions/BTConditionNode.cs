using System;

/// <summary>
/// If one of the conditions returns false the node fails.
/// </summary>
public class BTConditionNode : BTBaseNode
{
    private Func<bool>[] condition;

    public BTConditionNode ( params Func<bool>[] condition )
    {
        this.condition = condition;
    }

    protected override TaskStatus OnUpdate ()
    {
        foreach ( var condition in condition )
        {
            if ( !condition() )
            {
                return TaskStatus.Failed;
            }
        }
        return TaskStatus.Success;
    }
}

public class BTConditionNodeIfOneIsTrue : BTBaseNode
{
    private Func<bool>[] condition;

    public BTConditionNodeIfOneIsTrue ( params Func<bool>[] condition )
    {
        this.condition = condition;
    }

    protected override TaskStatus OnUpdate ()
    {
        foreach ( var condition in condition )
        {
            if ( condition() )
            {
                return TaskStatus.Success;
            }
        }
        return TaskStatus.Failed;
    }
}