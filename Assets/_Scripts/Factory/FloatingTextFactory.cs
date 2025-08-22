namespace FactorySystem
{
    public class FloatingTextFactory : FactorySystem<FloatingTextParticle, string>
    {
        protected override string LabelId => "FloatingText";

        protected override string TranslateStringKeyToID(string primaryKey)
        {
            return primaryKey;
        }

        public FloatingTextParticle GetText(string id)
        {
            tempObject = GetObject(id);
            return tempObject;
        }
    }
}