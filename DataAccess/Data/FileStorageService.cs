using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public class FileStorageService
    {
        private readonly IConfiguration _config;
        public FileStorageService(IConfiguration configuration)
        {
            _config = configuration;
        }
        public string GetSharedCsvFolderPath()
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
