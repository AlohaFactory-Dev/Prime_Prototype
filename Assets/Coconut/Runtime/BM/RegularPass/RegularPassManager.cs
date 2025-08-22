using System;
using System.Collections.Generic;
using System.Numerics;
using UniRx;
using Zenject;

namespace Aloha.Coconut
{
    public class RegularPassManager : IDisposable, IPropertyHandler
    {
        public IReadOnlyList<RegularPass> Passes => _regularPasses;

        private readonly List<RegularPass> _regularPasses = new();
        private readonly SaveData _saveData;

        private CompositeDisposable _compositeDisposable = new();

        public RegularPassManager(SaveDataManager saveDataManager, RegularPass.Factory regularPassFactory,
            IRegularPassDatabase regularPassDatabase, IRegularPassProgressProvider progressProvider)
        {
            _saveData = saveDataManager.Get<SaveData>("regular_pass_manager");

            List<RegularPassData> regularPassDatas = regularPassDatabase.GetEveryRegularPassData();
            foreach (RegularPassData passData in regularPassDatas)
            {
                if (!_saveData.passSaveDatas.ContainsKey(passData.id))
                {
                    _saveData.passSaveDatas[passData.id] = new Pass.SaveData();
                }

                var pass = regularPassFactory.Create(passData, _saveData.passSaveDatas[passData.id]);
                progressProvider.GetReactiveProgress(pass.Type)
                    .Subscribe(value => { pass.SetPassProgress(value); }).AddTo(_compositeDisposable);

                _regularPasses.Add(pass);
            }

            HandlingGroups = new List<PropertyTypeGroup> { regularPassDatabase.GetRegularPassTypeGroup() };
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        private class SaveData
        {
            public Dictionary<int, Pass.SaveData> passSaveDatas = new();
        }

        #region IPropertyHandler

        public List<PropertyTypeGroup> HandlingGroups { get; }

        void IPropertyHandler.Obtain(Property property)
        {
            foreach (RegularPass pass in _regularPasses)
            {
                if (pass.Id == property.type.id)
                {
                    pass.Pass.ActivateAdvanced();
                    return;
                }
            }
        }

        void IPropertyHandler.Use(Property property) { }

        void IPropertyHandler.Set(Property property) { }

        BigInteger IPropertyHandler.GetBalance(PropertyType property)
        {
            return 0;
        }

        #endregion
    }
}