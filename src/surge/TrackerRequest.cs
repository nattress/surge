using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surge
{
    enum TrackerRequestEvent
    {
        Started,
        Stopped,
        Completed
    }

    class TrackerRequest
    {
        readonly string InfoHash;
        string PeerId;
        int Port;
        int UploadedBytes;
        int DownloadedBytes;
        int RemainingBytes;
        TrackerRequestEvent Event;
    }
}
