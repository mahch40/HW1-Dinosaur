using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Tooltip("Copies of this layer, placed side by side with no gap")]
    [SerializeField] private Transform[] tiles;

    [Tooltip("1 = same speed as the ground, smaller = farther away and slower")]
    [SerializeField] private float speedMultiplier = 1f;

    [Tooltip("Leave 0 to measure the first tile's SpriteRenderer. Set it when a tile holds several sprites.")]
    [SerializeField] private float tileWidthOverride = 0f;

    private float[] startX;
    private float tileWidth;
    private float offset;

    private void Awake()
    {
        tileWidth = tileWidthOverride > 0f
            ? tileWidthOverride
            : tiles[0].GetComponent<SpriteRenderer>().bounds.size.x;

        startX = new float[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
            startX[i] = tiles[i].position.x;
    }

    private void Update()
    {
        offset = (offset + GameSpeed.Current * speedMultiplier * Time.deltaTime) % tileWidth;

        for (int i = 0; i < tiles.Length; i++)
        {
            Vector3 p = tiles[i].position;
            p.x = startX[i] - offset;
            tiles[i].position = p;
        }
    }
}