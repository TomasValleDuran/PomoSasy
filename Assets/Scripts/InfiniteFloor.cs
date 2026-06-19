using UnityEngine;

/// <summary>
/// Fakes an infinite floor: a tiled SpriteRenderer that always covers the
/// camera view and snaps its position to the tile grid. Because every tile is
/// identical, the whole-tile snapping is invisible and the floor looks endless.
/// Requires the SpriteRenderer's Draw Mode = Tiled and the texture's Wrap Mode = Repeat.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class InfiniteFloor : MonoBehaviour
{
    [Tooltip("Extra world units added around the view so the edge is never seen.")]
    [SerializeField] private float padding = 4f;

    [Tooltip("Camera to follow. Leave empty to use Camera.main.")]
    [SerializeField] private Camera targetCamera;

    [Tooltip("Override the tile size (world units). 0 = auto-derive from the sprite.")]
    [SerializeField] private float tileSizeOverride = 0f;

    private SpriteRenderer _sr;
    private float _tileSize;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (targetCamera == null) targetCamera = Camera.main;

        _tileSize = tileSizeOverride > 0f
            ? tileSizeOverride
            : _sr.sprite.rect.width / _sr.sprite.pixelsPerUnit;
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        // Cover the viewport (+padding), rounded up to whole tiles.
        float h = targetCamera.orthographicSize * 2f + padding * 2f;
        float w = h * targetCamera.aspect + padding * 2f;
        w = Mathf.Ceil(w / _tileSize) * _tileSize;
        h = Mathf.Ceil(h / _tileSize) * _tileSize;
        _sr.size = new Vector2(w, h);

        // Follow the camera but snap to the tile grid so the seams never show.
        Vector3 cam = targetCamera.transform.position;
        float x = Mathf.Round(cam.x / _tileSize) * _tileSize;
        float y = Mathf.Round(cam.y / _tileSize) * _tileSize;
        transform.position = new Vector3(x, y, transform.position.z);
    }
}
