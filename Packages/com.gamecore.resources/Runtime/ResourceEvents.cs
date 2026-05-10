using GameCore.Events;
using GameCore.Player;

namespace GameCore.Resources
{
    public struct ResourceAddedEvent : IGameEvent { public IPlayer Player; public ResourceBundle Added; }
    public struct ResourceRemovedEvent : IGameEvent { public IPlayer Player; public ResourceBundle Removed; }
    public struct ResourceInsufficientEvent : IGameEvent { public IPlayer Player; public ResourceBundle Needed; }
    public struct ResourceTransferredEvent : IGameEvent { public IPlayer From; public IPlayer To; public ResourceBundle Bundle; }
}
