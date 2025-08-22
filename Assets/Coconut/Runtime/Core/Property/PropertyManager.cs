using System;
using System.Collections.Generic;
using System.Numerics;
using UniRx;
using UnityEngine.Assertions;
using Zenject;

namespace Aloha.Coconut
{
    public class PropertyManager
    {
        public IObservable<PropertyUpdatedArgs> OnPropertyUpdated => _onPropertyUpdated;
        private Subject<PropertyUpdatedArgs> _onPropertyUpdated = new Subject<PropertyUpdatedArgs>();
        
        private readonly Dictionary<PropertyTypeGroup, IPropertyHandler> _propertyHandlers = new();
        private readonly Dictionary<PropertyTypeGroup, IPropertyExchanger> _propertyExchangers = new();
        
        private bool _isPropertyHandlersInitialized = false;
        private readonly LazyInject<List<IPropertyHandler>> _propertyHandlersLazy;
        
        private bool _isPropertyExchangersInitialized = false;
        private readonly LazyInject<List<IPropertyExchanger>> _propertyExchangersLazy;

        
        // 기능 특성상 IPropertyHandler/IPropertyExchanger가 constructor에서 PropertyManager에 의존하는 경우가 많음
        // 이 때 PropertyManager가 List<IPropertyHandler>, List<IPropertyExchanger>를 constructor에서 받아오면 Circular Dependency 발생
        // 따라서 LazyInject 처리
        public PropertyManager(List<IPropertyManagerRequirer> propertyManagerRequirers,
            LazyInject<List<IPropertyHandler>> propertyHandlersLazy, LazyInject<List<IPropertyExchanger>> propertyExchangersLazy)
        {
            foreach (var propertyManagerRequirer in propertyManagerRequirers)
            {
                propertyManagerRequirer.PropertyManager = this;
            }
            
            _propertyHandlersLazy = propertyHandlersLazy;
            _propertyExchangersLazy = propertyExchangersLazy;
        }

        public List<Property> Obtain(Property property, PlayerAction playerAction)
        {
            return ObtainInternal(new List<Property> { property }, playerAction);
        }

        public List<Property> Obtain(List<Property> properties, PlayerAction playerAction)
        {
            // 원본 List를 변경하지 않기 위해 복사본을 생성해서 ObtainInternal 처리
            return ObtainInternal(new List<Property>(properties), playerAction);
        }

        private List<Property> ObtainInternal(List<Property> properties, PlayerAction playerAction)
        {
            var result = new List<Property>();
            
            // PropertyExchanger의 동작이 Property 보유 상태에 따라 달라지는 경우를 고려해 획득 처리를 하나씩 진행
            foreach (var property in properties)
            {
                var exchangedProperties = new List<Property>() { property };

                // 현재 property에 대한 교환 처리
                for (var i = 0; i < exchangedProperties.Count; i++)
                {
                    if (!_isPropertyExchangersInitialized)
                    {
                        _isPropertyExchangersInitialized = true;
                        foreach (var exchanger in _propertyExchangersLazy.Value)
                        {
                            AddPropertyExchanger(exchanger);
                        }
                    }
                    
                    if (_propertyExchangers.ContainsKey(exchangedProperties[i].type.group))
                    {
                        var exchangeResult = _propertyExchangers[exchangedProperties[i].type.group].Exchange(exchangedProperties[i]);
                        exchangedProperties.RemoveAt(i);
                        exchangedProperties.InsertRange(i, exchangeResult);
                    }   
                }
                
                // 교환된 property들 실제 획득
                foreach (var exchangedProperty in exchangedProperties)
                {
                    var propertyHandler = GetPropertyHandler(exchangedProperty.type);
                    var previousBalance = propertyHandler.GetBalance(exchangedProperty.type);
                    propertyHandler.Obtain(exchangedProperty);
                    EventBus.Broadcast(new EVPlayerActionOccured(playerAction, new Dictionary<string, object>
                    {
                        { "type", "property_obtained" },
                        { "property", exchangedProperty },
                        { "previous_balance", previousBalance },
                        { "current_balance", propertyHandler.GetBalance(exchangedProperty.type) },
                    }));
                
                    _onPropertyUpdated.OnNext(new PropertyUpdatedArgs(exchangedProperty.type, GetBalance(exchangedProperty.type), exchangedProperty.amount));
                }
                
                result.AddRange(exchangedProperties);
            }

            return result;
        }

        private IPropertyHandler GetPropertyHandler(PropertyType propertyType)
        {
            if (!_isPropertyHandlersInitialized)
            {
                _isPropertyHandlersInitialized = true;
                foreach (var handler in _propertyHandlersLazy.Value)
                {
                    AddPropertyHandler(handler);
                }
            }
            
            Assert.IsTrue(_propertyHandlers.ContainsKey(propertyType.group),
                $"PropertyType group: {propertyType.group}에 대한 PropertyHandler가 없습니다.");
            return _propertyHandlers[propertyType.group];
        }

        public void Use(Property property, PlayerAction playerAction)
        {
            var propertyHandler = GetPropertyHandler(property.type);
            var previousBalance = propertyHandler.GetBalance(property.type);
            propertyHandler.Use(property);
            EventBus.Broadcast(new EVPlayerActionOccured(playerAction, new Dictionary<string, object>
            {
                { "type", "property_used" },
                { "property", property },
                { "previous_balance", previousBalance },
                { "current_balance", propertyHandler.GetBalance(property.type) },
            }));
            _onPropertyUpdated.OnNext(new PropertyUpdatedArgs(property.type, GetBalance(property.type), -property.amount));
        }

        public bool TryUse(Property property, PlayerAction playerAction)
        {
            if (HaveEnough(property))
            {
                Use(property, playerAction);
                return true;
            }
            
            return false;
        }

        public bool TryUse(List<Property> properties, PlayerAction playerAction)
        {
            if (!HaveEnough(properties)) return false;

            foreach (var property in properties)
            {
                Use(property, playerAction);
            }

            return true;
        }

        public void Set(Property property, PlayerAction playerAction)
        {
            var propertyHandler = GetPropertyHandler(property.type);
            var previousBalance = propertyHandler.GetBalance(property.type);
            propertyHandler.Set(property);

            EventBus.Broadcast(new EVPlayerActionOccured(playerAction, new Dictionary<string, object>
            {
                { "type", "property_set" },
                { "property", property },
                { "previous_balance", previousBalance },
                { "current_balance", propertyHandler.GetBalance(property.type) },
            }));
            _onPropertyUpdated.OnNext(new PropertyUpdatedArgs(property.type, GetBalance(property.type), property.amount));
        }

        public BigInteger GetBalance(PropertyType propertyType)
        {
            return GetPropertyHandler(propertyType).GetBalance(propertyType);
        }

        public bool HaveEnough(Property property)
        {
            return GetBalance(property.type) >= property.amount;
        }
        
        public bool HaveEnough(List<Property> properties)
        {
            foreach (var property in properties)
            {
                if (!HaveEnough(property)) return false;
            }

            return true;
        }
        
        public void AddPropertyHandler(IPropertyHandler propertyHandler)
        {
            foreach (var group in propertyHandler.HandlingGroups)
            {
                Assert.IsFalse(_propertyHandlers.ContainsKey(group),
                    $"PropertyType group: {group}에 대한 PropertyHandler가 중복되었습니다.");
                _propertyHandlers[group] = propertyHandler;
            }
        }
        
        public void AddPropertyExchanger(IPropertyExchanger propertyExchanger)
        {
            foreach (var group in propertyExchanger.HandlingGroups)
            {
                Assert.IsFalse(_propertyExchangers.ContainsKey(group),
                    $"PropertyType group: {group}에 대한 PropertyExchanger가 중복되었습니다.");
                _propertyExchangers[group] = propertyExchanger;
            }
        }
    }
}