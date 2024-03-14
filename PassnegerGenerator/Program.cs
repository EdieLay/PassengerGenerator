using PassengerGenerator;
using PassnegerGenerator;
using Serilog;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt",
                outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
    .CreateLogger();



Simulation simulation = new Simulation();
Flight flight = new("123", DateTime.Now.AddHours(2), 200, DateTime.Now);
simulation.Flights[flight.GUID] = flight;
simulation.Execute();



//await Log.CloseAndFlushAsync();

//DateTime time = DateTime.Now;
//Console.WriteLine(time);
//var delay = Task.Delay(2000);
//time = time.AddMinutes(59-time.Minute);
//time = time.AddHours(2);
//Console.WriteLine(DateTime.Now.Subtract(time).TotalMinutes);
//Thread.Sleep(1000);
//time = time.AddMinutes(1);
//Console.WriteLine(time);
//delay.Wait();
//Console.WriteLine("4");

