using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bitfinex.Net;
using Bitfinex.Net.Objects;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Sockets;
using CryptoZ.Tools;
using CryptoZ.Tools.KafkaTools;
using System.Diagnostics;

namespace CryptoZ
{
    public class BitfinexExchange : ITickerProducer
    {
        public string ExchName => "BITFINEX";
        public string ApiKeyEnvVar => "BITFINEX_KEY";

        BitfinexClient exch;
        BitfinexSocketClient sock;

        UpdateSubscription subscription;

        KafkaProducer _p;

        int msgCount = 0;

        //public BitfinexExchange(KafkaProducer p)
        public BitfinexExchange(string bootstrapServers, string topic)
        {
            var evKeys = Environment.GetEnvironmentVariable(ApiKeyEnvVar, EnvironmentVariableTarget.User);
            var keys = evKeys.Split('|');

            var clientOptions = new BitfinexClientOptions();
            clientOptions.ApiCredentials = new ApiCredentials(keys[0], keys[1]);
            this.exch = new BitfinexClient(clientOptions);
            //----------
            var socketOptions = new BitfinexSocketClientOptions();
            socketOptions.ApiCredentials = clientOptions.ApiCredentials;
            this.sock = new BitfinexSocketClient(socketOptions);

            //_p = p;
            _p = new KafkaProducer(bootstrapServers, topic);
        }


        public async Task DisplaySymbolCount()
        {
            var resSymbols = await exch.GetSymbolsAsync();
            var symbols = resSymbols.Data;
            Console.WriteLine($"[{ExchName}]   {symbols.Count()} symbols");
        }

        public async Task SubscribeAllTickerUpdates()
        {
            Console.WriteLine($"--- Starting {ExchName} SymbolTickerUpdates thread ---");
            var resSymbols = exch.GetSymbols();
            IEnumerable<string> symbols;
            // --- FOR DEBUGGING ONLY: CAN SET COMMAND-LINE ARGUMENTS ---
#if DEBUG
            if (Debugger.IsAttached)
                symbols = resSymbols.Data.Take(10);     // TODO: *** FOR TESTING ONLY ***
            else
                symbols = resSymbols.Data;
#else
            symbols = resSymbols.Data;
#endif
            int count = symbols.Count();
            int i = 0;
            foreach (var s in symbols)
            {
                ++i;
                await SubscribeSymbolTickerUpdates(s, i, count);
            }
        }

        private void ProduceToKafka(BitfinexStreamSymbolOverview tick, string symbol)
        {
            ++msgCount;
            if (msgCount % 50 == 0)
                Console.WriteLine($"[{ExchName}]   {msgCount} total symbol ticker updates received");
            int quoteVolume = 0;
            DateTime dt = DateTime.Now.ToUniversalTime();
            //Console.WriteLine($"{dt:G} [{ExchName} {symbol}]  {tick.LastPrice} ({tick.Volume}/{quoteVolume})    B {tick.BidSize} : {tick.Bid}  x  {tick.Ask} : {tick.AskSize} A");
            string msg = string.Format($"{dt:s},{ExchName},{symbol},{tick.LastPrice},{tick.Volume},{quoteVolume},{tick.BidSize},{tick.Bid},{tick.Ask},{tick.AskSize}");
            //Console.WriteLine(msg);
            _p.Produce(msg);
        }

        public async Task SubscribeSymbolTickerUpdates(string rawSymbol, int i = -1, int count = -1)
        {
            string symbol = "t" + rawSymbol.ToUpper();
            //Console.WriteLine($"  --- Subscribing to [{ExchName} {symbol}]    ( {i} / {count} ) ---");
            var crSubSymbolTicker = await sock.SubscribeToTickerUpdatesAsync(symbol, (tick) =>
            {
                Task.Factory.StartNew(() => ProduceToKafka(tick, symbol));
                /*//Console.WriteLine($"[{ExchName}]   1 symbol ticker updates received");
                int quoteVolume = 0;
                DateTime dt = DateTime.Now.ToUniversalTime();
                //Console.WriteLine($"{dt:G} [{ExchName} {symbol}]  {tick.LastPrice} ({tick.Volume}/{quoteVolume})    B {tick.BidSize} : {tick.Bid}  x  {tick.Ask} : {tick.AskSize} A");
                string msg = string.Format($"{dt:G},{ExchName},{symbol},{tick.LastPrice},{tick.Volume},{quoteVolume},{tick.BidSize},{tick.Bid},{tick.Ask},{tick.AskSize}");
                //Console.WriteLine(msg);
                _p.Produce(msg);*/
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
            var resSymbols = await exch.GetSymbolsAsync();
            var symbols = resSymbols.Data;
            Console.WriteLine($"[{ExchName}]   {symbols.Count()} symbols");
            FileTools.WriteStringsToCsv(symbols, FileTools.SymbolFilepath(ExchName), "Symbol");
        }


    } // class

} // namespace