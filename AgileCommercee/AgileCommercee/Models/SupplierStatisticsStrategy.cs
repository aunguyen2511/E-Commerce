using AgileCommercee.Entities;

namespace AgileCommercee.Models
{
    public class SupplierStatisticsStrategy : IStatisticsStrategy
    {
        public IEnumerable<StatisticResult> GetStatistics(IEnumerable<HangHoa> products)
        {
            return products
                .GroupBy(p => p.MaNcc)
                .Select(g => new StatisticResult
                {
                    Supplier = g.Key,
                    Count = g.Count(),
                    TotalValue = (decimal)g.Sum(p => p.DonGia * (1 - p.GiamGia))
                })
                .ToList();
        }
    }
}
