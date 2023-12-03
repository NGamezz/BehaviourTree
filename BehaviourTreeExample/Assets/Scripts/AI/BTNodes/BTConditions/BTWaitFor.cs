public class BTWaitFor : BTBaseNode
{
    private int amount = 0;
    private float currentAmount = 0;

    public BTWaitFor(int amount)
    {
        this.amount = amount;
    }

    protected override void OnEnter()
    {
        currentAmount = amount;
    }

    protected override TaskStatus OnUpdate()
    {
        if (currentAmount > 0)
        {
            currentAmount -= UnityEngine.Time.fixedDeltaTime;
            return TaskStatus.Running;
        }

        return TaskStatus.Success;
    }
}
