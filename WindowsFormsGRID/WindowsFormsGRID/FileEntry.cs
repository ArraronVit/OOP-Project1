using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace WindowsFormsGRID
{
    [Serializable]
    public class FileEntry
    {
        public string Name { get; private set; }
        public string Extension { get; private set; }
        public string FileName { get; private set; }
        public string FileRelativePath { get; private set; }
        public byte[] FileContents { get; private set; }
        public ulong Size { get; private set; }
        public string FIleTime { get; private set; }

        public FileEntry(string fullfilename)
        {
            Name = Path.GetFileNameWithoutExtension(fullfilename);
            Extension = Path.GetExtension(fullfilename);
            FileName = Name + Extension;
            FileRelativePath = Path.GetFullPath(fullfilename);
            FileContents = File.ReadAllBytes(fullfilename);

            //FileInfo finfo = new FileInfo(fullfilename);
            //finfo.Refresh();
            //var foo = finfo.CreationTime;
            //FIleTime = finfo.CreationTime.ToShortDateString() + " " + finfo.CreationTime.ToShortTimeString();
            FIleTime = (new FileInfo(fullfilename)).LastWriteTime.ToString();
            Size = (ulong)FileContents.Length;
        }
        //public FileEntry() { }
        public FileEntry(string filename, string filerelativepath, byte[] filecontents, string filetime, ulong size/*, bool isreal*/)
        {
            Name = Path.GetFileName(filename);
            Extension = Path.GetExtension(filename);
            FileName = filename;
            FileRelativePath = filerelativepath;
            FileContents = filecontents;
            FIleTime = filetime;
            Size = size;
        }


        public override string ToString()
        {
            return "FileName= " + Name + Extension + /*"\nFileRelativePath= "+FileRelativePath+*/"\t Size= " + Size.ToString() + " bytes" + "\t Time:" + FIleTime;
        }
    }

    [Serializable]
    public class FileContainer /*: IEnumerable<FileEntry>*/
    {
        public string Name { get; private set; }

        public int Count { get; private set; }
        public ulong Size { get; private set; }
        public bool IsEmpty { get; private set; }

        private List<FileEntry> files;
        private string password = string.Empty/*"vitalii"*/;
        private readonly string passwordForpassword = "vitalii";
        private readonly byte[] key = new byte[] { 0x43, 0x87, 0x23, 0x72 };
        private readonly int passwordLength = 64;
        private readonly int xorKey = 201;
        private byte[] passwordByte;

        private enum cryptType:byte
        {
            None,
            Decrypt,    
            Encrypt, 
            DecryptPass,    
            EncryptPass 
        }


        public FileContainer()
        {
            Name = string.Empty;
            //string path = Application.StartupPath;
            //Name = $@"{DateTime.Now.Ticks}.dat";
            Name = Path.GetTempFileName();
            files = new List<FileEntry>();
            Count = 0;
            Size = 0;
            IsEmpty = true;
            passwordByte = new byte[passwordLength];
            setPassword(string.Empty);
            //passwordByte = Enumerable.Repeat((byte)0x20, passwordLength).ToArray(); // space
        }
        public FileContainer(string name)
        {
            Name = name;
            files = new List<FileEntry>();
            passwordByte = new byte[passwordLength];
            //passwordByte = Enumerable.Repeat((byte)0x20, passwordLength).ToArray();
            //if (File.Exists(Name))
            Deserialize();
        }
        public IEnumerator<FileEntry> GetEnumerator()
        {
            foreach (FileEntry fileentry in files)
                yield return fileentry;
        }

        public void AddEntry(FileEntry fileentry)
        {
            files.Add(fileentry);
            ++Count;
            IsEmpty = false;
            Size += fileentry.Size;
            Serialize();
        }
        public void Remove(FileEntry fileentry)
        {
            files.Remove(fileentry);
            --Count;
            Size -= fileentry.Size;
            Serialize();
            if (Count <= 0)
                IsEmpty = true;
        }
        public void Remove(string filename)
        {
            FileEntry foo = find(filename);
            Remove(foo);
        }

        public void Clear()
        {
            files.Clear();
            Count = 0;
            Size = 0;
            Serialize();
        }
 
        public void Extract(string filename)
        {
            FileEntry foo = find(Path.GetFileName(filename));
            File.WriteAllBytes(filename, foo.FileContents);
            //Extract(foo);
        }

        public void SaveContainer(string name)
        {
            Name = name;
            Serialize();
        }
        public void Info()
        {
            Console.WriteLine("\nContainer FIleName: " + Name + "\nTotal files: " + Count.ToString() + " Total size: " + Size.ToString() + " bytes.\n");
            files.ForEach(x => { Console.WriteLine(x.ToString()); });
        }

        public void setPassword(string password)
        {
            passStringToByte();
            this.password = password;
            Serialize();
        }
        public string getPassword()
        {
            byte[] pass = TrimEnd(passwordByte);
            //return Encoding.BigEndianUnicode.GetString(pass);
            string foo= Encoding.BigEndianUnicode.GetString(pass);
            return cryptXOR(foo, xorKey);
        }

        private byte[] Crypt(byte[] input, cryptType crypttype)
        {
            CryptoStream cs=null;
            PasswordDeriveBytes pdb = null;
            MemoryStream ms = new MemoryStream();
            //Aes aes = new AesManaged();
            //aes.Key = pdb.GetBytes(aes.KeySize / 8);
            //aes.IV = pdb.GetBytes(aes.BlockSize / 8);

            if ((byte)crypttype > (byte)cryptType.Encrypt)
                pdb = new PasswordDeriveBytes(passwordForpassword, key);
            else
                pdb = new PasswordDeriveBytes(password, key);

            Aes aes = new AesManaged();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);

            if (crypttype == cryptType.Encrypt)
                cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            else if (crypttype == cryptType.Decrypt)
                cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write); 

            cs.Write(input, 0, input.Length);
            cs.Close();
            return ms.ToArray();
        }

        private  byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length ];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        private void Serialize()
        {
            byte[] filesBytes;
            //byte[] passBytes; //
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream memorystream = new MemoryStream())
            {
                formatter.Serialize(memorystream, files);
                filesBytes = memorystream.ToArray();
            }

            filesBytes = Crypt(filesBytes, cryptType.Encrypt);
 

            passStringToByte();
            //passBytes = Crypt(passwordByte, cryptType.EncryptPass); //
            //passBytes = Crypt(passBytes, cryptType.DecryptPass); //
            File.WriteAllBytes(Name, MergeArray(passwordByte, filesBytes));
            //File.WriteAllBytes(Name, MergeArray(passBytes, filesBytes));
        }

        private void passStringToByte()
        {
            passwordByte = Enumerable.Repeat((byte)0x0, passwordLength).ToArray();  // clear array
            string foo = cryptXOR(password, xorKey);
            passwordByte = Encoding.BigEndianUnicode.GetBytes(foo);
            //passwordByte = Encoding.BigEndianUnicode.GetBytes(password);            // coding
            Array.Resize(ref passwordByte, passwordLength);                         // resize
        }

        private void Deserialize()
        {
            byte[] data = File.ReadAllBytes(Name);
            byte[] clearContent = data.Skip(passwordLength).ToArray();
            Buffer.BlockCopy(data, 0, passwordByte, 0, passwordLength);

            //passwordByte = Crypt(passwordByte, cryptType.DecryptPass);

            passwordByte = TrimEnd(passwordByte);
            password = string.Empty;
            password = Encoding.BigEndianUnicode.GetString(passwordByte);

            password = cryptXOR(password, xorKey);

            clearContent = Crypt(clearContent, cryptType.Decrypt);

            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream memorystream = new MemoryStream(clearContent))
            {
                files = formatter.Deserialize(memorystream) as List<FileEntry>;
            }
            Count = files.Count;
            Size = 0;
            files.ForEach (x =>{ Size += x.Size; });
        }

        private FileEntry find(string filename)
        {
            return files.Find(x => x.FileName  == filename);
        }

        private  byte[] TrimEnd(byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != '\0');
            Array.Resize(ref array, lastIndex + 1);
            return array;
        }

        private  byte[] MergeArray(byte[] first, byte[] second)
        {
            byte[] array = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, array, 0, first.Length);
            Buffer.BlockCopy(second, 0, array, first.Length, second.Length);
            return array;
        }

        private string cryptXOR(string inputString, int key)
        {
            string outputString = string.Empty;
            foreach (char c in inputString)
                 outputString += (char) (c ^ key);
            return outputString;
        }
    }

}
