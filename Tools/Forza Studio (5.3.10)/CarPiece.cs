using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ForzaStudio
{
    // todo: separate into private fields and public properties

    [DebuggerDisplay("{Name}")]
    public class CarPiece
    {
        #region Fields
        public readonly CarSection Parent;
        public readonly EndianStream Stream;

        public string Name;
        public int FaceCount;
        public int VertexCount;

        private int[] Indices;   // raw piece indice data (triangle strips separated by 0xFFFFFF)
        public int[] IndexBuffer;   // triangle list indices

        // generate after section has been loaded
        public ForzaVertex[] Vertices;
        private List<List<int>> TriangleStrips = new List<List<int>>(); // indices separated by triangle strips

        public bool Visible = false; // indicates if it is checked or not

        // header data
        public uint Lod;        // level of detail indicator
        public uint unk2;
        public uint unk3;
        public uint unk4;
        public uint unk5;
        public uint unk6;
        public uint unk7;
        public uint unk8;
        public float unk9;
        public float unk10;
        public float unk11;
        public float unk12;
        public float unk13; // possible int
        public float unk14; // possible int
        public float unk15;
        public float unk16;
        public uint unk17;
        public int indexCount;
        public int indexSize;
        #endregion

        #region Properties

        #endregion

        #region Constructor
        // read header information
        public CarPiece(CarSection parent)
        {
            Parent = parent;
            Stream = parent.Stream;

            uint unknown = Stream.ReadUInt32();    // 2
            unknown = Stream.ReadUInt32();    // 0
            unknown = Stream.ReadUInt32();    // 0x2000000

            byte stringLength = Stream.ReadByte();
            Name = Stream.ReadASCII(stringLength);

            // read piece headerinformation
            Lod = Stream.ReadUInt32();
            unk2 = Stream.ReadUInt32();    // 6
            unk3 = Stream.ReadUInt32();    // 0
            unk4 = Stream.ReadUInt32();    // 1
            unk5 = Stream.ReadUInt32();    // 0
            unk6 = Stream.ReadUInt32();    // 0
            unk7 = Stream.ReadUInt32();    // 0
            unk8 = Stream.ReadUInt32();    // 0
            unk9 = Stream.ReadSingle();   // 1.0f
            unk10 = Stream.ReadSingle();   // 1.0f
            unk11 = Stream.ReadSingle();   // 1.0f
            unk12 = Stream.ReadSingle();   // 1.0f
            unk13 = Stream.ReadSingle();   // 0.0f (possible int)
            unk14 = Stream.ReadSingle();   // 0.0f (possible int)
            unk15 = Stream.ReadSingle();   // 1.0f
            unk16 = Stream.ReadSingle();   // 1.0f
            unk17 = Stream.ReadUInt32();    // 3
            indexCount = Stream.ReadInt32();
            indexSize = Stream.ReadInt32();

            Indices = ReadIndices(indexCount);
            unknown = Stream.ReadUInt32();    // 0   
        }
        #endregion

        #region Methods

        public int GetFaceCount(int[] indices)
        {
            int j = 0, faceCount = 0;
            while (j < indices.Length - 2)
            {
                if (indices[j + 2] != 0xFFFFFF)
                {
                    j++;
                    faceCount++;
                }
                else j += 3;
            }
            return faceCount;
        }

        // get unique indices to determine the vertex count
        public int GetVertexCount(int[] indices)
        {
            Hashtable ht = new Hashtable();
            foreach (int i in indices)
            {
                if (i != 0xFFFFFF)
                {
                    ht[i] = 0;
                }
            }
            return ht.Count;
        }

        private int[] ReadIndices(int count)
        {
            int[] indices = new int[count];
            int indexValue = 0;
            if (indexSize == 2)
            {
                for (int i = 0; i < indices.Length; i++)
                {
                    indexValue = Stream.ReadUInt16();
                    if (indexValue == 0xFFFF)
                    {
                        indexValue = 0xFFFFFF;
                    }
                    indices[i] = indexValue;
                }
            }
            else if (indexSize == 4)
            {
                // parse through piece data
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] = Stream.ReadInt32();
                }
            }
            return indices;
        }

        // generate vertices for the piece and update indices
        // call this externally after sections are finished loading, since they contain the vertex data...
        public void ProcessData()
        {
            FaceCount = GetFaceCount(Indices);
            VertexCount = GetVertexCount(Indices);
            GenerateVertices(ref Indices);
            TriangleStrips = GenerateTriangleStrips(Indices);
            IndexBuffer = GenerateTriangleList(Indices, FaceCount);
        }

        private void GenerateVertices(ref int[] indices)
        {
            int indicesLength = indices.Length;
            Vertices = new ForzaVertex[VertexCount];

            // build lookup table mapping each section relative index (key) with its piece relative index (value)
            int vertexIndex = 0;
            Hashtable ht = new Hashtable();
            for (int i = 0; i < indicesLength; i++)
            {
                int index = indices[i];
                if (index != 0xFFFFFF)
                {
                    if (ht[index] == null)
                    {
                        ht[index] = vertexIndex;
                        vertexIndex++;
                    }
                    indices[i] = (int)ht[index];    // update index
                }
            }

            // pick out piece vertices from parent section
            if (Lod == 0)
            {
                foreach (DictionaryEntry entry in ht)
                {
                    Vertices[(int)entry.Value] = Parent.VertexList[(int)entry.Key];
                }
            }
            else
            {
                foreach (DictionaryEntry entry in ht)
                {
                    Vertices[(int)entry.Value] = Parent.LodVertexList[(int)entry.Key];
                }
            }
        }

        // parse out indices as a list of triangle strips
        // might be useful later when developing an algorithm to recreate quads
        private List<List<int>> GenerateTriangleStrips(int[] indices)
        {
            List<List<int>> triStrips = new List<List<int>>();
            triStrips.Add(new List<int>());

            int stripIndex = 0;
            foreach (int i in indices)
            {
                if (i == 0xFFFFFF)
                {
                    triStrips.Add(new List<int>());
                    stripIndex++;
                }
                else triStrips[stripIndex].Add(i);
            }
            return triStrips;
        }

        private int[] GenerateTriangleList(int[] indices, int faceCount)
        {
            // generate triangle list from strip data
            int[] indexBuffer = new int[faceCount * 3];
            bool windingOrderSwitch = true;    // false = front facing when culling counterclockwise
            int f1, f2, f3, f = 0, listIndex = 0;
            while (f < indices.Length - 2)
            {
                f3 = indices[f + 2];
                if (f3 != 0xFFFFFF)
                {
                    f1 = indices[f];
                    f2 = indices[f + 1];
                    f++;

                    if (windingOrderSwitch)
                    {
                        indexBuffer[listIndex] = f1;
                        indexBuffer[listIndex + 1] = f2;
                        indexBuffer[listIndex + 2] = f3;
                    }
                    else
                    {
                        indexBuffer[listIndex] = f2;
                        indexBuffer[listIndex + 1] = f1;
                        indexBuffer[listIndex + 2] = f3;
                    }
                    listIndex += 3;
                    windingOrderSwitch ^= true;
                }
                else
                {
                    windingOrderSwitch = true; // false = front facing when culling counterclockwise
                    f += 3;
                }
            }
            return indexBuffer;
        }

        public Color GetRandomColor()
        {
            Random rand = new Random();
            return new Color(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255));
        }

        // parse through the list of triangle strips, checking for quads first, then tris...
        // if next four points lie on the same plane (or within a certain threshhold, its a quad)
        // otherwise next three are a tri and move on...

        // todo: generate quad+triangle strips for polygon extraction
        public void TestQuads()
        {


            //// loop through each strip data
            //for (int i = 0; i < Strips.Count; i++)
            //{
            //    for (int j = 0; j < Strips[i].Count;)
            //    {
            //        int remainder = Strips[i].Count - j;
            //        if (remainder == 4 || remainder > 5)
            //        {
            //            // look for quads

            //            Vector3 v1 = Parent.Vertices[Strips[i][j + 0]].Position;
            //            Vector3 v2 = Parent.Vertices[Strips[i][j + 1]].Position;
            //            Vector3 v3 = Parent.Vertices[Strips[i][j + 2]].Position;
            //            Vector3 v4 = Parent.Vertices[Strips[i][j + 3]].Position;

            //            Vector3 t1 = v1 - v2;
            //            Vector3 t2 = v2 - v3;
            //            Vector3 t3 = v3 - v4;
            //            Vector3 t4 = v4 - v1;

            //            Vector3 cross = Vector3.Cross(t1, t2);

            //            float r1 = Vector3.Dot(t1, cross);
            //            float r2 = Vector3.Dot(t2, cross);
            //            float r3 = Vector3.Dot(t3, cross);
            //            float r4 = Vector3.Dot(t4, cross);

            //            float result = r1 + r2 + r3 + r4;
            //            float thresh = 1.0E-8f;


            //            if (r1 < thresh && r2 < thresh && r3 < thresh && r4 < thresh)
            //            {
            //                // quad
            //                j += 3;
            //            }
            //            else
            //            {
            //                // triangle
            //                j++;
            //            }
            //        }
            //        else if (remainder > 2)
            //        {
            //            // triangle

            //            j++;
            //        }
            //        else if (remainder == 1)
            //        {
            //            j++;    // done
            //        }
            //        else
            //        {
            //            // shouldnt get here
            //        }
            //    }
            //}




            //for (int i = 0; i < Indices.Length;)
            //{
            //    uint i1 = Indices[i + 0];
            //    uint i2 = Indices[i + 1];
            //    uint i3 = Indices[i + 2];
            //    uint i4 = Indices[i + 3];

            //    if (i2 == 0xFFFFFF) // last quad
            //    {
            //        i+=2;    // next strip
            //    }
            //    else if (i1 == 0xFFFFFF)
            //    {
            //        i += 1;
            //    }
            //    else if (i3 == 0xFFFFFF)
            //    {
            //        i += 3;
            //    }
            //    else if (i4 == 0xFFFFFF)
            //    {
            //        i += 4;
            //    }
            //    else
            //    {
            //        // test next 4 as quad
            //        Vector3 v1 = Parent.Vertices[i1].Position;
            //        Vector3 v2 = Parent.Vertices[i2].Position;
            //        Vector3 v3 = Parent.Vertices[i3].Position;
            //        Vector3 v4 = Parent.Vertices[i4].Position;

            //        Vector3 t1 = v1 - v2;
            //        Vector3 t2 = v2 - v3;
            //        Vector3 t3 = v3 - v4;
            //        Vector3 t4 = v4 - v1;

            //        Vector3 cross = Vector3.Cross(t1, t2);

            //        float r1 = Vector3.Dot(t1, cross);
            //        float r2 = Vector3.Dot(t2, cross);
            //        float r3 = Vector3.Dot(t3, cross);
            //        float r4 = Vector3.Dot(t4, cross);



            //        float result = r1 + r2 + r3 + r4;
            //        float thresh = 1.0E-14f;


            //        if (r1 < thresh || r2 < thresh || r3 < thresh || r4 < thresh)
            //        {
            //            // quad
            //            i += 3;
            //        }
            //        else
            //        {
            //            // triangle
            //            i += 2;
            //        }


            //    }
            //}
        }

        /// <summary>
        /// Exports piece information to a given output stream.
        /// </summary>
        /// <param name="output"></param>
        public void Export(StreamWriter output)
        {
            // export vertices
            foreach (ForzaVertex v in Vertices)
            {
                output.WriteLine("v {0} {1} {2}",
                    FloatFormat(v.position.X),
                    FloatFormat(v.position.Y),
                    FloatFormat(v.position.Z));
            }

            // export textures
            foreach (ForzaVertex v in Vertices)
            {
                output.WriteLine("vt {0} {1}",
                    FloatFormat(v.texture0.X),
                    FloatFormat(v.texture0.Y));
            }

            // export normals
            foreach (ForzaVertex v in Vertices)
            {
                output.WriteLine("vn {0} {1} {2}",
                    FloatFormat(v.normal.X),
                    FloatFormat(v.normal.Y),
                    FloatFormat(v.normal.Z));
            }

            // export faces
            output.WriteLine("g {0}", Parent.Parent.Name + "_" + Parent.Name + "_" + Name);
            for (int i = 0; i < IndexBuffer.Length; i += 3)
            {
                output.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                    IndexBuffer[i + 0] + 1,
                    IndexBuffer[i + 1] + 1,
                    IndexBuffer[i + 2] + 1);
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
