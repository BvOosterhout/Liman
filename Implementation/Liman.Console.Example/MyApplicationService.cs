namespace Liman.ConsoleExample
{
    [LimanImplementation(LimanImplementationLifetime.Application)]
    internal class MyApplicationService : ILimanRunnable
    {
        private readonly IMyService service;

        public MyApplicationService(IMyService service)
        {
            this.service = service;
        }

        public void Run()
        {
            Console.WriteLine("Press 'Enter' to do something, or type 'quit' to exit.");

            while (true)
            {
                var line = Console.ReadLine();

                if (line == "quit")
                {
                    return;
                }
                else
                {
                    service.DoSomething();
                }
            }
        }

        public void Stop()
        {
            throw new NotSupportedException();
        }
    }
}
