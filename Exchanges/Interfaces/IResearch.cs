using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Binance.Net.Enums;
using Binance.Net.Interfaces;


namespace CryptoZ.Exchanges
{
    public interface IResearch
    {
        Task DisplaySymbolCount();
        Task WriteSymbolsCsv();
        Task SubscribeTickerUpdates(string symbol);
        Task UnsubscribeAllUpdates();
        Task<IEnumerable<IBinanceKline>> GetKlines(string symbol, KlineInterval interval = KlineInterval.OneDay);

    } // interface

} // namespace
