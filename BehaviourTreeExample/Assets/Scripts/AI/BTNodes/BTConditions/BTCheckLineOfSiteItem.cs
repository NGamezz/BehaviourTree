using System.Runtime.CompilerServices;
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
        var directionToTarget = positionToCheck.position - currentPosition.position;

        if (Vector3.Angle(currentPosition.forward, directionToTarget) <= fieldOfView)
        {
            var ray = new Ray(currentPosition.position, directionToTarget);
            Debug.DrawRay(currentPosition.position, directionToTarget, Color.blue);
            if (Physics.Raycast(ray, out RaycastHit hit, maxViewDistance))
            {
                if (hit.transform.root == positionToCheck)
                {
                    return TaskStatus.Success;
                }

                return TaskStatus.Failed;
            }
        }

        return TaskStatus.Failed;
    }
}

