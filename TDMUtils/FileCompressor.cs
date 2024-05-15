using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDMUtils
{

    public static class SaveCompressor
    {
        public enum SaveType
        {
            Standard,
            Compressed,
            CompressedByte,
            error
        }
        public class CompressedSave(string Save)
        {
            public byte[] Bytes { get; } = Compress(Save);

            public override string ToString()
            {
                return GetBytesAsString(Bytes);
            }
        }

        public static string Decompress(byte[] dataToDeCompress)
        {
            byte[] decompressedData = DecompressByte(dataToDeCompress);
            return Encoding.UTF8.GetString(decompressedData);
        }

        public static string Decompress(string String)
        {
            return Decompress(Convert.FromBase64String(String));
        }

        private static byte[] CompressByte(byte[] bytes)
        {
            using var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.SmallestSize))
            {
                gzipStream.Write(bytes, 0, bytes.Length);
            }
            return memoryStream.ToArray();
        }
        private static byte[] DecompressByte(byte[] bytes)
        {
            using var memoryStream = new MemoryStream(bytes);

            using var outputStream = new MemoryStream();
            using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                decompressStream.CopyTo(outputStream);
            }
            return outputStream.ToArray();
        }

        private static byte[] Compress(string String)
        {
            byte[] dataToCompress = Encoding.UTF8.GetBytes(String);
            byte[] compressedData = CompressByte(dataToCompress);
            return compressedData;
        }

        private static string GetBytesAsString(byte[] byteData)
        {
            return Convert.ToBase64String(byteData);
        }

        public static SaveType TestSaveFileType<T>(string FilePath, bool PrioritizeCompressed)
        {
            string Content = File.ReadAllText(FilePath);
            var ByteContent = File.ReadAllBytes(FilePath);
            if (PrioritizeCompressed)
            {
                if (TestCompressedByteSave<T>(ByteContent)) { return SaveType.CompressedByte; };
                if (TestStandardSave<T>(Content)) { return SaveType.Standard; };
            }
            else
            {
                if (TestStandardSave<T>(Content)) { return SaveType.Standard; };
                if (TestCompressedByteSave<T>(ByteContent)) { return SaveType.CompressedByte; };
            }
            if (TestCompressedSave<T>(Content)) { return SaveType.Compressed; }; //This type is never used but check for it just incase
            return SaveType.error;
        }

        public static bool TestStandardSave<T>(string FileContent)
        {
            return Utility.isJsonTypeOf<T>(FileContent);
        }
        public static bool TestCompressedSave<T>(string FileContent)
        {
            try
            {
                var DecompSave = Decompress(FileContent);
                return Utility.isJsonTypeOf<T>(DecompSave);
            }
            catch
            {
                return false;
            }
        }
        public static bool TestCompressedByteSave<T>(byte[] FileContent)
        {
            try
            {
                var DecompSave = Decompress(FileContent);
                return Utility.isJsonTypeOf<T>(DecompSave);
            }
            catch
            {
                return false;
            }
        }

        public static string GetSaveStringFromFile<T>(string FilePath, bool PrioritizeCompressed)
        {
            switch (SaveCompressor.TestSaveFileType<T>(FilePath, PrioritizeCompressed))
            {
                case SaveCompressor.SaveType.Standard:
                    return File.ReadAllText(FilePath);
                case SaveCompressor.SaveType.Compressed:
                    var Decomp = SaveCompressor.Decompress(File.ReadAllText(FilePath));
                    return (Decomp);
                case SaveCompressor.SaveType.CompressedByte:
                    var ByteDecomp = SaveCompressor.Decompress(File.ReadAllBytes(FilePath));
                    return (ByteDecomp);
            }
            return string.Empty;
        }
    }
}
