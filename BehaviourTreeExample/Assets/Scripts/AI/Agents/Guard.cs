using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.AI;

public class Guard : MonoBehaviour
{
    public float moveSpeed = 3;
    public float keepDistance = 1f;
    public Transform[] wayPoints;
    private BTBaseNode tree;
    private BTBaseNode treePlayerChase;
    private BTBaseNode treeWeaponAcquisition;
    private NavMeshAgent agent;
    private Animator animator;
    [SerializeField] private float fieldOfFiew = 180.0f;
    [SerializeField] private float maxViewDistance = 5.0f;
    [SerializeField] private float attackRange = 3.0f;

    [SerializeField] private Transform playerTransform;

    private Blackboard blackBoard = new();

    public void AcquireWeapon(IWeapon weapon)
    {
        if (blackBoard.GetVariable<IWeapon>(VariableNames.ENEMY_CURRENT_WEAPON) != null) { return; }

        blackBoard.SetVariable(VariableNames.ENEMY_CURRENT_WEAPON, weapon);
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        playerTransform = FindObjectOfType<Player>().transform;
    }

    private void Start()
    {
        //Create your Behaviour Tree here!
        blackBoard.SetVariable(VariableNames.ENEMY_HEALTH, 100);
        blackBoard.SetVariable(VariableNames.TARGET_POSITION, new Vector3(0, 0, 0));
        blackBoard.SetVariable(VariableNames.CURRENT_PATROL_INDEX, -1);
        blackBoard.SetVariable(VariableNames.CHASING_PLAYER, false);
        blackBoard.SetVariable(VariableNames.PLAYER_TRANSFORM, playerTransform);
        blackBoard.SetVariable(VariableNames.ENEMY_TRANSFORM, transform);
        blackBoard.SetVariable(VariableNames.ENEMY_FOV, fieldOfFiew);
        blackBoard.SetVariable(VariableNames.ENEMY_MAX_VIEW_DISTANCE, maxViewDistance);

        blackBoard.SetVariable(VariableNames.AVAILABLE_WEAPON_PICKUP_POINTS, FindObjectsOfType<WeaponPickupPoint>());

        tree =
            new BTRepeater(wayPoints.Length,
                new BTSequence(
                    new BTConditionNode(() => !blackBoard.GetVariable<bool>(VariableNames.CHASING_PLAYER)),
                    new BTGetNextPatrolPosition(wayPoints),
                    new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, keepDistance),
                    new BTWaitFor(4))
            );

        treePlayerChase =
            new BTSequence(
                new BTCheckLineOfSiteItem(blackBoard.GetVariable<float>(VariableNames.ENEMY_FOV), blackBoard.GetVariable<Transform>(VariableNames.ENEMY_TRANSFORM), blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM), blackBoard.GetVariable<float>(VariableNames.ENEMY_MAX_VIEW_DISTANCE)),
                new BTAlwaysSuccesTask(() => Debug.Log("Player within distance.")),
                new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.CHASING_PLAYER, true)),

                new BTRepeatWhile(() => blackBoard.GetVariable<bool>(VariableNames.CHASING_PLAYER),
                   new BTSelector(
                        new BTSequence(new BTGetPlayerPosition(VariableNames.PLAYER_TRANSFORM),
                            new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, keepDistance),
                            new BTConditionNode(() => Vector3.Distance(blackBoard.GetVariable<Transform>(VariableNames.ENEMY_TRANSFORM).position, blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM).position) < blackBoard.GetVariable<float>(VariableNames.ENEMY_MAX_VIEW_DISTANCE))),
                        new BTSequence(new BTAlwaysSuccesTask(() => Debug.Log("Player Outside Of Distance.")),
                            new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.CHASING_PLAYER, false)))
                )));

        //treeWeaponAcquisition =
        //    new BTSequence(
        //new BTConditionNode(() => blackBoard.GetVariable<IWeapon>(VariableNames.ENEMY_CURRENT_WEAPON) != null),
        //new BTConditionNode(() => blackBoard.GetVariable<bool>(VariableNames.CHASING_PLAYER)),

        //        );

        tree.SetupBlackboard(blackBoard);
        treePlayerChase.SetupBlackboard(blackBoard);
    }

    private void FixedUpdate()
    {
        tree.Tick();
        treePlayerChase.Tick();
    }
}
