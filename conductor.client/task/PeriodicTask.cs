using System;
using System.Threading;
using System.Threading.Tasks;

namespace conductor.client.task
{
  public class PeriodicTask
  {
    public static async Task Run(Action action, TimeSpan period, CancellationToken cancellationToken)
    {
      while (!cancellationToken.IsCancellationRequested)
      {
        await Task.Delay(period, cancellationToken);

        if (!cancellationToken.IsCancellationRequested)
          action();
      }
    }
  }
}