using AsynchronousProgramming.Modern;
using AsynchronousProgramming.Traditional.EventArgs;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace AsynchronousProgramming
{
    class Program
    {
        static void Main(string[] args)
        {
            TraditionalExample()
                .Wait();

            ModernExample()
               .Wait();
        }

        #region Traditional async operations

        static async Task TraditionalExample()
        {
            Console.WriteLine("[EAP] Started");
            await EAPExample();
            Console.WriteLine("[EAP] Finished");

            Console.WriteLine("[TAP] Started");
            await TAPExample();
            Console.WriteLine("[TAP] Finished");
        }

        static async Task EAPExample()
        {
            var primeNumberCalculator = new Traditional.PrimeNumberCalculator();

            primeNumberCalculator.CalculatePrimeCompleted += new Traditional.PrimeNumberCalculator.CalculatePrimeCompletedEventHandler(CalculatePrimeCompleted);
            primeNumberCalculator.ProgressChanged += new Traditional.PrimeNumberCalculator.ProgressChangedEventHandler(ProgressChanged);

            primeNumberCalculator.CalculatePrimeAsync(1299827, Guid.NewGuid());

            /*
             * Thread synchronization done the easy way
             * 
             * We need to hang the current thread to wait for the
             * prime number calculator to finish processing
             */
            Thread.Sleep(5000);

            primeNumberCalculator.CalculatePrimeAsync(12345678, Guid.NewGuid());

            /*
             * Notice how for small numbers the wait is to high
             * and for big numbers the wait to low
             * 
             * In order to implement a better thread synchronization you will need
             * to write extra complexity which is not the purpose of this example
             */ 
            Thread.Sleep(5000);
        }

        static void CalculatePrimeCompleted(object sender, CalculatePrimeCompletedEventArgs e)
        {
            Console.WriteLine("[EAP] The number is {0}",
                e.IsPrime ? "prime" : "not prime");
        }

        static void ProgressChanged(ProgressChangedEventArgs e)
        {
            // Do something when the progress changes
        }

        static async Task TAPExample()
        {
            //var primeNumberCalculator = new Modern.PrimeNumberCalculator(displayProgress: false);
            var primeNumberCalculator = new Modern.PrimeNumberCalculator(displayProgress: true);

            Console.WriteLine("[TAP] The first number is {0}",
                await primeNumberCalculator.CalculatePrimeAsync(1299827) ? "prime" : "not prime");

            Console.WriteLine("[TAP] The second number is {0}",
                await primeNumberCalculator.CalculatePrimeAsync(1234567) ? "prime" : "not prime");
        }

        #endregion

        #region Modern async operations

        const string FILE = "./index.html";
        const string WEB_PAGE = "http://laurentiu.microsoft.pub.ro/";

        static async Task ModernExample()
        {
            await BasicModernExamples();
            await TaskVsVoid.TryAsyncOperations();
        }

        static async Task BasicModernExamples()
        {
            var webClient = new WebClient();
            var fileIO = new FileIO();

            var blogData = await webClient.GetHtmlAsync(new Uri(WEB_PAGE));

            await fileIO.WriteToFileAsync(FILE, blogData);

            var fileData = await fileIO.ReadFromFileAsync(FILE);

            Console.WriteLine(fileData);
        }

        #endregion
    }
}
