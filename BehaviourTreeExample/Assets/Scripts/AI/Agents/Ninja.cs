using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Ninja : MonoBehaviour
{
    public float KeepDistance = 1.0f;
    public float MoveSpeed = 3;
    [Range(1, 20)]
    public float MaxDistanceToPlayer = 5;

    [SerializeField] private TMP_Text stateUiText;
    [SerializeField] private float delayBetweenSmokeBombs = 2.0f;

    [SerializeField] private GameObject smokeBombPrefab;
    [Range(40, 100)]
    [SerializeField] private float lineOfSightRadiusHide = 40.0f;
    [SerializeField] private LayerMask hideAbleLayer;

    [SerializeField] private BlackBoardObject sharedBlackboard;

    private BTBaseNode tree;
    private NavMeshAgent agent;
    private Animator animator;
    private Blackboard blackBoard = new();
    private bool gameOver = false;

    private void Awake ()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }
    private void OnEnable ()
    {
        EventManager.AddListener(EventType.GameOver, GameOver);
    }

    private void GameOver ()
    {
        gameOver = true;

        stateUiText.text = "State : Player is dead.";

        tree.OnReset();
    }

    private void Start ()
    {
        blackBoard.SetVariable(VariableNames.PLAYER_TRANSFORM, FindObjectOfType<Player>().transform);
        blackBoard.SetVariable(VariableNames.TARGET_POSITION, Vector3.zero);
        sharedBlackboard.blackBoard.SetVariable(VariableNames.IS_ATTACKING, false);

        tree =
            new BTSequence(
                new BTConditionNode(() => !gameOver),
                new BTSelector(
                    //Go to the player Position. Cancel if an attack starts.
                    new BTCancelIfFalse(() => !sharedBlackboard.blackBoard.GetVariable<bool>(VariableNames.IS_ATTACKING),
                        new BTConditionNode(() => Vector3.Distance(transform.position, blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM).position) > (MaxDistanceToPlayer / 2)),
                        new BTGetPosition(VariableNames.PLAYER_TRANSFORM, blackBoard),
                        new BTAlwaysSuccesTask(() => stateUiText.text = "Chasing Player."),
                        new BTMoveToPosition(agent, MoveSpeed, VariableNames.TARGET_POSITION, KeepDistance),
                        new BTConditionNode(() => sharedBlackboard.blackBoard.GetVariable<Transform>(VariableNames.CURRENT_ATTACKING_ENEMY) == null)),

                    //Hide once, then attack the enemy with smoke bombs for as long as it is attacking.
                    new BTSequence(
                        new BTConditionNode(() => sharedBlackboard.blackBoard.GetVariable<bool>(VariableNames.IS_ATTACKING), () => !sharedBlackboard.blackBoard.GetVariable<bool>(VariableNames.SMOKE_BOMB)),
                        new BTAlwaysSuccesTask(() => stateUiText.text = "Hiding."),
                        new BTGetNearbyObjects(transform.position, hideAbleLayer, lineOfSightRadiusHide, 5, VariableNames.ALL_HIDE_OBJECTS_IN_RANGE),
                        new BTFindHidePosition(VariableNames.ALL_HIDE_OBJECTS_IN_RANGE, agent, VariableNames.CURRENT_ATTACKING_ENEMY, sharedBlackboard.blackBoard, transform, MaxDistanceToPlayer),
                        new BTMoveToPosition(agent, MoveSpeed, VariableNames.TARGET_POSITION, KeepDistance),

                        new BTRepeatWhile(() => sharedBlackboard.blackBoard.GetVariable<bool>(VariableNames.IS_ATTACKING),
                            new BTCancelIfFalse(() => sharedBlackboard.blackBoard.GetVariable<bool>(VariableNames.IS_ATTACKING), () => !sharedBlackboard.blackBoard.GetVariable<bool>(VariableNames.SMOKE_BOMB),
                                new BTAlwaysSuccesTask(() => stateUiText.text = "Throwing Smoke."),
                                new BTAlwaysSuccesTask(() => Debug.Log("Throwing Smoke.")),
                                new BTGetPosition(VariableNames.CURRENT_ATTACKING_ENEMY, sharedBlackboard.blackBoard),
                                new BTLaunchObjectTo(transform, smokeBombPrefab, 10.0f, sharedBlackboard.blackBoard, VariableNames.CURRENT_ATTACKING_ENEMY),
                                new BTAlwaysSuccesTask(() => sharedBlackboard.blackBoard.SetVariable(VariableNames.SMOKE_BOMB, true)),
                                new BTWaitFor(delayBetweenSmokeBombs)
                            )),
                        new BTAlwaysFalse()
                        )));

        tree.SetupBlackboard(blackBoard);
    }

    private void FixedUpdate ()
    {
        tree?.Tick();
    }
}