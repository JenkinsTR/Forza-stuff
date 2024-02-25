using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ForzaStudio
{
    public struct ForzaVertex
    {
        public Vector3 position;
        public Vector2 texture0;
        public Vector2 texture1;
        public Vector3 normal;
        public Color color;

        public ForzaVertex(Vector3 position, Vector2 texture0, Vector2 texture1, Vector3 normal, Color color)
        {
            this.position = position;
            this.texture0 = texture0;
            this.texture1 = texture1;
            this.normal = normal;
            this.color = color;
        }

        public static VertexElement[] VertexElements =
        {
            new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
            new VertexElement(0, 12, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(0, 20, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(0, 28, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0),
            new VertexElement(0, 40, VertexElementFormat.Color, VertexElementMethod.Default, VertexElementUsage.Color, 0)
        };
        public static int SizeInBytes = 44;
    }

    public class CarVertex
    {
        #region Fields
        public readonly CarSection Parent;
        public readonly EndianStream Stream;
        public ForzaVertex Element;

        // translated vertex information
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Texture0Coordinate;
        public Vector2 Texture1Coordinate;
        public Vector3 unk1;
        public Vector3 unk2;
        public Vector3 unk3;
        public Vector3 unk4;
        public Vector3 unk5;
        public Vector3 unk6;
        public Vector3 unk7;    // something extra in the 44 byte vertex data
        #endregion

        #region Properties

        #endregion

        #region Constructor
        public CarVertex(CarSection parent, uint size)
        {
            Parent = parent;
            Stream = parent.Stream;

            float x, y, z, s;

            switch (size)
            {
                case 0x28:

                    // position
                    x = ConvertHalfToFloat(Stream.ReadUInt16());
                    y = ConvertHalfToFloat(Stream.ReadUInt16());
                    z = ConvertHalfToFloat(Stream.ReadUInt16());
                    s = ConvertHalfToFloat(Stream.ReadUInt16());
                    Position = new Vector3(x * s + parent.xOffset, y * s + parent.yOffset, z * s + parent.zOffset);

                    // textures
                    Texture0Coordinate = new Vector2(ConvertHalfToFloat(Stream.ReadUInt16()), 1.0f - ConvertHalfToFloat(Stream.ReadUInt16()));
                    Texture1Coordinate = new Vector2(ConvertHalfToFloat(Stream.ReadUInt16()), 1.0f - ConvertHalfToFloat(Stream.ReadUInt16()));

                    // normal
                    Normal = ToN3(Stream.ReadUInt32());

                    // unknown vertex blending weights etc...
                    unk1 = ToN3(Stream.ReadUInt32());
                    unk2 = ToN3(Stream.ReadUInt32());
                    unk3 = ToN3(Stream.ReadUInt32());
                    unk4 = ToN3(Stream.ReadUInt32());
                    unk5 = ToN3(Stream.ReadUInt32());

                    break;
                case 0x2C:

                    // position
                    x = ConvertHalfToFloat(Stream.ReadUInt16());
                    y = ConvertHalfToFloat(Stream.ReadUInt16());
                    z = ConvertHalfToFloat(Stream.ReadUInt16());
                    s = ConvertHalfToFloat(Stream.ReadUInt16());
                    Position = new Vector3(x * s + parent.xOffset, y * s + parent.yOffset, z * s + parent.zOffset);

                    // texture
                    Texture0Coordinate = new Vector2(ConvertHalfToFloat(Stream.ReadUInt16()), 1.0f - ConvertHalfToFloat(Stream.ReadUInt16()));

                    // normal
                    Normal = ToN3(Stream.ReadUInt32());

                    // unknown vertex blending weights etc...
                    unk1 = ToN3(Stream.ReadUInt32());
                    unk2 = ToN3(Stream.ReadUInt32());
                    unk3 = ToN3(Stream.ReadUInt32());
                    unk4 = ToN3(Stream.ReadUInt32());
                    unk5 = ToN3(Stream.ReadUInt32());
                    unk6 = ToN3(Stream.ReadUInt32());
                    unk7 = ToN3(Stream.ReadUInt32());

                    break;
                case 0x10:

                    // position
                    x = ConvertHalfToFloat(Stream.ReadUInt16());
                    y = ConvertHalfToFloat(Stream.ReadUInt16());
                    z = ConvertHalfToFloat(Stream.ReadUInt16());
                    s = ConvertHalfToFloat(Stream.ReadUInt16());
                    Position = new Vector3(x * s + parent.xOffset, y * s + parent.yOffset, z * s + parent.zOffset);

                    // textures
                    Texture0Coordinate = new Vector2(ConvertHalfToFloat(Stream.ReadUInt16()), 1.0f - ConvertHalfToFloat(Stream.ReadUInt16()));

                    // normal
                    Normal = ToN3(Stream.ReadUInt32());

                    break;
                default:
                    // other formats not supported yet...
                    throw new NotSupportedException("Unknown vertex format.");
            }

            // create vertex element
            Element = new ForzaVertex(Position, Texture0Coordinate, Texture1Coordinate, Normal, Color.Gray);
        }

        #endregion

        #region Methods
 
        /// <summary>
        /// Converts a 16-bit packed half-precision float to a 32-bit single precision float.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private unsafe float ConvertHalfToFloat(uint value)
        {
            uint mantissa = 0;
            uint exponent = 0;
            uint result;

            mantissa = value & 0x3FF;

            if ((value & 0x7C00) != 0)  // The value is normalized
            {
                exponent = (value >> 10) & 0x1F;
            }
            else if (mantissa != 0) // The value is denormalized
            {
                // Normalize the value in the resulting float
                exponent = 1;

                do
                {
                    exponent--;
                    mantissa <<= 1;
                } while ((mantissa & 0x400) == 0);

                mantissa &= 0x3FF;
            }
            else exponent = 0xFFFFFF90; // (uint)-112

            result = ((value & 0x8000) << 16) | ((exponent + 112) << 23) | (mantissa << 13); // sign | exponent | mantissa
            return *(float*)&result;
        }


        Vector3 ToN3(uint u)
        {
            Vector3 v = Vector3.Zero;
            uint[] SignExtendXY = new uint[] { 0, 0xFFFFF800 };
            uint[] SignExtendZ = new uint[] { 0, 0xFFFFFC00 };
            uint element = u & 0x7FF;
            v.X = (float)(short)(element | SignExtendXY[element >> 10]) / 1023.0f;
            element = (u >> 11) & 0x7FF;
            v.Y = (float)(short)(element | SignExtendXY[element >> 10]) / 1023.0f;
            element = (u >> 22) & 0x3FF;
            v.Z = (float)(short)(element | SignExtendZ[element >> 9]) / 511.0f;
            return v;
        }

        #endregion
    }
}