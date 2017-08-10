using System;
using System.Collections.Generic;
using System.Linq;
using conductor.client.http;
using conductor.client.task;
using conductor.client.worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace conductorPocWorker
{
  public class Program
  {
    static void Main(string[] args)
    {
      var consoleLogger = new ConsoleLogger("test", Filter, false);

      var taskClient = new TaskClient(new Uri("http://localhost:8080/api/"));

      int threadCount = 2;

      var builder = new WorkflowTaskCoordinator.Builder(consoleLogger);
      var coordinator = builder.WithWorkers(GetWorkers(consoleLogger).ToArray()).WithThreadCount(threadCount).WithTaskClient(taskClient).Build();

      //Start for polling and execution of the tasks
      coordinator.Init();

      Console.ReadLine();
    }

    private static IEnumerable<Worker> GetWorkers(ILogger logger)
    {
      return Enumerable.Range(1, 1).Select(id => id == 5 ? new SampleWorker(logger, "task_5") : new SampleWorker(logger, $"task_{id}"));
    }

    private static bool Filter(string s, LogLevel logLevel)
    {
      return true;
    }
  }
}