﻿using PassnegerGenerator;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", 
                outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}", 
                rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("Hello, world!");

int a = 10, b = 0;
try
{
    Log.Debug("Dividing {A} by {B}", a, b);
    Console.WriteLine(a / b);
}
catch (Exception ex)
{
    Log.Error(ex, "Something went wrong");
}
finally
{
    await Log.CloseAndFlushAsync();
}

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

