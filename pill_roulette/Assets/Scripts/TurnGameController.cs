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
    [SerializeField] private float startY  = 0.703f;       // only used to compute drop offset
    [SerializeField] private float revealY = 0.3242173f;   // only used to compute drop offset
    // NEW: how far to drop relative to each cup's local start Y
    private float DropOffset => startY - revealY;          // ≈ 0.3787827f

    [Header("Timings")]
    [SerializeField] private float dropDuration = 0.35f;
    [SerializeField] private float holdReveal  = 0.6f;
    [SerializeField] private float raiseDuration = 0.35f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0,0,1,1);

    [Header("Gameplay")]
    [SerializeField] private int playerHearts = 3;

    private Cup poisonedCup;
    private bool playerTurn;
    private bool canClick;
    private bool gameOver;

    // NEW: remember each cup’s starting LOCAL Y (don’t force world Y)
    private Dictionary<Cup, float> startLocalY = new Dictionary<Cup, float>();

    private void Start()
    {
        // Auto-fill cups if list is empty
        if (cups == null || cups.Count == 0)
            cups = new List<Cup>(FindObjectsOfType<Cup>());

        if (cups == null || cups.Count == 0)
        {
            Debug.LogError("TurnGameController: No cups found. Assign them in the Inspector.");
            enabled = false; return;
        }

        // Record each cup's current LOCAL Y and clear tint
        startLocalY.Clear();
        foreach (var c in cups)
        {
            if (c == null || c.SushiUnderCup == null)
            {
                Debug.LogError($"Cup '{c?.name}' missing or SushiUnderCup not set.");
                enabled = false; return;
            }
            c.SetPoisonVisual(false);
            startLocalY[c] = c.transform.localPosition.y;
        }

        // Pick and mark poisoned cup (logic only)
        poisonedCup = cups[Random.Range(0, cups.Count)];
        poisonedCup.SushiUnderCup.Poison();
        poisonedCup.SetPoisonVisual(true); // show red only during intro

        StartCoroutine(InitialReveal());
    }

    private IEnumerator InitialReveal()
    {
        float yStart  = startLocalY[poisonedCup];
        float yReveal = yStart - DropOffset;

        // lower only the poisoned cup
        yield return poisonedCup.StartCoroutine(
            poisonedCup.MoveLocalY(yStart, yReveal, dropDuration, moveCurve)
        );

        yield return new WaitForSeconds(holdReveal);

        // raise it back up
        yield return poisonedCup.StartCoroutine(
            poisonedCup.MoveLocalY(yReveal, yStart, raiseDuration, moveCurve)
        );

        // hide the red after the intro (if you want it only at start)
        poisonedCup.SetPoisonVisual(false);

        playerTurn = true;
        canClick   = true;
    }

    private void Update()
    {
        if (gameOver || !playerTurn || !canClick) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TryPickAtCenter();

        // Old input system alternative:
        // if (Input.GetMouseButtonDown(0)) TryPickAtCenter();
    }

    private void TryPickAtCenter()
    {
        Vector3 center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
        Ray ray = playerCamera.ScreenPointToRay(center);

        if (Physics.Raycast(ray, out var hit, 100f, cupLayer))
        {
            Cup cup = hit.collider.GetComponentInParent<Cup>();
            if (cup != null && cup.gameObject.activeSelf)
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

        yield return cup.StartCoroutine(
            cup.MoveLocalY(yStart, yReveal, dropDuration, moveCurve)
        );

        var sushi = cup.SushiUnderCup;
        bool poisoned = sushi != null && sushi.IsPoisoned;

        sushi?.Eat();
        cup.gameObject.SetActive(false);

        if (poisoned)
        {
            playerHearts--;
            if (playerHearts <= 0)
            {
                gameOver = true;
                Debug.Log("<color=red>Player ate poison. Game Over.</color>");
                yield break;
            }
        }

        var remaining = GetRemainingCups();
        if (remaining.Count == 0)
        {
            gameOver = true;
            Debug.Log(poisoned
                ? $"<color=yellow>All sushi eaten. You ate the poisoned one but survived. Hearts left: {playerHearts}</color>"
                : "<color=green>All sushi eaten safely! You win!</color>");
            yield break;
        }

        playerTurn = false;
        yield return new WaitForSeconds(0.6f);
        yield return NPCTurn(remaining);
    }

    private IEnumerator NPCTurn(List<Cup> remaining)
    {
        Cup choice = remaining[Random.Range(0, remaining.Count)];

        float yStart  = startLocalY[choice];
        float yReveal = yStart - DropOffset;

        yield return choice.StartCoroutine(
            choice.MoveLocalY(yStart, yReveal, dropDuration, moveCurve)
        );

        bool poisoned = choice.SushiUnderCup.IsPoisoned;

        choice.SushiUnderCup.Eat();
        choice.gameObject.SetActive(false);

        if (poisoned)
        {
            gameOver = true;
            Debug.Log("<color=cyan>NPC ate poison. You WIN!</color>");
            yield break;
        }

        remaining = GetRemainingCups();
        if (remaining.Count == 0)
        {
            gameOver = true;
            Debug.Log("<color=green>All sushi eaten safely! You win!</color>");
            yield break;
        }

        playerTurn = true;
        canClick   = true;
    }

    private List<Cup> GetRemainingCups()
    {
        var rem = new List<Cup>();
        foreach (var c in cups)
            if (c != null && c.gameObject.activeSelf)
                rem.Add(c);
        return rem;
    }
}
