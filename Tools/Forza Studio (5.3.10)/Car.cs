using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace ForzaStudio
{
    public class Car : IDisposable
    {
        #region Fields

        private FileStream File;
        public readonly EndianStream Stream;
        public CarSection[] Sections;

        // header values
        public float unk1;
        public float unk2;

        #endregion

        #region Properties
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
        }

        public int FaceCount
        {
            get
            {
                int faceCount = 0;
                foreach (CarSection section in Sections)
                {
                    foreach (CarPiece piece in section.Pieces)
                    {
                        faceCount += piece.FaceCount;
                    }
                }
                return faceCount;
            }
        }

        private int vertexCount = 0;
        public int VertexCount
        {
            get
            {
                if (vertexCount == 0)
                {
                    foreach (CarSection section in Sections)
                    {
                        vertexCount += section.Vertices.Length;
                        //if (section.LodVertices != null)
                        //{
                        //    vertexCount += section.LodVertices.Length;
                        //}
                    }
                }
                return vertexCount;
            }
        }

        public CarSection this[string name]
        {
            get
            {
                foreach (CarSection section in Sections)
                {
                    if (section.Name == name)
                        return section;
                }
                return null;
            }
        }

        #endregion

        #region Constructor / Destructor

        public Car(string filename)
        {
            name = Path.GetFileName(filename).Replace(".carbin", "");
            File = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            Stream = new EndianStream(File, true);

            // navigate through the header (todo: eventually separate this logic into a CarInformation class once more is figured out about it...)
            uint version = Stream.ReadUInt32();
            uint unknown = Stream.ReadUInt32();
            if (unknown != 0xD)
            {
                if (version == 1)
                {
                    Stream.Position += 0x100;
                }
                else if (version == 2)
                {
                    Stream.Position += 0x14;
                }
            }

            // more unknowns
            unknown = Stream.ReadUInt32();
            unknown = Stream.ReadUInt32();
            uint unknownCount = Stream.ReadUInt32();
            if (unknownCount > 0)
            {
                Stream.Position += (unknownCount * 0x8C);
            }
            Stream.Position += 0x1AC;

            uint unknownCount2 = Stream.ReadUInt32();
            if (unknownCount2 > 0)
            {
                uint index = 0;

                // skip first unknown data section
                for (index = 0; index < unknownCount2; ++index)
                {
                    uint firstUnknownDataCount = Stream.ReadUInt32();
                    Stream.Seek(firstUnknownDataCount * 4, SeekOrigin.Current);
                }

                // skip second unknown data section
                for (index = 0; index < unknownCount2; ++index)
                {
                    uint secondUnknownDataCount = Stream.ReadUInt32();
                    Stream.Seek(secondUnknownDataCount * 4, SeekOrigin.Current);
                }
            }

            // padding or delimiter, so skip it...
            uint unkPaddingDelimiter = Stream.ReadUInt32();

            // car section count
            uint sectionCount = Stream.ReadUInt32();
            Sections = new CarSection[sectionCount];

            // parse through each section
            for (int i = 0; i < sectionCount; i++)
            {
                Sections[i] = new CarSection(this);
            }
        }

        ~Car()
        {
            Dispose();
        }

        #endregion

        #region Methods
        public void Dispose()
        {
            if (Stream != null) Stream.Dispose();
            if (File != null) File.Dispose();
        }

        /// <summary>
        /// Gets the total face count for the specified lod level.
        /// </summary>
        /// <param name="lod"></param>
        /// <returns></returns>
        public int GetFaceCount(uint lod)
        {
            int faceCount = 0;
            foreach (CarSection section in Sections)
            {
                foreach (CarPiece piece in section.Pieces)
                {
                    if (piece.Lod == lod)
                    {
                        faceCount += piece.FaceCount;
                    }
                }
            }
            return faceCount;
        }

        /// <summary>
        /// Gets the total vertex count for the specified lod level.
        /// </summary>
        /// <param name="lod"></param>
        /// <returns></returns>
        public int GetVertexCount(uint lod)
        {
            foreach (CarSection section in Sections)
            {
                foreach (CarPiece piece in section.Pieces)
                {
                    if (piece.Lod == lod)
                    {
                        vertexCount += piece.VertexCount;
                    }
                }
            }
            return vertexCount;
        }


        public void Export(StreamWriter output, bool selective)
        {
            List<int> baseIndices = new List<int>();
            baseIndices.Add(0);

            // export vertices
            foreach (CarSection section in Sections)
            {
                foreach (CarPiece piece in section.Pieces)
                {
                    if (!selective || (selective && piece.Visible))
                    {
                        foreach (ForzaVertex v in piece.Vertices)
                        {
                            output.WriteLine("v {0} {1} {2}",
                                FloatFormat(v.position.X),
                                FloatFormat(v.position.Y),
                                FloatFormat(v.position.Z));
                        }
                        baseIndices.Add(baseIndices[baseIndices.Count - 1] + piece.Vertices.Length);
                    }
                }
            }

            // export textures
            foreach (CarSection section in Sections)
            {
                foreach (CarPiece piece in section.Pieces)
                {
                    if (!selective || (selective && piece.Visible))
                    {
                        foreach (ForzaVertex v in piece.Vertices)
                        {
                            output.WriteLine("vt {0} {1}",
                                FloatFormat(v.texture0.X),
                                FloatFormat(v.texture0.Y));
                        }
                    }
                }
            }

            // export normals
            foreach (CarSection section in Sections)
            {
                foreach (CarPiece piece in section.Pieces)
                {
                    if (!selective || (selective && piece.Visible))
                    {
                        foreach (ForzaVertex v in piece.Vertices)
                        {
                            output.WriteLine("vn {0} {1} {2}",
                                FloatFormat(v.normal.X),
                                FloatFormat(v.normal.Y),
                                FloatFormat(v.normal.Z));
                        }
                    }
                }
            }

            // export faces for each smoothing group
            int baseIndex = 0;
            foreach (CarSection section in Sections)
            {
                foreach (CarPiece piece in section.Pieces)
                {
                    if (!selective || (selective && piece.Visible))
                    {
                        output.WriteLine("g {0}", Name + "_" + section.Name + "_" + piece.Name);
                        for (int j = 0; j < piece.IndexBuffer.Length; j += 3)
                        {
                            output.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                                baseIndices[baseIndex] + piece.IndexBuffer[j + 0] + 1,
                                baseIndices[baseIndex] + piece.IndexBuffer[j + 1] + 1,
                                baseIndices[baseIndex] + piece.IndexBuffer[j + 2] + 1);
                        }
                        baseIndex++;
                    }
                }
            }

            output.Flush();
        }

        private string FloatFormat(float f)
        {
            return f.ToString(CultureInfo.InvariantCulture).Replace(',', '.');  // do the replace anyways just to be safe, ill find out the right way to do it later...
        }

        #endregion
    }
}
