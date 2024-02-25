using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ForzaStudio
{

    [DebuggerDisplay("{Name}")]
    public class CarSection
    {
        #region Fields
        public readonly Car Parent;
        public readonly EndianStream Stream;

        public string Name;
        public CarPiece[] Pieces;

        // lod0 vertex information
        public CarVertex[] Vertices;
        public ForzaVertex[] VertexList;

        // lod 1-5 vertex information
        public CarVertex[] LodVertices;
        public ForzaVertex[] LodVertexList;

        public float xOffset;
        public float yOffset;
        public float zOffset;
        public float flt4;
        public float flt5;
        public float flt6;
        public float flt7;
        public float flt8;
        public float flt9;
        public float flt10;
        public float flt11;
        public float flt12;
        public float flt13;
        public float flt14;

        #endregion

        #region Properties
        public CarPiece this[string name]
        {
            get
            {
                foreach (CarPiece piece in Pieces)
                {
                    if (piece.Name == name)
                        return piece;
                }
                return null;
            }
        }
        public int FaceCount
        {
            get
            {
                int faceCount = 0;
                foreach (CarPiece piece in Pieces)
                {
                    faceCount += piece.FaceCount;
                }
                return faceCount;
            }
        }


        public int VertexCount
        {
            get
            {
                return (int)Vertices.Length;
            }
        }
        #endregion

        #region Constructor
        public CarSection(Car parent)
        {
            Parent = parent;
            Stream = parent.Stream;

            // read primary section header (constant throughout all .carbin files) :D
            uint unk = Stream.ReadUInt32();
            xOffset = Stream.ReadSingle();
            yOffset = Stream.ReadSingle();
            zOffset = Stream.ReadSingle();
            flt4 = Stream.ReadSingle();
            flt5 = Stream.ReadSingle();
            flt6 = Stream.ReadSingle();
            flt7 = Stream.ReadSingle();
            flt8 = Stream.ReadSingle();
            flt9 = Stream.ReadSingle();
            flt10 = Stream.ReadSingle();
            flt11 = Stream.ReadSingle();
            flt12 = Stream.ReadSingle();
            flt13 = Stream.ReadSingle();
            flt14 = Stream.ReadSingle();

            unk = Stream.ReadUInt32();
            unk = Stream.ReadUInt32();
            uint unkCount = Stream.ReadUInt32();
            if (unkCount > 0)
            {
                // skip an array of float4 vectors
                Stream.Position += (unkCount * 16);
            }

            unk = Stream.ReadUInt32();
            uint unkCount2 = Stream.ReadUInt32();
            if (unkCount2 > 0)
            {
                // skip and array of 16-bit index values
                Stream.Position += (unkCount2 * 2);
            }

            unk = Stream.ReadUInt32();
            unk = Stream.ReadUInt32();

            byte stringLength = Stream.ReadByte();
            Name = Stream.ReadASCII(stringLength);

            // parse any extra lod vertex information
            uint unk1 = Stream.ReadUInt32();    // 2
            uint lodVertexCount = Stream.ReadUInt32();    // 0
            uint lodVertexSize = Stream.ReadUInt32();    // 0
            if (lodVertexCount > 0)
            {
                LodVertices = new CarVertex[lodVertexCount];
                for (int i = 0; i < lodVertexCount; i++)
                {
                    LodVertices[i] = new CarVertex(this, lodVertexSize);
                }
                LodVertexList = new ForzaVertex[LodVertices.Length];
                for (int i = 0; i < LodVertices.Length; i++)
                {
                    LodVertexList[i] = LodVertices[i].Element;
                }
            }

            uint unk4 = Stream.ReadUInt32();    // 1

            // parse through each piece
            uint piecesCount = Stream.ReadUInt32();
            Pieces = new CarPiece[piecesCount];
            for (int i = 0; i < piecesCount; i++)
            {
                Pieces[i] = new CarPiece(this);
            }

            // parse through section vertices
            uint unkVtxHdr1 = Stream.ReadUInt32();
            uint vertexCount = Stream.ReadUInt32();
            uint vertexSize = Stream.ReadUInt32();
            Vertices = new CarVertex[vertexCount];
            for (int i = 0; i < VertexCount; i++)
            {
                Vertices[i] = new CarVertex(this, vertexSize);
            }

            // create vertex buffer
            VertexList = new ForzaVertex[vertexCount];
            for (int i = 0; i < vertexCount; i ++)
            {
                VertexList[i] = Vertices[i].Element;
            }

            // generate extra per-piece information
            foreach (CarPiece piece in Pieces)
            {
                piece.ProcessData();
            }
        }
        #endregion

        #region Methods
        public void Export(StreamWriter output, bool selective)
        {
            List<int> baseIndices = new List<int>();
            baseIndices.Add(0);

            // export vertices
            foreach (CarPiece piece in Pieces)
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

            // export textures
            foreach (CarPiece piece in Pieces)
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

            // export normals
            foreach (CarPiece piece in Pieces)
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

            // export faces for each smoothing group
            for (int i = 0; i < Pieces.Length; i++)
            {
                if (!selective || (selective && Pieces[i].Visible))
                {
                    output.WriteLine("g {0}", Parent.Name + "_" + Name + "_" + Pieces[i].Name);
                    for (int j = 0; j < Pieces[i].IndexBuffer.Length; j += 3)
                    {
                        output.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                            baseIndices[i] + Pieces[i].IndexBuffer[j + 0] + 1,
                            baseIndices[i] + Pieces[i].IndexBuffer[j + 1] + 1,
                            baseIndices[i] + Pieces[i].IndexBuffer[j + 2] + 1);
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
