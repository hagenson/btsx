using Btsx;

namespace BtsxCli
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Mail Migration Tool Command Line");
            Console.WriteLine("========================\n");

            if (args.Length < 6)
            {
                Console.WriteLine("Usage: btsxcli <srcUser> <srcServer> <srcPassword> <dstUser> <dstServer> <dstPassword>");
                return;
            }

            src = new Creds
            {
                Server = args[0],
                User = args[1],
                Password = args[2]
            };

            dst = new Creds
            {
                Server = args[3],
                User = args[4],
                Password = args[5]
            };

            try
            {
                await MigrateMailAsync();
                Console.WriteLine("\n✓ Migration completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Fatal error during migration: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }
        private static Creds? src;
        private static Creds? dst;

        static async Task MigrateMailAsync()
        {
            var mover = new MailMover();
            mover.DeleteSource = false;
            mover.FoldersOnly = false;
            mover.ReplaceExisting = false;
            mover.SourceCredentials = src;
            mover.DestCredentials = dst;
            mover.ProgressUpdates = true;
            mover.StatusUpdate += Mover_ProgressUpdate;

            await mover.ExecuteAsync(CancellationToken.None);
            if (mover.Statistics != null)
            {
                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("Migration Summary:");
                Console.WriteLine($"  Total messages processed: {mover.Statistics.TotalMessages}");
                Console.WriteLine($"  Successfully migrated: {mover.Statistics.SuccessfulMessages}");
                Console.WriteLine($"  Duplicates skipped: {mover.Statistics.SkippedMessages}");
                Console.WriteLine($"  Failed: {mover.Statistics.FailedMessages}");
                Console.WriteLine(new string('=', 60));
            }

        }

        private static void Mover_ProgressUpdate(object sender, StatusEventArgs e)
        {
            if (e.Status != null)
            {
                if (e.Type == StatusType.Error)
                    Console.Error.WriteLine(e.Status);
                else
                    Console.WriteLine(e.Status);
            }
            if (e.Percentage != percentage)
            {
                Console.WriteLine("{0}% complete.", e.Percentage);
                percentage = e.Percentage;
            }
        }

        private static int percentage;
    }
}
