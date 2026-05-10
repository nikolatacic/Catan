using UnityEngine;
using GameCore.Events;
using GameCore.Player;

namespace GameCore.Build
{
    public class BuildManager : MonoBehaviour
    {
        public IBuildRule Rule { get; set; }

        // Validates placement via the injected rule and publishes the appropriate events.
        // Resource deduction is the caller's responsibility after a successful return.
        public bool TryPlace(IPlaceable piece, IBuildLocation location, IPlayer player)
        {
            EventBus.Publish(new BuildAttemptedEvent { Player = player, Piece = piece, Location = location });

            if (Rule == null || !Rule.CanPlace(piece, location, player))
            {
                var reason = Rule?.GetFailureReason(piece, location, player) ?? "No build rule set.";
                EventBus.Publish(new BuildFailedEvent { Player = player, Reason = reason });
                return false;
            }

            EventBus.Publish(new BuildSucceededEvent { Player = player, Piece = piece, Location = location });
            return true;
        }

        public bool TryRemove(IPlaceable piece, IBuildLocation location)
        {
            if (Rule == null || !Rule.CanRemove(piece, location, null))
            {
                EventBus.Publish(new BuildFailedEvent { Player = null, Reason = "Removal not allowed." });
                return false;
            }

            EventBus.Publish(new PieceRemovedEvent { Piece = piece, Location = location });
            return true;
        }
    }
}
