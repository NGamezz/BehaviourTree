using System;

/// <summary>
/// Similar to a sequence, but returns failed if the given condition changes to false.
/// </summary>
public class BTCancelIfFalse : BTComposite
{
    private int currentIndex = 0;

    private Func<bool> condition;

    public BTCancelIfFalse(Func<bool> condition, params BTBaseNode[] children) : base(children)
    {
        this.condition = condition;
    }

    protected override TaskStatus OnUpdate()
    {
        for (; currentIndex < children.Length; currentIndex++)
        {
            if (!condition())
            {
                OnReset();
                return TaskStatus.Failed;
            }

            var result = children[currentIndex].Tick();

            switch (result)
            {
                case TaskStatus.Success: continue;
                case TaskStatus.Failed: return TaskStatus.Failed;
                case TaskStatus.Running: return TaskStatus.Running;
            }
        }
        return TaskStatus.Success;
    }

    protected override void OnEnter()
    {
        currentIndex = 0;
    }

    protected override void OnExit()
    {
        currentIndex = 0;
    }

    public override void OnReset()
    {
        currentIndex = 0;
        foreach (var c in children)
        {
            c.OnReset();
        }
    }
}
