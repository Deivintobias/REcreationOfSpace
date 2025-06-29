using UnityEngine;

public class TemporaryIndicator : MonoBehaviour
{
    [Tooltip("Time in seconds before the indicator is destroyed.")]
    public float lifeTime = 1.0f;

    void Start()
    {
        // Schedule the GameObject to be destroyed after 'lifeTime' seconds.
        Destroy(gameObject, lifeTime);
    }
}
