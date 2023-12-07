using UnityEngine;

public class BTGetNearbyObjects : BTBaseNode
{
    private Vector3 position;
    private LayerMask layerMask;
    private float radius;
    private Collider[] colliders;
    private string variableName;

    public BTGetNearbyObjects ( Vector3 position, LayerMask layerMask, float radius, int amountOfColliders, string variableName )
    {
        this.position = position;
        this.layerMask = layerMask;
        this.radius = radius;
        colliders = new Collider[amountOfColliders];
        this.variableName = variableName;
    }

    protected override void OnEnter ()
    {
        for ( int i = 0; i < colliders.Length; i++ )
        {
            colliders[i] = null;
        }
    }

    protected override TaskStatus OnUpdate ()
    {
        if ( Physics.OverlapSphereNonAlloc(position, radius, colliders, layerMask) != 0 )
        {
            blackboard.SetVariable(variableName, colliders);

            return TaskStatus.Success;
        }
        return TaskStatus.Failed;
    }
}
