using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

namespace Budget
{

    /// <summary>
    /// Provides file-related utility functions for the Budget project.
    /// </summary>
    internal class BudgetFiles
    {
        // ====================================================================
        // verify that the name of the file exists and is it readable?
        // throws System.IO.FileNotFoundException if file does not exist
        // ====================================================================
        // TODO Does not actually check if file is readable
        /// <summary>
        /// Verifies that a specified file exists and throws if it does not.
        /// </summary>
        /// <param name="FilePath">Path of file to verify.</param>
        /// <exception cref="FileNotFoundException">Thrown when file does not
        /// exist.</exception>
        /// <example>
        /// This example gets a path to a readable file from the user:
        /// <code><![CDATA[
        /// string filepath = null;
        ///
        /// while (true) {
        /// 	try {
        /// 		Console.WriteLine("Enter a readable file path:");
        /// 		filepath = Console.ReadLine();
        /// 		BudgetFiles.VerifyReadFromFileName(filepath);
        /// 		break;
        /// 	} catch { }
        ///
        /// 	Console.WriteLine("File path " + filepath + " was not readable.");
        /// }
        /// ]]></code>
        /// </example>
        public static void VerifyReadFromFileName(String FilePath)
        {
            // ---------------------------------------------------------------
            // does FilePath exist?
            // ---------------------------------------------------------------
            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException("ReadFromFileException: FilePath (" + FilePath + ") does not exist");
            }
        }

        // ====================================================================
        // verify that the name of the file exists, or set the default file, and 
        // is it writable
        // ====================================================================
        /// <summary>
        /// Verifies that a specified file is writable and throws if it is not.
        /// </summary>
        /// <param name="FilePath">Path of file to verify.</param>
        /// <exception cref="Exception">
        /// Thrown when file is not writable for any reason.
        /// </exception>
        /// <example>
        /// This example verifies the writability of a user-specified file:
        /// <code><![CDATA[
        /// string filepath = null;
        ///
        /// while (true) {
        /// 	try {
        /// 		Console.WriteLine("Enter a writable file path:");
        /// 		filepath = Console.ReadLine();
        /// 		BudgetFiles.VerifyWriteToFileName(filepath);
        /// 		break;
        /// 	} catch { }
        ///
        /// 	Console.WriteLine("File path " + filepath + " was not writable.");
        /// }
        /// ]]></code>
        /// </example>
        public static void VerifyWriteToFileName(String FilePath)
        {
            // ---------------------------------------------------------------
            // does directory where you want to save the file exist?
            // ... this is possible if the user is specifying the file path
            // ---------------------------------------------------------------
            String folder = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(folder))
            {
                throw new Exception("SaveToFileException: FilePath (" + FilePath + ") does not exist");
            }

            // ---------------------------------------------------------------
            // can we write to it?
            // ---------------------------------------------------------------
            if (File.Exists(FilePath))
            {
                FileAttributes fileAttr = File.GetAttributes(FilePath);
                if ((fileAttr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    throw new Exception("SaveToFileException:  FilePath(" + FilePath + ") is read only");
                }
            }
        }



    }
}
