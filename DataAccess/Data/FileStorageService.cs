using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public class FileStorageService
    {
        public static string GetSharedCsvFolderPath()
        {
            // Get the solution directory (or a common root directory)
            string solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;

            // Define the path to the shared folder
            string sharedFolderPath = Path.Combine(solutionDirectory, "DataAccess/SharedFiles");
            // Ensure the directory exists
            if (!Directory.Exists(sharedFolderPath))
            {
                Directory.CreateDirectory(sharedFolderPath);
            }

            return sharedFolderPath;
        }
    }
}
