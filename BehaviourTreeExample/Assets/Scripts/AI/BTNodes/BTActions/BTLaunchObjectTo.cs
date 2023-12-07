using UnityEngine;

public class BTLaunchObjectTo : BTBaseNode
{
    private GameObject objectToLaunchPrefab;
    private Transform launcherTransform;
    private Vector3 targetPosition;
    private float moveSpeed;
    private Blackboard sharedBlackboard;
    private string targetVariableName;

    GameObject objectInstance;
    private bool launched = false;

    public BTLaunchObjectTo ( Transform launcherTransform, GameObject objectToLaunchPrefab, float moveSpeed, Blackboard sharedBlackboard, string targetVariableNameTransform )
    {
        this.objectToLaunchPrefab = objectToLaunchPrefab;
        this.launcherTransform = launcherTransform;
        this.moveSpeed = moveSpeed;
        this.targetVariableName = targetVariableNameTransform;
        this.sharedBlackboard = sharedBlackboard;
    }

    protected override TaskStatus OnUpdate ()
    {
        targetPosition = sharedBlackboard.GetVariable<Transform>(targetVariableName).position;

        if ( !launched )
        {
            launched = true;
            objectInstance = GameObject.Instantiate(objectToLaunchPrefab, launcherTransform.position, Quaternion.identity);
        }
        var directionToTarget = targetPosition - objectInstance.transform.position;

        if ( directionToTarget.magnitude > 1.0f )
        {
            objectInstance.transform.Translate(moveSpeed * Time.fixedDeltaTime * directionToTarget);
            return TaskStatus.Running;
        }
        else
        {
            launched = false;
            Object.Destroy(objectInstance);
            return TaskStatus.Success;
        }
    }
}
