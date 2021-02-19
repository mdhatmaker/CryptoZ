using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using CryptoZ.Tools;
using CryptoZ.Tools.KafkaTools;
using System.IO;

namespace CryptoZ
{
    // TickerStorage
    class Program
    {
        // Kafka configuration parameters
        static string bootstrapServers = "localhost:9092";
        static string topic = "crypto-marketdata-symbols";
        static string groupId = "marketdata-consumer-group";

        static KafkaConsumer kafkaConsumer;
        static Task consumerTask;

        static StreamWriter binanceWriter;
        static StreamWriter bittrexWriter;
        static StreamWriter bitfinexWriter;


        static void DisplayWelcomeMessage()
        {
            Console.WriteLine("\n=== WELCOME TO CRYPTO TICKER STORAGE ===\n");
            Console.WriteLine("This .NET Core app will listen for ticker updates via");
            Console.WriteLine("Kafka and store this data for future analysis.\n");
            Console.WriteLine("(See 'README.md' for more information.)\n");
        }

        static void DisplayUsageMessage()
        {
            Console.WriteLine("usage: dotnet TickerStorage all");
            //Console.WriteLine("       dotnet CryptoDataVacuum [binance|bittrex|bitfinex]");
            Console.WriteLine();
        }


        static async Task Main(string[] args)
        {
            DisplayWelcomeMessage();


            // --- FOR DEBUGGING ONLY: CAN SET COMMAND-LINE ARGUMENTS ---
#if DEBUG
            if (Debugger.IsAttached && args.Length == 0)
            {
                //args = new string[] { "display" };
                //args = new string[] { "code" };
                args = new string[] { "csv" };
                //args = new string[] { "sql" };
            }
#endif

            if (args.Length == 0)
            {
                DisplayUsageMessage();
            }
            else if (args[0].ToUpper() == "DISPLAY")
            {
                await StartTickerConsumer_DISPLAY();
            }
            else if (args[0].ToUpper() == "CODE")
            {
                await StartTickerConsumer_CODE();
            }
            else if (args[0].ToUpper() == "CSV")
            {
                await StartTickerConsumer_CSV();
            }
            else if (args[0].ToUpper() == "SQL")
            {
                await StartTickerConsumer_SQL();
            }
            else
            {
                Console.WriteLine($"Uknown initial argument '{args[0]}'\n");
                DisplayUsageMessage();
                System.Environment.Exit(0);
            }

            Console.WriteLine("\n\nPress ENTER to exit");
            Console.ReadLine();

            CleanUp();

        } // end of Main

        static async Task StartTickerConsumer_DISPLAY(int sleepSeconds = 2)
        {
            kafkaConsumer = new KafkaConsumer(bootstrapServers, topic, groupId);
            consumerTask = Task.Factory.StartNew(() => kafkaConsumer.Start((cr) =>
                {
                    var msg = cr.Message.Value;
                    Console.WriteLine(msg);
                }));
        }

        static async Task StartTickerConsumer_CODE(int sleepSeconds = 2)
        {
            kafkaConsumer = new KafkaConsumer(bootstrapServers, topic, groupId);
            consumerTask = Task.Factory.StartNew(() => kafkaConsumer.Start((cr) =>
                {
                    var msg = cr.Message.Value;
                    var values = msg.Split(',');
                    if (values[1] == "BINANCE")
                        Console.Write("B");
                    else if (values[1] == "BITFINEX")
                        Console.Write("F");
                    else if (values[1] == "BITTREX")
                        Console.Write("t");
                    else
                        Console.WriteLine($"\nUnknown Exchange: {values[1]}");
                }));
        }

        static async Task StartTickerConsumer_CSV(int sleepSeconds = 2)
        {
            InitializeCsvFiles();

            kafkaConsumer = new KafkaConsumer(bootstrapServers, topic, groupId);
            consumerTask = Task.Factory.StartNew(() => kafkaConsumer.Start((cr) =>
                {
                    var msg = cr.Message.Value;
                    var values = msg.Split(',');
                    if (values[1] == "BINANCE")
                    {
                        binanceWriter.WriteLine(msg);
                    }
                    else if (values[1] == "BITFINEX")
                    {
                        bitfinexWriter.WriteLine(msg);
                    }
                    else if (values[1] == "BITTREX")
                    {
                        bittrexWriter.WriteLine(msg);
                    }
                    else
                        Console.WriteLine($"\n  --- Unknown Exchange: {values[1]}");
                }));
        }

        static async Task StartTickerConsumer_SQL(int sleepSeconds = 2)
        {
            kafkaConsumer = new KafkaConsumer(bootstrapServers, topic, groupId);
            consumerTask = Task.Factory.StartNew(() => kafkaConsumer.Start((cr) =>
                {
                    var msg = cr.Message.Value;
                    var values = msg.Split(',');
                    if (values[1] == "BINANCE")
                        Console.Write("B");
                    else if (values[1] == "BITFINEX")
                        Console.Write("F");
                    else if (values[1] == "BITTREX")
                        Console.Write("t");
                    else
                        Console.WriteLine($"\nUnknown Exchange: {values[1]}");
                }));
        }

        static void InitializeCsvFiles()
        {
            binanceWriter = InitializeCsv("BINANCE");
            bittrexWriter = InitializeCsv("BITTREX");
            bitfinexWriter = InitializeCsv("BITFINEX");
        }

        // where exchName like "BINANCE"
        static StreamWriter InitializeCsv(string exchName)
        {
            var filepath = FileTools.SubdirFilepath("tickers", $"tickers.{exchName}.csv", true);
            var writer = new StreamWriter(filepath);
            Console.WriteLine($"File created: {filepath}");
            // Write the headers to first line of file
            writer.WriteLine("Time,Exchange,Symbol,LastPrice,BaseVolume,QuoteVolume,BidQty,BidPrice,AskPrice,AskQty");
            return writer;
        }

        static void disposeWriter(StreamWriter writer)
        {
            if (writer != null)
            {
                writer.Close();
                writer = null;
            }
        }

        static void CleanUp()
        {
            disposeWriter(binanceWriter);
            disposeWriter(bittrexWriter);
            disposeWriter(bitfinexWriter);

            // TODO: stop consumerTask?
            // TODO: clean up kafkaConsumer
        }

    } // class

} // namespace

