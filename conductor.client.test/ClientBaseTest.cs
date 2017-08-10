using System;
using System.Collections.Generic;
using conductor.common.metadata.tasks;
using evs.conductor.client.http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace conductor.client.test
{
  [TestClass]
  public class ClientBaseTest : ClientBase
  {
    [TestMethod]
    public void TestMethod1()
    {
      var result = GetForEntity<List<Task>>("tasks/poll/batch/{taskType}", new Dictionary<string, object> { { "taskType", "task_1" } });
      Assert.IsNotNull(result);
    }

    public ClientBaseTest() : base(new Uri("http://localhost:8080/api/")) { }
  }
}
