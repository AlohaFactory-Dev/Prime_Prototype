using Aloha.Coconut;

public class DiscountablePrice : PropertyPrice
{
    public Property OriginalPrice { get; private set; }
    
    public DiscountablePrice(Property property, PropertyManager propertyManager) : base(property, propertyManager)
    {
        OriginalPrice = property;
    }
    
    public void ApplyDiscount(decimal discount)
    {
        Property = new Property(OriginalPrice.type, (int)((int)OriginalPrice.amount * (1 - discount)));
    }

    public class Factory
    {
        private readonly PropertyManager _propertyManager;

        public Factory(PropertyManager propertyManager)
        {
            _propertyManager = propertyManager;
        }

        public DiscountablePrice Create(Property property)
        {
            return new DiscountablePrice(property, _propertyManager);
        }
    }
}