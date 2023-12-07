using System;

public class BTAlwaysSuccesTask : BTBaseNode
{
    private Action action;

    public BTAlwaysSuccesTask ( Action action )
    {
        this.action = action;
    }

    protected override void OnEnter ()
    {
        action?.Invoke();
    }

    protected override TaskStatus OnUpdate ()
    {
        return TaskStatus.Success;
    }
}
