using UnityEngine;
using System.Collections;

public class CameraCinematic : MonoBehaviour
{
    public Transform enemyViewAnchor;
    public Transform playerViewAnchor;

    [Header("Cinematic Settings")]
    public float panDuration = 3f;
    public float holdDuration = 1f;

    [Tooltip("Add rotation here (e.g., Y = 10) to look further right at the end of the pan.")]
    public Vector3 endRotationOffset = new Vector3(0, 0, 0);

    public IEnumerator PlayPan()
    {
        float elapsed = 0f;
        Vector3 startPos = enemyViewAnchor.position;
        Quaternion startRot = enemyViewAnchor.rotation;

        Vector3 endPos = playerViewAnchor.position;
        
        Quaternion endRot = playerViewAnchor.rotation * Quaternion.Euler(endRotationOffset);

        // Cinematic Pan
        while (elapsed < panDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / panDuration);
            transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / panDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        
        transform.position = endPos;
        transform.rotation = endRot;

        // Pause at the end
        yield return new WaitForSeconds(holdDuration);
    }
}
