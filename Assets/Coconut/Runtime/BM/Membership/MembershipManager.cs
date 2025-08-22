using System;
using System.Collections.Generic;
using System.Numerics;
using UniRx;
using Zenject;

namespace Aloha.Coconut
{
    public class MembershipManager : IPropertyHandler, IDisposable
    {
        public List<Membership> MembershipList { get; } = new();
        public IObservable<Unit> OnDailyReset => _onDailyReset;
        public string RedDotPath => _membershipDatabase.GetRedDotPath();

        private readonly Subject<Unit> _onDailyReset = new();

        private readonly SaveData _saveData;
        private readonly IMembershipDatabase _membershipDatabase;
        private readonly PeriodicResetHandler _periodicResetHandler;

        public MembershipManager(SaveDataManager saveDataManager, PeriodicResetHandler periodicResetHandler,
            [InjectOptional] IMembershipDatabase membershipDatabase)
        {
            _saveData = saveDataManager.Get<SaveData>("membership_manager");
            _membershipDatabase = membershipDatabase;
            _membershipDatabase ??= new DefaultMembershipDatabase();

            _periodicResetHandler = periodicResetHandler;
            HandlingGroups = new List<PropertyTypeGroup> { _membershipDatabase.GetMembershipTypeGroup() };
        }

        [Inject]
        internal void Inject(Membership.Factory membershipFactory)
        {
            var membershipDataList = _membershipDatabase.GetMembershipDataList();
            foreach (var membershipData in membershipDataList)
            {
                var saveData = _saveData.membershipSaveDatas.TryGetValue(membershipData.id, out var data)
                    ? data
                    : new Membership.SaveData();
                _saveData.membershipSaveDatas.TryAdd(membershipData.id, saveData);

                var membership = membershipFactory.Create(membershipData, saveData);
                membership.LinkRedDot($"{_membershipDatabase.GetRedDotPath()}/{membership.Id}/DailyRewards");
                MembershipList.Add(membership);
            }

            _periodicResetHandler.AddResetCallback(ResetPeriod.Daily, "membership_manager", DailyReset);
        }

        private void DailyReset()
        {
            foreach (var membership in MembershipList)
            {
                membership.DailyReset();
            }
        }

        public void Dispose()
        {
            _periodicResetHandler.RemoveResetCallback(ResetPeriod.Daily, "membership_manager");
        }

        private class SaveData
        {
            public Dictionary<int, Membership.SaveData> membershipSaveDatas = new();
        }

        #region IPropertyHandler

        public List<PropertyTypeGroup> HandlingGroups { get; }

        public void Obtain(Property property)
        {
            foreach (var membership in MembershipList)
            {
                if (membership.Id != property.type.id) continue;

                membership.AddSpan((int)property.amount);
                break;
            }
        }

        public void Use(Property property)
        {
            throw new NotImplementedException();
        }

        public void Set(Property property)
        {
            throw new NotImplementedException();
        }

        public BigInteger GetBalance(PropertyType property)
        {
            return 0;
        }

        #endregion
    }
}