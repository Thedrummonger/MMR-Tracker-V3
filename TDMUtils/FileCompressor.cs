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
        public enum CompressionFormat
        {
            None,
            Base64,
            Byte,
            error
        }
        public class CompressedFile(string FileContent)
        {
            public string Uncompressed { get; } = FileContent;
            public byte[] Bytes { get; } = Compress(FileContent);

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

        public static CompressionFormat GetFileCompressionFormat<T>(string FilePath, bool PrioritizeCompressed)
        {
            if (!File.Exists(FilePath)) { return CompressionFormat.error; }
            string Content = File.ReadAllText(FilePath);
            var ByteContent = File.ReadAllBytes(FilePath);
            if (PrioritizeCompressed)
            {
                if (TestForByteFile<T>(ByteContent)) { return CompressionFormat.Byte; };
                if (TestForUncompressedFile<T>(Content)) { return CompressionFormat.None; };
            }
            else
            {
                if (TestForUncompressedFile<T>(Content)) { return CompressionFormat.None; };
                if (TestForByteFile<T>(ByteContent)) { return CompressionFormat.Byte; };
            }
            if (TestForCompressedFile<T>(Content)) { return CompressionFormat.Base64; }; //This type is never used but check for it just incase
            return CompressionFormat.error;
        }

        private static bool TestForUncompressedFile<T>(string FileContent)
        {
            return Utility.isJsonTypeOf<T>(FileContent);
        }
        private static bool TestForCompressedFile<T>(string FileContent)
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
        private static bool TestForByteFile<T>(byte[] FileContent)
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

        public static string DecompressFile<T>(string FilePath, bool PrioritizeCompressed)
        {
            return GetFileCompressionFormat<T>(FilePath, PrioritizeCompressed) switch
            {
                CompressionFormat.None => File.ReadAllText(FilePath),
                CompressionFormat.Base64 => Decompress(File.ReadAllText(FilePath)),
                CompressionFormat.Byte => Decompress(File.ReadAllBytes(FilePath)),
                _ => string.Empty,
            };
        }
    }
}
