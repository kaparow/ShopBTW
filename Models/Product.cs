using System.ComponentModel.DataAnnotations;

namespace ShopBTW.Models;

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Тип запчасти для робота (гусеницы, двигатель и т.п.)</summary>
    [Required]
    public RobotPartType PartType { get; set; }

    /// <summary>Цена — денежный тип, зададим точность в OnModelCreating</summary>
    public decimal Price { get; set; }

    /// <summary>Остаток на складе</summary>
    public int Stock { get; set; }

    // Можно добавить ещё характеристик.

}
