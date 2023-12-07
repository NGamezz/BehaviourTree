using System;

/// <summary>
/// Similar to a sequence, but returns failed if the given condition changes to false. Can be quite heavy.
/// </summary>
public class BTCancelIfFalse : BTComposite
{
    private int currentIndex = 0;
    private int amountOfConditions = 1;

    private Func<bool> condition;
    private Func<bool> condition2;

    public BTCancelIfFalse ( Func<bool> condition, params BTBaseNode[] children ) : base(children)
    {
        this.condition = condition;
        amountOfConditions = 1;
    }

    public BTCancelIfFalse ( Func<bool> condition, Func<bool> condition2, params BTBaseNode[] children ) : base(children)
    {
        this.condition = condition;
        this.condition2 = condition2;
        amountOfConditions = 2;
    }

    protected override TaskStatus OnUpdate ()
    {
        for ( ; currentIndex < children.Length; currentIndex++ )
        {
            if ( amountOfConditions == 1 )
            {
                if ( !condition() )
                {
                    OnReset();
                    return TaskStatus.Failed;
                }
            }
            else
            {
                if ( !condition() || !condition2() )
                {
                    OnReset();
                    return TaskStatus.Failed;
                }
            }

            var result = children[currentIndex].Tick();

            switch ( result )
            {
                case TaskStatus.Success:
                    continue;
                case TaskStatus.Failed:
                    return TaskStatus.Failed;
                case TaskStatus.Running:
                    return TaskStatus.Running;
            }
        }
        return TaskStatus.Success;
    }

    protected override void OnEnter ()
    {
        currentIndex = 0;
    }

    protected override void OnExit ()
    {
        currentIndex = 0;
    }

    public override void OnReset ()
    {
        currentIndex = 0;
        foreach ( var c in children )
        {
            c.OnReset();
        }
    }
}
