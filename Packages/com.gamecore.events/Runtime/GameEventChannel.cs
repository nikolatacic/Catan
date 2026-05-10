using UnityEngine;
using UnityEngine.Events;

namespace GameCore.Events
{
    public abstract class GameEventChannel<T> : ScriptableObject
    {
        private event UnityAction<T> _onRaised;

        public void Raise(T value) => _onRaised?.Invoke(value);
        public void Register(UnityAction<T> listener) => _onRaised += listener;
        public void Unregister(UnityAction<T> listener) => _onRaised -= listener;
    }
}
