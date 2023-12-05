using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Guard : MonoBehaviour, IWeaponOwner
{
    public float moveSpeed = 3;
    public float keepDistance = 1f;
    public Transform[] wayPoints;

    [SerializeField] private float fieldOfFiew = 180.0f;
    [SerializeField] private float maxViewDistance = 5.0f;
    [SerializeField] private float attackRange = 3.0f;

    private BTBaseNode patrolTree;
    private BTBaseNode treePlayerChase;
    private BTBaseNode treeWeaponAcquisition;
    private BTBaseNode treeAttackPlayer;

    private List<BTBaseNode> trees = new();

    [SerializeField] private TMP_Text stateUiText;
    [SerializeField] private BlackBoardObject sharedBlackBoard;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform playerTransform;
    private Blackboard blackBoard = new();

    private bool gameRunning = true;

    public void AcquireWeapon(Weapon weapon)
    {
        if (blackBoard.GetVariable<Weapon>(VariableNames.ENEMY_CURRENT_WEAPON) != null) { return; }

        Debug.Log("Weapon Set.");

        stateUiText.text = "State : Chasing Player.";

        blackBoard.SetVariable(VariableNames.ENEMY_CURRENT_WEAPON, weapon);
        blackBoard.SetVariable(VariableNames.LOOKING_FOR_WEAPON, false);
        treeWeaponAcquisition.OnReset();
    }

    private void OnEnable()
    {
        EventManager.AddListener(EventType.GameOver, GameOver);
    }

    private void OnDisable()
    {
        blackBoard.ClearBlackBoard(true, true);
        EventManager.RemoveListener(EventType.GameOver, GameOver);
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

    //Make the attacking enemy a list, so multiple enemies could attack at once. To be fixed, Guard Attacking + Ninja Detection of attack.
    private void SetupBehaviourTrees()
    {
        //Create your Behaviour Tree here!
        blackBoard.SetVariable(VariableNames.OWN_HEALTH, 100);
        blackBoard.SetVariable(VariableNames.TARGET_POSITION, new Vector3(0, 0, 0));
        blackBoard.SetVariable(VariableNames.CURRENT_PATROL_INDEX, -1);
        blackBoard.SetVariable(VariableNames.CHASING_PLAYER, false);
        blackBoard.SetVariable(VariableNames.PLAYER_TRANSFORM, playerTransform);
        blackBoard.SetVariable(VariableNames.OWN_TRANSFORM, transform);
        blackBoard.SetVariable<Weapon>(VariableNames.ENEMY_CURRENT_WEAPON, null);

        blackBoard.SetVariable(VariableNames.IS_ATTACKING, false);
        blackBoard.SetVariable(VariableNames.ON_RESET_PATROL, false);
        blackBoard.SetVariable(VariableNames.AVAILABLE_WEAPON_PICKUP_POINTS, FindObjectsOfType<WeaponPickupPoint>().Select(x => x.GetComponent<Transform>()).ToArray());

        patrolTree =
            new BTRepeater(wayPoints.Length,
                new BTSequence(
                    new BTConditionNode(() => gameRunning),
                    new BTConditionNode(() => !blackBoard.GetVariable<bool>(VariableNames.LOOKING_FOR_WEAPON)),
                    new BTConditionNode(() => !blackBoard.GetVariable<bool>(VariableNames.CHASING_PLAYER)),
                    new BTGetNextPatrolPosition(wayPoints, VariableNames.ON_RESET_PATROL, true),
                    new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, keepDistance),
                    new BTAlwaysSuccesTask(() => stateUiText.text = "State : Patrolling."),
                    new BTWaitFor(2.0f))
            );

        treePlayerChase =
            new BTSequence(
                new BTConditionNode(() => gameRunning),
                new BTCheckLineOfSiteItem(fieldOfFiew, transform, blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM), maxViewDistance - 1.0f),
                new BTAlwaysSuccesTask(() => Debug.Log("Player within distance.")),
                new BTAlwaysSuccesTask(() => stateUiText.text = "State : Chasing Player."),

                new BTAlwaysSuccesTask(() => patrolTree.OnReset()),
                new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.ON_RESET_PATROL, true)),
                new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.CHASING_PLAYER, true)),

                //Repeats while the chasing player variable is true.
                new BTRepeatWhile(() => blackBoard.GetVariable<bool>(VariableNames.CHASING_PLAYER),
                    new BTSelector(

                       //For initiating the weapon acquisition tree.
                       new BTSequence(
                           new BTConditionNode(() => blackBoard.GetVariable<Weapon>(VariableNames.ENEMY_CURRENT_WEAPON) == null),
                           new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.LOOKING_FOR_WEAPON, true)),
                           new BTAlwaysSuccesTask(() => stateUiText.text = "State : Looking For Weapon.")
                           ),

                       //For initiating the attack tree.
                       new BTSequence(
                           new BTConditionNode(() => Vector3.Distance(transform.position, blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM).position) < attackRange),
                           new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.IS_ATTACKING, true)),

                           new BTAlwaysSuccesTask(() => sharedBlackBoard.blackBoard.SetVariable(VariableNames.IS_ATTACKING, true)),
                           new BTAlwaysSuccesTask(() => sharedBlackBoard.blackBoard.SetVariable(VariableNames.CURRENT_ATTACKING_ENEMY, transform)),

                           new BTAlwaysSuccesTask(() => stateUiText.text = "State : Attacking.")
                           ),

                       //For performing the player chase.
                       new BTSequence(
                           new BTGetPlayerPosition(VariableNames.PLAYER_TRANSFORM),
                           new BTAlwaysSuccesTask(() => { stateUiText.text = "State : Chasing Player."; treeAttackPlayer.OnReset(); }),
                           new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, keepDistance),
                           new BTConditionNode(() => Vector3.Distance(transform.position, blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM).position) < maxViewDistance + 1.0f)
                           ),

                       //For Disabling the player chase.
                       new BTSequence(
                           new BTAlwaysSuccesTask(() => Debug.Log("Player Outside Of Distance.")),
                           new BTWaitFor(2.0f),
                           new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.CHASING_PLAYER, false)),
                           new BTAlwaysSuccesTask(() => stateUiText.text = "State : Disable Chasing Player.")
                            )
          )));

        treeWeaponAcquisition =
            new BTSequence(
                new BTConditionNode(() => gameRunning),
                new BTConditionNode(() => blackBoard.GetVariable<Weapon>(VariableNames.ENEMY_CURRENT_WEAPON) == null),
                new BTConditionNode(() => blackBoard.GetVariable<bool>(VariableNames.LOOKING_FOR_WEAPON)),
                new BTGetNearestFromArray(blackBoard.GetVariable<Transform[]>(VariableNames.AVAILABLE_WEAPON_PICKUP_POINTS), blackBoard.GetVariable<Transform>(VariableNames.OWN_TRANSFORM), blackBoard, VariableNames.ENEMY_NEAREST_WEAPON_POSITION),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.ENEMY_NEAREST_WEAPON_POSITION, keepDistance)
          );

        treeAttackPlayer =
            new BTSequence(
                new BTConditionNode(() => gameRunning),
                new BTConditionNode(() => blackBoard.GetVariable<bool>(VariableNames.IS_ATTACKING)),

                new BTRepeatWhile(() => blackBoard.GetVariable<bool>(VariableNames.IS_ATTACKING),
                    new BTSelector(
                        //Handling For During The Attack.
                        new BTSequence(
                            new BTWaitFor(2.5f),
                            new BTAlwaysSuccesTask(() => blackBoard.GetVariable<Transform>(VariableNames.OWN_TRANSFORM).LookAt(blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM))),
                            new BTAlwaysSuccesTask(() => blackBoard.GetVariable<Weapon>(VariableNames.ENEMY_CURRENT_WEAPON).Attack(blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM).GetComponent<IDamagable>())),
                            new BTConditionNode(() => Vector3.Distance(transform.position, blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM).position) < attackRange)
                        ),

                        //Disabling the attack once the distance is greater than the attack range.
                        new BTSequence(
                            new BTAlwaysSuccesTask(() => Debug.Log("Attack Reset.")),
                            new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.IS_ATTACKING, false)),

                            new BTAlwaysSuccesTask(() => sharedBlackBoard.blackBoard.SetVariable(VariableNames.IS_ATTACKING, false)),
                            new BTAlwaysSuccesTask(() => sharedBlackBoard.blackBoard.SetVariable<Transform>(VariableNames.CURRENT_ATTACKING_ENEMY, null)),

                            new BTAlwaysSuccesTask(() => treeAttackPlayer.OnReset())
                        )
          )));

        patrolTree.SetupBlackboard(blackBoard);
        treeAttackPlayer.SetupBlackboard(blackBoard);
        treeWeaponAcquisition.SetupBlackboard(blackBoard);
        treePlayerChase.SetupBlackboard(blackBoard);

        trees.Add(treeAttackPlayer);
        trees.Add(patrolTree);
        trees.Add(treeWeaponAcquisition);
        trees.Add(treePlayerChase);
    }

    private void GameOver()
    {
        gameRunning = false;
        foreach (BTBaseNode tree in trees)
        {
            tree?.OnReset();
        }
    }

    private void FixedUpdate()
    {
        if (!gameRunning) { return; }

        foreach (BTBaseNode tree in trees)
        {
            if (!gameRunning) { return; }
            tree?.Tick();
        }
    }
}