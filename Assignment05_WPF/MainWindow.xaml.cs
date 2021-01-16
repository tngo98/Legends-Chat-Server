/* FILE           : MainWindow.xaml.cs
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Input;
using System.Net.Sockets;
using System.Threading;

namespace Assignment05_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Initalize Variables
        private bool keepListening = true;
        private string[] idDelim = new string[] { "/[+UserId+]/" };

        // TCP Connection 
        private TcpClient client;
        private NetworkStream stream;
        private Int32 portNumber = 13000;
        private string serverIP = "127.0.0.1";

        // Thread to listen
        private Thread listener;


        // FUNCTION		:	MainWindow()
        // DESCRIPTION	:	This function sets up the the main window
        public MainWindow()
        {
            InitializeComponent();

            Closing += Close_Window;

            // start thread listening to server
            listener = new Thread(new ParameterizedThreadStart(ListenForChatServer));

            keepListening = true;

            // start the stream
            listener.Start(stream);

            // try catch to catch any exceptions
            try
            {
                // connect the client to the server
                client = new TcpClient(serverIP, portNumber);
                // create the stream
                stream = client.GetStream();
            }
            catch (ArgumentNullException Ae)
            {
                MessageBox.Show(Ae.ToString());
            }
            catch (SocketException Se)
            {
                MessageBox.Show(Se.ToString());
            }
        }

        //	FUNCTION    : ListenForChatServer
        //	DESCRIPTION : this function keeps listening to the server
        //                and gets data from it and submits it to the 
        //                chat box
        //	PARAMETERS  : Object: o
        //	RETURNS     : NONE
        private void ListenForChatServer(object o)
        {
            // initialize bytes and string data
            Int32 bytes;
            Byte[] data = new Byte[256];
            String responseData = String.Empty;

            // loop listening to for data from the server
            while (keepListening != false)
            {
                // reset string and data
                responseData = string.Empty;
                data = new byte[256];

                // if something is in the stream
                if (stream != null)
                {
                    if (stream.CanRead == true && stream.DataAvailable == true)
                    {
                        // get the data stream
                        bytes = stream.Read(data, 0, data.Length);

                        // convert data to string
                        responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                        // parse the id
                        string[] sResponse = responseData.Split(idDelim, StringSplitOptions.None);

                        // add the message to the chatbox
                        SubmitToChat(sResponse[1], sResponse[0]);
                    }
                }
            }
        }

        //	FUNCTION    : SubmitToChat
        //	DESCRIPTION : this function takes the data from the server
        //                adds it to the chatBox
        //	PARAMETERS  : string : recievedMessage
        //                string : senderID
        //	RETURNS     : NONE
        private void SubmitToChat(string recivedMessage, string senderID)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                chatBox.Items.Add(recivedMessage);

                // scroll box to new item
                chatBox.Items.MoveCurrentToLast();
                chatBox.ScrollIntoView(chatBox.Items.CurrentItem);
            }
            ));
        }

        //	FUNCTION    : Send_Button_Click
        //	DESCRIPTION : When the send button is clicked if there is text in the text box
        //                translate the message and submit it to the chat
        //	PARAMETERS  : Object : o
        //	RETURNS     : NONE
        private void Send_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((messageText.Text.Length != 0) && (messageText.Text != " "))
            {
                string recievedMessage = messageText.Text;

                // try catch to catch any exceptions thrown
                try
                {
                    // translate the message into ASCII and then store into array
                    Byte[] data = System.Text.Encoding.ASCII.GetBytes(recievedMessage);

                    // if stream isn't null send the message to the server
                    if (stream == null)
                    {
                        MessageBox.Show("Error: Server Offline");
                    }
                    else
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    // submit  the message to the chatBox
                    SubmitToChat(recievedMessage, "You");

                }
                catch (SocketException Se)
                {
                    MessageBox.Show(Se.ToString());
                }
                catch (ArgumentNullException ARGe)
                {
                    MessageBox.Show(ARGe.ToString());
                }

                // reset message box text
                messageText.Text = "";
            }
        }

        //	FUNCTION    : MessageText_Enter_Press
        //	DESCRIPTION : If the user presses the enter button it will send the
        //                message in they inputted
        //	PARAMETERS  : Object : sender
        //              : KeyEventArgs : e
        //	RETURNS     : NONE
        private void MessageText_Enter_Press(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Send_Button_Click(sender, e);
            }
        }
        //
        // METHOD		:	MainWindow_Close()
        // DESCRIPTION	:	This method shuts down all the running functions and threads when the 
        //                  user exits the program
        // PARAMETERS   :   object sender       :   object that called the funtion
        //                  CancelEventArgs e   :   any arguments if an event is canceled
        // RETURNS		:	void
        //
        private void Close_Window(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // try catch to catch and exceptions thrown
            try
            {
                Byte[] data = System.Text.Encoding.ASCII.GetBytes("--A Client Has Disconnected--");

                stream.Write(data, 0, data.Length);

                // stop the client from listening to server
                keepListening = false;

                // close Stream and client
                stream.Close();
                client.Close();
            }
            catch (SocketException Se)
            {
                MessageBox.Show(Se.ToString());
            }
            catch (ArgumentNullException ARGe)
            {
                MessageBox.Show(ARGe.ToString());
            }
        }
    }
}
