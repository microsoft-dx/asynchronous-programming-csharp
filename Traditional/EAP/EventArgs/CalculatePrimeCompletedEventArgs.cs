using System;
using System.ComponentModel;

namespace AsynchronousProgramming.Traditional.EventArgs
{
    public class CalculatePrimeCompletedEventArgs : AsyncCompletedEventArgs
    {
        private int numberToTestValue = 0;
        private int firstDivisorValue = 1;
        private bool isPrimeValue;

        public CalculatePrimeCompletedEventArgs(
            int numberToTest,
            int firstDivisor,
            bool isPrime,
            Exception e,
            bool canceled,
            object state) : base(e, canceled, state)
        {
            numberToTestValue = numberToTest;
            firstDivisorValue = firstDivisor;
            isPrimeValue = isPrime;
        }

        public int NumberToTest
        {
            get
            {
                RaiseExceptionIfNecessary();

                return numberToTestValue;
            }
        }

        public int FirstDivisor
        {
            get
            {
                RaiseExceptionIfNecessary();

                return firstDivisorValue;
            }
        }

        public bool IsPrime
        {
            get
            {
                RaiseExceptionIfNecessary();

                return isPrimeValue;
            }
        }
    }
}
