using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Aloha.Coconut
{
    internal class RedDotNode
    {
        public IReadOnlyReactiveProperty<bool> Notified => _notified;
        public string Name { get; }

        private ReactiveProperty<bool> _notified = new ReactiveProperty<bool>();
        private List<RedDotNode> _children = new List<RedDotNode>();
        private Subject<Unit> OnChildrenStateChanged = new Subject<Unit>();

        internal RedDotNode(string name)
        {
            Name = name;
            OnChildrenStateChanged.ThrottleFrame(1)
                .Subscribe(_ =>
                {
                    var notified = false;
                    foreach (var child in _children)
                    {
                        notified |= child.Notified.Value;
                    }

                    _notified.Value = notified;
                });
        }

        internal RedDotNode Get(List<string> path)
        {
            if (path.Count == 1)
            {
                return GetChild(path[0]);
            }

            RedDotNode childNode = GetChild(path[0]);
            path.RemoveAt(0);
            return childNode.Get(path);
        }

        private RedDotNode GetChild(string name)
        {
            foreach (RedDotNode badgeNoticeNode in _children)
            {
                if (name.Equals(badgeNoticeNode.Name))
                {
                    return badgeNoticeNode;
                }
            }

            return AddChild(name);
        }

        private RedDotNode AddChild(string name)
        {
            RedDotNode newChild = new RedDotNode(name);

            _children.Add(newChild);
            newChild.Notified
                .Subscribe(n =>
                {
                    if (n) _notified.Value = true;
                    else OnChildrenStateChanged.OnNext(Unit.Default);
                });

            return newChild;
        }

        public void SetNotified(bool notified)
        {
            if (_children.Count == 0)
            {
                _notified.Value = notified;
            }
            else
            {
                Debug.LogError("Only leaf badge notice node can be notified manually.");
            }
        }

        public void TurnOffAll()
        {
            if (_children.Count == 0)
            {
                _notified.Value = false;
            }
            else
            {
                foreach (var child in _children)
                {
                    child.TurnOffAll();
                }
            }
        }
    }
}