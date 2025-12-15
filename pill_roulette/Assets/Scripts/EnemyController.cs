using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioClip[] _audioClips;
    [SerializeField] private AudioClip _coughClip;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private TurnGameController _gameManager;
    [SerializeField] private HeartUI enemyHeartUI;
    
    private int _health = 3;
    private bool _isIdle = true;
    public int npcHearts => _health;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator.SetBool("Idle", true);
        if (enemyHeartUI != null)
        {
            enemyHeartUI.SetHearts(_health);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_isIdle)
        {
            _audioSource.clip = _audioClips[(int)Random.Range(0, 3)];
            _audioSource.Play();
            _isIdle = false;
        }
    }

    public IEnumerator TakeTurn(List<Cup> cups)
    {
        TransitionToReach();
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
        yield return new WaitForSeconds(.42f);

        TransitionToEat();
        yield return new WaitForSeconds(.5f);

        bool poisoned = choice.SushiUnderCup.IsPoisoned;
        choice.SushiUnderCup.Eat();

        if (poisoned)
        {
            _health--;
            
            if (enemyHeartUI != null)
            {
                enemyHeartUI.SetHearts(_health);
            }

            _audioSource.clip = _coughClip;
            _audioSource.Play();
            Debug.Log($"<color=cyan>NPC ate POISON. NPC hearts left: {_health}</color>");

            if (_health <= 0)
            {
                _gameManager.GameOver = true;
                TransitionToDying();
                yield return new WaitForSeconds(.5f);
                TransitionToDead();
                Debug.Log("<color=cyan>NPC died. YOU WIN!</color>");
                yield break;
            }

            TransitionToIdle();
            // Round ends because poison was eaten → start next round
            yield return new WaitForSeconds(0.6f);
            yield return _gameManager.SetupNewRound();
            _isIdle = true;
            yield break;
        }
        TransitionToIdle();
        yield return new WaitForSeconds(1f);
        _isIdle = true;
    }

    private void TransitionToIdle()
    {
        ResetAllBools();
        _animator.SetBool("Idle", true);
    }

    private void TransitionToReach()
    {
        ResetAllBools();
        _animator.SetBool("Reaching", true);
    }

    private void TransitionToEat()
    {
        ResetAllBools();
        _animator.SetBool("Eating", true);
    }

    private void TransitionToDying()
    {
        ResetAllBools();
        _animator.SetBool("Dying", true);
    }

    private void TransitionToDead()
    {
        ResetAllBools();
        _animator.SetBool("Dead", true);
    }

    private void ResetAllBools()
    {
        _animator.SetBool("Idle", false);
        _animator.SetBool("Reaching", false);
        _animator.SetBool("Eating", false);
        _animator.SetBool("Dying", false);
        _animator.SetBool("Dead", false);
    }
}
