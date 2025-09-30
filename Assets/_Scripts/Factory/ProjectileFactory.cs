namespace FactorySystem
{
    public class ProjectileFactory : FactorySystem<Projectile, string>
    {
        protected override string LabelId => "Projectile";

        protected override string TranslateStringKeyToID(string primaryKey)
        {
            return primaryKey;
        }

        public Projectile GetAttackObject(string attackObjectId)
        {
            tempObject = GetObject(attackObjectId);
            return tempObject;
        }
    }
}