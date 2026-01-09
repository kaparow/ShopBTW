using Microsoft.EntityFrameworkCore;
using ShopBTW.Data;
using ShopBTW.Models;
using ShopBTW.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ShopBTW.Telegram;

public class TelegramUpdateHandler
{
    private readonly AppDbContext _db;
    private readonly CartService _cart;

    public TelegramUpdateHandler(AppDbContext db, CartService cart)
    {
        _db = db;
        _cart = cart;
    }

    private static Task SendMenuAsync(ITelegramBotClient bot, long chatId, string text, CancellationToken ct)
    {
        return bot.SendMessage(
            chatId: chatId,
            text: text,
            replyMarkup: TelegramKeyboards.Main,
            cancellationToken: ct
        );
    }

    public async Task Handle(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Message?.Text is null) return;

        var chatId = update.Message.Chat.Id;
        var tgUserId = update.Message.From!.Id;

        // ✅ единственное объявление text
        var text = update.Message.Text.Trim();

        // ✅ Кнопки -> команды
        text = text switch
        {
            "🛒 Корзина" => "/cart",
            "🗄️ Каталог" => "/catalog",
            "➕ Добавить" => "/add",
            "❌ Удалить"  => "/remove",
            "🏷️ Оформить" => "/checkout",
            _ => text
        };

        // /start
        if (text == "/start")
        {
            await SendMenuAsync(bot, chatId,
                "Привет! Выбирай действие кнопками 👇\n\n" +
                "🗄️ Каталог\n🛒 Корзина\n➕ Добавить\n❌ Удалить\n🏷️ Оформить\n\n" +
                "Перед покупками привяжи аккаунт: /link <customerId>",
                ct);
            return;
        }

        // Кнопки “➕ Добавить / ❌ Удалить” без параметров
        if (text == "/add")
        {
            await SendMenuAsync(bot, chatId,
                "Добавление товара:\n/add <productId> <qty>\nПример: /add 3 1",
                ct);
            return;
        }

        if (text == "/remove")
        {
            await SendMenuAsync(bot, chatId,
                "Удаление товара:\n/remove <productId>\nПример: /remove 3",
                ct);
            return;
        }

        // 1) Привязка /link 1
        if (text.StartsWith("/link"))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || !int.TryParse(parts[1], out var customerId))
            {
                await SendMenuAsync(bot, chatId, "Пример: /link 1", ct);
                return;
            }

            var link = await _db.TelegramUserLinks
                .FirstOrDefaultAsync(x => x.TelegramUserId == tgUserId, ct);

            if (link == null)
            {
                _db.TelegramUserLinks.Add(new TelegramUserLink
                {
                    TelegramUserId = tgUserId,
                    CustomerId = customerId
                });
            }
            else
            {
                link.CustomerId = customerId;
            }

            await _db.SaveChangesAsync(ct);

            await SendMenuAsync(bot, chatId, $"Ок ✅ Привязал к CustomerId={customerId}", ct);
            return;
        }
        // /catalog
        if (text == "/catalog")
        {
            var products = await _db.Products
                .OrderBy(p => p.Id)
                .Take(10)
                .ToListAsync(ct);

            if (products.Count == 0)
            {
                await SendMenuAsync(bot, chatId, "Товаров нет", ct);
                return;
            }

            foreach (var p in products)
            {
                await SendMenuAsync(
                    bot,
                    chatId,
                    $"{p.Id}. {p.Name}\nТип: {(RobotPartType)p.PartType}\nЦена: {p.Price}\n" +
                    $"Добавить: /add {p.Id} 1",
                    ct);
            }
            return;
        }
        // 2) Все остальные команды требуют привязку
        var userLink = await _db.TelegramUserLinks
            .FirstOrDefaultAsync(x => x.TelegramUserId == tgUserId, ct);

        if (userLink == null)
        {
            await SendMenuAsync(bot, chatId, "Сначала привяжи аккаунт: /link <customerId>", ct);
            return;
        }

        var customerId2 = userLink.CustomerId;

        

        // /add 5 2
        if (text.StartsWith("/add "))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2 || parts.Length > 3)
            {
                await SendMenuAsync(bot, chatId, "Пример: /add 5 1", ct);
                return;
            }

            if (!int.TryParse(parts[1], out var productId))
            {
                await SendMenuAsync(bot, chatId, "productId должен быть числом. Пример: /add 5 1", ct);
                return;
            }

            var qty = 1;
            if (parts.Length == 3 && !int.TryParse(parts[2], out qty)) qty = 1;
            if (qty <= 0) qty = 1;

            try
            {
                var cartDto = await _cart.AddItemAsync(customerId2, productId, qty);
                await SendMenuAsync(bot, chatId, "Добавлено ✅\n\n" + FormatCart(cartDto), ct);
            }
            catch (Exception ex)
            {
                await SendMenuAsync(bot, chatId, "Ошибка: " + ex.Message, ct);
            }
            return;
        }

        // /cart
        if (text == "/cart")
        {
            var cartDto = await _cart.GetCartAsync(customerId2);
            await SendMenuAsync(bot, chatId, FormatCart(cartDto), ct);
            return;
        }

        // /set 5 3
        if (text.StartsWith("/set"))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3 ||
                !int.TryParse(parts[1], out var productId) ||
                !int.TryParse(parts[2], out var qty))
            {
                await SendMenuAsync(bot, chatId, "Пример: /set 5 3", ct);
                return;
            }

            try
            {
                var cartDto = await _cart.UpdateQuantityAsync(customerId2, productId, qty);
                await SendMenuAsync(bot, chatId, "Готово ✅\n\n" + FormatCart(cartDto), ct);
            }
            catch (Exception ex)
            {
                await SendMenuAsync(bot, chatId, "Ошибка: " + ex.Message, ct);
            }
            return;
        }

        // /remove 5
        if (text.StartsWith("/remove"))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || !int.TryParse(parts[1], out var productId))
            {
                await SendMenuAsync(bot, chatId, "Пример: /remove 5", ct);
                return;
            }

            try
            {
                var cartDto = await _cart.RemoveItemAsync(customerId2, productId);
                await SendMenuAsync(bot, chatId, "Удалил ✅\n\n" + FormatCart(cartDto), ct);
            }
            catch (Exception ex)
            {
                await SendMenuAsync(bot, chatId, "Ошибка: " + ex.Message, ct);
            }
            return;
        }

        // /checkout
        if (text == "/checkout")
        {
            try
            {
                var res = await _cart.CheckoutAsync(customerId2);
                await SendMenuAsync(bot, chatId,
                    $"Заказ оформлен ✅\nOrderId: {res.OrderId}\nДата: {res.CreatedAt}\nСумма: {res.Total}",
                    ct);
            }
            catch (Exception ex)
            {
                await SendMenuAsync(bot, chatId, "Ошибка: " + ex.Message, ct);
            }
            return;
        }

        await SendMenuAsync(bot, chatId, "Не понял. Нажми /start", ct);
    }

    private static string FormatCart(ShopBTW.DTOs.CartDto cart)
    {
        var items = cart.Items.ToList();
        if (items.Count == 0) return "Корзина пустая";

        var lines = items.Select(i => $"{i.ProductId}. {i.ProductName} x{i.Quantity} = {i.LineTotal}");

        return "Корзина:\n" +
               string.Join("\n", lines) +
               $"\n\nИтого: {cart.Total}";
    }
}
