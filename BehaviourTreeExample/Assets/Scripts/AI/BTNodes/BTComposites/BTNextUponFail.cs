using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTNextUponFail : BTComposite
{
    private int currentIndex = 0;

    public BTNextUponFail(params BTBaseNode[] children) : base(children) { }

    protected override TaskStatus OnUpdate()
    {
        for (; currentIndex < children.Length; currentIndex++)
        {
            var result = children[currentIndex].Tick();
            switch (result)
            {
                case TaskStatus.Success: continue;
                case TaskStatus.Failed: continue;
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