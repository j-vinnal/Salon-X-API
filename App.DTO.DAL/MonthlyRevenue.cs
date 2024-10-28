using Base.Contracts;

namespace App.DTO.DAL;

public class MonthlyRevenue 
{
    
    public string Month { get; set; } = default!;
    public decimal Revenue { get; set; }
    
}