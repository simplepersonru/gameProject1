using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player is spawned after dying.
    /// Regenerates the level and resets the player and camera to the start.
    /// </summary>
    public class PlayerSpawn : Simulation.Event<PlayerSpawn>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            var player = model.player;
            player.collider2d.enabled = true;
            player.controlEnabled = false;

            if (player.audioSource && player.respawnAudio)
                player.audioSource.PlayOneShot(player.respawnAudio);

            // Regenerate the level (resets spawn point, platforms and camera floor)
            if (model.levelGenerator != null)
            {
                model.levelGenerator.RegenerateLevel();
            }
            else
            {
                // Fallback: just teleport to the existing spawn point
                player.health.Increment();
                player.Teleport(model.spawnPoint.transform.position);
                player.jumpState = PlayerController.JumpState.Grounded;
                player.animator.SetBool("dead", false);

                if (model.virtualCamera != null)
                {
                    model.virtualCamera.Follow = player.transform;
                    model.virtualCamera.LookAt = player.transform;
                }
                if (model.cameraController != null)
                    model.cameraController.Enable();

                Simulation.Schedule<EnablePlayerInput>(2f);
            }
        }
    }
}