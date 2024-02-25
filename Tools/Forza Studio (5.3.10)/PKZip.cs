//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.IO.Compression;

//namespace ForzaStudio
//{
//    public class PkZip : IDisposable
//    {
//        private FileStream File;

//        public enum CompType : short
//        {
//            Stored = 0,
//            Shrunk,
//            Reduce1,
//            Reduce2,
//            Reduce3,
//            Reduce4,
//            Implode,
//            Token,
//            Deflate,
//            Deflate64,

//            LZX = 21
//        }

//        List<ZipDirEntry> entries;
//        public List<ZipDirEntry> Entries
//        {
//            get { return entries; }
//            set { entries = value; }
//        }
//        EndianStream io;
//        public EndianStream Io
//        {
//            get { return io; }
//            set { io = value; }
//        }

//        public PkZip(string file)
//        {
//            File = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
//            io = new EndianStream(File, false);
//        }

//        ~PkZip()
//        {
//            Dispose();
//        }

//        public void Dispose()
//        {
//            if (File != null) File.Dispose();
//        }

//        public void Read()
//        {
//            // Move to the EndLocator
//            // io.SeekTo((int)io.Stream.Length - 0x36);

//            // Read it to make sure its a PkZip file
//            io.Seek((int)io.Length - 0x16, SeekOrigin.Begin);
//            if (io.ReadInt32() != 0x06054B50)
//                throw new Exception("This is not a PkZip File!");

//            // Lets read the structure
//            io.Seek((int)io.Length - 0x16, SeekOrigin.Begin);
//            ZipEndLocator el = new ZipEndLocator();
//            el.Read(io);

//            // Now that we have this info lets read our directory entries
//            entries = new List<ZipDirEntry>();

//            io.Seek(el.DirectoryOffset, SeekOrigin.Begin);
//            for (int x = 0; x < el.EntriesInDirectory; x++)
//            {
//                ZipDirEntry entry = new ZipDirEntry();
//                entry.Read(io);
//                entries.Add(entry);
//            }
//        }

//        public struct ZipDirEntry
//        {
//            int compressedSize;
//            CompType compression;
//            uint crc;
//            short diskNumberStart;
//            int externalAttributes;
//            string extraField;
//            short extraFieldLength;
//            // string deFileCommand;
//            short fileCommentLength;
//            short fileDate;
//            string fileName;
//            short fileNameLength;
//            short fileTime;
//            short flags;
//            int headerOffset;
//            short internalAttributes;
//            int signature; // 0x02014b50 - 0x48530A02
//            int uncompressedSize;
//            short versionMadeBy;
//            short versionToExtract;

//            int offset;

//            public string FileName
//            {
//                get { return fileName; }
//                set
//                {
//                    fileName = value;
//                    fileNameLength = (short)fileName.Length;
//                }
//            }

//            public void Read(EndianStream es)
//            {
//                offset = (int)es.Position;

//                signature = es.ReadInt32();
//                versionMadeBy = es.ReadInt16();
//                versionToExtract = es.ReadInt16();
//                flags = es.ReadInt16();
//                compression = (CompType)es.ReadInt16();
//                fileTime = es.ReadInt16();
//                fileDate = es.ReadInt16();
//                crc = es.ReadUInt32();
//                compressedSize = es.ReadInt32();
//                uncompressedSize = es.ReadInt32();
//                fileNameLength = es.ReadInt16();
//                extraFieldLength = es.ReadInt16();
//                fileCommentLength = es.ReadInt16();
//                diskNumberStart = es.ReadInt16();
//                internalAttributes = es.ReadInt16();
//                externalAttributes = es.ReadInt32();
//                headerOffset = es.ReadInt32();
//                fileName = es.ReadString(fileNameLength);
//                extraField = es.ReadString(extraFieldLength);
//                // deFileCommand = es.ReadString(deExtraFieldLength);
//            }

//            public void Write(EndianStream es)
//            {
//                es.Write(signature);
//                es.Write(versionMadeBy);
//                es.Write(versionToExtract);
//                es.Write(flags);
//                es.Write((short)compression);
//                es.Write(fileTime);
//                es.Write(fileDate);
//                es.Write(crc);
//                es.Write(compressedSize);
//                es.Write(uncompressedSize);
//                es.Write(fileNameLength);
//                es.Write(extraFieldLength);
//                es.Write(fileCommentLength);
//                es.Write(diskNumberStart);
//                es.Write(internalAttributes);
//                es.Write(externalAttributes);
//                es.Write(headerOffset);
//                es.WriteAsciiString(fileName, fileNameLength);
//                es.WriteAsciiString(extraField, extraFieldLength);
//                // deFileCommand = es.ReadString(deExtraFieldLength);
//            }

//            public void Extract(EndianStream es, string newFileName)
//            {
//                // Move to the record
//                es.Seek(headerOffset, SeekOrigin.Begin);

//                // Read the record
//                ZipFileRecord record = new ZipFileRecord();
//                record.Read(es);

//                // Decompress our data
//                byte[] buffer = record.DecompressData();

//                // Create and write our file
//                FileStream fs = new FileStream(newFileName, FileMode.Create,
//                    FileAccess.Write);
//                fs.Write(buffer, 0, buffer.Length);
//                fs.Close();
//            }

//            public void Inject(EndianStream io, string newFileName)
//            {
//                // Move to the record
//                io.Seek(headerOffset, SeekOrigin.Begin);

//                // Read the record
//                ZipFileRecord record = new ZipFileRecord();
//                record.Read(io);

//                // Now lets read the data we will be injecting
//                FileStream fs = new FileStream(newFileName, FileMode.Open,
//                    FileAccess.Read);
//                byte[] buffer = new byte[(int)fs.Length];
//                fs.Read(buffer, 0, buffer.Length);
//                fs.Close();

//                // Now lets compress our data
//                record.CompressData(buffer);

//                // Lets check if our size grew, if so we cant do this!
//                if (record.CompressedSize > compressedSize)
//                    throw new Exception("New compressed length is too large" +
//                        "to fit without rebuilding");

//                // Write any null padding we need (not sure if this will work)
//                io.Write(new byte[compressedSize - record.CompressedSize]);

//                // Alright we are fine to write this back lets do it
//                io.Seek(headerOffset, SeekOrigin.Begin);
//                record.Write(io);

//                // Now lets fix this records data to match then write
//                compressedSize = record.CompressedSize;
//                uncompressedSize = record.UncompressedSize;
//                crc = record.Crc;

//                io.Seek(offset, SeekOrigin.Begin);
//                Write(io);
//            }

//            public override string ToString()
//            {
//                return fileName;
//            }
//        }

//        struct ZipEndLocator
//        {
//            string comment;
//            short commentLength;
//            internal int DirectoryOffset;
//            int directorySize;
//            short diskNumber;
//            internal short EntriesInDirectory;
//            short entriesOnDisk;
//            int signature; // 0x06054b50
//            short startDiskNumber;

//            public void Read(EndianStream er)
//            {
//                signature = er.ReadInt32();
//                diskNumber = er.ReadInt16();
//                startDiskNumber = er.ReadInt16();
//                entriesOnDisk = er.ReadInt16();
//                EntriesInDirectory = er.ReadInt16();
//                directorySize = er.ReadInt32();
//                DirectoryOffset = er.ReadInt32();
//                commentLength = er.ReadInt16();
//                comment = er.ReadString(commentLength);
//            }

//            public void Write(EndianStream ew)
//            {
//                ew.Write(signature);
//                ew.Write(diskNumber);
//                ew.Write(startDiskNumber);
//                ew.Write(entriesOnDisk);
//                ew.Write(EntriesInDirectory);
//                ew.Write(directorySize);
//                ew.Write(DirectoryOffset);
//                ew.Write(commentLength);
//                ew.WriteAsciiString(comment, commentLength);
//            }
//        }

//        struct ZipFileRecord
//        {
//            internal int CompressedSize;
//            CompType compression;
//            internal uint Crc;
//            string extraField;
//            short extraFieldLength;
//            short fileDate;
//            string fileName;
//            short fileNameLength;
//            short fileTime;
//            short flags;
//            int signature; // 0x04034b50
//            internal int UncompressedSize;
//            short version;

//            byte[] data;

//            public void Read(EndianStream er)
//            {
//                signature = er.ReadInt32();
//                version = er.ReadInt16();
//                flags = er.ReadInt16();
//                compression = (CompType)er.ReadInt16();
//                fileTime = er.ReadInt16();
//                fileDate = er.ReadInt16();
//                Crc = er.ReadUInt32();
//                CompressedSize = er.ReadInt32();
//                UncompressedSize = er.ReadInt32();
//                fileNameLength = er.ReadInt16();
//                extraFieldLength = er.ReadInt16();
//                fileName = er.ReadString(fileNameLength);
//                extraField = er.ReadString(extraFieldLength);

//                data = er.ReadBytes(CompressedSize);
//            }

//            public void Write(EndianStream ew)
//            {
//                ew.Write(signature);
//                ew.Write(version);
//                ew.Write(flags);
//                ew.Write((short)compression);
//                ew.Write(fileTime);
//                ew.Write(fileDate);
//                ew.Write(Crc);
//                ew.Write(CompressedSize);
//                ew.Write(UncompressedSize);
//                ew.Write(fileNameLength);
//                ew.Write(extraFieldLength);
//                ew.WriteAsciiString(fileName, fileNameLength);
//                ew.WriteAsciiString(extraField, extraFieldLength);

//                ew.Write(data);
//            }

//            public byte[] DecompressData()
//            {
//                // Decompress our data
//                byte[] buffer;
//                switch (compression)
//                {
//                    case CompType.Stored:
//                        {
//                            buffer = data;
//                            break;
//                        }
//                    case CompType.Deflate:
//                        {
//                            MemoryStream ms = new MemoryStream(data);
//                            DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress);

//                            buffer = new byte[UncompressedSize];
//                            if (ds.Read(buffer, 0, UncompressedSize) != UncompressedSize)
//                                throw new Exception("Decompresson Error: Bad Decompress Length");

//                            break;
//                        }
//                    case CompType.LZX:
//                        {
//                            //Create our decompression context
//                            int decompressionContext = 0;
//                            XCompress.XMemCreateDecompressionContext(
//                                XCompress.XMemCodecType.LZX,
//                                0, 0, ref decompressionContext);

//                            //Reset our context first
//                            XCompress.XMemResetDecompressionContext(decompressionContext);

//                            //Now lets read and decompress
//                            buffer = new byte[UncompressedSize];
//                            XCompress.XMemDecompressStream(decompressionContext,
//                                buffer, ref UncompressedSize,
//                                data, ref CompressedSize);

//                            //Go ahead and destory our context
//                            XCompress.XMemDestroyDecompressionContext(decompressionContext);

//                            break;
//                        }
//                    default:
//                        {
//                            throw new Exception("Compression type " +
//                                compression + " not supported");
//                        }
//                }

//                // Check our CRC32 now
//                if (new Crc32().ComputeChecksum(buffer) != Crc)
//                    throw new Exception("Decompresson Error: Bad CRC");

//                // Return our data since we are good
//                return buffer;
//            }

//            public void CompressData(byte[] decompressedData)
//            {
//                // Set a few values
//                Crc = new Crc32().ComputeChecksum(decompressedData);
//                UncompressedSize = decompressedData.Length;

//                // Compress our data
//                byte[] buffer;
//                switch (compression)
//                {
//                    case CompType.Stored:
//                        {
//                            buffer = decompressedData;
//                            break;
//                        }
//                    case CompType.Deflate:
//                        {
//                            MemoryStream ms = new MemoryStream();
//                            DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress);
//                            ds.Write(decompressedData, 0, decompressedData.Length);
//                            buffer = ms.ToArray();
//                            break;
//                        }
//                    default:
//                        {
//                            throw new Exception("Compression type " +
//                                compression + " not supported");
//                        }
//                }

//                // Set our compressed length and set our data
//                CompressedSize = buffer.Length;
//                data = buffer;
//            }
//        }
//    }
//}