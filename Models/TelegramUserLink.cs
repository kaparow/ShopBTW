namespace ShopBTW.Models;

public class TelegramUserLink
{
    public int Id { get; set; }                 // identity PK
    public long TelegramUserId { get; set; }    // обычное поле
    public int CustomerId { get; set; }
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
}
