using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBTW.DTOs;
using ShopBTW.Services;
using System.Security.Claims;

namespace ShopBTW.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly CartService _cartService;

    public CartController(CartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        int customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _cartService.GetCartAsync(customerId);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem(AddToCartDto dto)
    {
        int customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _cartService.AddItemAsync(customerId, dto.ProductId, dto.Quantity);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResultDto>> Checkout()
    {
        int customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _cartService.CheckoutAsync(customerId);
    }
}
