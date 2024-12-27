using AgileCommercee.Entities;

namespace AgileCommercee.Models
{
    public class CategoryStatisticsStrategy1 : IStatisticsStrategy1
    {
        public IEnumerable<object> GetStatistics(MyEstoreContext context)
        {
            return context.HangHoas
                .GroupBy(p => p.MaLoaiNavigation.TenLoai)
                .Select(g => new CategoryStatistic
                {
                    MaLoaiNavigation = g.Key,
                    NumOfProduct = g.Count()
                })
                .ToList();
        }
    }
}
