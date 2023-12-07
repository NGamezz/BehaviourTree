using System.Threading.Tasks;
///
/// The Selector tries to find a child which return success, if one succeeds it return success, if a child fails it tries the next child, if all children return failed, it returns failed
///
public class BTSelector : BTComposite
{
    private int currentIndex = 0;

    public BTSelector ( params BTBaseNode[] children ) : base(children) { }

    protected override TaskStatus OnUpdate ()
    {
        for ( ; currentIndex < children.Length; currentIndex++ )
        {
            var result = children[currentIndex].Tick();
            switch ( result )
            {
                case TaskStatus.Success:
                    return TaskStatus.Success;
                case TaskStatus.Failed:
                    continue;
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
        Parallel.For(0, children.Length, i =>
        {
            children[i].OnReset();
        });
    }
}
