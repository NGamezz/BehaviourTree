using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Jobs;
using UnityEngine.Rendering;

public class Ninja : MonoBehaviour
{
    private BTBaseNode tree;
    private BTBaseNode hidingBehaviour;

    private NavMeshAgent agent;
    public float keepDistance = 1.0f;
    public float moveSpeed = 3;
    public float maxDistanceToPlayer = 5;
    private Animator animator;
    private Blackboard blackBoard = new();

    [SerializeField] private BlackBoardObject sharedBlackboard;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        //TODO: Create your Behaviour tree here
        blackBoard.SetVariable(VariableNames.PLAYER_TRANSFORM, FindObjectOfType<Player>().transform);

        tree =//Make the hiding behaviour.
            new BTSequence(
                new BTConditionNode(() => Vector3.Distance(transform.position, blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM).position) > maxDistanceToPlayer),
                new BTGetPlayerPosition(VariableNames.PLAYER_TRANSFORM),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, keepDistance)

                //new BTConditionNode(() => sharedBlackboard.blackBoard.GetVariable<bool>(VariableNames.IS_ATTACKING)),
                //new BTAlwaysSuccesTask(() => Debug.Log($"Enemy Attacked Player. Attacking Enemy = {sharedBlackboard.blackBoard.GetVariable<Transform>(VariableNames.CURRENT_ATTACKING_ENEMY)}"))
                );

        tree.SetupBlackboard(blackBoard);
    }

    private void FixedUpdate()
    {
        tree?.Tick();
    }
}
