namespace AgileCommercee.Models
{
    public class StatisticResult
    {
        public string Supplier { get; set; }
        public string Category { get; set; } = string.Empty; // Có thể đặt giá trị mặc định là một chuỗi rỗng
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
    }
}