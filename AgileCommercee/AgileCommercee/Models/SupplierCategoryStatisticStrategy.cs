using AgileCommercee.Entities;

namespace AgileCommercee.Models
{
    public class SupplierCategoryStatisticStrategy : IStatisticsStrategy
    {
        public IEnumerable<StatisticResult> GetStatistics(IEnumerable<HangHoa> products)
        {
            return products
                .GroupBy(p => new
                {
                    MaLoaiNavigation = p.MaLoaiNavigation.TenLoai,
                    MaNccNavigation = p.MaNccNavigation.TenCongTy
                })
                .Select(g => new StatisticResult
                {
                    Category = g.Key.MaLoaiNavigation,
                    Supplier = g.Key.MaNccNavigation,
                    Count = g.Count(),
                    TotalValue = (decimal)g.Sum(p => p.DonGia * (1 - p.GiamGia))
                })
                .ToList();
        }
    }
}