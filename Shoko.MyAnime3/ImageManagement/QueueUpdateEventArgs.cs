namespace Shoko.MyAnime3.ImageManagement
{
    public class QueueUpdateEventArgs
    {
        public readonly int queueCount;

        public QueueUpdateEventArgs(int queueCount)
        {
            this.queueCount = queueCount;
        }
    }
}