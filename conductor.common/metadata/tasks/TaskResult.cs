using System.Collections.Generic;

namespace conductor.common.metadata.tasks
{
  public class TaskResult
  {
    public Status Status { get; set; }
    public List<string> Logs { get; set; }
    public Dictionary<string, object> OutputData { get; set; }
    public string WorkerId { get; set; }
    public long CallbackAfterSeconds { get; set; }
    public string ReasonForIncompletion { get; set; }
    public string TaskId { get; set; }
    public string WorkflowInstanceId { get; set; }

    public TaskResult(Task task)
    {
      Logs = new List<string>();
      WorkflowInstanceId = task.WorkflowInstanceId;
      TaskId = task.TaskId;
      ReasonForIncompletion = task.ReasonForIncompletion;
      CallbackAfterSeconds = task.CallbackAfterSeconds;
      Status = task.Status;
      WorkerId = task.WorkerId;
      OutputData = task.OutputData;
    }
  }
}