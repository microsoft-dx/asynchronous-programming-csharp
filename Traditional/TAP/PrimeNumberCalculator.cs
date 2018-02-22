using System;
using System.Collections;
using System.Threading.Tasks;

namespace AsynchronousProgramming.Modern
{
    public class PrimeNumberCalculator
    {
        private bool displayProgress = false;
        private volatile int lastPrimeNumber = -1;

        public PrimeNumberCalculator(bool displayProgress)
        {
            this.displayProgress = displayProgress;
        }

        public Task<bool> CalculatePrimeAsync(int numberToTest)
        {
            var firstDivisor = 1;

            var task = Task
                .FromResult(BuildPrimeNumberList(numberToTest))
                .ContinueWith((prevTask) => IsPrime(prevTask.Result, numberToTest, out firstDivisor));

            return task;
        }

        private bool IsPrime(ArrayList primes, int n, out int firstDivisor)
        {
            var foundDivisor = false;
            var exceedsSquareRoot = false;

            var i = 0;
            var divisor = 0;
            firstDivisor = 1;

            while ((i < primes.Count) && !foundDivisor && !exceedsSquareRoot)
            {
                divisor = (int)primes[i++];

                if (divisor * divisor > n)
                {
                    exceedsSquareRoot = true;
                }
                else if (n % divisor == 0)
                {
                    firstDivisor = divisor;
                    foundDivisor = true;
                }
            }

            return !foundDivisor;
        }

        private ArrayList BuildPrimeNumberList(int numberToTest)
        {
            var primes = new ArrayList();
            var firstDivisor = 1;
            var n = 5;

            primes.Add(2);
            primes.Add(3);

            while (n < numberToTest)
            {
                if (IsPrime(primes, n, out firstDivisor))
                {
                    var progressPercentage = (int)((float)n / (float)numberToTest * 100);

                    lastPrimeNumber = n;

                    if (displayProgress)
                    {
                        Console.WriteLine("[TAP] Progress: {0}%", progressPercentage);
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                    }

                    primes.Add(n);
                }

                // Skip even numbers.
                n += 2;
            }

            return primes;
        }
    }
}
