using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Surge.Util;

namespace Surge
{
    public class InvalidTorrentMetadataException : Exception
    {

    }

    public class TorrentFile : TorrentInfo
    {
        public static TorrentInfo CreateFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("Torrent file could not be found: {0}", filePath));
            }
            
            var data = Bencoding.ParseStream(File.OpenRead(filePath));
            var torrentInfo = new TorrentFile();

            if (data.Tokens.First().Type != BencodingType.Dictionary)
            {
                throw new InvalidTorrentMetadataException();
            }

            var dict = ((BencodedDictionary)data.Tokens.First()).Data;
            if (!dict.ContainsKey("announce") || dict["announce"].Type != BencodingType.String)
            {
                throw new InvalidTorrentMetadataException();
            }

            torrentInfo.Announce = new Uri(((BencodedString)dict["announce"]).DataAsString);

            if (dict.ContainsKey("announce-list"))
            {
                var announceList = dict["announce-list"].AsList();

                foreach (var item in announceList)
                {
                    torrentInfo.AnnounceList.Add(new Uri(item.AsList()[0].AsString()));
                }
            }

            if (dict.ContainsKey("comment"))
            {
                torrentInfo.Comment = dict["comment"].AsString();
            }

            if (dict.ContainsKey("created by"))
            {
                torrentInfo.Creator = dict["created by"].AsString();
            }

            if (dict.ContainsKey("creation date"))
            {
                torrentInfo.CreationDate = ((long)dict["creation date"].AsInteger()).FromUnixTime();
            }

            if (dict.ContainsKey("info") && dict["info"].Type == BencodingType.Dictionary)
            {
                var infoDictionary = dict["info"].AsDictionary();

                if (infoDictionary.ContainsKey("length"))
                {
                    torrentInfo.FileInformation.Files.Add(ParseFileInfo(infoDictionary, true));
                }
                else if (infoDictionary.ContainsKey("files") && infoDictionary["files"].Type == BencodingType.List)
                {
                    var fileList = infoDictionary["files"].AsList();

                    foreach (var file in fileList)
                    {
                        Debug.Assert(file.Type == BencodingType.Dictionary);
                        torrentInfo.FileInformation.Files.Add(ParseFileInfo(file.AsDictionary(), false));
                    }
                }
                else
                {
                    throw new InvalidTorrentMetadataException();
                }

                torrentInfo.FileInformation.PieceLength = infoDictionary["piece length"].AsInteger();

                var pieces = infoDictionary["pieces"].AsData();
                Debug.Assert(pieces.Length % 20 == 0);

                for (int i = 0; i < pieces.Length; i += 20)
                {
                    torrentInfo.FileInformation.Hashes.Add(new ArraySegment<byte>(pieces, i, 20).ToArray());
                }
            }
            else
            {
                throw new InvalidTorrentMetadataException();
            }
            
            return torrentInfo;
        }

        static FileInfo ParseFileInfo(Dictionary<string, BencodedData> dictionary, bool singleFile)
        {
            long length = dictionary["length"].AsInteger();
            byte[] md5 = { };
            if (dictionary.ContainsKey("md5sum"))
            {
                md5 = dictionary["md5sum"].AsData();
            }

            string name = string.Empty;
            if (singleFile)
            {
                name = dictionary["name"].AsString();
            }
            else
            {
                name = dictionary["path"].AsList()[0].AsString();
            }

            return new FileInfo(length, md5, name);
        }
    }
    
}
