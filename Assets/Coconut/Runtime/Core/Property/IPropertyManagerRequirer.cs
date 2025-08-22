namespace Aloha.Coconut
{
    // PropertyManager보다 먼저 생성되어야 하는데 PropertyManager를 사용할 경우, 나중에 생성된 PropertyManager를 연결해주기 위해 필요
    public interface IPropertyManagerRequirer
    {
        PropertyManager PropertyManager { set; }
    }
}