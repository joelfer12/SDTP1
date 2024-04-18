using System.Net.Sockets;
using System.Text;

class Meuprograma
{
    static void Main(string[] args)
    {
        Console.WriteLine("O cliente faz parte da nossa rede? (1 = sim / 0 = nao)");
        int resposta = Convert.ToInt32(Console.ReadLine());

        if (resposta == 1)
        {
            Connect("127.0.0.1", resposta.ToString());
        }
        else
        {
            Console.WriteLine("O cliente nao faz parte da nossa rede. Conexao nao estabelecida");
        }
    }


    static void Connect(String server, String message)
    {
        try
        {
            // Create a TcpClient.
            // Note, for this client to work you need to have a TcpServer
            // connected to the same address as specified by the server, port
            // combination.
            Int32 port = 13000;

            // Prefer a using declaration to ensure the instance is Disposed later.
            using TcpClient client = new TcpClient(server, port);
            
            client.ReceiveTimeout = 10000;

            // Translate the passed message into ASCII and store it as a Byte array.
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

            // Get a client stream for reading and writing.
            NetworkStream stream = client.GetStream();

            // Send the message to the connected TcpServer.
            stream.Write(data, 0, data.Length);

            Console.WriteLine("Sent: {0}", message);

            // Receive the server response.

            // Buffer to store the response bytes.
            data = new Byte[256];

            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Console.WriteLine("Received: {0}", responseData);

            // Explicit close is not necessary since TcpClient.Dispose() will be
            // called automatically.
            // stream.Close();
            // client.Close();

            if (responseData == "100 OK")
            {
                Console.WriteLine("Connection accepted!\n");
                Console.WriteLine("Type 'QUIT' to exit.\n");
                Console.WriteLine("Type 'COMPLETO' para avisar que a tarefa que lhe foi alocada está completa\n");
                Console.WriteLine("Type 'NOVATAREFA' para lhe ser alocada uma nova tarefa\n");

                // Iniciar uma thread para ler as mensagens do servidor
                Thread readThread = new Thread(() => ReadMessages(stream, client));
                readThread.Start();

                while (true)
                {
                    Console.Write("Enter command: ");
                    string input = Console.ReadLine();
                    data = Encoding.ASCII.GetBytes(input);
                    stream.Write(data, 0, data.Length);
                    Console.WriteLine("Sent: {0}", input);

                    if (input.ToUpper() == "QUIT")
                    {
                        break;
                    }
                }
            }
            else if (responseData == "0")
            {
                Console.WriteLine("Server rejected the connection.");
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();

        }
        catch (ArgumentNullException e)
        {
            Console.WriteLine("ArgumentNullException: {0}", e);
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }

        Console.WriteLine("\n Press Enter to continue...");
        Console.Read();
    }

    static void ReadMessages(NetworkStream stream, TcpClient client)
    {
        Byte[] data = new Byte[256];
        while (true)
        {
            try
            {
                int bytes = stream.Read(data, 0, data.Length);
                if (bytes == 0)
                {
                    Console.WriteLine("Connection closed by server.");
                    break;
                }
                string responseData = Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);

                if (responseData.StartsWith("Opções:"))
                {
                    Console.WriteLine(responseData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while reading from server: {0}", ex.Message);
                break;
            }
        }
        stream.Close();     //Fechar a stream
        client.Close();     //Fechar o cliente  
    }
}