using AsynchronousProgramming.Traditional.EventArgs;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace AsynchronousProgramming.Traditional
{
    public class PrimeNumberCalculator
    {
        /////////////////////////////////////////////////////////////
        #region Private delegates

        private delegate void WorkerEventHandler(int numberToCheck, AsyncOperation asyncOp);

        private SendOrPostCallback onProgressReportDelegate;
        private SendOrPostCallback onCompletedDelegate;

        #endregion

        /////////////////////////////////////////////////////////////
        #region Public events

        public delegate void ProgressChangedEventHandler(ProgressChangedEventArgs e);
        public delegate void CalculatePrimeCompletedEventHandler(object sender, CalculatePrimeCompletedEventArgs e);

        public event ProgressChangedEventHandler ProgressChanged;
        public event CalculatePrimeCompletedEventHandler CalculatePrimeCompleted;

        #endregion

        /////////////////////////////////////////////////////////////
        #region Construction and destruction

        private HybridDictionary userStateToLifetime;

        public PrimeNumberCalculator()
        {
            userStateToLifetime = new HybridDictionary();

            InitializeDelegates();
        }

        protected virtual void InitializeDelegates()
        {
            onProgressReportDelegate = new SendOrPostCallback(ReportProgress);
            onCompletedDelegate = new SendOrPostCallback(CalculateCompleted);
        }

        #endregion

        /////////////////////////////////////////////////////////////
        #region Async methods

        public virtual void CalculatePrimeAsync(int numberToTest, object taskId)
        {
            var asyncOp = AsyncOperationManager.CreateOperation(taskId);

            // Multiple threads will access the task dictionary,
            // so it must be locked to serialize access.
            lock (userStateToLifetime.SyncRoot)
            {
                if (userStateToLifetime.Contains(taskId))
                    throw new ArgumentException("Task ID parameter must be unique", "taskId");

                userStateToLifetime[taskId] = asyncOp;
            }

            var workerDelegate = new WorkerEventHandler(CalculateWorker);

            workerDelegate.BeginInvoke(numberToTest, asyncOp, null, null);
        }

        public void CancelAsync(object taskId)
        {
            var asyncOp = userStateToLifetime[taskId] as AsyncOperation;

            if (asyncOp == null)
                return;

            lock (userStateToLifetime.SyncRoot)
            {
                userStateToLifetime.Remove(taskId);
            }
        }

        #endregion

        /////////////////////////////////////////////////////////////
        #region Worker thread

        private void CalculateWorker(int numberToTest, AsyncOperation asyncOp)
        {
            var isPrime = false;
            var firstDivisor = 1;

            Exception e = null;

            if (!IsTaskCanceled(asyncOp.UserSuppliedState))
            {
                try
                {
                    var primes = BuildPrimeNumberList(numberToTest, asyncOp);

                    isPrime = IsPrime(primes, numberToTest, out firstDivisor);
                }
                catch (Exception ex)
                {
                    e = ex;
                }
            }

            var isTaskCanceled = IsTaskCanceled(asyncOp.UserSuppliedState);

            CompletionMethod(numberToTest, firstDivisor, isPrime, e, isTaskCanceled, asyncOp);
        }

        private ArrayList BuildPrimeNumberList(int numberToTest, AsyncOperation asyncOp)
        {
            ProgressChangedEventArgs e = null;

            var primes = new ArrayList();
            var firstDivisor = 1;
            var n = 5;

            primes.Add(2);
            primes.Add(3);

            while (n < numberToTest && !IsTaskCanceled(asyncOp.UserSuppliedState))
            {
                if (IsPrime(primes, n, out firstDivisor))
                {
                    var progressPercentage = (int)((float)n / (float)numberToTest * 100);

                    e = new CalculatePrimeProgressChangedEventArgs(n, progressPercentage, asyncOp.UserSuppliedState);

                    asyncOp.Post(onProgressReportDelegate, e);

                    primes.Add(n);

                    // Yield the rest of this time slice.
                    Thread.Sleep(0);
                }

                // Skip even numbers.
                n += 2;
            }

            return primes;
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

        #endregion

        /////////////////////////////////////////////////////////////
        #region Notification mechanisms

        private bool IsTaskCanceled(object taskId)
        {
            return userStateToLifetime[taskId] == null;
        }

        private void CalculateCompleted(object operationState)
        {
            var e = operationState as CalculatePrimeCompletedEventArgs;

            OnCalculatePrimeCompleted(e);
        }

        private void ReportProgress(object state)
        {
            var e = state as ProgressChangedEventArgs;

            OnProgressChanged(e);
        }

        protected void OnCalculatePrimeCompleted(
            CalculatePrimeCompletedEventArgs e)
        {
            if (CalculatePrimeCompleted != null)
            {
                CalculatePrimeCompleted(this, e);
            }
        }

        protected void OnProgressChanged(ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(e);
            }
        }

        private void CompletionMethod(int numberToTest, int firstDivisor, bool isPrime,
            Exception exception, bool canceled, AsyncOperation asyncOp)

        {
            if (!canceled)
            {
                lock (userStateToLifetime.SyncRoot)
                {
                    userStateToLifetime.Remove(asyncOp.UserSuppliedState);
                }
            }

            var e = new CalculatePrimeCompletedEventArgs(numberToTest, firstDivisor, isPrime,
                exception, canceled, asyncOp.UserSuppliedState);

            asyncOp.PostOperationCompleted(onCompletedDelegate, e);
        }

        #endregion
    }
}
