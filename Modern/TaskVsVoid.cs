using System;
using System.Threading.Tasks;

namespace AsynchronousProgramming.Modern
{
    public class TaskVsVoid
    {
        private static async void AsyncVoid()
        {
            // This exception is not caught
            throw new NotImplementedException("AsyncVoid");
        }

        private static async Task AsyncTask()
        {
            throw new NotImplementedException("AsyncTask");
        }

        public static async Task TryAsyncOperations()
        {
            try
            {
                AsyncVoid();
                await AsyncTask();
            }
            catch (Exception e)
            {
                // This is the AsyncTask exception
                Console.WriteLine(e.Message);
            }
        }
    }
}
