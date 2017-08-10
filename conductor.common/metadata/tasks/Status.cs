namespace conductor.common.metadata.tasks
{
  public class Status
  {
    public bool Terminal { get; }
    public bool Successful { get; }
    public bool Retriable { get; }

    private Status(bool terminal, bool successful, bool retriable)
    {
      Terminal = terminal;
      Successful = successful;
      Retriable = retriable;
    }

    public static Status IN_PROGRESS = new Status(false, true, true);
    public static Status CANCELED = new Status(true, false, false);
    public static Status FAILED = new Status(true, false, true);
    public static Status COMPLETED = new Status(true, true, true);
    public static Status COMPLETED_WITH_ERRORS = new Status(true, true, true);
    public static Status SCHEDULED = new Status(false, true, true);
    public static Status TIMED_OUT = new Status(true, false, true);
    public static Status READY_FOR_RERUN = new Status(false, true, true);
    public static Status SKIPPED = new Status(true, true, false);
  }
}