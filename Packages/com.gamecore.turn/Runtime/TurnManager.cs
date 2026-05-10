using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Turn
{
    public class TurnManager : MonoBehaviour
    {
        public List<ITurnActor> Actors { get; } = new();
        public ITurnActor CurrentActor { get; protected set; }
        public TurnPhase CurrentPhase { get; protected set; }
        public int TurnNumber { get; protected set; }

        public virtual void NextTurn() => throw new System.NotImplementedException();
        public virtual void AdvancePhase() => throw new System.NotImplementedException();
        public virtual void SkipActor(ITurnActor actor, string reason) => throw new System.NotImplementedException();
    }
}
