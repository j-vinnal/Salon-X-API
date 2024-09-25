namespace App.DTO.Public.v1;

public class MonthlyRevenue
{
    public string Month { get; set; } = default!;
    public decimal Revenue { get; set; }
}