namespace RemoveEntity.Sample;

public static class Program
{
    public static void Main(string[] args)
    {
        Task.Factory.StartNew(Consumer.Execute, TaskCreationOptions.LongRunning);
        Thread.Sleep(500);

        Producer.Execute();
    }
}