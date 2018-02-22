using System.IO;
using System.Threading.Tasks;

namespace AsynchronousProgramming.Modern
{
    public class FileIO
    {
        public async Task<string> ReadFromFileAsync(string filePath)
        {
            using (var fileStream = new StreamReader(filePath))
            {
                return await fileStream.ReadToEndAsync();
            }
        }

        public async Task WriteToFileAsync(string filePath, string data)
        {
            using (var fileStream = new StreamWriter(filePath))
            {
                await fileStream.WriteLineAsync(data);
            }
        }
    }
}
