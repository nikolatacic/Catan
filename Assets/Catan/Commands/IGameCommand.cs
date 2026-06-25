namespace Catan.Commands
{
    // ── Command pattern: player intents that may cross the network ────────────
    // Hotseat: dispatcher executes locally on this device.
    // Multiplayer (Phase 5): dispatcher serializes the command and sends it
    //   from the client to the host via ServerRpc. Host executes; result fans
    //   out to all clients via existing EventBus subscribers.
    //
    // Pure UI-mode toggles (BeginPlaceSettlement, CancelPlacement, etc.) are
    // NOT commands — they stay local to the active player's device.
    // ──────────────────────────────────────────────────────────────────────────

    public interface IGameCommand
    {
        void Execute(UI.GameManager manager);
    }
}
