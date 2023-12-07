public class BTAlwaysFalse : BTBaseNode
{
    protected override TaskStatus OnUpdate ()
    {
        return TaskStatus.Failed;
    }
}
