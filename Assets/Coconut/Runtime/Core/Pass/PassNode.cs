using System.Numerics;

namespace Aloha.Coconut
{
    public class PassNode
    {
        public enum State
        {
            Locked, Claimable, Claimed
        }

        public int PassLevel { get; }
        public Property FreeReward { get; }
        public Property AdvancedReward { get; }
        public Property PremiumReward { get; }
    
        public State FreeRewardState { get; internal set; }
        public State AdvancedRewardState { get; internal set; }
        public State PremiumRewardState { get; internal set; }
        public bool IsAdvancedActivated { get; internal set; }
        public bool IsPremiumActivated { get; internal set; }
        
        public string FreeRewardRedDotPath { get; private set; }
        public string AdvancedRewardRedDotPath { get; private set; }
        public string PremiumRewardRedDotPath { get; private set; }

        public PassNode(PassNodeData passNodeData)
        {
            PassLevel = passNodeData.passLevel;
            FreeRewardState = State.Locked;
            AdvancedRewardState = State.Locked;
            PremiumRewardState = State.Locked;
            
            FreeReward = new Property(passNodeData.reward1Alias, passNodeData.reward1Amount);
            if (passNodeData.reward2Amount > 0)
            {
                AdvancedReward = new Property(passNodeData.reward2Alias, passNodeData.reward2Amount, true);   
            }
            if (passNodeData.reward3Amount > 0)
            {
                PremiumReward = new Property(passNodeData.reward3Alias, passNodeData.reward3Amount, true);
            }
        }
        
        internal void LinkRedDot(string redDotPath)
        {
            FreeRewardRedDotPath = $"{redDotPath}/Free";
            AdvancedRewardRedDotPath = $"{redDotPath}/Advanced";
            PremiumRewardRedDotPath = $"{redDotPath}/Premium";
        }

        internal void UpdateRedDot()
        {
            RedDot.SetNotified(FreeRewardRedDotPath, FreeRewardState == State.Claimable);
            RedDot.SetNotified(AdvancedRewardRedDotPath, IsAdvancedActivated && AdvancedRewardState == State.Claimable);
            RedDot.SetNotified(PremiumRewardRedDotPath, IsPremiumActivated && PremiumRewardState == State.Claimable);
        }
    }

    public struct PassNodeData
    {
        [CSVColumn] public int passLevel;
        [CSVColumn] public string reward1Alias;
        [CSVColumn] public BigInteger reward1Amount;
        [CSVColumn] public string reward2Alias;
        [CSVColumn] public BigInteger reward2Amount;
        [CSVColumn] public string reward3Alias;
        [CSVColumn] public BigInteger reward3Amount;
    }
}