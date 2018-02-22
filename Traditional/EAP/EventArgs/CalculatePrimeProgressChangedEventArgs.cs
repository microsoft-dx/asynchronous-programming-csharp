using System.ComponentModel;

namespace AsynchronousProgramming.Traditional.EventArgs
{
    public class CalculatePrimeProgressChangedEventArgs : ProgressChangedEventArgs
    {
        private int latestPrimeNumberValue = 1;

        public CalculatePrimeProgressChangedEventArgs(
            int latestPrime,
            int progressPercentage,
            object userToken) : base(progressPercentage, userToken)
        {
            latestPrimeNumberValue = latestPrime;
        }

        public int LatestPrimeNumber
        {
            get
            {
                return latestPrimeNumberValue;
            }
        }
    }
}
