namespace AnySoftDesktop.Models;

public class CustomPayment
{
    public int? Id { get; set; }
    public int UserId { get; set; }
    public string? Number { get; set; }
    public CustomDate ExpirationDate { get; set; } = new CustomDate();
    public string? Cvc { get; set; }
    public bool IsActive { get; set; }
}