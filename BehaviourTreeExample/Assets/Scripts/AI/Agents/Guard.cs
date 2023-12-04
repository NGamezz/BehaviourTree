using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Guard : MonoBehaviour
{
    public float moveSpeed = 3;
    public float keepDistance = 1f;
    public Transform[] wayPoints;

    public float fieldOfFiew = 180.0f;
    public float maxViewDistance = 5.0f;
    public float attackRange = 3.0f;

    private BTBaseNode tree;
    private BTBaseNode treePlayerChase;
    private BTBaseNode treeWeaponAcquisition;
    private BTBaseNode treeAttackPlayer;

    public TMP_Text stateUiText;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform playerTransform;
    private Blackboard blackBoard = new();

    public void AcquireWeapon(IWeapon weapon)
    {
        if (blackBoard.GetVariable<IWeapon>(VariableNames.ENEMY_CURRENT_WEAPON) != null) { return; }

        Debug.Log("Weapon Set.");

        stateUiText.text = "State : Chasing Player.";

        blackBoard.SetVariable(VariableNames.ENEMY_CURRENT_WEAPON, weapon);
        blackBoard.SetVariable(VariableNames.LOOKING_FOR_WEAPON, false);
        treeWeaponAcquisition.OnReset();
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        playerTransform = FindObjectOfType<Player>().transform;
    }

    private void Start()
    {
        SetupBehaviourTrees();
    }

    private void SetupBehaviourTrees()
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
        blackBoard.SetVariable<IWeapon>(VariableNames.ENEMY_CURRENT_WEAPON, null);

        blackBoard.SetVariable(VariableNames.ATTACK, false);
        blackBoard.SetVariable(VariableNames.ON_RESET_PATROL, false);
        blackBoard.SetVariable(VariableNames.AVAILABLE_WEAPON_PICKUP_POINTS, FindObjectsOfType<WeaponPickupPoint>().Select(x => x.GetComponent<Transform>()).ToArray());

        tree =
            new BTRepeater(wayPoints.Length,
                new BTSequence(
                    new BTConditionNode(() => !blackBoard.GetVariable<bool>(VariableNames.LOOKING_FOR_WEAPON)),
                    new BTConditionNode(() => !blackBoard.GetVariable<bool>(VariableNames.CHASING_PLAYER)),
                    new BTGetNextPatrolPosition(wayPoints, VariableNames.ON_RESET_PATROL, true),
                    new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, keepDistance),
                           new BTAlwaysSuccesTask(() => stateUiText.text = "State : Patrolling."),
                    new BTWaitFor(2.0f))
            );

        //Remove the blackboard stuff for fov.
        treePlayerChase =
            new BTSequence(
                new BTCheckLineOfSiteItem(fieldOfFiew, blackBoard.GetVariable<Transform>(VariableNames.ENEMY_TRANSFORM), blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM), maxViewDistance - 1.0f),
                new BTAlwaysSuccesTask(() => Debug.Log("Player within distance.")),

                new BTAlwaysSuccesTask(() => tree.OnReset()),
                new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.ON_RESET_PATROL, true)),
                new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.CHASING_PLAYER, true)),

                //Repeats while the chasing player variable is true.
                new BTRepeatWhile(() => blackBoard.GetVariable<bool>(VariableNames.CHASING_PLAYER),
                    new BTSelector(

                       //For initiating the weapon acquisition tree.
                       new BTSequence(
                           new BTConditionNode(() => blackBoard.GetVariable<IWeapon>(VariableNames.ENEMY_CURRENT_WEAPON) == null),
                           new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.LOOKING_FOR_WEAPON, true)),
                           new BTAlwaysSuccesTask(() => stateUiText.text = "State : Looking For Weapon.")
                           ),

                       //For initiating the attack tree.
                       new BTSequence(
                           new BTConditionNode(() => Vector3.Distance(blackBoard.GetVariable<Transform>(VariableNames.ENEMY_TRANSFORM).position, playerTransform.position) < attackRange),
                           new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.ATTACK, true)),
                           new BTAlwaysSuccesTask(() => stateUiText.text = "State : Attacking.")
                           ),

                       //For performing the player chase.
                       new BTSequence(new BTGetPlayerPosition(VariableNames.PLAYER_TRANSFORM),
                           new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, keepDistance),
                           new BTConditionNode(() => Vector3.Distance(blackBoard.GetVariable<Transform>(VariableNames.ENEMY_TRANSFORM).position, blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM).position) < maxViewDistance),
                           new BTAlwaysSuccesTask(() => stateUiText.text = "State : Chasing Player.")
                           ),

                       //For Disabling the player chase.
                       new BTSequence(new BTAlwaysSuccesTask(() => Debug.Log("Player Outside Of Distance.")),
                            new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.CHASING_PLAYER, false)),
                           new BTAlwaysSuccesTask(() => stateUiText.text = "State : Disable Chasing Player."),
                            new BTWaitFor(1.0f)
                            )
           )));

        treeWeaponAcquisition =
            new BTSequence(
                new BTConditionNode(() => blackBoard.GetVariable<bool>(VariableNames.LOOKING_FOR_WEAPON)),
                new BTGetNearestFromArray(blackBoard.GetVariable<Transform[]>(VariableNames.AVAILABLE_WEAPON_PICKUP_POINTS), blackBoard.GetVariable<Transform>(VariableNames.ENEMY_TRANSFORM), blackBoard, VariableNames.ENEMY_NEAREST_WEAPON),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.ENEMY_NEAREST_WEAPON, keepDistance)
          );

        treeAttackPlayer =
            new BTSequence(
                new BTConditionNode(() => blackBoard.GetVariable<bool>(VariableNames.ATTACK)),
                new BTRepeatWhile(() => blackBoard.GetVariable<bool>(VariableNames.ATTACK),
                    new BTSelector(

                        //Handling For During The Attack.
                        new BTSequence(
                            new BTAlwaysSuccesTask(() => Debug.Log("Attack")),
                            new BTConditionNode(() => Vector3.Distance(blackBoard.GetVariable<Transform>(VariableNames.ENEMY_TRANSFORM).position, blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM).position) < attackRange),
                            new BTWaitFor(0.5f)
                        ),

                        //Disabling the attack once the distance is greater than the attack range.
                        new BTSequence(
                            new BTAlwaysSuccesTask(() => Debug.Log("Attack Reset.")),
                            new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.ATTACK, false)),
                            new BTAlwaysSuccesTask(() => treeAttackPlayer.OnReset())
                        )
          )));

        tree.SetupBlackboard(blackBoard);
        treeAttackPlayer.SetupBlackboard(blackBoard);
        treeWeaponAcquisition.SetupBlackboard(blackBoard);
        treePlayerChase.SetupBlackboard(blackBoard);
    }

    private void OnDisable()
    {
        blackBoard.ClearBlackBoard(true, true);
    }

    private void FixedUpdate()
    {
        treeAttackPlayer.Tick();
        tree.Tick();
        treeWeaponAcquisition.Tick();
        treePlayerChase.Tick();
    }
}
