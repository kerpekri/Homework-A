using System;
using System.IO;
using System.Net;
using Atea.Framework.Util;
using Microsoft.Extensions.Logging;

namespace Atea.Framework
{
    /// <summary>
    /// File repository handling. Note: A repository consists of text files with integer Id's as file names.
    /// </summary>
    public class FileRepository
    {
        /*
         Refactoring comments:
         Constructor added (workingDirectory and log can only be set via the constructor!).
         ReadResult is optional.
         ReadFromFile returns the file contents.
         WriteToFile returns no result (before it returned the filename!).
         UpdateFile is deprecated (use WriteToFile instead).
         */

        protected string m_workingDirectory;
        protected ILogger m_log;

        /// <summary>
        /// Optional event returning the contents of the repository file when reading from.
        /// </summary>
        public event EventHandler<EventArgs<string>> ReadResult;

        /// <summary>
        /// Create an instance of a file repository in the specified folder.
        /// </summary>
        /// <param name="workingDirectory">The folder where repository files are located.</param>
        /// <param name="log">Trace logger.</param>
        public FileRepository(string workingDirectory, ILogger log)
        {
            m_workingDirectory = workingDirectory;
            m_log = log;
        }

        /// <summary>
        /// Loads the specified repository file.
        /// </summary>
        /// <param name="id">Id of the repository file.</param>
        /// <returns>Contents of the repository file.</returns>
        public string ReadFromFile(int id)
        {
            string filename = GetRepositoryFileName(id);

            m_log.LogInformation($"Reading from file: {filename}");
            string contents = File.ReadAllText(filename);

            ReadResult?.Invoke(this, new EventArgs<string>(contents));
            return contents;
        }

        /// <summary>
        /// Writes to the specified repository file (overwrites any existings contents).
        /// </summary>
        /// <param name="id">Id of the repository file.</param>
        /// <param name="contents">Contents of the repository file.</param>
        public void WriteToFile(int id, string contents)
        {
            string filename = GetRepositoryFileName(id);
            m_log.LogInformation($"Writing to file: {filename}");
            File.WriteAllText(filename, contents);
        }

        /// <summary>
        /// Deletes the specified repository file.
        /// </summary>
        /// <param name="id">Id of the repository file.</param>
        /// <returns>True if the file has been deleted or did not exist. Exception if the file can not be deleted.</returns>
        public bool DeleteFile(int id)
        {
            try
            {
                string filename = GetRepositoryFileName(id);
                m_log.LogInformation($"Deleting file: {filename}");
                File.Delete(filename);

                return true;
            }
            catch (Exception ex)
            {
                // Note: KKE: We usually had a custom WebApiException, that we could throw with status code.
                // and show the user userMessage(2 param) to users and inner exception was more for us(3 param)
                throw new WebApiException(HttpStatusCode.InternalServerError, "Could not delete file!", ex);
            }
        }

        protected string GetRepositoryFileName(int id)
        {
            if (id < 0)
            {
                throw new ArgumentException($"Repository Id out of range: {id}");
            }

            string fileName = Path.Combine(m_workingDirectory, $"{id}.txt");

            if (File.Exists(fileName) == false) throw new FileNotFoundException("Requested file doesn't exist in the directory");

            return fileName;
        }

        [Obsolete("This method is obsolete. Use WriteToFile instead.", false)]
        public void UpdateFile(int id, string contents)
        {
            WriteToFile(id, contents);
            ReadResult?.Invoke(this, new EventArgs<string>(contents));
        }
    }
}
