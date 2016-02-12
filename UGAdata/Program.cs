using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Delimon.Win32.IO;

namespace UGAdata
{
    class Program
    {
        // Read nullterminated string
        public static string readNullterminated(BinaryReader reader)
        {
            var char_array = new List<byte>();
            string str = "";
            if (reader.BaseStream.Position == reader.BaseStream.Length)
            {
                byte[] char_bytes2 = char_array.ToArray();
                str = Encoding.UTF8.GetString(char_bytes2);
                return str;
            }
            byte b = reader.ReadByte();
            while ((b != 0x00) && (reader.BaseStream.Position != reader.BaseStream.Length))
            {
                char_array.Add(b);
                b = reader.ReadByte();
            }
            byte[] char_bytes = char_array.ToArray();
            str = Encoding.UTF8.GetString(char_bytes);
            return str;
        }

        // From http://stackoverflow.com/a/23544421/5343630
        static void CreateDirectoryRecursively(string path)
        {
            string[] pathParts = path.Split('\\');

            for (int i = 0; i < pathParts.Length; i++)
            {
                if (i > 0)
                    pathParts[i] = Delimon.Win32.IO.Path.Combine(pathParts[i - 1], pathParts[i]);

                if (!Delimon.Win32.IO.Directory.Exists(pathParts[i]))
                    Delimon.Win32.IO.Directory.CreateDirectory(pathParts[i]);
            }
        }

        static void Main(string[] args)
        {
            // Print header
            Console.WriteLine("UGAdata by MHVuze\n");

            // Process arguments
            if (args.Length < 1)
            {
                Console.WriteLine("ERROR: Invalid amount of arguments.\n");
                Console.WriteLine("Use UGAdata gamedata.bin");
                return;
            }

            string input = args[0];

            // Check files
            if (!System.IO.File.Exists(input))
            {
                Console.WriteLine("ERROR: File does not exist.");
                return;
            }

            if (!System.IO.File.Exists("gamedata.log"))
                System.IO.File.Delete("gamedata.log");

            // Process header
            BinaryReader br_input = new BinaryReader(System.IO.File.OpenRead(input));

            int magic = br_input.ReadInt32();
            if (magic != 0x00324748)
            {
                Console.WriteLine("ERROR: File does not have HG2 magic.");
                return;
            }

            br_input.BaseStream.Seek(0x1C, SeekOrigin.Current);
            int count = br_input.ReadInt32();

            br_input.BaseStream.Seek(0x08, SeekOrigin.Current);
            int name_pt_offset = br_input.ReadInt32();

            Console.WriteLine("File count: {0}\nName pointer table offset: 0x{1}\n", count, name_pt_offset.ToString("X8"));

            // Process files
            for (int i = 0; i < count; i++)
            {
                // Read from file info table
                int unk1 = br_input.ReadInt32();
                int size = br_input.ReadInt32();
                UInt32 file_offset = br_input.ReadUInt32();

                // Read from file name pointer table
                br_input.BaseStream.Seek(name_pt_offset + (i * 4), SeekOrigin.Begin);
                UInt32 name_offset = br_input.ReadUInt32();

                // Read from file name table
                br_input.BaseStream.Seek(name_offset, SeekOrigin.Begin);
                string name = readNullterminated(br_input);

                // Read file to array
                br_input.BaseStream.Seek(file_offset, SeekOrigin.Begin);
                byte[] file_data = br_input.ReadBytes(size);

                // Extract file
                string target_dir = (Delimon.Win32.IO.Path.GetDirectoryName(input) + "\\" + name.Substring(0, name.LastIndexOf("/") + 1)).Replace("/", "\\");
                string target_file = (Delimon.Win32.IO.Path.GetDirectoryName(input) + "\\" + name).Replace("/", "\\");
                if (!Delimon.Win32.IO.Directory.Exists(target_dir))
                    CreateDirectoryRecursively(target_dir);

                Console.WriteLine("Processing {0}", name);
                using (Stream extract = Delimon.Win32.IO.File.Create(target_file))
                {
                    extract.Write(file_data, 0, size);
                }

                // Write to log file
                using (StreamWriter log = new StreamWriter("gamedata.log", true, Encoding.UTF8))
                {
                    log.WriteLine(name + "," + name_offset.ToString("X8") + "," + unk1.ToString("X8") + "," + size.ToString("X8") + "," + file_offset.ToString("X8"));
                }

                // Move to next starting point
                br_input.BaseStream.Seek(0x30 + ((i + 1) * 0x0C), SeekOrigin.Begin);
            }
            Console.WriteLine("INFO: Done.");
        }
    }
}
