using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Surge;
using Xunit;

namespace SurgeTests
{
    public class BencodingTests
    {
        private void AssertIsInteger(BencodedData data, int value)
        {
            Assert.True(data.Type == BencodingType.Integer);
            BencodedInteger dataAsInteger = data as BencodedInteger;
            Assert.NotNull(dataAsInteger);
            Assert.True(dataAsInteger.Data == value);
        }

        private void AssertIsString(BencodedData data, string value)
        {
            Assert.True(data.Type == BencodingType.String);
            BencodedString dataAsString = data as BencodedString;
            Assert.NotNull(dataAsString);
            Assert.True(dataAsString.DataAsString.Equals(value));
        }

        [Fact]
        public void ParseString1()
        {
            var bencoding = Bencoding.ParseStream(new MemoryStream(Encoding.UTF8.GetBytes("4:snap")));
            
            List<BencodedData> result = new List<BencodedData>(bencoding.Tokens);

            Assert.True(result.Count == 1);
            AssertIsString(result[0], "snap");
        }

        [Fact]
        public void ParseStringNonAscii()
        {
            byte[] input = new byte[] { 52, 58, 4, 255, 254, 253 };
            var bencoding = Bencoding.ParseStream(new MemoryStream(input));
            List<BencodedData> result = new List<BencodedData>(bencoding.Tokens);

            Assert.True(result.Count == 1);
            Assert.True(result[0].Type == BencodingType.String);
            BencodedString dataAsString = result[0] as BencodedString;
            Assert.NotNull(dataAsString);
            Assert.Equal(new byte[] { 4, 255, 254, 253 }, dataAsString.Data);
        }

        [Fact]
        public void ParseStringTooShort()
        {
            Assert.Throws(typeof(BencodingException), () => 
            {
                Bencoding.ParseStream(new MemoryStream(Encoding.UTF8.GetBytes("4:sna")));
            });
        }

        [Fact]
        public void ParsePositiveInteger()
        {
            var bencoding = Bencoding.ParseStream(new MemoryStream(Encoding.UTF8.GetBytes("i234e")));

            List<BencodedData> result = new List<BencodedData>(bencoding.Tokens);

            Assert.True(result.Count == 1);
            AssertIsInteger(result[0], 234);
        }

        [Fact]
        public void ParseNegativeInteger()
        {
            var bencoding = Bencoding.ParseStream(new MemoryStream(Encoding.UTF8.GetBytes("i-234e")));

            List<BencodedData> result = new List<BencodedData>(bencoding.Tokens);

            Assert.True(result.Count == 1);
            AssertIsInteger(result[0], -234);
        }

        [Theory]
        [InlineData("i-e")]
        [InlineData("ie")]
        [InlineData("i3")]
        public void ParseIntegerNegativeTest(string data)
        {
            Assert.Throws(typeof(BencodingException), () => Bencoding.ParseStream(new MemoryStream(Encoding.UTF8.GetBytes(data))));
        }

        [Fact]
        public void ParseList()
        {
            var bencoding = Bencoding.ParseStream(new MemoryStream(Encoding.UTF8.GetBytes("li123ei-234e5:helloe")));

            List<BencodedData> result = new List<BencodedData>(bencoding.Tokens);

            Assert.True(result.Count == 1);
            Assert.True(result[0].Type == BencodingType.List);
            BencodedList dataAsList = result[0] as BencodedList;
            Assert.NotNull(dataAsList);
            Assert.True(dataAsList.Data.Count == 3);


            AssertIsInteger(dataAsList.Data[0], 123);
            AssertIsInteger(dataAsList.Data[1], -234);
            AssertIsString(dataAsList.Data[2], "hello");
        }

        [Fact]
        public void ParseEmptyList()
        {
            var bencoding = Bencoding.ParseStream(new MemoryStream(Encoding.UTF8.GetBytes("le")));

            List<BencodedData> result = new List<BencodedData>(bencoding.Tokens);

            Assert.True(result.Count == 1);
            Assert.True(result[0].Type == BencodingType.List);
            BencodedList dataAsList = result[0] as BencodedList;
            Assert.NotNull(dataAsList);
            Assert.True(dataAsList.Data.Count == 0);
        }

        [Fact]
        public void ParseListNested()
        {
            var bencoding = Bencoding.ParseStream(new MemoryStream(Encoding.UTF8.GetBytes("llli123ei-234e5:helloeee")));

            List<BencodedData> result = new List<BencodedData>(bencoding.Tokens);

            Assert.True(result.Count == 1);
            Assert.True(result[0].Type == BencodingType.List);
            BencodedList dataAsList = result[0] as BencodedList;
            Assert.NotNull(dataAsList);
            Assert.True(dataAsList.Data.Count == 1);
            Assert.True(dataAsList.Data[0].Type == BencodingType.List);
            BencodedList nestedList1 = dataAsList.Data[0] as BencodedList;
            Assert.NotNull(nestedList1);
            Assert.True(nestedList1.Data.Count == 1);
            Assert.True(nestedList1.Data[0].Type == BencodingType.List);
            BencodedList nestedList2 = nestedList1.Data[0] as BencodedList;
            Assert.NotNull(nestedList2);
            Assert.True(nestedList2.Data.Count == 3);

            AssertIsInteger(nestedList2.Data[0], 123);
            AssertIsInteger(nestedList2.Data[1], -234);
            AssertIsString(nestedList2.Data[2], "hello");
        }

        [Fact]
        public void ParseDict()
        {
            var bencoding = Bencoding.ParseStream(new MemoryStream(Encoding.UTF8.GetBytes("d5:Simon4:Cool5:Edrea4:Cutee")));

            List<BencodedData> result = new List<BencodedData>(bencoding.Tokens);

            Assert.True(result.Count == 1);
            Assert.True(result[0].Type == BencodingType.Dictionary);
            BencodedDictionary dataAsDict = result[0] as BencodedDictionary;
            Assert.NotNull(dataAsDict);
            Assert.True(dataAsDict.Data.Count == 2);

            var dict = dataAsDict.Data;

            Assert.True(dict.ContainsKey("Simon"));
            AssertIsString(dict["Simon"], "Cool");

            Assert.True(dict.ContainsKey("Edrea"));
            AssertIsString(dict["Edrea"], "Cute");
        }
    }
}
