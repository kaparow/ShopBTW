using Telegram.Bot.Types.ReplyMarkups;

namespace ShopBTW.Telegram;

public static class TelegramKeyboards
{
    public static ReplyKeyboardMarkup Main => new(new[]
    {
        new KeyboardButton[] { new("🗄️ Каталог"), new("🛒 Корзина") },
        new KeyboardButton[] { new("➕ Добавить"), new("❌ Удалить") },
        new KeyboardButton[] { new("🏷️ Оформить") },
    })
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = false
    };
}
