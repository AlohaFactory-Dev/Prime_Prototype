using Aloha.Coconut;

public class EVIAPComplete : Event
{
    public string isoCurrencyCode;
    public double price;
    public string transactionId;
    public string itemId;
    public string itemName;
    public string productId;
    public string productName;

    public EVIAPComplete(string isoCurrencyCode, double price, string itemId, string itemName, string transactionId, 
        string productId, string productName)
    {
        this.isoCurrencyCode = isoCurrencyCode;
        this.price = price;
        this.itemId = itemId;
        this.itemName = itemName;
        this.transactionId = transactionId;
        this.productId = productId;
        this.productName = productName;
    }
}