namespace Trim.Models;

public class OptionPrice
{
    public int Id { get; set; }
    public string Group { get; set; } = ""; // "CabSize", "Engine", "Gearbox", ...
    public int EnumValue { get; set; }      // (int)TwojEnum
    public decimal Price { get; set; }      // dopłata / cena opcji
}