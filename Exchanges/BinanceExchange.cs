using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Sockets;
using CryptoZ.Tools;
using CryptoZ.Tools.KafkaTools;
using Binance.Net.Objects.Spot.MarketData;
using Binance.Net.Objects.Spot.SpotData;
using Binance.Net.Enums;

namespace CryptoZ.Exchanges
{
    public class BinanceExchange : ITickerProducer, IResearch
    {
        public string ExchName => "BINANCE";
        public string ApiKeyEnvVar => "BINANCE_KEY";

        BinanceClient exch;
        BinanceSocketClient sock;

        UpdateSubscription subscription;

        KafkaProducer _p;

        public BinanceExchange()
        {
            var evKeys = Environment.GetEnvironmentVariable(ApiKeyEnvVar, EnvironmentVariableTarget.User);
            var keys = evKeys.Split('|');

            var clientOptions = new BinanceClientOptions();
            clientOptions.ApiCredentials = new ApiCredentials(keys[0], keys[1]);
            this.exch = new BinanceClient(clientOptions);
            //----------
            var socketOptions = new BinanceSocketClientOptions();
            socketOptions.ApiCredentials = clientOptions.ApiCredentials;
            this.sock = new BinanceSocketClient(socketOptions);
        }

        public BinanceExchange(string bootstrapServers, string topic) : this()
        {
            _p = new KafkaProducer(bootstrapServers, topic);
        }


        #region ========== ITickerProducer IMPLEMENTATION ===============================
        public async Task DisplaySymbolCount()
        {
            var eiRes = await exch.Spot.System.GetExchangeInfoAsync();
            var ei = eiRes.Data;
            var symbols = ei.Symbols;
            Console.WriteLine($"[{ExchName}]   {symbols.Count()} symbols");
        }

        public async Task SubscribeAllTickerUpdates()
        {
            Console.WriteLine($"--- Starting {ExchName} SymbolTickerUpdates thread ---");
            var crSubSymbolTicker = sock.Spot.SubscribeToAllSymbolTickerUpdates((ticks) =>
            {
                Task.Factory.StartNew(() => ProduceToKafka(ticks));
            });

            this.subscription = crSubSymbolTicker.Data;
        }

        /*public async Task UnsubscribeSymbolTickerUpdates()
        {
            await sock.Unsubscribe(subscription);
        }*/

        public async Task UnsubscribeAllUpdates()
        {
            await sock.UnsubscribeAll();
        }

        public async Task WriteSymbolsCsv()
        {
            var eiRes = await exch.Spot.System.GetExchangeInfoAsync();
            var ei = eiRes.Data;
            var symbols = ei.Symbols;
            Console.WriteLine($"[{ExchName}]   {symbols.Count()} symbols");
            FileTools.WriteObjectsToCsv(symbols, FileTools.SymbolFilepath(ExchName));
        }
        #endregion ======================================================================


        private void ProduceToKafka(IEnumerable<IBinanceTick> ticks)
        {
            Console.WriteLine($"[{ExchName}]   {ticks.Count()} symbol ticker updates received");
            foreach (var tick in ticks)
            {
                //tick.LastQuantity
                //Console.WriteLine($"{tick.CloseTime:G} [{ExchName} {tick.Symbol}]  {tick.LastPrice} ({tick.BaseVolume}/{tick.QuoteVolume})    B {tick.LastPrice}{tick.BidQuantity} : {tick.BidPrice}  x  {tick.AskPrice} : {tick.AskQuantity} A");
                string msg = ToCsv(tick);
                //Console.WriteLine(msg);
                _p.Produce(msg);
            }
        }

        private string ToCsv(IBinanceTick tick)
        {
            return string.Format($"{tick.CloseTime:s},{ExchName},{tick.Symbol},{tick.LastPrice},{tick.BaseVolume},{tick.QuoteVolume},{tick.BidQuantity},{tick.BidPrice},{tick.AskPrice},{tick.AskQuantity}");
        }


        public async Task<BinanceExchangeInfo> GetExchangeInfo()
        {
            var crei = await exch.Spot.System.GetExchangeInfoAsync();
            var ei = crei.Data;
            return ei;
        }

        public async Task<BinanceAccountInfo> GetAccountInfo()
        {
            var crai = await exch.General.GetAccountInfoAsync();
            var ai = crai.Data;
            return ai;
        }


        public async Task<IEnumerable<IBinanceKline>> GetKlines(string symbol, KlineInterval interval = KlineInterval.OneDay)
        {
            CancellationToken ct = default;
            var crklines = await exch.Spot.Market.GetKlinesAsync(symbol, KlineInterval.ThreeMinutes, startTime: null, endTime: null, limit: 1000, ct);
            var klines = crklines.Data;
            return klines;
        }

        public async Task SubscribeKlineUpdates(string symbol, KlineInterval interval)
        {
            var crSubKline = await sock.Spot.SubscribeToKlineUpdatesAsync(symbol, interval, (crKline) =>
            {
                var kline = crKline.Data;
                Console.WriteLine(kline.ToString());
            });
        }

        public async Task SubscribeTickerUpdates(string symbol)
        {
            Console.WriteLine($"--- Starting {ExchName} {symbol} TickerUpdates thread ---");
            var crSubSymbolTicker = await sock.Spot.SubscribeToSymbolTickerUpdatesAsync(symbol, (tick) =>
            {
                Console.WriteLine(ToCsv(tick));
                //Task.Factory.StartNew(() => ProduceToKafka(tick));
            });

            //this.subscription = crSubSymbolTicker.Data;
        }

        public async Task SubscribeBookUpdates(string symbol)
        {
         
            var ctSubBookTicker = await sock.Spot.SubscribeToBookTickerUpdatesAsync(symbol, (book) =>
            {
                Console.WriteLine(book.ToString());
            });
        }

    } // class

} // namespace
