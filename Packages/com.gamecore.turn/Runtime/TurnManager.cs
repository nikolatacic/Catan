using System.Collections.Generic;
using GameCore.Events;
using UnityEngine;

namespace GameCore.Turn
{
    public class TurnManager : MonoBehaviour
    {
        public List<ITurnActor> Actors { get; } = new();
        public ITurnActor CurrentActor { get; protected set; }
        public TurnPhase CurrentPhase { get; protected set; }
        public int TurnNumber { get; protected set; }

        public virtual void NextTurn()
        {
            if (CurrentActor != null)
                EventBus.Publish(new TurnEndedEvent { Actor = CurrentActor });

            if (Actors.Count == 0) return;

            int currentIndex = Actors.IndexOf(CurrentActor);
            int nextIndex = (currentIndex + 1) % Actors.Count;
            CurrentActor = Actors[nextIndex];
            TurnNumber++;

            EventBus.Publish(new TurnStartedEvent { Actor = CurrentActor, TurnNumber = TurnNumber });
        }

        public virtual void AdvancePhase()
        {
            var allPhases = System.Enum.GetValues(typeof(TurnPhase));
            var previousPhase = CurrentPhase;
            CurrentPhase = (TurnPhase)(((int)CurrentPhase + 1) % allPhases.Length);
            EventBus.Publish(new PhaseChangedEvent { From = previousPhase, To = CurrentPhase });
        }

        public virtual void SkipActor(ITurnActor actor, string reason)
        {
            EventBus.Publish(new ActorSkippedEvent { Actor = actor, Reason = reason });
        }
    }
}
