using System;
using System.Collections.Generic;
using System.Linq;
using MuhasebeStokWebApp.Exceptions.Strategies;

namespace MuhasebeStokWebApp.Exceptions
{
    /// <summary>
    /// Exception stratejilerini yöneten factory sınıfı
    /// </summary>
    public class ExceptionStrategyFactory
    {
        private readonly IEnumerable<IExceptionStrategy> _strategies;
        private readonly IExceptionStrategy _defaultStrategy;

        /// <summary>
        /// Exception stratejilerini enjekte eden constructor
        /// </summary>
        public ExceptionStrategyFactory(IEnumerable<IExceptionStrategy> strategies)
        {
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
            _defaultStrategy = new DefaultExceptionStrategy();
        }

        /// <summary>
        /// Exception tipine göre uygun stratejiyi döndürür
        /// </summary>
        /// <param name="exception">İşlenecek exception</param>
        /// <returns>Exception stratejisi</returns>
        public IExceptionStrategy GetStrategy(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            // Uygun stratejiyi bul
            var strategy = _strategies.FirstOrDefault(s => s.CanHandle(exception));
            
            // Uygun strateji yoksa defaultStrategy'yi döndür
            return strategy ?? _defaultStrategy;
        }
    }
} 