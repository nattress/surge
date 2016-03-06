using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Surge
{
    enum BencodingType
    {
        Dictionary,
        List,
        Integer,
        String
    }

    internal class BencodingException : Exception
    {
        public BencodingException(string message) : base(message) {}
    }

    abstract internal class BencodedData
    {
        public abstract BencodingType Type { get; }

        public long AsInteger()
        {
            if (!(this is BencodedInteger))
            {
                throw new BencodingException("Invalid cast");
            }
            return ((BencodedInteger)this).Data;
        }

        public string AsString()
        {
            if (!(this is BencodedString))
            {
                throw new BencodingException("Invalid cast");
            }
            return ((BencodedString)this).DataAsString;
        }

        public byte[] AsData()
        {
            if (!(this is BencodedString))
            {
                throw new BencodingException("Invalid cast");
            }
            return ((BencodedString)this).Data;
        }

        public List<BencodedData> AsList()
        {
            if (!(this is BencodedList))
            {
                throw new BencodingException("Invalid cast");
            }
            return ((BencodedList)this).Data;
        }

        public Dictionary<string, BencodedData> AsDictionary()
        {
            if (!(this is BencodedDictionary))
            {
                throw new BencodingException("Invalid cast");
            }
            return ((BencodedDictionary)this).Data;
        }
    }

    internal class BencodedString : BencodedData
    {
        public override BencodingType Type
        {
            get { return BencodingType.String; }
        }

        public override string ToString()
        {
            return DataAsString;
        }

        public BencodedString(byte[] data)
        {
            _data = data;
        }

        public byte[] Data
        {
            get
            {
                return _data;
            }
        }

        public string DataAsString
        {
            get
            {
                return Encoding.UTF8.GetString(_data);
            }
        }

        byte[] _data;
    }

    internal class BencodedInteger : BencodedData
    {
        public override BencodingType Type
        {
            get { return BencodingType.Integer; }
        }

        public override string ToString()
        {
            return _data.ToString();
        }

        public BencodedInteger(long integer)
        {
            _data = integer;
        }

        public long Data
        {
            get
            {
                return _data;
            }
        }

        long _data;
    }

    class BencodedList : BencodedData
    {
        public override BencodingType Type
        {
            get { return BencodingType.List; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[");
            foreach (var item in _data)
            {
                sb.AppendLine(item.ToString() + ",");
            }
            sb.AppendLine("]");

            return sb.ToString();
        }

        public BencodedList(List<BencodedData> data)
        {
            _data = data;
        }

        public List<BencodedData> Data
        {
            get
            {
                return _data;
            }
        }
        List<BencodedData> _data;
    }

    class BencodedDictionary : BencodedData
    {
        public override BencodingType Type
        {
            get { return BencodingType.Dictionary; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            foreach (var item in _data)
            {
                sb.AppendLine(item.Key + " => " + item.Value.ToString() + ",");
            }
            sb.AppendLine("}");

            return sb.ToString();
        }

        public BencodedDictionary(Dictionary<string, BencodedData> data)
        {
            _data = data;
        }

        public Dictionary<string, BencodedData> Data
        {
            get
            {
                return _data;
            }
        }

        Dictionary<string, BencodedData> _data;
    }

    internal class Bencoding
    {
        List<BencodedData> _tokens;

        byte[] _encodedData;
        int _position;

        private Bencoding(BinaryReader binaryStream)
        {
            _tokens = new List<BencodedData>();
            _encodedData = binaryStream.ReadBytes((int)binaryStream.BaseStream.Length);
        }
        
        private long ParseUnsignedInt()
        {
            byte[] numbers = Encoding.UTF8.GetBytes("0123456789");
            
            string number = "";
            while (_position < _encodedData.Length && numbers.Contains(_encodedData[_position]))
            {
                number += (char)_encodedData[_position++];
            }

            if (number == "")
            {
                // Didn't parse any numeric characters before encountering end of string or an invalid character
                if (_position >= _encodedData.Length)
                    throw new BencodingException("Expected integer but encountered end of data.");
                else
                    throw new BencodingException("Expected integer but encountered an invalid character.");
            }

            long intResult = 0;
            if (!long.TryParse(number, out intResult))
            {
                throw new BencodingException("Integer could not be parsed: " + number);
            }
            
            return intResult;
        }

        private void CheckEndOfString()
        {
            if (_position >= _encodedData.Length)
                throw new BencodingException("Unexpected end of string.");
        }

        /// <summary>
        /// Parse a string of the form &lt;length&gt;:&lt;data&gt;.  Ie, 5:hello.  The data is UTF8 encoded if strings, otherwise can be any binary data.
        /// </summary>
        private BencodedString ParseString()
        {
            // Read the numbers
            int stringLength = (int)ParseUnsignedInt();

            // Expect a colon
            CheckEndOfString();
            if (_encodedData[_position++] != ':')
            {
                throw new BencodingException("Expected string length separator colon.");
            }

            byte[] data = new byte[stringLength];
            if (_position + stringLength > _encodedData.Length)
            {
                throw new BencodingException("Invalid string. Length of string exceeds length of input data.");
            }
            Buffer.BlockCopy(_encodedData, _position, data, 0, stringLength);
            _position += stringLength;
            return new BencodedString(data);
        }

        /// <summary>
        /// Parse an integer of the form i[-][0-9]+e
        /// </summary>
        private BencodedInteger ParseInteger()
        {
            // Read the leading i
            if ((char)_encodedData[_position++] != 'i')
            {
                throw new BencodingException("Expected beginning sentinel character for integer.");
            }

            CheckEndOfString();

            // Allow for negative numbers
            int negation = 1;
            if ((char)_encodedData[_position] == '-')
            {
                _position++;
                negation = -1;
            }

            // Numbers must have at least one digit
            CheckEndOfString();
            long number = ParseUnsignedInt() * negation;

            CheckEndOfString();
            if ((char)_encodedData[_position++] != 'e')
            {
                throw new BencodingException("Expected ending sentinel character for integer.");
            }

            return new BencodedInteger(number);
        }

        private BencodedList ParseList()
        {
            // Read the leading l
            if ((char)_encodedData[_position++] != 'l')
            {
                throw new BencodingException("Expected beginning sentinel character for list.");
            }

            CheckEndOfString();
            var list = new List<BencodedData>();
            while ((char)_encodedData[_position] != 'e')
            {
                list.Add(ParseAnyType());

                CheckEndOfString();
            }

            _position++;
            return new BencodedList(list);
        }

        private BencodedDictionary ParseDictionary()
        {
            // Read the leading d
            if ((char)_encodedData[_position++] != 'd')
            {
                throw new BencodingException("Expected beginning sentinel character for dictionary.");
            }

            CheckEndOfString();

            var dict = new Dictionary<string, BencodedData>();
            while ((char)_encodedData[_position] != 'e')
            {
                var key = ParseString();
                CheckEndOfString();
                var value = ParseAnyType();

                dict.Add(key.DataAsString, value);
                CheckEndOfString();
            }

            _position++;

            return new BencodedDictionary(dict);
        }

        private BencodedData ParseAnyType()
        {
            switch ((char)_encodedData[_position])
            {
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '0':
                    return ParseString();
                case 'i':
                    return ParseInteger();
                case 'l':
                    return ParseList();
                case 'd':
                    return ParseDictionary();
            }

            throw new BencodingException("Unexpected token in data to decode.");
        }

        private bool ParseNextToken()
        {
            if (_position >= _encodedData.Length)
                return false;

            _tokens.Add(ParseAnyType());

            return true;
        }

        private void ParseAllTokens()
        {
            for (; ; )
            {
                if (!ParseNextToken())
                    break;
            }

            if (_position != _encodedData.Length)
            {
                throw new BencodingException("Extra characters at the end of the encoded string");
            }
        }

        #region Public API
        public static Bencoding ParseTorrentFile(string torrentFileName)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(torrentFileName));
            Bencoding bencoding = new Bencoding(br);

            bencoding.ParseAllTokens();
            
            return bencoding;
        }

        public static Bencoding ParseStream(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            Bencoding bencoding = new Bencoding(r);

            bencoding.ParseAllTokens();

            return bencoding;
        }

        public IEnumerable<BencodedData> Tokens
        {
            get
            {
                return _tokens;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var token in _tokens)
            {
                sb.AppendLine(token.ToString());
            }

            return sb.ToString();
        }
        #endregion
    }
}
