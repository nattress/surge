using Surge.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surge
{
    /// <summary>
    /// A TorrentInfo object represents logically a BitTorrent torrent metadata file's contents.
    /// That's information to find a tracker, along with the contents of the torrent.
    /// </summary>
    public class TorrentInfo
    {
        public class FileInfo
        {
            public long Length { get; private set; }
            public byte[] Md5Sum { get; private set; }
            public string Path { get; private set; }

            public FileInfo(long length, byte[] md5, string path)
            {
                Length = length;
                Md5Sum = md5;
                Path = path;
            }
        }

        public class FileInfoCollection
        {
            public List<FileInfo> Files { get; set; }
            public string FolderName { get; set; }
            public long PieceLength { get; set; }
            public List<byte[]> Hashes { get; set; }

            public FileInfoCollection()
            {
                Files = new List<FileInfo>();
                Hashes = new List<byte[]>();
            }
        }

        public Uri Announce { get; protected set; }
        public List<Uri> AnnounceList { get; protected set; }
        public string Comment { get; protected set; }
        public string Creator { get; protected set; }
        public DateTime CreationDate { get; protected set; }
        public FileInfoCollection FileInformation { get; protected set; }

        public bool HasCreationDate
        {
            get
            {
                return CreationDate != DateTime.MinValue;
            }
        }

        protected TorrentInfo()
        {
            CreationDate = DateTime.MaxValue;
            AnnounceList = new List<Uri>();
            FileInformation = new FileInfoCollection();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            //sb.Append()
            return "";
        }
    }
}
