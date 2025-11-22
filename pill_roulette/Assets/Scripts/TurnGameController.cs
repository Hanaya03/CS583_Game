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
    [SerializeField] private float revealY = 0.3242173f;   // reference to compute the drop amount
    private float DropOffset => startY - revealY;          // ≈ 0.3787827f

    [Header("Timings")]
    [SerializeField] private float dropDuration = 0.35f;
    [SerializeField] private float holdReveal  = 0.6f;
    [SerializeField] private float raiseDuration = 0.35f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0,0,1,1);

    [Header("Gameplay")]
    [SerializeField] private int playerHearts = 3;
    [SerializeField] private int npcHearts    = 3;

    private Cup poisonedCup;
    private bool playerTurn;
    private bool canClick;
    private bool gameOver;

    // remember each cup’s starting LOCAL Y (don’t force world Y)
    private Dictionary<Cup, float> startLocalY = new Dictionary<Cup, float>();

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

    private IEnumerator SetupNewRound()
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

        // Hide the red for the actual gameplay
        poisonedCup.SetPoisonVisual(false);

        // Player starts each round
        playerTurn = true;
        canClick   = true;

        Debug.Log($"[ROUND START] PlayerHearts={playerHearts}  NPCHearts={npcHearts}");
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
        yield return NPCTurn();
    }

    private IEnumerator NPCTurn()
    {
        // Collect remaining sushi (still active)
        List<Cup> remaining = new List<Cup>();
        foreach (var c in cups)
            if (c.SushiUnderCup.gameObject.activeSelf)
                remaining.Add(c);

        if (remaining.Count == 0)
        {
            Debug.Log("<color=green>All sushi were safe this round (before NPC turn).</color>");
            yield return SetupNewRound();
            yield break;
        }

        // NPC random pick among remaining
        Cup choice = remaining[Random.Range(0, remaining.Count)];

        float yStart  = startLocalY[choice];
        float yReveal = yStart - DropOffset;

        yield return choice.StartCoroutine(choice.MoveLocalY(yStart, yReveal, dropDuration, moveCurve));

        bool poisoned = choice.SushiUnderCup.IsPoisoned;
        choice.SushiUnderCup.Eat();

        if (poisoned)
        {
            npcHearts--;
            Debug.Log($"<color=cyan>NPC ate POISON. NPC hearts left: {npcHearts}</color>");

            if (npcHearts <= 0)
            {
                gameOver = true;
                Debug.Log("<color=cyan>NPC died. YOU WIN!</color>");
                yield break;
            }

            // Round ends because poison was eaten → start next round
            yield return new WaitForSeconds(0.6f);
            yield return SetupNewRound();
            yield break;
        }

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
}
