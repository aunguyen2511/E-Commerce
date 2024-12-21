using AgileCommercee.Entities;

namespace AgileCommercee.Models
{
    public interface IStatisticsStrategy
    {
        IEnumerable<StatisticResult> GetStatistics(IEnumerable<HangHoa> products);
    }
}
//interface thóng kê