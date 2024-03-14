using System.IO.Compression;
using System.Text;

namespace save_demo_1
{
    public partial class Main : Form
    {
        bool useAppData = false;

        public Main()
        {
            InitializeComponent();
        }

        static byte CompressBooleans(bool[] booleans)
        {
            if (booleans.Length > 8)
                throw new ArgumentException("Too many booleans to compress");

            byte compressedNumber = 0;
            for (int i = 0; i < booleans.Length; i++)
            {
                if (booleans[i])
                {
                    compressedNumber |= (byte)(1 << i);
                }
            }
            return compressedNumber;
        }

        static bool[] DecompressBooleans(byte compressedNumber, int length)
        {
            if (length > 8)
                throw new ArgumentException("Too many booleans to decompress");

            bool[] booleans = new bool[length];
            for (int i = 0; i < length; i++)
            {
                booleans[i] = (compressedNumber & (1 << i)) != 0;
            }
            return booleans;
        }

        static byte[] CompressString(string input)
        {
            byte[] inputBytes = Encoding.Unicode.GetBytes(input); // UTF-16 encoding

            using (MemoryStream outputStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    gzipStream.Write(inputBytes, 0, inputBytes.Length);
                }
                return outputStream.ToArray();
            }
        }

        static string DecompressBytes(byte[] compressedBytes)
        {
            using (MemoryStream inputStream = new MemoryStream(compressedBytes))
            {
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (GZipStream gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        gzipStream.CopyTo(outputStream);
                    }
                    byte[] decompressedBytes = outputStream.ToArray();
                    return Encoding.Unicode.GetString(decompressedBytes); // UTF-16 decoding
                }
            }
        }

        private void save(string path)
        {
            using (Stream stream = new FileStream(path + @"\0.save", FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // Writing header bytes
                byte[] headers = { 0x1F, 0xEA };
                foreach (byte header in headers)
                {
                    writer.Write(header);
                }

                // Writing string length
                byte[] text = CompressString(textBox1.Text);
                byte[] lengthBytes = BitConverter.GetBytes(text.Length);
                Array.Reverse(lengthBytes);
                foreach (byte b in lengthBytes)
                {
                    writer.Write(b);
                }

                // Writing Unicode string
                byte[] stringBytes = text;
                foreach (byte b in stringBytes)
                {
                    writer.Write(b);
                }

                bool[] bools = { radioButton2.Checked, checkBox1.Checked, checkBox2.Checked, checkBox3.Checked, checkBox4.Checked };

                byte total = CompressBooleans(bools);

                writer.Write(total);

                // Writing footer bytes
                byte[] footer = { 0x73, 0xA3 };
                foreach (byte feet in footer)
                {
                    writer.Write(feet);
                }

            }
        }

        private void load(string path)
        {
            using (Stream stream = new FileStream(path + @"\0.save", FileMode.Open))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                byte[] expectedHeader = { 0x1F, 0xEA };

                // Read the actual header from the file using ReadBytes
                byte[] actualHeader = reader.ReadBytes(expectedHeader.Length);

                if (!Enumerable.SequenceEqual(expectedHeader, actualHeader))
                {
                    return;
                }

                int stringLength;

                // Read the length bytes from the file
                byte[] lengthBytes = reader.ReadBytes(sizeof(int));
                Array.Reverse(lengthBytes);// Convert the byte array back to an integer
                stringLength = BitConverter.ToInt32(lengthBytes, 0);

                // Read the Encoded Unicode string from the file
                byte[] stringBytes = reader.ReadBytes(stringLength);

                // Convert the byte array to a string using Unicode encoding
                string text = DecompressBytes(stringBytes);

                // Read the byte containing the flags from the file
                bool[] bools = DecompressBooleans(reader.ReadByte(), 5);

                byte[] expectedFooter = { 0x73, 0xA3 };

                // Read the actual header from the file using ReadBytes
                byte[] actualFooter = reader.ReadBytes(expectedFooter.Length);

                if (!Enumerable.SequenceEqual(expectedFooter, actualFooter))
                {
                    return;
                }

                textBox1.Text = text;
                radioButton2.Checked = bools[0];
                radioButton1.Checked = !bools[0];
                checkBox1.Checked = bools[1];
                checkBox2.Checked = bools[2];
                checkBox3.Checked = bools[3];
                checkBox4.Checked = bools[4];
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            string[] useAppDataArgs = { "--useAppData", "-u", "/u" };
            string[] helpArgs = { "--help", "-h", "-?", "/h", "/?" };
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                if (helpArgs.Contains(args[1]))
                {
                    MessageBox.Show("Example SaveData Creator by Henrisen\n" +
                        "Usage: save_demo_1.exe [OPTIONS]\n" +
                        "   --help -h -? /h /?   :   Help (This Screen)\n" +
                        "   --useAppData -u /u   :   Uses AppData instead of This Directory" +
                        "                            For Storing The .save file");
                    Environment.Exit(0);
                }
                useAppData = useAppDataArgs.Contains(args[1]);
            }
            FormClosing += Main_FormClosing;
            string? path = useAppData ? (Environment.GetEnvironmentVariable("localappdata") + @"\thehsi_savedemo") : ".";
            if (path == null) Environment.Exit(0);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (File.Exists(path + @"\0.save")) load(path);
        }

        private void Main_FormClosing(object? sender, FormClosingEventArgs e)
        {
            string? path = useAppData ? (Environment.GetEnvironmentVariable("localappdata") + @"\thehsi_savedemo") : ".";
            if (path == null) Environment.Exit(0);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            save(path);
        }
    }
}
