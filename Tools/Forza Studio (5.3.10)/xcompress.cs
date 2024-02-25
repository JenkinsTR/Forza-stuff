using System;
using System.Runtime.InteropServices;

namespace ForzaStudio
{
    public static class XCompress
    {
        private static bool is64Bit;
        static bool Is64Bit { get { return is64Bit; } }

        static XCompress()
        {
            is64Bit = IntPtr.Size == 8;
        }

        public enum XMemCodecType
        {
            Default = 0,
            LZX = 1
        }

        public struct XMemCodecParametersLZX
        {
            public int Flags;
            public int WindowSize;
            public int CompressionPartitionSize;
        }

        public const int CompressStream = 0x00000001;

        #region 32Bit

        [DllImport("xcompress32.dll", EntryPoint = "XMemCreateDecompressionContext")]
        private static extern int XMemCreateDecompressionContext32(
            XMemCodecType codecType,
            int pCodecParams,
            int flags, ref int pContext);

        [DllImport("xcompress32.dll", EntryPoint = "XMemDestroyDecompressionContext")]
        private static extern void XMemDestroyDecompressionContext32(int context);

        [DllImport("xcompress32.dll", EntryPoint = "XMemResetDecompressionContext")]
        private static extern int XMemResetDecompressionContext32(int context);

        [DllImport("xcompress32.dll", EntryPoint = "XMemDecompressStream")]
        private static extern int XMemDecompressStream32(int context,
            byte[] pDestination, ref int pDestSize,
            byte[] pSource, ref int pSrcSize);

        [DllImport("xcompress32.dll", EntryPoint = "XMemCreateCompressionContext")]
        private static extern int XMemCreateCompressionContext32(
            XMemCodecType codecType, int pCodecParams,
            int flags, ref int pContext);

        [DllImport("xcompress32.dll", EntryPoint = "XMemDestroyCompressionContext")]
        private static extern void XMemDestroyCompressionContext32(int context);

        [DllImport("xcompress32.dll", EntryPoint = "XMemResetCompressionContext")]
        private static extern int XMemResetCompressionContext32(int context);

        [DllImport("xcompress32.dll", EntryPoint = "XMemCompressStream")]
        private static extern int XMemCompressStream32(int context,
            byte[] pDestination, ref int pDestSize,
            byte[] pSource, ref int pSrcSize);

        #endregion

        #region 32Bit

        [DllImport("xcompress64.dll", EntryPoint = "XMemCreateDecompressionContext")]
        private static extern int XMemCreateDecompressionContext64(
            XMemCodecType codecType,
            int pCodecParams,
            int flags, ref int pContext);

        [DllImport("xcompress64.dll", EntryPoint = "XMemDestroyDecompressionContext")]
        private static extern void XMemDestroyDecompressionContext64(int context);

        [DllImport("xcompress64.dll", EntryPoint = "XMemResetDecompressionContext")]
        private static extern int XMemResetDecompressionContext64(int context);

        [DllImport("xcompress64.dll", EntryPoint = "XMemDecompressStream")]
        private static extern int XMemDecompressStream64(int context,
            byte[] pDestination, ref int pDestSize,
            byte[] pSource, ref int pSrcSize);

        [DllImport("xcompress64.dll", EntryPoint = "XMemCreateCompressionContext")]
        private static extern int XMemCreateCompressionContext64(
            XMemCodecType codecType, int pCodecParams,
            int flags, ref int pContext);

        [DllImport("xcompress64.dll", EntryPoint = "XMemDestroyCompressionContext")]
        private static extern void XMemDestroyCompressionContext64(int context);

        [DllImport("xcompress64.dll", EntryPoint = "XMemResetCompressionContext")]
        private static extern int XMemResetCompressionContext64(int context);

        [DllImport("xcompress64.dll", EntryPoint = "XMemCompressStream")]
        private static extern int XMemCompressStream64(int context,
            byte[] pDestination, ref int pDestSize,
            byte[] pSource, ref int pSrcSize);

        #endregion

        public static int XMemCreateDecompressionContext(
            XMemCodecType codecType,
            int pCodecParams,
            int flags, ref int pContext)
        {
            if (Is64Bit)
                return XMemCreateDecompressionContext64(
                     codecType, pCodecParams, flags, ref pContext);

            return XMemCreateDecompressionContext32(
                   codecType, pCodecParams, flags, ref pContext);
        }

        public static void XMemDestroyDecompressionContext(int context)
        {
            if (Is64Bit)
                XMemDestroyDecompressionContext64(context);
            else
                XMemDestroyDecompressionContext32(context);
        }

        public static int XMemResetDecompressionContext(int context)
        {
            if (is64Bit)
                return XMemResetDecompressionContext64(context);

            return XMemResetDecompressionContext32(context);
        }

        public static int XMemDecompressStream(int context,
            byte[] pDestination, ref int pDestSize,
            byte[] pSource, ref int pSrcSize)
        {
            if (Is64Bit)
                return XMemDecompressStream64(context, pDestination,
                    ref pDestSize, pSource, ref pSrcSize);

            return XMemDecompressStream32(context, pDestination,
                   ref pDestSize, pSource, ref pSrcSize);
        }

        public static int XMemCreateCompressionContext(
            XMemCodecType codecType, int pCodecParams,
            int flags, ref int pContext)
        {
            if (Is64Bit)
                return XMemCreateCompressionContext64(
                    codecType, pCodecParams, flags, ref pContext);

            return XMemCreateCompressionContext32(
                    codecType, pCodecParams, flags, ref pContext);
        }

        public static void XMemDestroyCompressionContext(int context)
        {
            if (Is64Bit)
                XMemDestroyCompressionContext64(context);
            else
                XMemDestroyCompressionContext32(context);
        }

        public static int XMemResetCompressionContext(int context)
        {
            if (Is64Bit)
                return XMemResetCompressionContext64(context);

            return XMemResetCompressionContext32(context);
        }

        public static int XMemCompressStream(int context,
            byte[] pDestination, ref int pDestSize,
            byte[] pSource, ref int pSrcSize)
        {
            if (Is64Bit)
                return XMemCompressStream64(context,
                    pDestination, ref pDestSize,
                    pSource, ref pSrcSize);

            return XMemCompressStream32(context,
                    pDestination, ref pDestSize,
                    pSource, ref pSrcSize);
        }

    }
}