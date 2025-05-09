namespace Liman.ConsoleExample
{
    [LimanService(LimanServiceLifetime.Application)]
    internal class MyApplicationService : ILimanRunnable
    {
        private readonly IMyService service;
        private bool isRunning = true;

        public MyApplicationService(IMyService service)
        {
            this.service = service;
        }

        public void Run()
        {
            Console.WriteLine("Press 'Enter' to do something, or type 'quit' to exit.");

            while (isRunning)
            {
                var line = Console.ReadLine();

                if (line == "quit")
                {
                    isRunning = false;
                }
                else
                {
                    service.DoSomething();
                }
            }
        }
    }
}
