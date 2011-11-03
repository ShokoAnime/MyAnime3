using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using AniDBHelper;

namespace MyAnimePlugin3.DataHelpers
{
	class Hasher
    {
        public class Hashes
        {
            public string ed2k;
            public string sha1;
            public string crc32;
            public string md5;
        }

        
        public delegate int OnHashProgress([MarshalAs(UnmanagedType.LPWStr)]string strFileName, int nProgressPct);

        #region DLL functions
        [DllImport("hasher.dll", EntryPoint = "CalculateHashes_AsyncIO")]
        private static extern int CalculateHashes_callback_dll(
            [MarshalAs(UnmanagedType.LPWStr)] string szFileName,
            [MarshalAs(UnmanagedType.LPArray)] byte[] hash,
            [MarshalAs(UnmanagedType.FunctionPtr)] OnHashProgress lpHashProgressFunc
         );

        // Calculates hash immediately (with progress)
        protected static bool CalculateHashes_dll(string strFileName, ref byte[] hash, OnHashProgress HashProgress)
        {
            OnHashProgress pHashProgress = new OnHashProgress(HashProgress);
            GCHandle gcHashProgress = GCHandle.Alloc(pHashProgress); //to make sure the GC doesn't dispose the delegate

            int nResult = CalculateHashes_callback_dll(strFileName, hash, pHashProgress);

            return (nResult == 0);
        }

        public static bool UseDll()
        {
            return File.Exists("hasher.dll");
        }

        public static string HashToString(byte[] hash, int start, int length)
        {
            if (hash == null || hash.Length == 0)
                return string.Empty;

            StringBuilder hex = new StringBuilder(length * 2);
            for (int x = start; x < start + length; x++)
            {
                hex.AppendFormat("{0:x2}", hash[x]);
            }
            return hex.ToString().ToUpper();
        }

        #endregion

        public static Hashes CalculateHashes(string strPath, OnHashProgress onHashProgress)
        {
            if (UseDll())
            {
                byte[] hash = new byte[56];
                Hashes rhash = new Hashes();
                    
                if (CalculateHashes_dll(strPath, ref hash, onHashProgress))
                {
                    rhash.ed2k = HashToString(hash, 0, 16);
                    rhash.crc32 = HashToString(hash, 16, 4);
                    rhash.md5 = HashToString(hash, 20, 16);
                    rhash.sha1 = HashToString(hash, 36, 20);
                    
                }
                else
                {
                    rhash.ed2k = string.Empty;
                    rhash.crc32 = string.Empty;
                    rhash.md5 = string.Empty;
                    rhash.sha1 = string.Empty;
                }
                return rhash;
            }
            return CalculateHashes_here(strPath, onHashProgress);
        }
       
        protected static Hashes CalculateHashes_here(string strPath, OnHashProgress onHashProgress)
        {
            FileStream fs;
            Hashes rhash=new Hashes();
            FileInfo fi = new FileInfo(strPath);
            fs = fi.OpenRead();
            int lChunkSize = 9728000;

            string sHash = "";
            long nBytes = (long)fs.Length;

            long nBytesRemaining = (long)fs.Length;
            int nBytesToRead = 0;

            long nBlocks = nBytes / lChunkSize;
            long nRemainder = nBytes % lChunkSize; //mod
            if (nRemainder > 0)
                nBlocks++;

            byte[] baED2KHash = new byte[16 * nBlocks];

            if (nBytes > lChunkSize)
                nBytesToRead = lChunkSize;
            else
                nBytesToRead = (int)nBytesRemaining;

            if (onHashProgress != null)
                onHashProgress(strPath, 0);

            MD4 md4 = MD4.Create();
            MD5 md5 = MD5.Create();
            SHA1 sha1 = SHA1.Create();
            Crc32 crc32=new Crc32();

            byte[] ByteArray = new byte[nBytesToRead];

            long iOffSet = 0;
            long iChunkCount = 0;
            while (nBytesRemaining > 0)
            {
                iChunkCount++;

                Console.WriteLine("Hashing Chunk: " + iChunkCount.ToString());

                int nBytesRead = fs.Read(ByteArray, 0, nBytesToRead);
                byte[] baHash = md4.ComputeHash(ByteArray, 0, nBytesRead);
                md5.TransformBlock(ByteArray, 0, nBytesRead, ByteArray, 0);
                sha1.TransformBlock(ByteArray, 0, nBytesRead, ByteArray, 0);
                crc32.TransformBlock(ByteArray, 0, nBytesRead, ByteArray, 0);
                int percentComplete = (int)((float)iChunkCount / (float)nBlocks * 100);
                if (onHashProgress != null)
                    onHashProgress(strPath, percentComplete);

                int j = (int)((iChunkCount - 1) * 16);
                for (int i = 0; i < 16; i++)
                    baED2KHash[j + i] = baHash[i];

                iOffSet += lChunkSize;
                nBytesRemaining = nBytes - iOffSet;
                if (nBytesRemaining < lChunkSize)
                    nBytesToRead = (int)nBytesRemaining;

            }
            md5.TransformFinalBlock(ByteArray, 0, 0);
            sha1.TransformFinalBlock(ByteArray, 0, 0);
            crc32.TransformFinalBlock(ByteArray, 0, 0);


            fs.Close();

            if (onHashProgress != null)
                onHashProgress(strPath, 100);

            byte[] baHashFinal = md4.ComputeHash(baED2KHash);
            rhash.ed2k = BitConverter.ToString(baHashFinal).Replace("-", "").ToUpper();
            rhash.crc32 = BitConverter.ToString(crc32.Hash).Replace("-", "").ToUpper();
            rhash.md5 = BitConverter.ToString(md5.Hash).Replace("-", "").ToUpper(); 
            rhash.sha1 = BitConverter.ToString(sha1.Hash).Replace("-", "").ToUpper(); 
            return rhash;
        }
    }
}
