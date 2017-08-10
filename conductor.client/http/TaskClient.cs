using System;
using System.Collections.Generic;
using conductor.common.metadata.tasks;
using evs.conductor.client.http;

namespace conductor.client.http
{
  public class TaskClient : ClientBase
  {
    public TaskClient(Uri root) : base(root)
    {
    }

    public object pollForTask(string taskType, string hostname)
    {
      throw new NotImplementedException();
    }

    public List<Task> Poll(string taskType, string domain, string workerId, int count, int timeoutInMillisecond)
    {
      var param = new Dictionary<string, object>
      { {"taskType", taskType},
        { "workerid", workerId},
        //{ "count", count},
        //{ "timeout", timeoutInMillisecond},
        //{ "domain", domain}
      };
      return GetForEntity<List<Task>>("tasks/poll/batch/{taskType}{?workerid,count,timeout,domain}", param);
    }

    public void UpdateTask(TaskResult task)
    {
      PostForEntity("tasks", task);
    }

    public bool Ack(string taskId, string workerId)
    {
      var param = new Dictionary<string, object>
      { {"taskId", taskId},
        { "workerid", workerId}
      };
      return PostForEntity<object, bool> ("tasks/{taskId}/ack", param, null);
    }
  }
}