using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// If you use the old input system, remove the next line and use Input.GetMouseButtonDown(0)
using UnityEngine.InputSystem;

public class TurnGameController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask cupLayer; // layer of cup colliders
    [SerializeField] private List<Cup> cups;     // assign 3 cups in inspector

    [Header("Heights (reference values)")]
    [SerializeField] private float startY  = 0.703f;       // reference to compute the drop amount
    public float STARTY => startY;
    [SerializeField] private float revealY = 0.3242173f;   // reference to compute the drop amount
    public float REVEALY => revealY;
    private float DropOffset => startY - revealY;          // ≈ 0.3787827f
    public float DROPOFFSET => DropOffset;

    [Header("Timings")]
    [SerializeField] private float dropDuration = 0.35f;
    public float DROPDURAION => dropDuration;
    [SerializeField] private float holdReveal  = 0.6f;
    public float HOLDREVEAL => holdReveal;
    [SerializeField] private float raiseDuration = 0.35f;
    public float RAISEDURATION => raiseDuration;
    [SerializeField] private float shuffleDuration = 1.5f;
    [SerializeField] private int shuffleSwaps = 9; // number of swaps during shuffle
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0,0,1,1);
    public AnimationCurve MOVECURVE => moveCurve;

    [Header("Gameplay")]
    [SerializeField] private int playerHearts = 3;
    [SerializeField] private EnemyController _npc;

    //adding players hearts and npc hearts to the UI
    [Header("Gameplay")]
    [SerializeField] private HeartUI heartUI;   


    private Cup poisonedCup;
    private bool playerTurn;
    private bool canClick;
    private bool gameOver;
    public bool GameOver{get{return gameOver;} set{gameOver = value;}}

    // remember each cup’s starting LOCAL Y (don’t force world Y)
    private Dictionary<Cup, float> startLocalY = new Dictionary<Cup, float>();
    public Dictionary<Cup, float> STARTLOCALY => startLocalY;

    private void Awake()
    {
        // Fallback if you forgot to drag the camera
        if (playerCamera == null) playerCamera = Camera.main;
    }

    private void Start()
    {
        if (cups == null || cups.Count == 0)
            cups = new List<Cup>(FindObjectsOfType<Cup>());

        if (cups == null || cups.Count == 0)
        {
            Debug.LogError("TurnGameController: No cups found. Assign them in the Inspector.");
            enabled = false; return;
        }

        if (heartUI != null)
        {
            heartUI.SetHearts(playerHearts);
        }
            

        // Cache start local Y and ensure visuals cleared
        startLocalY.Clear();
        foreach (var c in cups)
        {
            if (c == null || c.SushiUnderCup == null)
            {
                Debug.LogError($"Cup '{c?.name}' missing or SushiUnderCup not set.");
                enabled = false; return;
            }
            startLocalY[c] = c.transform.localPosition.y;
            c.SetPoisonVisual(false);
            c.SushiUnderCup.ClearPoison();
            c.SushiUnderCup.gameObject.SetActive(true); // ensure visible for the round
            c.gameObject.SetActive(true);               // cups stay active across rounds
        }

        // Launch first round
        StartCoroutine(SetupNewRound());
    }

    public IEnumerator SetupNewRound()
    {
        if (gameOver) yield break;

        // Reset all sushi for the new round
        foreach (var c in cups)
        {
            c.SushiUnderCup.ClearPoison();
            c.SushiUnderCup.gameObject.SetActive(true);
            c.SetPoisonVisual(false);

            // snap cups to their start local Y (don’t move sushi)
            var lp = c.transform.localPosition;
            c.transform.localPosition = new Vector3(lp.x, startLocalY[c], lp.z);
        }

        // Choose one poisoned
        poisonedCup = cups[Random.Range(0, cups.Count)];
        poisonedCup.SushiUnderCup.Poison();

        // Show red only at the start reveal
        poisonedCup.SetPoisonVisual(true);

        // Initial reveal (only the poisoned cup moves)
        float yStart  = startLocalY[poisonedCup];
        float yReveal = yStart - DropOffset;

        yield return poisonedCup.StartCoroutine(poisonedCup.MoveLocalY(yStart, yReveal, dropDuration, moveCurve));
        yield return new WaitForSeconds(holdReveal);
        yield return poisonedCup.StartCoroutine(poisonedCup.MoveLocalY(yReveal, yStart, raiseDuration, moveCurve));

        // Hide the red for the actual gameplay before shuffling
        poisonedCup.SetPoisonVisual(false);

        // Shuffle the cups so player doesn't know which is poisoned
        yield return ShuffleCups();

        // Player starts each round
        playerTurn = true;
        canClick   = true;

        Debug.Log($"[ROUND START] PlayerHearts={playerHearts}  NPCHearts={_npc.npcHearts}");
    }

    private void Update()
    {
        if (gameOver || !playerTurn || !canClick) return;

        // New Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TryPickAtCenter();

        // Old Input alternative:
        // if (Input.GetMouseButtonDown(0)) TryPickAtCenter();
    }

    private void TryPickAtCenter()
    {
        Vector3 center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
        Ray ray = playerCamera.ScreenPointToRay(center);

        if (Physics.Raycast(ray, out var hit, 100f, cupLayer))
        {
            Cup cup = hit.collider.GetComponentInParent<Cup>();
            if (cup != null && cup.gameObject.activeSelf && cup.SushiUnderCup.gameObject.activeSelf)
            {
                canClick = false;
                StartCoroutine(PlayerRevealsAndEats(cup));
            }
        }
    }

    private IEnumerator PlayerRevealsAndEats(Cup cup)
    {
        float yStart  = startLocalY[cup];
        float yReveal = yStart - DropOffset;

        // Reveal player’s pick
        yield return cup.StartCoroutine(cup.MoveLocalY(yStart, yReveal, dropDuration, moveCurve));

        bool poisoned = cup.SushiUnderCup.IsPoisoned;
        cup.SushiUnderCup.Eat();    // remove sushi
        // Keep the cup active; only sushi disappears

        if (poisoned)
        {
            playerHearts--;

            //update the hearts
            if (heartUI != null)
            {
                heartUI.SetHearts(playerHearts);
            }

            Debug.Log($"<color=orange>Player ate POISON. Hearts left: {playerHearts}</color>");

            if (playerHearts <= 0)
            {
                gameOver = true;
                Debug.Log("<color=red>Player died. Game Over.</color>");
                yield break;
            }

            // Round ends because poison was eaten → start next round
            yield return new WaitForSeconds(0.6f);
            yield return SetupNewRound();
            yield break;
        }

        // If no sushi left (all safe), start next round
        if (AllSushiGone())
        {
            Debug.Log("<color=green>All sushi were safe this round.</color>");
            yield return new WaitForSeconds(0.6f);
            yield return SetupNewRound();
            yield break;
        }

        // NPC turn
        playerTurn = false;
        yield return new WaitForSeconds(0.6f);
        yield return _npc.TakeTurn(cups);

        // If no sushi left, next round
        if (AllSushiGone())
        {
            Debug.Log("<color=green>All sushi were safe this round.</color>");
            yield return new WaitForSeconds(0.6f);
            yield return SetupNewRound();
            yield break;
        }

        // Back to player
        playerTurn = true;
        canClick   = true;
    }

    private bool AllSushiGone()
    {
        foreach (var c in cups)
            if (c.SushiUnderCup.gameObject.activeSelf)
                return false;
        return true;
    }

    private IEnumerator ShuffleCups()
{
    if (cups == null || cups.Count < 2) yield break;

    Debug.Log($"<color=magenta>===== SHUFFLE START: {cups.Count} cups, {shuffleSwaps} swaps =====</color>");

    // Store all cup WORLD positions (cups may have same local but different world positions)
    List<Vector3> cupWorldPositions = new List<Vector3>();
    foreach (var cup in cups)
    {
        var worldPos = cup.transform.position;
        cupWorldPositions.Add(worldPos);
        Debug.Log($"Cup [{cup.name}] local position: {cup.transform.localPosition}, world position: {worldPos}");
    }

    // Check if all cups are at different world positions (using XZ plane distance, ignoring Y)
    bool hasDifferentPositions = false;
    for (int i = 0; i < cupWorldPositions.Count && !hasDifferentPositions; i++)
    {
        for (int j = i + 1; j < cupWorldPositions.Count; j++)
        {
            Vector2 pos1 = new Vector2(cupWorldPositions[i].x, cupWorldPositions[i].z);
            Vector2 pos2 = new Vector2(cupWorldPositions[j].x, cupWorldPositions[j].z);
            if (Vector2.Distance(pos1, pos2) > 0.01f)
            {
                hasDifferentPositions = true;
                break;
            }
        }
    }
    
    if (!hasDifferentPositions)
    {
        Debug.LogWarning("<color=yellow>Cups appear to be at the same XZ world position. Shuffle may not be visible.</color>");
        // Continue anyway - the shuffle logic will still work
    }

    // Create a mapping: cup index -> target position index
    int[] cupToTargetPosition = new int[cups.Count];
    for (int i = 0; i < cups.Count; i++)
    {
        cupToTargetPosition[i] = i;
    }

    // Perform multiple animated swaps
    float swapDuration = shuffleDuration / shuffleSwaps;
    Debug.Log($"Swap duration: {swapDuration}s, Total shuffle: {shuffleDuration}s");
    
    for (int swap = 0; swap < shuffleSwaps; swap++)
    {
        // Pick two random cup indices
        int cupIndex1 = Random.Range(0, cups.Count);
        int cupIndex2 = Random.Range(0, cups.Count);
        while (cupIndex2 == cupIndex1 && cups.Count > 1)
        {
            cupIndex2 = Random.Range(0, cups.Count);
        }

        // Swap target positions
        int tempTarget = cupToTargetPosition[cupIndex1];
        cupToTargetPosition[cupIndex1] = cupToTargetPosition[cupIndex2];
        cupToTargetPosition[cupIndex2] = tempTarget;

        Debug.Log($"<color=yellow>--- Swap {swap + 1}/{shuffleSwaps}: Swapping cups {cupIndex1} ↔ {cupIndex2} ---</color>");
        
        // Start all movements using world positions
        for (int i = 0; i < cups.Count; i++)
        {
            int targetPosIndex = cupToTargetPosition[i];
            Vector3 targetWorldPos = cupWorldPositions[targetPosIndex];
            
            Debug.Log($"  Cup {i} ({cups[i].name}) → world position {targetPosIndex}: {targetWorldPos} (current: {cups[i].transform.position})");
            StartCoroutine(cups[i].MoveToWorldPosition(targetWorldPos, swapDuration, moveCurve));
        }

        // Wait for movements to complete
        yield return new WaitForSeconds(swapDuration + 0.05f);
    }
    
    Debug.Log("<color=magenta>===== SHUFFLE COMPLETE =====</color>");
}

}
