namespace FactorySystem
{
    public class AttackObjectFactory : FactorySystem<AttackObject, string>
    {
        protected override string LabelId => "AttackObject";

        protected override string TranslateStringKeyToID(string primaryKey)
        {
            return primaryKey;
        }

        public AttackObject GetAttackObject(string attackObjectId)
        {
            tempObject = GetObject(attackObjectId);
            return tempObject;
        }
    }
}