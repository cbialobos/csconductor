using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using conductor.client.http;
using conductor.client.worker;
using conductor.common.metadata.tasks;
using Microsoft.Extensions.Logging;
using Task = conductor.common.metadata.tasks.Task;

namespace conductor.client.task
{
  public class WorkflowTaskCoordinator
  {
    public class Builder
    {
      private ILogger m_logger;

      public Builder(ILogger logger)
      {
        this.m_logger = logger;
      }

      public Worker[] TaskWorkers { get; set; }
      public int ThreadCount { get; set; }
      public TaskClient TaskClient { get; set; }

      public Builder WithWorkers(IEnumerable<Worker> workers)
      {
        TaskWorkers = workers.ToArray();
        return this;
      }

      public Builder WithThreadCount(int threadCount)
      {
        ThreadCount = threadCount;
        return this;
      }

      public Builder WithTaskClient(TaskClient taskClient)
      {
        TaskClient = taskClient;
        return this;
      }

      public WorkflowTaskCoordinator Build()
      {
        var sleepWhenRetry = 500;
        var updateRetryCount = 3;
        return new WorkflowTaskCoordinator(m_logger, TaskClient, TaskWorkers, ThreadCount,  sleepWhenRetry, updateRetryCount);
      }
    }

    private readonly ILogger m_logger;
    private readonly Worker[] workers;
    private int m_threadCount;
    private readonly int m_sleepWhenRetry;
    private readonly int m_updateRetryCount;
    private readonly TaskClient client;

    public WorkflowTaskCoordinator(ILogger logger, TaskClient client, Worker[] workers, int threadCount, int sleepWhenRetry, int updateRetryCount)
    {
      m_logger = logger;
      this.workers = workers;
      this.m_threadCount = threadCount;
      m_sleepWhenRetry = sleepWhenRetry;
      m_updateRetryCount = updateRetryCount;
      this.client = client;
    }

    public void Init()
    {
      if (m_threadCount == -1)
      {
        m_threadCount = workers.Count();
      }

      Console.WriteLine("Initialized the worker with {threadCount} threads");

      var cancelationToken = new CancellationTokenSource();

      foreach (var worker in workers)
      {
        new TaskFactory().StartNew(() => 
          PeriodicTask.Run(() => PollForTask(worker), worker.PollingInterval, cancelationToken.Token), 
          TaskCreationOptions.LongRunning);
      }
    }

    private void PollForTask(Worker worker)
    {
      if (worker.IsPaused)
      {
        //WorkflowTaskMetrics.paused(worker.getTaskDefName());
        m_logger.LogDebug($"Worker {nameof(worker)} has been paused. Not polling anymore!");
        return;
      }

      m_logger.LogDebug($"Polling {worker.TaskDefName}, domain={"domain"}, count = {worker.PollCount} timeout = {worker.LongPollTimeoutInMs} ms");

      try
      {

        var taskType = worker.TaskDefName;
        //Stopwatch sw = WorkflowTaskMetrics.pollTimer(worker.getTaskDefName());
        var tasks = client.Poll(taskType, "", worker.Identity, worker.PollCount, worker.LongPollTimeoutInMs);
        //sw.stop();
        m_logger.LogDebug($"Polled { worker.TaskDefName}, for domain {"domain"} and receivd {tasks.Count} tasks");
        foreach (var task in tasks)
        {
          try
          {
            Execute(worker, task);
          }
          catch (Exception t)
          {
            task.Status = Status.FAILED;
            TaskResult result = new TaskResult(task);
            handleException(t, result, worker, true, task);
          }
        }
      }
      catch (Exception e)
      {
        //WorkflowTaskMetrics.pollingException(worker.getTaskDefName(), e);
        m_logger.LogError($"Error when polling for task {e.Message}");
      }
    }

    private void Execute(Worker worker, Task task)
    {
      string taskType = task.TaskDefName;
      try
      {

        if (!worker.PreAck(task))
        {
          m_logger.LogDebug($"Worker {taskType} decided not to ack the task {task.TaskId}");
          return;
        }

        if (!client.Ack(task.TaskId, worker.Identity))
        {
          //WorkflowTaskMetrics.ackFailed(worker.getTaskDefName());
          m_logger.LogError($"Ack failed for {taskType}, id {task.TaskId}");
          return;
        }

      }
      catch (Exception e)
      {
        m_logger.LogError($"ack exception for {worker.TaskDefName} : {e.Message}");
        //WorkflowTaskMetrics.ackException(worker.getTaskDefName(), e);
        return;
      }

      //Stopwatch sw = WorkflowTaskMetrics.executionTimer(worker.getTaskDefName());

      TaskResult result = null;
      try
      {

        m_logger.LogDebug($"Executing task {nameof(task)} on worker {nameof(worker)}");
        result = worker.Execute(task);
        result.WorkflowInstanceId = task.WorkflowInstanceId;
        result.TaskId = task.TaskId;
      }
      catch (Exception e)
      {
        m_logger.LogError($"Unable to execute task {task} : {e.Message}");
        if (result == null)
        {
          task.Status = Status.FAILED;
          result = new TaskResult(task);
        }
        handleException(e, result, worker, false, task);
      }
      finally
      {
        //sw.stop();
      }

      m_logger.LogDebug($"Task {task.TaskId} executed by worker {nameof(worker)} with status {task.Status}");
      UpdateWithRetry(m_updateRetryCount, task, result, worker);
    }

    private void handleException(Exception t, TaskResult result, Worker worker, bool updateTask, Task task)
    {
      //WorkflowTaskMetrics.executionException(worker.getTaskDefName(), t);
      result.Status = Status.FAILED;
      result.ReasonForIncompletion = "Error while executing the task: " + t;
      result.Logs.Add(t.StackTrace);

      UpdateWithRetry(m_updateRetryCount, task, result, worker);
    }

    private void UpdateWithRetry(int count, Task task, TaskResult result, Worker worker)
    {
      if (count < 0)
      {
        worker.OnErrorUpdate(task);
        return;
      }

      try
      {
        client.UpdateTask(result);
      }
      catch (Exception t)
      {
        //WorkflowTaskMetrics.updateTaskError(worker.getTaskDefName(), t);
        m_logger.LogError(new EventId(1), t, $"Unable to update {result} on count {count}");

        Thread.Sleep(m_sleepWhenRetry);
        UpdateWithRetry(--count, task, result, worker);

      }
    }
  }

}