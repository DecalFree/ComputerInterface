using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ComputerInterface
{
    internal class PropUpdateBinder
    {
        private readonly Dictionary<string, Action> _actions = new();

        public void Bind(string name, Action callback)
        {
            _actions.Add(name, callback);
        }

        public void Clear()
        {
            _actions.Clear();
        }

        public void PropertyChanged(object src, PropertyChangedEventArgs args)
        {
            if (_actions.TryGetValue(args.PropertyName, out Action action))
            {
                action.Invoke();
            }
        }
    }
}