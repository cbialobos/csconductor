using System;
using conductor.client.worker;
using conductor.common.metadata.tasks;
using Microsoft.Extensions.Logging;

namespace conductorPocWorker
{
  public class SampleWorker : Worker
  {
    private readonly ILogger m_logger;
    public override string TaskDefName { get; }

    public SampleWorker(ILogger logger, string taskDefName)
    {
      m_logger = logger;
      TaskDefName = taskDefName;
    }

    public override TaskResult Execute(Task task)
    {
      m_logger.LogDebug($"Executing {TaskDefName}");

      var result = new TaskResult(task) {Status = Status.COMPLETED};

      //Register the output of the task
      result.OutputData.Add("outputKey1", "value");
      result.OutputData.Add("oddEven", 1);
      result.OutputData.Add("mod", 4);

      return result;
    }
  }
}
