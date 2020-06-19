using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SqlUtils
{
    internal class StrongName
    {
        internal static byte[] CreatePublicKeyToken(byte[] publicKey)
        {
            byte[] buffer;
            if ((publicKey == null) || (publicKey.Length == 0))
            {
                throw new ArgumentException("Cannot create key token. Invalid empty public key.");
            }
            int pcbStrongNameToken = 0;
            IntPtr zero = IntPtr.Zero;
            if (!StrongNameTokenFromPublicKey(publicKey, publicKey.Length, out zero, out pcbStrongNameToken) || (zero == IntPtr.Zero))
            {
                throw new Exception("Failed to create public key token from the public key.");
            }
            try
            {
                if ((pcbStrongNameToken <= 0) || (pcbStrongNameToken > 0x7fffffff))
                {
                    throw new Exception("Got an invalid key token from the public key.");
                }
                byte[] destination = new byte[pcbStrongNameToken];
                Marshal.Copy(zero, destination, 0, pcbStrongNameToken);
                buffer = destination;
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    StrongNameFreeBuffer(zero);
                }
            }
            return buffer;
        }

        private static byte[] ExtractPublicKey(byte[] keyBlob)
        {
            byte[] buffer;
            if ((keyBlob == null) || (keyBlob.Length == 0))
            {
                throw new Exception("Invalid empty key.");
            }
            if (keyBlob.Length == 160)
            {
                return keyBlob;
            }
            IntPtr zero = IntPtr.Zero;
            try
            {
                int pcbPublicKeyBlob = 0;
                if (!StrongNameGetPublicKey(null, keyBlob, keyBlob.Length, out zero, out pcbPublicKeyBlob))
                {
                    throw new Exception("Failed to retrieve the public key.");
                }
                byte[] destination = new byte[pcbPublicKeyBlob];
                Marshal.Copy(zero, destination, 0, pcbPublicKeyBlob);
                buffer = destination;
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    StrongNameFreeBuffer(zero);
                }
            }
            return buffer;
        }

        internal static byte[] ExtractPublicKeyFromKeyFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("Invalid empty file path for key file.");
            }
            if (!File.Exists(filePath))
            {
                throw new Exception("The key file '" + Path.GetFileName(filePath) + "' could not be found.");
            }
            byte[] buffer = null;
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                buffer = new byte[(int) stream.Length];
                stream.Read(buffer, 0, (int) stream.Length);
            }
            return ExtractPublicKey(buffer);
        }

        [DllImport("mscoree.dll")]
        private static extern void StrongNameFreeBuffer(IntPtr pbMemory);
        [DllImport("mscoree.dll")]
        private static extern bool StrongNameGetPublicKey([MarshalAs(UnmanagedType.LPWStr)] string wszKeyContainer, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] byte[] pbKeyBlob, [MarshalAs(UnmanagedType.U4)] int cbKeyBlob, out IntPtr ppbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbPublicKeyBlob);
        [DllImport("mscoree.dll")]
        private static extern bool StrongNameTokenFromPublicKey([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] pbPublicKeyBlob, int cbPublicKeyBlob, out IntPtr ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken);
    }
}

