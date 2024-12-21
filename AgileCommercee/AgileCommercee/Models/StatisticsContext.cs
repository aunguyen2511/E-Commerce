using AgileCommercee.Entities;

namespace AgileCommercee.Models
{
    public class StatisticsContext
    {
        private IStatisticsStrategy _strategy;

        public void SetStrategy(IStatisticsStrategy strategy)
        {
            _strategy = strategy;
        }

        public IEnumerable<HangHoa> ExecuteStrategy(IEnumerable<HangHoa> products)
        {
            return (IEnumerable<HangHoa>)_strategy.GetStatistics(products);
        }
    }
}
