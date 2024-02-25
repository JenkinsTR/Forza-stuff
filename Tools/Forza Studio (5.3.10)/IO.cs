using System;
using System.IO;

namespace PkZip
{
    public enum EndianType
    {
        BigEndian,
        LittleEndian
    }

    public class EndianIo
    {
        private readonly bool isFile;
        private bool isOpen;
        private Stream stream;
        private readonly string filepath = string.Empty;
        private readonly EndianType endiantype = EndianType.LittleEndian;

        private EndianReader @in;
        private EndianWriter @out;

        public bool Opened { get { return isOpen; } }
        public bool Closed { get { return !isOpen; } }
        public EndianReader In { get { return @in; } }
        public EndianWriter Out { get { return @out; } }
        public Stream Stream { get { return stream; } }
        public long Position { get { return stream.Position; } }

        public EndianIo(string filePath, EndianType endianStyle)
        {
            endiantype = endianStyle;
            filepath = filePath;
            isFile = true;
        }
        public EndianIo(Stream stream, EndianType endianStyle)
        {
            endiantype = endianStyle;
            this.stream = stream;
            isFile = false;
        }
        public EndianIo(byte[] buffer, EndianType endianStyle)
        {
            endiantype = endianStyle;
            stream = new MemoryStream(buffer);
            isFile = false;
        }

        public void SeekTo(int offset)
        {
            SeekTo(offset, SeekOrigin.Begin);
        }
        public void SeekTo(long offset)
        {
            SeekTo(offset, SeekOrigin.Begin);
        }
        public void SeekTo(long offset, SeekOrigin seekOrigin)
        {
            stream.Seek(offset, seekOrigin);
        }

        public void Open()
        {
            Open(FileMode.OpenOrCreate);
        }
        public void Open(FileMode fileMode)
        {
            if (isOpen) return;

            if (isFile)
                stream = new FileStream(filepath, fileMode, FileAccess.ReadWrite);

            @in = new EndianReader(stream, endiantype);
            @out = new EndianWriter(stream, endiantype);

            isOpen = true;
        }

        public void Close()
        {
            if (isOpen == false) return;

            stream.Close();
            @in.Close();
            @out.Close();

            isOpen = false;
        }

        public byte[] ToArray()
        {
            // If this is a memory stream lets return our data
            return ((MemoryStream)stream).ToArray();
        }
    }

    public class EndianReader : BinaryReader
    {
        private readonly EndianType endianStyle;

        public EndianReader(Stream stream, EndianType endianStyle)
            : base(stream)
        {
            this.endianStyle = endianStyle;
        }

        public void SeekTo(int offset)
        {
            SeekTo(offset, SeekOrigin.Begin);
        }
        public void SeekTo(long offset)
        {
            SeekTo(offset, SeekOrigin.Begin);
        }
        public void SeekTo(long offset, SeekOrigin seekOrigin)
        {
            BaseStream.Seek(offset, seekOrigin);
        }

        public override short ReadInt16()
        {
            return ReadInt16(endianStyle);
        }
        public short ReadInt16(EndianType endianType)
        {
            byte[] buffer = base.ReadBytes(2);

            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToInt16(buffer, 0);
        }

        public override ushort ReadUInt16()
        {
            return ReadUInt16(endianStyle);
        }
        public ushort ReadUInt16(EndianType endianType)
        {
            byte[] buffer = base.ReadBytes(2);

            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToUInt16(buffer, 0);
        }

        public override int ReadInt32()
        {
            return ReadInt32(endianStyle);
        }
        public int ReadInt32(EndianType endianType)
        {
            byte[] buffer = base.ReadBytes(4);

            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToInt32(buffer, 0);
        }

        public override uint ReadUInt32()
        {
            return ReadUInt32(endianStyle);
        }
        public uint ReadUInt32(EndianType endianType)
        {
            byte[] buffer = base.ReadBytes(4);

            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToUInt32(buffer, 0);
        }

        public override long ReadInt64()
        {
            return ReadInt64(endianStyle);
        }
        public long ReadInt64(EndianType endianType)
        {
            byte[] buffer = base.ReadBytes(8);

            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToInt64(buffer, 0);
        }

        public override ulong ReadUInt64()
        {
            return ReadUInt64(endianStyle);
        }
        public ulong ReadUInt64(EndianType endianType)
        {
            byte[] buffer = base.ReadBytes(8);

            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToUInt64(buffer, 0);
        }

        public override float ReadSingle()
        {
            return ReadSingle(endianStyle);
        }
        public float ReadSingle(EndianType endianType)
        {
            byte[] buffer = base.ReadBytes(4);

            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToSingle(buffer, 0);
        }

        public override double ReadDouble()
        {
            return ReadDouble(endianStyle);
        }
        public double ReadDouble(EndianType endianType)
        {
            byte[] buffer = base.ReadBytes(4);

            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToDouble(buffer, 0);
        }

        public string ReadNullTerminatedString()
        {
            string newString = string.Empty;
            byte temp;
            while ((temp = ReadByte()) != 0x00)
            {
                if (temp != 0x00) newString += (char)temp;
                else break;
            }
            return newString;
        }

        public string ReadAsciiString(int length)
        {
            return ReadAsciiString(length, endianStyle);
        }
        public string ReadAsciiString(int length, EndianType endianType)
        {
            return System.Text.Encoding.UTF8.GetString(ReadBytes(length));
        }

        public string ReadUnicodeString(int length)
        {
            return ReadUnicodeString(length, endianStyle);
        }
        public string ReadUnicodeString(int length, EndianType endianType)
        {
            string newString = string.Empty;
            int howMuch = 0;
            for (int x = 0; x < length; x++)
            {
                ushort tempChar = ReadUInt16(endianType);
                howMuch++;
                if (tempChar != 0x00)
                    newString += (char)tempChar;
                else
                    break;
            }

            int size = (length - howMuch) * sizeof(UInt16);
            BaseStream.Seek(size, SeekOrigin.Current);

            return newString;
        }

        public string ReadUnicodeNullTermString()
        {
            return ReadUnicodeNullTermString(endianStyle);
        }
        public string ReadUnicodeNullTermString(EndianType endianType)
        {
            string newString = string.Empty;
            while (true)
            {
                ushort tempChar = ReadUInt16(endianType);
                if (tempChar != 0x00)
                    newString += (char)tempChar;
                else
                    break;
            }
            return newString;
        }

        public new string ReadString()
        {
            string newString = string.Empty;
            int howMuch = 0;
            while (true)
            {
                byte tempChar = ReadByte();
                howMuch++;
                if (tempChar != 0x00)
                    newString += (char)tempChar;
                else
                    break;
            }

            var size = (newString.Length - howMuch) * sizeof(byte);
            BaseStream.Seek(size + 1, SeekOrigin.Current);

            return newString;
        }
        public string ReadString(int length)
        {
            return ReadAsciiString(length);
        }

        public int ReadInt24()
        {
            return ReadInt24(endianStyle);
        }
        public int ReadInt24(EndianType endianType)
        {
            byte[] buffer = base.ReadBytes(3);

            if (endianType == EndianType.BigEndian)
                return (buffer[0] << 16) | (buffer[1] << 8) | buffer[2];

            return (buffer[2] << 16) | (buffer[1] << 8) | buffer[0];
        }
    }

    public class EndianWriter : BinaryWriter
    {
        private readonly EndianType endianStyle;

        public EndianWriter(Stream stream, EndianType endianStyle)
            : base(stream)
        {
            this.endianStyle = endianStyle;
        }

        public void SeekTo(int offset)
        {
            SeekTo(offset, SeekOrigin.Begin);
        }
        public void SeekTo(long offset)
        {
            SeekTo(offset, SeekOrigin.Begin);
        }
        public void SeekTo(long offset, SeekOrigin seekOrigin)
        {
            BaseStream.Seek(offset, seekOrigin);
        }

        public override void Write(short value)
        {
            Write(value, endianStyle);
        }
        public void Write(short value, EndianType endianType)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public override void Write(ushort value)
        {
            Write(value, endianStyle);
        }
        public void Write(ushort value, EndianType endianType)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public override void Write(int value)
        {
            Write(value, endianStyle);
        }
        public void Write(int value, EndianType endianType)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public override void Write(uint value)
        {
            Write(value, endianStyle);
        }
        public void Write(uint value, EndianType endianType)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public override void Write(long value)
        {
            Write(value, endianStyle);
        }
        public void Write(long value, EndianType endianType)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public override void Write(ulong value)
        {
            Write(value, endianStyle);
        }
        public void Write(ulong value, EndianType endianType)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public override void Write(float value)
        {
            Write(value, endianStyle);
        }
        public void Write(float value, EndianType endianType)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public override void Write(double value)
        {
            Write(value, endianStyle);
        }
        public void Write(double value, EndianType endianType)
        {
            var buffer = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public void WriteAsciiString(string @string, int length)
        {
            WriteAsciiString(@string, length, endianStyle);
        }
        public void WriteAsciiString(string @string, int length, EndianType endianType)
        {
            int strLen = @string.Length;
            for (int x = 0; x < strLen; x++)
            {
                if (x > length)
                    break;//just incase they pass a huge string

                var val = (byte)@string[x];
                Write(val);
            }

            int nullSize = (length - strLen) * sizeof(byte);
            if (nullSize > 0)
                Write(new byte[nullSize]);
        }

        public void WriteUnicodeString(string @string, int length)
        {
            WriteUnicodeString(@string, length, endianStyle);
        }
        public void WriteUnicodeString(string @string, int length, EndianType endianType)
        {
            int strLen = @string.Length;
            for (int x = 0; x < strLen; x++)
            {
                if (x > length)
                    break;//just incase they pass a huge string

                ushort val = @string[x];
                Write(val, endianType);
            }

            int nullSize = (length - strLen) * sizeof(ushort);
            if (nullSize > 0)
                Write(new byte[nullSize]);
        }

        public void WriteUnicodeNullTermString(string @string)
        {
            WriteUnicodeNullTermString(@string, endianStyle);
        }
        public void WriteUnicodeNullTermString(string @string, EndianType endianType)
        {
            int strLen = @string.Length;
            for (int x = 0; x < strLen; x++)
            {
                ushort val = @string[x];
                Write(val, endianType);
            }
            Write((ushort)0, endianType);
        }

        public void WriteInt24(int value)
        {
            WriteInt24(value, endianStyle);
        }
        public void WriteInt24(int value, EndianType endianType)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            Array.Resize(ref buffer, 3);
            if (endianType == EndianType.BigEndian)
                Array.Reverse(buffer);

            base.Write(buffer);
        }
    }
}