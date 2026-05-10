using UnityEngine;
using GameCore.Events;

namespace Catan
{
    [CreateAssetMenu(menuName = "Catan/Events/Resource Channel")]
    public class CatanResourceChannel : GameEventChannel<ResourceProducedEvent> { }
}
