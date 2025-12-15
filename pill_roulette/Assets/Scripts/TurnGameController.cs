using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// If you use the old input system, remove the next line and use Input.GetMouseButtonDown(0)
using UnityEngine.InputSystem;

public class TurnGameController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private CanvasGroup fadePanel;
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
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0,0,1,1);
    public AnimationCurve MOVECURVE => moveCurve;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Gameplay")]
    [SerializeField] private int playerHearts = 3;
    [SerializeField] private EnemyController _npc;

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
        if (gameOver){
            yield return new WaitForSeconds(2f);
            yield return Fade(0f, 1f, 1f);           
            if(playerHearts <= 0)
                SceneManager.LoadScene("WinScene", LoadSceneMode.Single);
            SceneManager.LoadScene("LoseScene", LoadSceneMode.Single);
        }

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

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;

        Color c = fadeImage.color;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float a = Mathf.Lerp(from, to, fadeCurve.Evaluate(t));

            c.a = a;
            fadeImage.color = c;

            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }
}
