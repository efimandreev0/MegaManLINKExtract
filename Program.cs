using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaManLINK
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args[0].Contains(".link") || args[0].Contains(".LINK"))
            {
                Extract(args[0]);
            }
            else
            {
                Rebuild(args[0], args[1]);
            }
        }
        public static void Extract(string arc)
        {
            var reader = new BinaryReader(File.OpenRead(arc));
            string str = arc.Replace(".LINK", "").Replace(".link","") + "//";
            reader.BaseStream.Position = 0xC;
            int count = ((reader.ReadInt32() - 24) / 8) / 2;
            int[] pointers0 = new int[count];
            int[] size0 = new int[count];
            reader.BaseStream.Position = 0xC;
            Directory.CreateDirectory(arc.Replace(".LINK", "").Replace(".link", ""));
            for (int i = 0; i < count; i++)
            {
                pointers0[i] = reader.ReadInt32();
                size0[i] = reader.ReadInt32();
                reader.BaseStream.Position += 8;
                int pos = (int)reader.BaseStream.Position;
                reader.BaseStream.Position = pointers0[i];
                string name = Encoding.UTF8.GetString(reader.ReadBytes(4));
                reader.BaseStream.Position -= 4;
                byte[] bytes = reader.ReadBytes(size0[i]);
                File.WriteAllBytes(str + i + "." + name, bytes);
                reader.BaseStream.Position = pos;
            }

            
        }
        public static void Rebuild(string input, string archive)
        {
            string[] files = Directory.GetFiles(input);
            int[] pointers = new int[files.Length];
            int[] size = new int[files.Length];
            using (BinaryReader reader = new BinaryReader(File.OpenRead(archive)))
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(archive)))
            {
                reader.BaseStream.Position = 0xC;
                int blockOffset = reader.ReadInt32();
                writer.BaseStream.Position = blockOffset;
                for (int i = 0; i < files.Length; i++)
                {
                    byte[] file = File.ReadAllBytes(files[i]);
                    pointers[i] = (int)reader.BaseStream.Position;
                    size[i] = file.Length;
                    writer.Write(file);
                }
                writer.BaseStream.Position = 0xC;
                for (int i = 0; i < files.Length; i++)
                {
                    writer.Write(pointers[i]);
                    writer.Write(size[i]);
                    writer.BaseStream.Position += 8;
                }
            }
        }
    }
}
