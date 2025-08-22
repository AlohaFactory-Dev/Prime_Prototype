using System.Collections.Generic;
using UnityEngine;

namespace Aloha.Coconut
{
    public class PlayerAction
    {
        public static PlayerAction UNTRACKED;
        public static PlayerAction DEBUG;
        public static PlayerAction ERROR;
        public static PlayerAction TEST;
        
        private static Dictionary<int, Dictionary<int, PlayerAction>> _eventActions = new();
        private static Dictionary<int, string> _eventGroups = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Load()
        {
            _eventActions.Clear();

            _eventActions[0] = new Dictionary<int, PlayerAction>();
            UNTRACKED = new(0, "DEV", 0, "UNTRACKED");
            _eventActions[0][0] = UNTRACKED;
            
            DEBUG = new(0, "DEV", 1, "DEBUG");
            _eventActions[0][1] = DEBUG;
            
            ERROR = new(0, "DEV", 2, "ERROR");
            _eventActions[0][2] = ERROR;
            
            TEST = new(0, "DEV", 3, "TEST");
            _eventActions[0][3] = TEST;

            var eventActionObjects = TableManager.Get<PlayerAction>("player_actions");
            foreach (var eventActionObject in eventActionObjects)
            {
                Add(eventActionObject);
            }
            
            Debug.Log($"PlayerAction Loaded: {_eventActions.Count}");
        }

        public static PlayerAction Get(int actionId)
        {
            // 특별히 지정하지 않는 경우, groupId는 actionId에서 백의 자리까지를 0으로 바꾼 값임
            // ex) actionId 11011 -> groupId 11000
            return Get((actionId / 1000) * 1000, actionId);
        }
        
        public static PlayerAction Get(PlayerActionName actionName)
        {
            return Get((int)actionName);
        }

        public static PlayerAction Get(int groupId, int actionId)
        {
            if (!_eventActions.ContainsKey(groupId))
            {
                Debug.LogError($"Unknown Group: {groupId}");
                return new PlayerAction(groupId, "UNKNOWN", actionId, "UNKNOWN");
            }
            
            if (!_eventActions[groupId].ContainsKey(actionId))
            {
                Debug.LogError($"Unknown Action: {actionId}");
                _eventActions[groupId][actionId] = new PlayerAction(groupId, _eventGroups[groupId], actionId, "UNKNOWN");
            }

            return _eventActions[groupId][actionId];
        }

        public static void Add(int groupId, string groupName, int actionId, string actionName)
        {
            Add(new PlayerAction(groupId, groupName, actionId, actionName));
        }
        
        private static void Add(PlayerAction playerAction)
        {
            if (!_eventGroups.ContainsKey(playerAction.groupId))
            {
                _eventGroups[playerAction.groupId] = playerAction.groupName;   
            }
            else if (_eventGroups[playerAction.groupId] != playerAction.groupName)
            {
                Debug.LogError($"Group Name Mismatch: {playerAction.groupId} {playerAction.groupName} {_eventGroups[playerAction.groupId]}");
            }
            
            if (!_eventActions.ContainsKey(playerAction.groupId))
            {
                _eventActions[playerAction.groupId] = new Dictionary<int, PlayerAction>();
            }

            if (_eventActions[playerAction.groupId].ContainsKey(playerAction.actionId))
            {
                Debug.LogError($"Duplicate Action ID: {playerAction.groupId} {playerAction.actionId}");
                return;
            }
            
            _eventActions[playerAction.groupId][playerAction.actionId] = playerAction;
        }
        
        public static List<PlayerAction> GetAll()
        {
            var result = new List<PlayerAction>();
            foreach (var group in _eventActions.Values)
            {
                result.AddRange(group.Values);
            }

            return result;
        }
        
        public static implicit operator PlayerAction(PlayerActionName actionName)
        {
            return Get(actionName);
        }

        [CSVColumn] public readonly int groupId;
        [CSVColumn] public readonly string groupName;
        [CSVColumn] public readonly int actionId;
        [CSVColumn] public readonly string actionName;
        
        public PlayerAction() { }
        
        public PlayerAction(int groupId, string groupName, int actionId, string actionName)
        {
            this.groupId = groupId;
            this.groupName = groupName;
            this.actionId = actionId;
            this.actionName = actionName;
        }
    }
    
    /// <exclude />
    public class EVPlayerActionOccured : Event
    {
        public readonly PlayerAction playerAction;
        public readonly Dictionary<string, object> parameters;

        public EVPlayerActionOccured(PlayerAction playerAction, Dictionary<string, object> parameters)
        {
            this.playerAction = playerAction;
            this.parameters = parameters;
        }
    }
}
