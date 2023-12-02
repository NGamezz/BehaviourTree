using System;

public class BTConditionNode : BTBaseNode
{
    private Func<bool> condition;

    public BTConditionNode(Func<bool> condition)
    {
        this.condition = condition;
    }

    protected override TaskStatus OnUpdate()
    {
        if (condition())
        {
            return TaskStatus.Success;
        }
        else
        {
            return TaskStatus.Failed;
        }
    }
}