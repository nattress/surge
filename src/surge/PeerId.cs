using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surge
{
    public class PeerId
    {
        private static readonly string RandomCharacterSource = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.-";
        private static readonly string ClientId = "Sg-";

        // Todo: Read from config
        private static readonly string VersionNumber = "0.0.1";

        private static string s_peerId;

        public static string Id
        {
            get
            {
                if (string.IsNullOrEmpty(s_peerId))
                {
                    StringBuilder sb = new StringBuilder();
                    
                    sb.Append(ClientId + VersionNumber);

                    int numberOfRandomCharactersNeeded = 20 - sb.Length;
                    var random = new Random();

                    for (int i = 0; i < numberOfRandomCharactersNeeded; i++)
                    {
                        sb.Append(RandomCharacterSource.Substring(random.Next(RandomCharacterSource.Length), 1));
                    }

                    s_peerId = sb.ToString();
                }

                return s_peerId;
            }
        }
    }
}
