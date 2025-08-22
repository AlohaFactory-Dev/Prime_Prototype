namespace Aloha.Coconut.IdleRewards
{
    public class IdleReward
    {
        public PropertyType type;
        public float amount;
        
        public IdleReward(PropertyType type, float amount)
        {
            this.type = type;
            this.amount = amount;
        }
    }
}