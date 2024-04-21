using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServiMoto_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // Endereço IP e porta do servidor
            string serverIp = "127.0.0.1";
            int serverPort = 8888;

            try
            {
                // Conectar ao servidor
                TcpClient client = new TcpClient(serverIp, serverPort);
                Console.WriteLine("Conectado ao servidor.");

                // Obter o stream de comunicação
                NetworkStream stream = client.GetStream();

                // Ler a mensagem inicial do servidor
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Servidor: " + message);

                // Exemplo de envio de ID do cliente para o servidor
                Console.Write("Digite o ID do cliente: ");
                string clientId = Console.ReadLine();
                string sendMessage = "ID_DO_CLIENTE:" + clientId + "\n";
                byte[] sendData = Encoding.ASCII.GetBytes(sendMessage);
                stream.Write(sendData, 0, sendData.Length);

                // Aguardar resposta do servidor
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Servidor: " + message);

                // Exemplo de solicitação para concluir uma tarefa

                // Exemplo de envio de ID do cliente para o servidor
                string resposta;
                do
                {
                    Console.Write("Concluiu a tarefa? (S/N) ");
                    resposta = Console.ReadLine().ToUpper();

                    if (resposta == "S")
                    {
                        sendMessage = "TASK_COMPLETED:" + resposta + "\n";
                        sendData = Encoding.ASCII.GetBytes(sendMessage);
                        stream.Write(sendData, 0, sendData.Length);

                        // Aguardar resposta do servidor
                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                        message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        
                    }
                    else if (resposta == "N")
                    {
                        sendMessage = "TASK_COMPLETED:" + resposta + "\n";
                        sendData = Encoding.ASCII.GetBytes(sendMessage);
                        stream.Write(sendData, 0, sendData.Length);

                        // Aguardar resposta do servidor
                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                        message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        
                    }
                    else
                    {
                        Console.WriteLine("Resposta invalida. Por favor, responda com 'S' para sim ou 'N' para nao.");
                    }
                } while (resposta != "S" && resposta != "N");


                // Aguardar resposta do servidor
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Servidor: " + message);


                Console.Write("Solicitar nova tarefa? (S/N): ");               
                string resposta1 = Console.ReadLine().ToUpper();

                
                do
                {
                    if (resposta1 == "S")
                    {
                        sendMessage = "REQUEST_TASK:" + resposta1 + "\n";
                        sendData = Encoding.ASCII.GetBytes(sendMessage);
                        stream.Write(sendData, 0, sendData.Length);
                        // Aguardar resposta do servidor
                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                        message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        

                    }
                    else if (resposta1 == "N")
                    {
                        sendMessage = "REQUEST_TASK:" + resposta1 + "\n";
                        sendData = Encoding.ASCII.GetBytes(sendMessage);
                        stream.Write(sendData, 0, sendData.Length);
                        // Aguardar resposta do servidor
                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                        message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Nao lhe sera atribuida outra tarefa");
                        


                    }
                    else
                    {
                        Console.WriteLine("Resposta invalida. Por favor, responda com 'S' para sim ou 'N' para nao.");
                        resposta = Console.ReadLine().ToUpper(); // Solicitar nova entrada
                    }
                } while (resposta1 != "S" && resposta1 != "N");



                // Solicitar ao usuário para encerrar a comunicação
                Console.WriteLine("Digite 'QUIT' para encerrar a comunicação.");
                string userInput = Console.ReadLine();

                // Enviar mensagem de encerramento para o servidor
                sendMessage = userInput + "\n";
                sendData = Encoding.ASCII.GetBytes(sendMessage);
                stream.Write(sendData, 0, sendData.Length);

                // Loop para continuar aguardando a resposta do servidor até receber "400 BYE"
                while (true)
                {
                    // Aguardar resposta do servidor
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Servidor: " + message);

                    // Verificar se a mensagem recebida é "400 BYE" e encerrar a conexão
                    if (message.Trim() == "400 BYE")
                    {
                        Console.WriteLine("Comunicação encerrada pelo servidor.");
                        break;
                    }
                }

                // Fechar a conexão com o servidor
                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro: " + ex.Message);
            }
        }
    }
}
