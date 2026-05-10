using GameCore.Turn;

namespace Catan
{
    public class CatanTurnManager : TurnManager
    {
        public CatanTurnPhase CurrentCatanPhase { get; private set; }
        public DiceManager DiceManager { get; } = new();
        public RobberSystem RobberSystem { get; } = new();

        protected virtual void OnPhaseStart(CatanTurnPhase phase) => throw new System.NotImplementedException();
        public void HandleDiceRoll(int total) => throw new System.NotImplementedException();
    }
}
