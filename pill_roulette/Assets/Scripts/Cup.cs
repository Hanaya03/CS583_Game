using UnityEngine;
using System.Collections;

public class Cup : MonoBehaviour
{
    [SerializeField] private Sushi sushiUnderCup;
    [SerializeField] private GameObject plate; // Plate_01_Free GameObject
    [SerializeField] private Renderer[] renderersToTint;
    
    public Sushi SushiUnderCup => sushiUnderCup;
    
    public void SetPoisonVisual(bool on)
    {
        if (renderersToTint == null) return;
        foreach (var r in renderersToTint)
            if (r) r.material.color = on ? Color.red : Color.white;
    }
    
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
    
    public IEnumerator MoveToLocalPosition(Vector3 targetLocalPos, float duration, AnimationCurve curve)
    {
        Vector3 from = transform.localPosition;
        float currentY = from.y;
        targetLocalPos.y = currentY;
        
        // DEBUG: Log the movement
        Debug.Log($"<color=cyan>[{name}] SHUFFLE START: from {from} to {targetLocalPos}, duration={duration}</color>");
        
        // Check if we're actually moving anywhere
        float distance = Vector3.Distance(from, targetLocalPos);
        if (distance < 0.01f)
        {
            Debug.Log($"<color=yellow>[{name}] Already at target position! Distance: {distance}</color>");
            yield break;
        }
        
        float t = 0f;
        int frameCount = 0;
        
        while (t < 1f)
        {
            float deltaTime = Time.deltaTime;
            t += deltaTime / duration;
            float k = curve != null ? curve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            Vector3 currentPos = Vector3.Lerp(from, targetLocalPos, k);
            currentPos.y = currentY;
            transform.localPosition = currentPos;
            
            frameCount++;
            
            // Log every 10 frames
            if (frameCount % 10 == 0)
            {
                Debug.Log($"[{name}] Frame {frameCount}: t={t:F3}, deltaTime={deltaTime:F4}, pos={currentPos}");
            }
            
            yield return null;
        }
        
        // Final position
        targetLocalPos.y = currentY;
        transform.localPosition = targetLocalPos;
        
        Debug.Log($"<color=green>[{name}] SHUFFLE COMPLETE: Final pos={transform.localPosition}, Frames={frameCount}</color>");
    }
    
    // Move to a target world position while preserving Y in local space
    // Also moves the plate and sushi along with the cup
    public IEnumerator MoveToWorldPosition(Vector3 targetWorldPos, float duration, AnimationCurve curve)
    {
        Vector3 fromWorld = transform.position;
        float currentLocalY = transform.localPosition.y; // Preserve local Y
        
        // Convert target world position to local position relative to parent
        Vector3 targetLocalPos;
        if (transform.parent != null)
        {
            targetLocalPos = transform.parent.InverseTransformPoint(targetWorldPos);
        }
        else
        {
            targetLocalPos = targetWorldPos;
        }
        targetLocalPos.y = currentLocalY; // Keep the same local Y
        
        Vector3 fromLocal = transform.localPosition;
        
        // Stores the initial world positions of plate and sushi
        bool plateNeedsMoving = plate != null && plate.transform.parent != transform;
        bool sushiNeedsMoving = sushiUnderCup != null && sushiUnderCup.transform.parent != transform;
        
        Vector3 plateWorldOffset = Vector3.zero;
        Vector3 sushiWorldOffset = Vector3.zero;
        if (plateNeedsMoving)
        {
            plateWorldOffset = plate.transform.position - transform.position;
        }
        if (sushiNeedsMoving)
        {
            sushiWorldOffset = sushiUnderCup.transform.position - transform.position;
        }
        
        // Check if we're actually moving anywhere
        float distance = Vector3.Distance(fromLocal, targetLocalPos);
        if (distance < 0.01f)
        {
            yield break;
        }
        
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = curve != null ? curve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            Vector3 currentLocalPos = Vector3.Lerp(fromLocal, targetLocalPos, k);
            currentLocalPos.y = currentLocalY; // Always preserve Y
            transform.localPosition = currentLocalPos;
            
            // Move plate and sushi to maintain their relative world position to the cup
            // (only if they're not children - children move automatically)
            if (plateNeedsMoving && plate != null)
            {
                Vector3 newCupWorldPos = transform.position;
                plate.transform.position = newCupWorldPos + plateWorldOffset;
            }
            if (sushiNeedsMoving && sushiUnderCup != null)
            {
                Vector3 newCupWorldPos = transform.position;
                sushiUnderCup.transform.position = newCupWorldPos + sushiWorldOffset;
            }
            
            yield return null;
        }
        
        // Final position
        targetLocalPos.y = currentLocalY;
        transform.localPosition = targetLocalPos;
        
        // Ensure plate and sushi are at final positions (only if not children)
        if (plateNeedsMoving && plate != null)
        {
            Vector3 finalCupWorldPos = transform.position;
            plate.transform.position = finalCupWorldPos + plateWorldOffset;
        }
        if (sushiNeedsMoving && sushiUnderCup != null)
        {
            Vector3 finalCupWorldPos = transform.position;
            sushiUnderCup.transform.position = finalCupWorldPos + sushiWorldOffset;
        }
    }
}