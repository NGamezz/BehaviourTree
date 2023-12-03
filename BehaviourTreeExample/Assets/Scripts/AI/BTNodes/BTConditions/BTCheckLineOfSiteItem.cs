using UnityEngine;

public class BTCheckLineOfSiteItem : BTBaseNode
{
    private Transform currentPosition;
    private Transform positionToCheck;
    private float fieldOfView;
    private float maxViewDistance;

    public BTCheckLineOfSiteItem(float fieldOfView, Transform currentPosition, Transform positionToCheck, float maxViewDistance)
    {
        this.fieldOfView = fieldOfView;
        this.currentPosition = currentPosition;
        this.positionToCheck = positionToCheck;
        this.maxViewDistance = maxViewDistance;
    }

    protected override void OnEnter()
    {
    }

    protected override TaskStatus OnUpdate()
    {
        Vector3 directionToTarget = positionToCheck.position - currentPosition.position;

        Debug.DrawRay(currentPosition.position, directionToTarget, Color.blue);

        if (Vector3.Angle(currentPosition.forward, directionToTarget) <= fieldOfView)
        {
            if (Physics.Raycast(currentPosition.position, directionToTarget, out RaycastHit hit, maxViewDistance))
            {
                if (hit.transform.root == positionToCheck)
                {
                    return TaskStatus.Success;
                }
            }
        }
        return TaskStatus.Failed;
    }
}
