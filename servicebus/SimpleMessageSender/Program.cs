using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Text.Json;

using CommandLine;

using console = SimpleMessageSender.ColorConsole;


CommandLineOptions options = new CommandLineOptions();
//Console.CancelKeyPress += (s, e) => Console.WriteLine("Done!");

//Parsing command line aguments
Parser.Default.ParseArguments<CommandLineOptions>(args)
                    .WithNotParsed(e =>
                    {
                        console.WriteLine("Argument errors:");
                        foreach (var err in e)
                        {
                            console.WriteLine($"{err.Tag}: {err}", ConsoleColor.Red);
                        }
                        Console.WriteLine();
                    })
                   .WithParsed(o =>
                   {
                       options = o;
                   });

//Running message sender in an infinite loop
await ServiceBusMessageSender(options);



async static Task ServiceBusMessageSender(CommandLineOptions o)
{
    try
    {
        //Get service bus settings from configuration (required secrets.json or user secrets)
        IConfiguration config = new ConfigurationBuilder()
                               .AddJsonFile("secrets.json", optional:true)
                               .AddUserSecrets<Program>(optional: true)
                               .Build();
        ServiceBusSettings settings = config
                             .GetRequiredSection("ServiceBus")
                             .Get<ServiceBusSettings>();
        string connectionString = settings.ConnectionString ?? "";


        ServiceBusClient client = new ServiceBusClient(connectionString);
        ServiceBusSender sender = client.CreateSender(o.Topic);

        while (true)
        {
            Console.Clear();
            console.WriteLine("Azure Service Bus Messenger", ConsoleColor.Green);
            console.WriteLine($"v {Assembly.GetExecutingAssembly().GetName().Version}", ConsoleColor.Green);
            Console.WriteLine();

            console.WriteLine($"Enter the message to send to topic {o.Topic}", ConsoleColor.Yellow);
            console.WriteLine($"(Press ctrl+c or enter an empty sting to quit)", ConsoleColor.Yellow);
            Console.Write("> ");
            string userMessage = Console.ReadLine() ?? "";
            if (userMessage.Trim() == string.Empty)
            {
                break;
            }

            ServiceBusMessage message = new ServiceBusMessage { Subject = "" };
            message.MessageId = Guid.NewGuid().ToString();
            message.Subject = "";
            message.ContentType = "application/json;charset=utf-8";
            message.ApplicationProperties.Add("sender", "SimpleMessageSender");
            message.Body = new BinaryData(JsonSerializer.Serialize(new
            {
                Message = userMessage
            }));
            ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
            if (!messageBatch.TryAddMessage(message))
            {
                throw new Exception($"Cannot add message to batch!");
            }
            await sender.SendMessagesAsync(messageBatch);
            console.WriteLine("Message sent", ConsoleColor.Green);
            await Task.Delay(1000);
        }
        
        console.WriteLine("Done!");
    }
    catch (Exception e)
    {
        console.WriteLine($"\n{e.GetType()}", ConsoleColor.Red);
        console.WriteLine($"{e.Message}\n", ConsoleColor.Red);
    }
}


