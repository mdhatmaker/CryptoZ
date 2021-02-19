# CryptoZ - A Proof-of-Concept Crypto Infrastructure Using Kafka

## Introduction

This project will demonstrate subscribing to crypto ticker (price) data for Binance, Bittrex, and Bitfinex. It will publishe this data to Kafka, and a separate consumer app will listen for these Kafka messages and store them in either .CSV files or a SQL database.

## Configuration

### Visual Studio

The CryptoZ solution contains multiple C# projects, and it can be opened in Visual Studio. This code has been created/tested on Windows, but because it uses the .NET Core 3.1 framework, it should run on Linux/Mac with only minor modification.

If you choose the .CSV file storage option, the .CSV files should be stored in the "/cryptoZ/tickers" directory by default. This can be changed easily in the *Tools.FileTools* class (and will be moved to AppSettings in upcoming versions--see the Future Enhancements section below).

### Environment Variables For API Keys/Secrets

In order to avoid disclosing API keys and secrets, this project looks to the following environment variables:

- BINANCE_KEY
- BITTREX_KEY
- BITFINEX_KEY

The expected format is a string with a vertical bar ("|") separating the key and secret. The *scripts* folder contains a PowerShell script which you can configure to easily set these environment variables.

### Kafka

This project uses Kafka to disseminate ticker price data for Binance, Bittrex, and Bitfinex.

By default, the expected Kafka configuration is as follows:
- bootstrapServers = "localhost:9092"
- topic = "crypto-marketdata-symbols"
- groupId = "marketdata-consumer-group" (consumer apps only)

## How To Run It

Before running the C# code, you will need to set the API key environment variables, and you need to install and configure Kafka on your local machine. (This will be improved to use a Kafka docker image in the near future).

If you have Visual Studio, you can load up the CryptoZ solution and start the default *AppLauncher* project. This project will allow you to launch various versions of a producer and consumer.

The AppLauncher will present a menu like the following:

(image here)

From this menu, you can first launch a consumer (for example, #3 to write data to .CSV files or #1 to only display the ticker data). Then, launch a producer (for example, #5 for a producer that will run for 5 minutes or #6 for a producer that will run indefinitely until ESC is pressed).

## Architecture Notes

As presented, this project relies on one or more "producers" which perform the work of connecting to the various crypto exchanges, subscribing to ticker (price) updates, and publishing these prices as Kafka messages. Correspondingly, there are one or more "consumers", such as the TickerStorage project, which listens to a Kafka topic and stores incoming ticker data to .CSV files or a SQL database.

There are several reasons to consider a Kafka-based architecture for a crypto-trading infrastructure, but here are some of the main ones:
- Kafka is scalable for both performance and fault tolerance. Companies such as Netflix use Kafka to process billions of messages per day, and prop trading firms use Kafka for their price data infrastructure.
- Kafka allows for language-agnostic consumer-app development. A historical price archiver like TickerStorage can be written in C# while more performance-intensive apps like trading algos can be written in C++ (or Go, Rust, etc.)
- Kafka allows for language-agnostic producer-app development. The prodcer apps can be selectively upgraded to C++ (or even FPGA) for faster performance. Additionally, multiple producers can be run, each located in different geographic areas to improve latency.
- Because Kafka "decouples" the client apps from the underlying exchange data, any upddates or rewrites of producer apps (even in different languages) should require no changes in the client apps. If we switch from a websockets exchange feed to a FIX feed (available on a few of the crypto exchanges), the client apps should continue to work seamlessly.

## Future Enhancements

The following is a list of upcoming project features (not necessarily implemented in the order they appear):

- Move to using a dockerized Kafka setup for ease of setup/demo
- Implement error-handling and unit tests
- Run performance tests and store baseline performance values for each crypto exchange
- Create AVRO objects for ticker data and use AVRO Schema Registry in Kafka (currently publishing simple comma-delimeted strings)
- Create sample C++ app that demonstrates consuming Kafka ticker data
- Add monitoring tools that perform heartbeat and performance tracking (along with notification via text/email/Prowl)
- Create second demo producer (BTC_Producer) which *only* publishes ticker data for BTCUSD
- Use AppSettings for configuration of Kafka, data directories, API keys, etc.
- Complete output-to-SQL option for ticker price data (currently .csv is implemented)
- Create single Docker image that will contain the Kafka setup, SQL setup, and appropriately configured .NET installation
- 