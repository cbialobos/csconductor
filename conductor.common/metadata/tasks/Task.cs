using System.Collections.Generic;

namespace conductor.common.metadata.tasks
{
  public class Task
  {
    public string TaskType { get; set; }
    public Status Status { get; set; }
    public Dictionary<string, object> InputData { get; } = new Dictionary<string, object>();
    public string ReferenceTaskName { get; set; }
    public string WorkflowInstanceId { get; set; }
    public string TaskId { get; set; }
    public string ReasonForIncompletion { get; set; }
    public long CallbackAfterSeconds { get; set; }
    public Dictionary<string, object> OutputData { get; } = new Dictionary<string, object>();
    public string WorkerId { get; set; }
    public string TaskDefName { get; set; }
  }
}
