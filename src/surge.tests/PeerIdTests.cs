using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Surge;
using Xunit;

namespace SurgeTests
{
    public class PeerIdTests
    {
        [Fact]
        public void PeerIdLength()
        {
            Assert.True(PeerId.Id.Length == 20);
        }

        [Fact]
        public void PeerIdStability()
        {
            // PeerId.Id should be the same for a session
            var id1 = PeerId.Id;
            var id2 = PeerId.Id;

            Assert.True(id1 == id2);
        }
    }
}
