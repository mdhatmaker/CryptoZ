using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using CryptoZ.Exchanges;
using Binance.Net.Interfaces;
using System.Threading.Tasks;

namespace CryptoZ
{
    class Program
    {
        static bool exitApp = false;
        static string thisExeFilepath, thisExePath;


        static IResearch _binance, _bittrex, _bitfinex;

        // Singleton exchange clients
        static IResearch binance => _binance ?? (_binance = new BinanceExchange());
        //static ITickerProducer bittrex => _bittrex ?? (_bittrex = new BittrexExchange());
        //static ITickerProducer bitfinex => _bitfinex ?? (_bitfinex = new BitfinexExchange());

        static void Main(string[] args)
        {
            DisplayWelcomeMessage();

            Console.WriteLine("\n*** Press ESC exit ***\n");
            do
            {
                DisplayMenu();
                while (!Console.KeyAvailable)
                {
                    Thread.Sleep(300);
                }
                var key = Console.ReadKey(true).Key;
                Console.WriteLine();
                if (key == ConsoleKey.Escape)
                    exitApp = true;
                else
                    ProcessKeyChoice(key);
            } while (exitApp == false);

            Console.WriteLine("\n*** Press ESC to stop publisher ***\n");
            do
            {
                while (!Console.KeyAvailable)
                {
                    Thread.Sleep(500);
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            CleanUp();
        }

        static void CleanUp()
        {
            // dispose of objects and other clean-up code
        }

        static void DisplayWelcomeMessage()
        {
            Console.WriteLine("\n=== WELCOME TO CRYPTOZ DEMO LAB ===\n");
            Console.WriteLine("This .NET Core app provides a menu to demo various");
            Console.WriteLine("experimental features.\n");
        }

        static void DisplayMenu()
        {
            Console.WriteLine();
            Console.WriteLine("1. Demo Klines");
            Console.WriteLine("2. ");
            Console.WriteLine("3. ");
            Console.WriteLine("4. ");
            Console.WriteLine("5. ");
            Console.WriteLine("6. ");
            Console.WriteLine("7. ");
            Console.WriteLine("8. ");
            Console.WriteLine("9. ");
            Console.WriteLine("0. ");
            Console.WriteLine("ESC to Exit.");

            Console.WriteLine();
            Console.Write("Enter choice: ");
        }

        static void InitializePaths()
        {
            thisExeFilepath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            thisExePath = System.IO.Path.GetDirectoryName(thisExeFilepath);
            //tickerStorageExe = FileTools.GetProjectExeFilepathReleaseOrDebug(thisExePath, "TickerStorage", "TickerStorage.exe", "netcoreapp3.1", "Consumers");
            //tickerProducerExe = FileTools.GetProjectExeFilepathReleaseOrDebug(thisExePath, "TickerProducer", "TickerProducer.exe", "netcoreapp3.1", "Producers");
        }

        static void ProcessKeyChoice(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.D1:
                    DemoKlines();
                    break;
                case ConsoleKey.D2:
                    break;
                case ConsoleKey.D3:
                    break;
                case ConsoleKey.D4:
                    break;
                case ConsoleKey.D5:
                    break;
                case ConsoleKey.D6:
                    break;
                case ConsoleKey.D7:
                    break;
                case ConsoleKey.D8:
                    break;
                case ConsoleKey.D9:
                    break;
                case ConsoleKey.D0:
                    break;
                default:
                    Console.WriteLine($"Unrecognized choice '{key}'.");
                    break;
            }
        }

        static string ToStr(IBinanceKline kl)
        {
            return string.Format($"{kl.OpenTime} {kl.CloseTime} O:{kl.Open} H:{kl.High} L:{kl.Low} C:{kl.Close}  Vb:{kl.BaseVolume} Vq:{kl.QuoteVolume} Vtb:{kl.TakerBuyBaseVolume} Vtq:{kl.TakerBuyQuoteVolume}   Trds:{kl.TradeCount}");
        }

        static string ToCsv(IBinanceKline kl)
        {
            return string.Format($"{kl.OpenTime},{kl.CloseTime},{kl.Open},{kl.High},{kl.Low},{kl.Close},{kl.BaseVolume},{kl.QuoteVolume},{kl.TakerBuyBaseVolume},{kl.TakerBuyQuoteVolume},{kl.TradeCount}");
        }

        static async Task DemoKlines()
        {
            string symbol = "BTCUSDT";
            var klines = await binance.Klines(symbol);
            foreach (var kl in klines)
            {
                Console.WriteLine($"{symbol} {ToStr(kl)}");
            }
        }


    } // class

} // namespace
