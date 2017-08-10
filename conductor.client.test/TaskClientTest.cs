using System;
using conductor.client.http;
using conductor.common.metadata.tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace conductor.client.test
{
  [TestClass]
  public class TaskClientTest
  {
    private TaskClient m_testClient;

    [TestInitialize]
    public void init()
    {
m_testClient = new TaskClient(new Uri("http://localhost:8080/api/"));
    }

    [TestMethod]
    public void Poll()
    {
      var value = m_testClient.Poll("task_1", "", "worker1", 1, 1000);
      Assert.IsNotNull(value);
    }

    [TestMethod]
    public void UpdateTaskIsWorking()
    {
      m_testClient.UpdateTask(new TaskResult(new Task {TaskId = "1", Status = Status.COMPLETED}) );
      Assert.IsTrue(true);
    }
  }
}
