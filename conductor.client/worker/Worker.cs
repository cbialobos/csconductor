using System;
using System.Net;
using conductor.common.metadata.tasks;

namespace conductor.client.worker
{
  public abstract class Worker
  {
    public abstract string TaskDefName { get; }
   public abstract TaskResult Execute(Task task);
    public virtual bool PreAck(Task task)
    {
      return true;
    }
    public virtual void OnErrorUpdate(Task task) { }
    public virtual bool IsPaused => false;
    public virtual string Identity => Dns.GetHostName();
    public virtual int PollCount => 1;
    public virtual TimeSpan PollingInterval => TimeSpan.FromSeconds(1);
    public virtual int LongPollTimeoutInMs => 100;
  }
}
