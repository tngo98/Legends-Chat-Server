/* FILE           : Program.cs
 * PROJECT        : Assignment 05
 * PROGRAMMER     : Devin Caron & Tommy Ngo
 * FIRST VERSION  : 2020-11-08
 * DESCRIPTION    : This program is a chat system that allows multiple people
 *                  to message one another. The WPF is the client side that 
 *                  communicates to the server that sends the messages to the
 *                  other clients connected.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Assignment05_Server
{
    class Program
    {
        // listen bool to check that the server should be listening for clients
        private static bool listen = true;

        // lists to store the clients, their threads and their messages
        private static List<Thread> clientThreadList = new List<Thread>();
        private static List<TcpClient> clientList = new List<TcpClient>();
        private static List<NetworkStream> clientStreamList = new List<NetworkStream>();

        static void Main(string[] args)
        {
            // Start the server null
            TcpListener chatServer = null;

            // try catch to catch issues with the sockets
            try
            {
                // Set the server to the proper IP and Port
                IPAddress serverIP = IPAddress.Parse("127.0.0.1");
                Int32 portNumber = 13000;

                chatServer = new TcpListener(serverIP, portNumber);

                // Start the chatServer
                chatServer.Start();

                // Listen for Clients trying to connect
                while (true)
                {
                    Console.Write("Waiting on a connection request... \n");

                    // Accept a the client
                    TcpClient client = chatServer.AcceptTcpClient();

                    Console.WriteLine("Connected!\n");

                    // add the client to the list
                    clientList.Add(client);

                    // create new stream for the client
                    NetworkStream stream = client.GetStream();
                    clientStreamList.Add(stream);

                    // create a thread for the client that listens to the server to get new messages
                    Thread newClient = new Thread(new ParameterizedThreadStart(ThreadClient));

                    // all the thread to the list
                    clientThreadList.Add(newClient);

                    // start running the new client
                    newClient.Start(client);
                }
            }
            // catch any exceptions thrown
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}\n", e);
            }

            // stop the server from listening
            chatServer.Stop();   
        }

        //	FUNCTION    : ThreadClient
        //	DESCRIPTION : This function sets the thread created by the new
        //                new client to listen to the server until they exit.
        //	PARAMETERS  : Object: o
        //	RETURNS     : NONE
        public static void ThreadClient(Object o)
        {
            // initiate the client
            TcpClient client = (TcpClient)o;

            // initialize bytes and data string
            byte[] bytes = new byte[256];
            string data = string.Empty;

            // store the client stream
            NetworkStream stream = client.GetStream();

            // loop variable to store stream.read data
            int streamCount = 0;

            // loop listening for incoming data
            while (listen == true)
            {
                while ((streamCount = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // convert data to ascii string
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, streamCount);

                    // display data recieved 
                    Console.WriteLine("\nReceived: {0}", data);

                    if (data != "/[+ClientEnd+]/")
                    {
                        // loop  clientStreamsList
                        for (int j = 0; j < clientStreamList.Count; j++)
                        {
                            // check to make sure the current element in the list isn't the same stream used in this clientThread
                            if (clientStreamList[j] != stream)
                            {
                                // convert the clientMessage recieved from the client to bytes
                                byte[] clientMessage = System.Text.Encoding.ASCII.GetBytes(("Client " + j + "/[+UserId+]/" + data));

                                // send data to the other clients connected
                                clientStreamList[j].Write(clientMessage, 0, clientMessage.Length);
                            }
                        }
                    }
                    else
                    {
                        // close out the client
                        for (int j = 0; j < clientStreamList.Count; j++)
                        {
                            if (clientStreamList[j] == stream)
                            {
                                // close out client and stream
                                client.Close();
                                stream.Close();
                                clientThreadList[j].Interrupt();
                            }
                        }
                    }

                }
            }
        }
    }
}
