using UnityEngine;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Procedurally generates the game level at runtime.
    /// Creates left/right boundary walls, a starting floor, and randomly
    /// placed platforms that extend upward. The number of platforms can be
    /// configured in the Inspector.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Level Dimensions")]
        /// <summary>Half-width of the playable area (total width = 2 * levelHalfWidth).</summary>
        public float levelHalfWidth = 4.5f;
        /// <summary>Thickness of the boundary walls.</summary>
        public float wallThickness = 1f;
        /// <summary>Y position of the starting floor.</summary>
        public float floorY = -1f;

        [Header("Platforms")]
        /// <summary>Total number of platforms generated above the floor.</summary>
        public int platformCount = 20;
        /// <summary>Minimum horizontal width of a platform.</summary>
        public float platformMinWidth = 1.5f;
        /// <summary>Maximum horizontal width of a platform.</summary>
        public float platformMaxWidth = 3.5f;
        /// <summary>Thickness (height) of each platform.</summary>
        public float platformThickness = 0.3f;
        /// <summary>Base vertical distance between consecutive platforms.</summary>
        public float verticalSpacing = 2.5f;
        /// <summary>Random variation applied to vertical spacing.</summary>
        public float verticalVariance = 0.5f;

        [Header("Player")]
        /// <summary>Reference to the Player prefab (assigned in Inspector).</summary>
        public GameObject playerPrefab;

        // ── internals ─────────────────────────────────────────────────────
        private GameObject levelRoot;
        private Sprite whiteSprite;
        private PlatformerModel model;

        // ── Unity lifecycle ───────────────────────────────────────────────

        void Awake()
        {
            whiteSprite = CreateWhiteSprite();
        }

        void Start()
        {
            model = Simulation.GetModel<PlatformerModel>();
            GenerateLevel();
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// Destroys the current level and generates a fresh one.
        /// Resets the camera floor and repositions the player at the spawn point.
        /// </summary>
        public void RegenerateLevel()
        {
            if (levelRoot != null)
                Destroy(levelRoot);

            GenerateLevel();
        }

        // ── Private helpers ───────────────────────────────────────────────

        void GenerateLevel()
        {
            levelRoot = new GameObject("Level");

            // Starting floor
            CreatePlatform("Floor",
                new Vector2(0f, floorY),
                new Vector2(levelHalfWidth * 2f, platformThickness),
                new Color(0.3f, 0.5f, 0.3f));

            // Boundary walls (tall enough to cover all platforms)
            float wallHeight = (platformCount + 2) * verticalSpacing + 20f;
            float wallCenterY = wallHeight * 0.5f + floorY;
            CreatePlatform("LeftWall",
                new Vector2(-levelHalfWidth - wallThickness * 0.5f, wallCenterY),
                new Vector2(wallThickness, wallHeight),
                new Color(0.5f, 0.5f, 0.5f));
            CreatePlatform("RightWall",
                new Vector2(levelHalfWidth + wallThickness * 0.5f, wallCenterY),
                new Vector2(wallThickness, wallHeight),
                new Color(0.5f, 0.5f, 0.5f));

            // Randomly positioned platforms
            float y = floorY + verticalSpacing;
            for (int i = 0; i < platformCount; i++)
            {
                float width = Random.Range(platformMinWidth, platformMaxWidth);
                float halfW = levelHalfWidth - width * 0.5f;
                float x = Random.Range(-halfW, halfW);
                float yOffset = Random.Range(-verticalVariance, verticalVariance);

                CreatePlatform($"Platform_{i}",
                    new Vector2(x, y + yOffset),
                    new Vector2(width, platformThickness),
                    new Color(0.4f, 0.6f, 0.4f));

                y += verticalSpacing;
            }

            SetupPlayer();
        }

        void SetupPlayer()
        {
            // Create a spawn point just above the floor
            var spawnObj = new GameObject("SpawnPoint");
            float spawnY = floorY + platformThickness * 0.5f + 0.5f;
            spawnObj.transform.position = new Vector3(0f, spawnY, 0f);
            spawnObj.transform.parent = levelRoot.transform;
            model.spawnPoint = spawnObj.transform;

            // Spawn or teleport the player
            if (model.player == null)
            {
                if (playerPrefab != null)
                {
                    var playerGO = Instantiate(playerPrefab, spawnObj.transform.position, Quaternion.identity);
                    model.player = playerGO.GetComponent<PlayerController>();
                }
            }
            else
            {
                model.player.Teleport(spawnObj.transform.position);
                model.player.health.Increment();
                model.player.animator.SetBool("dead", false);
                model.player.jumpState = PlayerController.JumpState.Grounded;
            }

            // Aim the camera at the new spawn Y
            if (model.cameraController != null && model.player != null)
            {
                model.cameraController.SetTarget(model.player.transform, spawnY);
                model.player.controlEnabled = true;
            }
        }

        void CreatePlatform(string name, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.position = new Vector3(position.x, position.y, 0f);
            go.transform.parent = levelRoot.transform;
            go.layer = 0; // Default layer – collides with all layers

            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = whiteSprite;
            sr.color = color;
            sr.size = size;
            sr.drawMode = SpriteDrawMode.Tiled;
        }

        static Sprite CreateWhiteSprite()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
