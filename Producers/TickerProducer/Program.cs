using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using CryptoZ.Tools;

namespace CryptoZ
{
    // TickerProducer
    class Program
    {
        // Kafka configuration parameters
        static string bootstrapServers = "localhost:9092";
        static string topic = "crypto-marketdata-symbols";
        //static string groupId = "marketdata-consumer-group";

        static ITickerProducer _binance, _bittrex, _bitfinex;
        
        // Singleton exchange clients
        static ITickerProducer binance => _binance ?? (_binance = new BinanceExchange(bootstrapServers, topic));
        static ITickerProducer bittrex => _bittrex ?? (_bittrex = new BittrexExchange(bootstrapServers, topic));
        static ITickerProducer bitfinex => _bitfinex ?? (_bitfinex = new BitfinexExchange(bootstrapServers, topic));


        static void DisplayWelcomeMessage()
        {
            Console.WriteLine("\n=== WELCOME TO CRYPTO TICKER PRODUCER ===\n");
            Console.WriteLine("This .NET Core app will subscribe to price updates for crypto");
            Console.WriteLine("symbols on Binance, Bittrex, and Bitfinex.\n");
            Console.WriteLine("Use the 'set-crypto-api-env-vars.ps1' script in 'scripts' folder");
            Console.WriteLine("to set your API keys.\n");
            Console.WriteLine("(See 'README.md' for more information.)\n");
        }

        static void DisplayUsageMessage()
        {
            Console.WriteLine("usage: dotnet TickerProducer all");
            Console.WriteLine("       dotnet TickerProducer [binance|bittrex|bitfinex]");
            Console.WriteLine();
        }


        static async Task Main(string[] args)
        {
            DisplayWelcomeMessage();


            // --- FOR DEBUGGING ONLY: CAN SET COMMAND-LINE ARGUMENTS ---
#if DEBUG
            if (Debugger.IsAttached && args.Length == 0)
            {
                //args = new string[] { "binance" };
                args = new string[] { "all" };
                //args = new string[] { "all" , "1" };
            }
#endif

            if (args.Length == 0)
            {
                DisplayUsageMessage();
            }
            else if (args[0].ToUpper() == "ALL")
            {
                int minutes;
                if (args.Length > 1 && int.TryParse(args[1], out minutes))
                    await StartTickerProducers(minutes);
                else
                    await StartTickerProducers();
            }
            else if (args[0].ToUpper() == "BINANCE")
            {
            }
            else if (args[0].ToUpper() == "BITTREX")
            {
            }
            else if (args[0].ToUpper() == "BITFINEX")
            {
            }
            else
            {
                Console.WriteLine($"Uknown initial argument '{args[0]}'\n");
                DisplayUsageMessage();
                System.Environment.Exit(0);
            }

            Console.WriteLine("\n\nPress ENTER to exit");
            Console.ReadLine();

        } // end of Main


        // where runMinutes is the number of minutes the publisher should run
        // (default -1 runs forever)
        static async Task StartTickerProducers(int runMinutes = -1)
        {
            Console.WriteLine($"\n--- Running ticker producers for each exchange in 2 second(s) ---");
            Thread.Sleep(2 * 1000);

            // Display symbol counts for each exchange
            await binance.DisplaySymbolCount();
            await bittrex.DisplaySymbolCount();
            await bitfinex.DisplaySymbolCount();

            // subscribe to ticker updates
            await binance.SubscribeAllTickerUpdates();
            await bittrex.SubscribeAllTickerUpdates();
            await bitfinex.SubscribeAllTickerUpdates();

            // wait a while to let ticker updates publish...
            if (runMinutes < 0)
            {
                Console.WriteLine("\n*** Press ESC to stop publisher ***\n");
                do
                {
                    while (!Console.KeyAvailable)
                    {
                        Thread.Sleep(500);
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            }
            else
            {
                Console.WriteLine($"\n*** Publisher will run for {runMinutes} minute(s) ***\n");
                int runSeconds = 60 * runMinutes;
                Thread.Sleep(runSeconds * 1000);
            }

            
            // unsubscribe from updates
            await binance.UnsubscribeAllUpdates();
            await bittrex.UnsubscribeAllUpdates();
            await bitfinex.UnsubscribeAllUpdates();


            return;
        }



    } // class

} // namespace

