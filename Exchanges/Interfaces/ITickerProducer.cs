using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CryptoZ.Exchanges
{
    public interface ITickerProducer
    {
        Task DisplaySymbolCount();
        Task WriteSymbolsCsv();
        Task SubscribeAllTickerUpdates();
        //Task UnsubscribeTickerUpdates();
        Task UnsubscribeAllUpdates();
    } // interface

} // namespace
