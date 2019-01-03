using System.IO;
using System.Threading;

namespace QuikDB.Util
{
    class QuikDBFileHelper
    {

        public static void createInitialDirectories()
        {
            Directory.CreateDirectory(@"C:\QuikDB\log");
            Directory.CreateDirectory(@"C:\QuikDB\backups");
            Directory.CreateDirectory(@"C:\QuikDB\temp");

            Thread.Sleep(1500);

        }
    }
}
