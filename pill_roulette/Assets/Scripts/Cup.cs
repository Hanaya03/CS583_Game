using UnityEngine;
using System.Collections;

public class Cup : MonoBehaviour
{
    [SerializeField] private Sushi sushiUnderCup;
    [SerializeField] private Renderer[] renderersToTint; // assign the cup renderers here

    public Sushi SushiUnderCup => sushiUnderCup;

    public void SetPoisonVisual(bool on)
    {
        if (renderersToTint == null) return;
        foreach (var r in renderersToTint)
            if (r) r.material.color = on ? Color.red : Color.white;
    }

    // NEW: animate in LOCAL space so parent height doesn't mess us up
    public IEnumerator MoveLocalY(float fromY, float toY, float duration, AnimationCurve curve)
    {
        Vector3 from = transform.localPosition; from.y = fromY;
        Vector3 to   = transform.localPosition; to.y   = toY;

        transform.localPosition = from;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = curve != null ? curve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            transform.localPosition = Vector3.Lerp(from, to, k);
            yield return null;
        }
    }
}
