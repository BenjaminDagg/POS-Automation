using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model
{
    public class TransactionPortalClient : IDisposable
    {
        public TcpClient tcpClient;
        private string hostname;
        private int port;
        private NetworkStream stream;
        private StreamWriter writer;
        private StreamReader reader;

        public TransactionPortalClient(string ipAddress, int _port)
        {
            hostname = ipAddress;
            port = _port;
        }


        public void Connect()
        {

            tcpClient = new TcpClient(hostname, port);
            stream = tcpClient.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            //Read connection message, advance the stream
            string res = reader.ReadLine();

        }


        public string Execute(string message)
        {
            
            writer.WriteLine(message);
            writer.Flush();

            // String to store the response ASCII representation.

            string res = reader.ReadLine();

            return res;
        }


        public string Read()
        {
            if (tcpClient.GetStream().DataAvailable)
            {
                
                Byte[] data = System.Text.Encoding.ASCII.GetBytes("test");
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = tcpClient.GetStream().Read(data, 0, data.Length); //(**This receives the data using the byte method**)
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes); //(**This converts it to string**)

                return responseData;
                
            }

            return String.Empty;
            
        }

        public void CLose()
        {
            Console.WriteLine("closing");
            //tcpClient.GetStream().Close();
            tcpClient.Close();

            reader.Dispose();
            writer.Dispose();
            stream.Dispose();
        }


        public void Dispose()
        {
            Console.WriteLine("disposing");
            tcpClient.Client.Close();
            tcpClient.Close();
            reader.Dispose();
            writer.Dispose();
            stream.Dispose();
        }


    }
}
