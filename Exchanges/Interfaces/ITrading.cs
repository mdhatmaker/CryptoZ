using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Binance.Net.Enums;
using Binance.Net.Interfaces;

namespace CryptoZ.Exchanges
{
    public interface ITrading
    {
        Task DisplaySymbolCount();
        Task WriteSymbolsCsv();
        Task UnsubscribeAllUpdates();
        Task<IEnumerable<IBinanceKline>> GetKlines(string symbol, KlineInterval interval = KlineInterval.OneDay);
        Task SubscribeKlineUpdates(string symbol, KlineInterval interval);
        Task SubscribeTickerUpdates(string symbol);
        Task SubscribeBookUpdates(string symbol);

    } // interface

} // namespace