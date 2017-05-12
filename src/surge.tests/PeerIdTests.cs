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
    }
}
