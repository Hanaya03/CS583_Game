using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private TurnGameController _gameManager;
    private int _health = 3;
    public int npcHearts => _health;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator TakeTurn(List<Cup> cups)
    {
        // Collect remaining sushi (still active)
        List<Cup> remaining = new List<Cup>();
        foreach (var c in cups)
            if (c.SushiUnderCup.gameObject.activeSelf)
                remaining.Add(c);

        if (remaining.Count == 0)
        {
            Debug.Log("<color=green>All sushi were safe this round (before NPC turn).</color>");
            yield return _gameManager.SetupNewRound();
            yield break;
        }

        // NPC random pick among remaining
        Cup choice = remaining[Random.Range(0, remaining.Count)];

        float yStart  = _gameManager.STARTLOCALY[choice];
        float yReveal = yStart - _gameManager.DROPOFFSET;

        yield return choice.StartCoroutine(choice.MoveLocalY(yStart, yReveal, _gameManager.RAISEDURATION, _gameManager.MOVECURVE));

        bool poisoned = choice.SushiUnderCup.IsPoisoned;
        choice.SushiUnderCup.Eat();

        if (poisoned)
        {
            _health--;
            Debug.Log($"<color=cyan>NPC ate POISON. NPC hearts left: {_health}</color>");

            if (_health <= 0)
            {
                _gameManager.GameOver = true;
                Debug.Log("<color=cyan>NPC died. YOU WIN!</color>");
                yield break;
            }

            // Round ends because poison was eaten → start next round
            yield return new WaitForSeconds(0.6f);
            yield return _gameManager.SetupNewRound();
            yield break;
        }
    }
}
