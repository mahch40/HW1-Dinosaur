using UnityEngine;

// Base class for everything that scrolls toward the player and reacts when touched.
[RequireComponent(typeof(Collider2D))]
public abstract class Interactable : MonoBehaviour
{
    [SerializeField] private float destroyX = -12f;   // past this x it is no longer needed

    protected virtual void Update()
    {
        transform.position += Vector3.left * (GameSpeed.Current * Time.deltaTime);

        if (transform.position.x < destroyX)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        OnPlayerContact(player);
    }

    // Each subclass defines its own reaction.
    protected abstract void OnPlayerContact(PlayerController player);
}