using AgileCommercee.Entities;

namespace AgileCommercee.Models
{
    public class CategoryStatisticsStrategy : IStatisticsStrategy
    {
        public IEnumerable<StatisticResult> GetStatistics(IEnumerable<HangHoa> products)
        {
            return products
        .Where(p => p.MaLoaiNavigation != null) // Kiểm tra null trước khi truy cập
        .GroupBy(p => p.MaLoaiNavigation.TenLoai)
        .Select(g => new StatisticResult
        {
            Category = g.Key, // Tên loại
            Count = g.Count(),
            TotalValue = (decimal)g.Sum(p => p.DonGia * (1 - p.GiamGia))
        })
        .ToList();
        }
    }
}
