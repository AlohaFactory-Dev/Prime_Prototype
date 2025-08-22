using System.Collections.Generic;
using System.Linq;

namespace Aloha.Coconut
{
    public class RegularPass
    {
        public int Id => _data.id;
        public string Type => _data.type;
        public int From => _data.from;
        public int To => _data.to;
        public bool IsActive => Pass.CurrentLevel >= 0;
        public bool IsCompleted => Pass.CurrentLevel >= Pass.MaxLevel; 
        public string RedDotPath => Pass.RedDotPath;
    
        public IAPProduct AdvancedProduct { get; }
        public Pass Pass { get; }

        private readonly RegularPassData _data;

        private RegularPass(RegularPassData data, Pass pass, IAPProduct advancedProduct)
        {
            _data = data;
            Pass = pass;
            AdvancedProduct = advancedProduct;
        }

        public void SetPassProgress(int progressValue)
        {
            Pass.SetLevel(progressValue - From + 1);
        }

        public List<Property> ClaimAll(PlayerAction freePlayerAction, PlayerAction premiumPlayerAction)
        {
            var result = Pass.ClaimFreeRewards(freePlayerAction); 
            if (Pass.IsAdvancedActivated) result.AddRange(Pass.ClaimAdvancedRewards(premiumPlayerAction));
            return result;
        }

        public class Factory
        {
            private readonly Pass.Factory _passFactory;
            private readonly IIAPManager _iapManager;
            private readonly IRegularPassDatabase _regularPassDatabase;

            public Factory(Pass.Factory passFactory, IIAPManager iapManager, IRegularPassDatabase regularPassDatabase)
            {
                _passFactory = passFactory;
                _iapManager = iapManager;
                _regularPassDatabase = regularPassDatabase;
            }
        
            public RegularPass Create(RegularPassData data, Pass.SaveData passSaveData)
            {
                var nodes = data.nodeDatas.Select(nodeData => new PassNode(nodeData)).ToList();
                Pass pass = _passFactory.Create(1, nodes, passSaveData);
                pass.LinkRedDot($"{_regularPassDatabase.GetRedDotPath()}/{data.id}");
                
                return new RegularPass(data, pass, _iapManager.GetProduct(data.iapId));
            }
        }
    }
}