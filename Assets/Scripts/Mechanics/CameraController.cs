using UnityEngine;
using Platformer.Core;
using Platformer.Gameplay;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Controls the main camera to follow the player upward only.
    /// The camera never moves down once it has risen to a height.
    /// Also detects when the player falls below the visible area and triggers death.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        /// <summary>
        /// How smoothly the camera follows the player.
        /// </summary>
        public float smoothSpeed = 3f;

        /// <summary>
        /// Vertical offset above the player position.
        /// </summary>
        public float yOffset = 2f;

        /// <summary>
        /// Extra margin below the camera bottom edge before player death is triggered.
        /// </summary>
        public float deathMarginBelow = 0.5f;

        private Transform target;
        private float minCameraY;
        private Camera cam;
        private bool isActive;

        void Awake()
        {
            cam = GetComponent<Camera>();
            minCameraY = transform.position.y;
            isActive = false;
        }

        /// <summary>
        /// Sets the camera target and resets the minimum Y floor.
        /// </summary>
        public void SetTarget(Transform newTarget, float spawnY)
        {
            target = newTarget;
            minCameraY = spawnY + yOffset;
            transform.position = new Vector3(transform.position.x, minCameraY, transform.position.z);
            isActive = true;
        }

        /// <summary>
        /// Disables camera follow (e.g. on player death).
        /// </summary>
        public void Disable()
        {
            isActive = false;
        }

        /// <summary>
        /// Re-enables camera follow.
        /// </summary>
        public void Enable()
        {
            isActive = true;
        }

        void LateUpdate()
        {
            if (target == null || !isActive) return;

            // Move camera up when player rises, never down
            float desiredY = target.position.y + yOffset;
            if (desiredY > minCameraY)
                minCameraY = desiredY;

            Vector3 targetPos = new Vector3(transform.position.x, minCameraY, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

            // Kill player if they fall below the visible area
            float cameraBottom = transform.position.y - cam.orthographicSize - deathMarginBelow;
            if (target.position.y < cameraBottom)
            {
                Simulation.Schedule<PlayerDeath>(0);
            }
        }
    }
}
