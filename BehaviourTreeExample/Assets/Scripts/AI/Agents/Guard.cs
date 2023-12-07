using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;

public class Guard : MonoBehaviour, IWeaponOwner
{
    public float MoveSpeed = 3;
    public float KeepDistance = 1f;
    public Transform[] WayPoints;

    [SerializeField] private float fieldOfFiew = 90.0f;
    [SerializeField] private float maxViewDistance = 5.0f;
    [SerializeField] private float attackRange = 3.0f;
    [SerializeField] private float delayBetweenAttack = 3.0f;

    [SerializeField] private TMP_Text stateUiText;
    [SerializeField] private BlackBoardObject sharedBlackBoard;
    [SerializeField] private float waitTime = 4;

    private BTBaseNode patrolTree;
    private BTBaseNode treePlayerChase;
    private BTBaseNode treeWeaponAcquisition;
    private BTBaseNode treeAttackPlayer;

    private List<BTBaseNode> trees = new();

    private NavMeshAgent agent;
    private Animator animator;
    private Blackboard blackBoard = new();
    private Player player;
    private bool gameOver = false;

    private BTBaseNode chaseMovement;

    public void AcquireWeapon ( Weapon weapon )
    {
        if ( blackBoard.GetVariable<Weapon>(VariableNames.ENEMY_CURRENT_WEAPON) != null )
        { return; }

        Debug.Log("Weapon Set.");

        stateUiText.text = "State : Chasing Player.";

        blackBoard.SetVariable(VariableNames.ENEMY_CURRENT_WEAPON, weapon);
        blackBoard.SetVariable(VariableNames.LOOKING_FOR_WEAPON, false);
        treeWeaponAcquisition.OnReset();
    }

    private void OnEnable ()
    {
        EventManager.AddListener(EventType.GameOver, GameOver);
    }

    private void OnDisable ()
    {
        blackBoard.ClearBlackBoard(true, true);
        EventManager.RemoveListener(EventType.GameOver, GameOver);
    }

    private void Awake ()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player = FindObjectOfType<Player>();
    }

    private void Start ()
    {
        SetupBehaviourTrees();
    }

    //Make the attacking enemy a list, so multiple enemies could attack at once. Todo, Ninja smokebomb.
    private void SetupBehaviourTrees ()
    {
        blackBoard.SetVariable(VariableNames.OWN_HEALTH, 100);
        blackBoard.SetVariable(VariableNames.TARGET_POSITION, Vector3.zero);
        blackBoard.SetVariable(VariableNames.CURRENT_PATROL_INDEX, -1);
        blackBoard.SetVariable(VariableNames.CHASING_PLAYER, false);
        blackBoard.SetVariable(VariableNames.PLAYER_TRANSFORM, player.transform);
        blackBoard.SetVariable(VariableNames.OWN_TRANSFORM, transform);
        blackBoard.SetVariable<Weapon>(VariableNames.ENEMY_CURRENT_WEAPON, null);

        blackBoard.SetVariable(VariableNames.IS_ATTACKING, false);
        blackBoard.SetVariable(VariableNames.ON_RESET_PATROL, false);

        sharedBlackBoard.blackBoard.SetVariable(VariableNames.IS_ATTACKING, false);
        sharedBlackBoard.blackBoard.SetVariable(VariableNames.SMOKE_BOMB, false);

        if ( sharedBlackBoard.blackBoard.GetVariable<Transform[]>(VariableNames.AVAILABLE_WEAPON_PICKUP_POINTS) == null )
        {
            sharedBlackBoard.blackBoard.SetVariable(VariableNames.AVAILABLE_WEAPON_PICKUP_POINTS, FindObjectsOfType<WeaponPickupPoint>().Select(x => x.GetComponent<Transform>()).ToArray());
        }

        if ( WayPoints.Length != 0 )
        {
            patrolTree =
                new BTRepeater(WayPoints.Length,
                    new BTSequence(
                        new BTConditionNode(() => !blackBoard.GetVariable<bool>(VariableNames.CHASING_PLAYER)),
                        new BTGetNextPatrolPosition(WayPoints, VariableNames.ON_RESET_PATROL, true),
                        new BTMoveToPosition(agent, MoveSpeed, VariableNames.TARGET_POSITION, KeepDistance),
                        new BTAlwaysSuccesTask(() => stateUiText.text = "State : Patrolling."),
                        new BTWaitFor(2.0f))
                );
        }

        treePlayerChase =
            new BTSequence(
                new BTConditionNode(() => !gameOver),
                new BTCheckLineOfSiteItem(fieldOfFiew, transform, blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM), maxViewDistance),
                new BTAlwaysSuccesTask(() => Debug.Log("Player within distance.")),
                new BTAlwaysSuccesTask(() => stateUiText.text = "State : Chasing Player."),

                new BTAlwaysSuccesTask(() => patrolTree.OnReset()),
                new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.ON_RESET_PATROL, true)),
                new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.CHASING_PLAYER, true)),

                //Repeats while the chasing player variable is true.
                new BTRepeatWhile(() => blackBoard.GetVariable<bool>(VariableNames.CHASING_PLAYER),
                    new BTSelector(

                       //During weapon search, wait.
                       new BTSequence(
                           new BTConditionNode(() => blackBoard.GetVariable<Weapon>(VariableNames.ENEMY_CURRENT_WEAPON) == null),
                           new BTWaitFor(1)
                           ),

                       //During Attack/Smoked, Wait
                       new BTSequence(
                           new BTConditionNodeIfOneIsTrue(() => Vector3.Distance(transform.position, blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM).position) < attackRange,
                           () => sharedBlackBoard.blackBoard.GetVariable<bool>(VariableNames.SMOKE_BOMB)),
                           new BTWaitFor(1)
                           ),

                       //Perform the player chase.
                       new BTSequence(
                           new BTGetPosition(VariableNames.PLAYER_TRANSFORM, blackBoard),
                           new BTAlwaysSuccesTask(() => stateUiText.text = "State : Chasing Player."),
                           chaseMovement = new BTMoveToPosition(agent, MoveSpeed, VariableNames.TARGET_POSITION, KeepDistance),
                           new BTConditionNode(() => Vector3.Distance(transform.position, blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM).position) < maxViewDistance)
                           ),

                       //Disable the player chase.
                       new BTSequence(
                           new BTAlwaysSuccesTask(() => Debug.Log("Player Outside Of Distance.")),
                           new BTWaitFor(2.0f),
                           new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.CHASING_PLAYER, false)),
                           new BTAlwaysSuccesTask(() => stateUiText.text = "State : Disable Chasing Player.")
                            )
          )));

        treeWeaponAcquisition =
            new BTSequence(
                new BTConditionNode(() => !gameOver, () => blackBoard.GetVariable<Weapon>(VariableNames.ENEMY_CURRENT_WEAPON) == null, () => blackBoard.GetVariable<bool>(VariableNames.CHASING_PLAYER)),
                new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.LOOKING_FOR_WEAPON, true)),
                new BTAlwaysSuccesTask(() => patrolTree.OnReset()),
                new BTAlwaysSuccesTask(() => stateUiText.text = "State : Looking For Weapon."),
                new BTGetNearestFromArray(sharedBlackBoard.blackBoard.GetVariable<Transform[]>(VariableNames.AVAILABLE_WEAPON_PICKUP_POINTS), blackBoard.GetVariable<Transform>(VariableNames.OWN_TRANSFORM),
                blackBoard, VariableNames.ENEMY_NEAREST_WEAPON_POSITION),
                new BTMoveToPosition(agent, MoveSpeed, VariableNames.ENEMY_NEAREST_WEAPON_POSITION, KeepDistance)
          );

        treeAttackPlayer =
            new BTSequence(
                new BTCheckLineOfSiteItem(fieldOfFiew, transform, blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM), maxViewDistance),
                new BTConditionNode(() => !gameOver, () => Vector3.Distance(transform.position, blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM).position) < attackRange,
                () => blackBoard.GetVariable<Weapon>(VariableNames.ENEMY_CURRENT_WEAPON) != null),
                new BTAlwaysSuccesTask(() => patrolTree.OnReset()),
                new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.IS_ATTACKING, true)),
                new BTAlwaysSuccesTask(() => sharedBlackBoard.blackBoard.SetVariable(VariableNames.IS_ATTACKING, true)),
                new BTAlwaysSuccesTask(() => sharedBlackBoard.blackBoard.SetVariable(VariableNames.CURRENT_ATTACKING_ENEMY, transform)),
                new BTAlwaysSuccesTask(() => chaseMovement.OnReset()),

                new BTRepeatWhile(() => blackBoard.GetVariable<bool>(VariableNames.IS_ATTACKING),
                    new BTSelector(

                        //Handling For During The Attack.
                        new BTSequence(
                            new BTConditionNode(() => !sharedBlackBoard.blackBoard.GetVariable<bool>(VariableNames.SMOKE_BOMB)),
                            new BTCancelIfFalse(() => Vector3.Distance(transform.position, blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM).position) < attackRange,
                            () => !sharedBlackBoard.blackBoard.GetVariable<bool>(VariableNames.SMOKE_BOMB),
                                 new BTAlwaysSuccesTask(() => stateUiText.text = "State : Attacking."),
                                 new BTAlwaysSuccesTask(() => blackBoard.GetVariable<Transform>(VariableNames.OWN_TRANSFORM).LookAt(blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM))),
                                 new BTWaitFor(delayBetweenAttack),
                                 new BTAlwaysSuccesTask(() => blackBoard.GetVariable<Weapon>(VariableNames.ENEMY_CURRENT_WEAPON).Attack(player, player.transform.position, gameObject)))),

                        //Handling For During The Smoke Window.
                        new BTSequence(
                            new BTRepeatWhile(() => sharedBlackBoard.blackBoard.GetVariable<bool>(VariableNames.SMOKE_BOMB),
                                new BTSequence(
                                    new BTAlwaysSuccesTask(() => sharedBlackBoard.blackBoard.SetVariable(VariableNames.IS_ATTACKING, false)),
                                    new BTAlwaysSuccesTask(() => stateUiText.text = "State : Smoked."),
                                    new BTWaitFor(waitTime),
                                    new BTAlwaysSuccesTask(() => sharedBlackBoard.blackBoard.SetVariable(VariableNames.SMOKE_BOMB, false)),
                                    new BTAlwaysFalse())),
                           new BTAlwaysFalse()
                            ),

                        //Disabling the attack once the distance is greater than the attack range.
                        new BTSequence(
                            new BTAlwaysSuccesTask(() =>
                            {
                                Debug.Log("Attack Reset.");
                                sharedBlackBoard.blackBoard.SetVariable(VariableNames.SMOKE_BOMB, false);
                                blackBoard.SetVariable(VariableNames.IS_ATTACKING, false);
                                sharedBlackBoard.blackBoard.SetVariable(VariableNames.IS_ATTACKING, false);
                                sharedBlackBoard.blackBoard.SetVariable<Transform>(VariableNames.CURRENT_ATTACKING_ENEMY, null);
                                treeAttackPlayer?.OnReset();
                            })
                        ))));

        patrolTree?.SetupBlackboard(blackBoard);
        treeAttackPlayer?.SetupBlackboard(blackBoard);
        treeWeaponAcquisition?.SetupBlackboard(blackBoard);
        treePlayerChase?.SetupBlackboard(blackBoard);

        trees.Add(treeAttackPlayer);
        trees.Add(patrolTree);
        trees.Add(treeWeaponAcquisition);
        trees.Add(treePlayerChase);
    }

    private void GameOver ()
    {
        gameOver = true;
        blackBoard.SetVariable(VariableNames.CHASING_PLAYER, false);

        foreach ( var t in trees )
        {
            t?.OnReset();
        }
    }

    private void FixedUpdate ()
    {
        foreach ( BTBaseNode tree in trees )
        {
            tree?.Tick();
        }
    }
}