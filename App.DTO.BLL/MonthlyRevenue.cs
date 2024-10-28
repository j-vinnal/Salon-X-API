

using Base.Contracts;

namespace App.DTO.BLL;

public class MonthlyRevenue 
{
    public string Month { get; set; } = default!;
    public decimal Revenue { get; set; }
}