using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace ScRatHost
{
    class Helper
    {
        public static byte[] key = Convert.FromBase64String("FfWdKwgTkkfGsRUic9kXfBD3ofzUSgRFoQgGNGcVXOg=");
        public static byte[] vec = Convert.FromBase64String("UxvcXgNT5Vpq03n+2QAhKA==");
        public static byte[] encrypt(byte[] data)
        {
            Aes aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = vec;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor();

            byte[] encryptedData;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (BinaryWriter sw = new BinaryWriter(cs))
                    {
                        sw.Write(data);
                    }
                    encryptedData = ms.ToArray();
                }
            }
            aesAlg.Dispose();
            return encryptedData;
        }
        public static byte[] decrypt(byte[] data)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = vec;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor();

                using (MemoryStream ms = new MemoryStream(data))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (BinaryReader sr = new BinaryReader(cs))
                        {
                            using (MemoryStream m = new MemoryStream())
                            {
                                sr.BaseStream.CopyTo(m);
                                return m.ToArray();
                            }
                        }
                    }
                }
            }
        }
        public static void writeFile(string path, byte[] data)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(path)))
                {
                    writer.Write(data);
                    writer.Flush();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public static byte[] readFile(string path)
        {
            try
            {
                return File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }
        public static string httpGet(string uri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
