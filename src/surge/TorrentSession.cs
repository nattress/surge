using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surge
{
    /// <summary>
    /// A TorrentSession object manages an active download / seeding of a torrent
    /// </summary>
    class TorrentSession
    {
        TorrentInfo _torrentInfo;

        public TorrentSession(TorrentInfo info)
        {
            _torrentInfo = info;
        }
    }
}
