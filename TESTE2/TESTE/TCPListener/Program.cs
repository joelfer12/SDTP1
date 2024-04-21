using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiMoto_Server
{
    class Program
    {
       
        static void Main(string[] args)
        {
            // Defina o endereço IP e a porta do servidor
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 8888;

            // Inicie o servidor
            TcpListener server = new TcpListener(ipAddress, port);
            server.Start();
            Console.WriteLine("Servidor ServiMoto iniciado...");

            // Loop infinito para aceitar conexões dos clientes
            while (true)
            {
                // Aceitar a conexão do cliente
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Cliente conectado!");

                // Iniciar uma nova thread para lidar com o cliente
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.Start(client);
            }
        }

        // Método para lidar com as solicitações do cliente
        static void HandleClient(object obj)
        {
            // Obtenha o cliente TCP
            TcpClient client = (TcpClient)obj;

            // Obtenha o stream de dados para comunicação
            NetworkStream stream = client.GetStream();

            // Envie a mensagem de "100 OK" ao cliente
            string response = "100 OK\n";
            byte[] data = Encoding.ASCII.GetBytes(response);
            stream.Write(data, 0, data.Length);

            // Loop para receber e processar as mensagens do cliente
            while (true)
            {
                // Leia a mensagem do cliente
                byte[] buffer = new byte[5000];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim().ToUpper(); // Converta para maiúsculas

                // Processar a mensagem do cliente
                if (message.StartsWith("ID_DO_CLIENTE:"))
                {
                    // Processar o ID do cliente
                    string clientId = message.Substring("ID_DO_CLIENTE:".Length).Trim();
                    Console.WriteLine("ID do Cliente: " + clientId);

                    if (!CheckIfClientExists(clientId))
                    {
                        // Cliente não existe, então atualize o arquivo com o novo cliente e um serviço aleatório
                        string randomService = GetRandomServiceId();
                        UpdateAllocationFile(clientId, randomService);
                        Console.WriteLine($"Novo cliente adicionado: {clientId}, Servico: {randomService}");
                    }

                    // Consultar o arquivo de alocação para encontrar o serviço associado ao cliente
                    string serviceId = GetServiceForClient(clientId);
                     
                    if (!string.IsNullOrEmpty(serviceId))
                    {
                        response = "Servico associado ao cliente: " + serviceId + "\n";

                        // Verificar se o cliente já possui uma tarefa associada
                        string taskInfo = GetTaskForClient(serviceId, clientId);
                        if (!string.IsNullOrEmpty(taskInfo))
                        {
                            // Cliente possui uma tarefa associada
                            response += "Tarefa associada ao cliente: " + taskInfo + "\n";

                            // Verificar se a tarefa está concluída
                            string[] taskParts = taskInfo.Split(',');
                            if (taskParts.Length == 3 && taskParts[2].Trim() == "Concluido")
                            {
                                // A tarefa já está concluída, não precisa de confirmação
                                response += "Esta tarefa ja esta concluída.\n";
                            }
                            else
                            {
                                // Solicitar confirmação para concluir a tarefa
                                
                                // Enviar resposta ao cliente
                                data = Encoding.ASCII.GetBytes(response);
                                stream.Write(data, 0, data.Length);
                                // Receber resposta do cliente
                                bytesRead = stream.Read(buffer, 0, buffer.Length);
                                message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim().ToUpper();

                                if (message.StartsWith("TASK_COMPLETED:")) { 
                                    string resposta = message.Substring("TASK_COMPLETED:".Length).Trim();
                               

                                if (resposta == "S")
                                {
                                        // Cliente confirmou a conclusão da tarefa
                                        // Atualizar o estado da tarefa para "Concluido"
                                        string taskId = taskParts[0].Trim();
                                    UpdateTaskState(serviceId, taskId);

                                    response = "Tarefa concluida com sucesso.\n";
                                }
                                
                                else if (resposta == "N")
                                {
                                    // Cliente não confirmou a conclusão da tarefa
                                    response = "A sua tarefa continuara em curso.\n";
                                }
                                

                                // Enviar resposta ao cliente
                                data = Encoding.ASCII.GetBytes(response);
                                stream.Write(data, 0, data.Length);
                                }
                            }
                            
                        }
                        else
                        {
                            // Cliente não possui uma tarefa associada, então você pode atribuir uma nova tarefa
                            string taskDescription = GetFirstUnallocatedTask(serviceId, clientId);
                            if (!string.IsNullOrEmpty(taskDescription))
                            {
                                // Associar a tarefa ao cliente
                                // Aqui você atualizaria o estado da tarefa para "Em curso" e associaria o ID do cliente a ela no arquivo de serviço correspondente
                               
                                response += "Nova tarefa atribuida ao cliente: " + taskDescription + "\n";

                                
                                
                                // Solicitar confirmação para concluir a tarefa
                                data = Encoding.ASCII.GetBytes(response);
                                stream.Write(data, 0, data.Length);
                                // Receber resposta do cliente
                                bytesRead = stream.Read(buffer, 0, buffer.Length);
                                message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim().ToUpper();

                                

                                if (message.StartsWith("TASK_COMPLETED:"))
                                {
                                    string resposta = message.Substring("TASK_COMPLETED:".Length).Trim();
                                    string[] taskParts = taskDescription.Split(',');
                                    
                                    Console.WriteLine("Resposta: " + resposta);
                                    if (resposta == "S")
                                    {
                                        // Cliente confirmou a conclusão da tarefa
                                        // Atualizar o estado da tarefa para "Concluido"
                                        string taskId = taskParts[0].Trim();
                                        UpdateTaskState(serviceId, taskId);
                                       

                                        response = "Tarefa concluida com sucesso.\n";

                                        // Receber resposta do cliente
                                        // Enviar resposta ao cliente

                                        
                                        // Solicitar confirmação para concluir a tarefa

                                        // Enviar resposta ao cliente
                                        data = Encoding.ASCII.GetBytes(response);
                                        stream.Write(data, 0, data.Length);
                                        stream.Write(data, 0, data.Length);
                                        stream.Flush();
                                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                                        message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim().ToUpper();

                                        
                                        
                                        
                                        if (message.StartsWith("REQUEST_TASK:"))
                                        {
                                            resposta = message.Substring("REQUEST_TASK:".Length).Trim();
                                            
                                            

                                            if (resposta == "S")
                                            {
                                               
                                                taskDescription = GetFirstUnallocatedTask(serviceId, clientId);
                                                if (!string.IsNullOrEmpty(taskDescription))
                                                {
                                                    // Atribuir a nova tarefa ao cliente
                                                    response += "Nova tarefa atribuida ao cliente: " + taskDescription + "\n";
                                                }
                                                else
                                                {
                                                    // Não há mais tarefas disponíveis
                                                    response += "Nao ha mais tarefas disponíveis no momento.\n";
                                                }


                                            } 
                                            else if(resposta =="N")
                                            {
                                                if (message == "QUIT")
                                                {// Enviar mensagem de "400 BYE" e encerrar a conexão
                                                    response = "400 BYE\n";
                                                    data = Encoding.ASCII.GetBytes(response);
                                                    stream.Write(data, 0, data.Length);
                                                    Console.WriteLine("Cliente desconectado.");
                                                    stream.Close();
                                                    client.Close();

                                                    break; // Sair do loop para encerrar a conexão
                                                }
                                            }
                                        }

                                    }

                                    else if (resposta == "N")
                                    {
                                        // Cliente não confirmou a conclusão da tarefa
                                        response = "A sua tarefa continuara em curso.\n";
                                        // Enviar resposta ao cliente
                                        data = Encoding.ASCII.GetBytes(response);
                                        stream.Write(data, 0, data.Length);
                                    }
                                    


                                   
                                }
                            }
                            else
                            {
                                // Não há tarefas disponíveis
                                response += "Nao ha tarefas disponíveis no momento.\n";
                            }


                            
                        }
                    }
                    else
                    {
                        response = "Erro: Cliente não associado a nenhum serviço.\n";
                    }

                // Enviar resposta ao cliente
                data = Encoding.ASCII.GetBytes(response);
                    stream.Write(data, 0, data.Length);
                }
                else if (message == "QUIT")
                {
                    // Enviar mensagem de "400 BYE" e encerrar a conexão
                    response = "400 BYE\n";
                    data = Encoding.ASCII.GetBytes(response);
                    stream.Write(data, 0, data.Length);
                    Console.WriteLine("Cliente desconectado.");
                    stream.Close();
                    client.Close();

                    break; // Sair do loop para encerrar a conexão
                }
                else
                {

                    // Mensagem inválida
                   
                    response = "Erro: Comando nao reconhecido.\n";
                    data = Encoding.ASCII.GetBytes(response);
                    stream.Write(data, 0, data.Length);
                }
            }
        }

        
        // Método para obter o serviço associado a um cliente
        static string GetServiceForClient(string clientId)
        {
            Console.WriteLine("Procurando serviço para o cliente: " + clientId);

            // Lógica para consultar o arquivo de alocação e retornar o serviço associado ao cliente
            using (StreamReader reader = new StreamReader("Alocacao_Cliente_Servico.csv"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine("Lendo linha do arquivo: " + line);
                    string[] parts = line.Split(',');
                    if (parts.Length == 2 && parts[0].Trim().Equals(clientId, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Serviço encontrado: " + parts[1].Trim());
                        return parts[1].Trim();
                    }
                }
            }

            Console.WriteLine("Cliente não associado a nenhum serviço.");
            return null; // Retorna null se o cliente não estiver associado a nenhum serviço
        }

        // Método para verificar se o cliente já existe no arquivo de alocação de serviço para cliente
        static bool CheckIfClientExists(string clientId)
        {
            using (StreamReader reader = new StreamReader("Alocacao_Cliente_Servico.csv"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 2 && parts[0].Trim().Equals(clientId, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Método para atualizar o arquivo de alocação de serviço para cliente com um novo cliente e um serviço aleatório
        static void UpdateAllocationFile(string clientId, string serviceId)
        {
            using (StreamWriter writer = File.AppendText("Alocacao_Cliente_Servico.csv"))
            {
                writer.WriteLine(clientId + "," + serviceId);
            }
        }

        // Método para atualizar o estado de uma tarefa para "Concluído"
        static void UpdateTaskState(string serviceId, string taskId)
        {
            // Identificar o arquivo de serviço correspondente
            string filename = serviceId + ".csv";
            string tempFile = "temp.csv";

            Console.WriteLine("Atualizando estado da tarefa...");

            // Abrir o arquivo de serviço original para leitura e um arquivo temporário para escrita
            using (StreamReader reader = new StreamReader(filename))
            using (StreamWriter writer = new StreamWriter(tempFile))
            {
                string line;
                // Ler e processar cada linha do arquivo
                while ((line = reader.ReadLine()) != null)
                {
                    // Dividir a linha nos elementos separados por vírgula
                    string[] parts = line.Split(',');
                    if (parts.Length == 4 && parts[0].Trim().Equals(taskId, StringComparison.OrdinalIgnoreCase))
                    {
                        // Se o ID da tarefa corresponder ao ID fornecido
                        Console.WriteLine("Tarefa encontrada: " + line);

                        // Atualizar o estado da tarefa para "Concluído"
                        parts[2] = "Concluido";
                        Console.WriteLine("Estado da tarefa atualizado para 'Concluido'.");

                        // Escrever a linha atualizada no arquivo temporário
                        writer.WriteLine(string.Join(",", parts));
                    }
                    else
                    {
                        // Se o ID da tarefa não corresponder ao ID fornecido
                        // Escrever a linha sem fazer alterações
                        writer.WriteLine(line);
                    }
                }
            }

            // Substituir o arquivo original pelo arquivo temporário
            File.Delete(filename);
            File.Move(tempFile, filename);

            Console.WriteLine("Estado da tarefa atualizado com sucesso.");
        }


        // Método para obter um serviço aleatório entre Servico_A e Servico_D
        static string GetRandomServiceId()
        {
            string[] serviceIds = { "Servico_A", "Servico_B", "Servico_C", "Servico_D" };
            Random rand = new Random();
            return serviceIds[rand.Next(0, serviceIds.Length)];
        }

        //// Método para ler o arquivo CSV correspondente a um serviço
        //static void ReadServiceFile(string filePath)
        //{
        //    // Use um Mutex para garantir que apenas uma thread acesse o arquivo por vez
        //    Mutex mutex;
        //    if (filePath.EndsWith("A.csv"))
        //        mutex = mutexA;
        //    else if (filePath.EndsWith("B.csv"))
        //        mutex = mutexB;
        //    else if (filePath.EndsWith("C.csv"))
        //        mutex = mutexC;
        //    else if (filePath.EndsWith("D.csv"))
        //        mutex = mutexD;
        //    else
        //        return; // Arquivo não reconhecido, retornar

        //    mutex.WaitOne(); // Aguardar o Mutex

        //    try
        //    {
        //        // Lógica para ler e processar o arquivo CSV aqui
        //        using (StreamReader reader = new StreamReader(filePath))
        //        {
        //            string line;
        //            while ((line = reader.ReadLine()) != null)
        //            {
        //                // Processar cada linha do arquivo
        //                string[] parts = line.Split(',');
        //                if (parts.Length == 4)
        //                {
        //                    string taskId = parts[0];
        //                    string description = parts[1];
        //                    string state = parts[2];
        //                    string clientId = parts[3];

        //                    // Agora você tem os dados de cada tarefa, faça o que for necessário com eles
        //                    Console.WriteLine($"TarefaID: {taskId},Descrição: {description},Estado: {state},ClienteID: {clientId}");
        //                }
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        mutex.ReleaseMutex(); // Liberar o Mutex após a leitura do arquivo
        //    }
        //}

        // Método para obter a primeira tarefa não alocada dentro de um serviço e atribuí-la ao novo cliente

        // Mutexes para garantir o acesso sequencial aos arquivos de serviço
        static Mutex mutexA = new Mutex();
        static Mutex mutexB = new Mutex();
        static Mutex mutexC = new Mutex();
        static Mutex mutexD = new Mutex();

        // Método para obter o Mutex correspondente ao serviço
        static Mutex GetMutexForService(string serviceId)
        {
            switch (serviceId)
            {
                case "Servico_A":
                    return mutexA;
                case "Servico_B":
                    return mutexB;
                case "Servico_C":
                    return mutexC;
                case "Servico_D":
                    return mutexD;
                default:
                    // Se o serviço não for reconhecido, retorna null
                    return null;
            }
        }
        static string GetFirstUnallocatedTask(string serviceId, string clientId)
        {
            Console.WriteLine("Procurando primeira tarefa não alocada para o serviço: " + serviceId);

            // Lógica para consultar o arquivo de serviço e retornar a primeira tarefa não alocada
            string filename = serviceId + ".csv";
            if (File.Exists(filename))
            {
                Console.WriteLine("Lendo arquivo de serviço: " + filename);
                string[] lines = File.ReadAllLines(filename);
                for (int i = 1; i < lines.Length; i++) // Começar do índice 1 para pular a linha de cabeçalho
                {
                    Console.WriteLine("Lendo linha do arquivo: " + lines[i]);
                    string[] parts = lines[i].Split(',');
                    if (parts.Length == 4 && string.IsNullOrEmpty(parts[3].Trim()) && parts[2].Trim() == "Nao alocado")
                    {
                        // Aqui você pode atualizar o estado da tarefa para "Em curso" e associar o ID do cliente a ela no arquivo de serviço correspondente
                        Console.WriteLine("Tarefa não alocada encontrada: " + lines[i].Trim());

                        // Atualizar o estado da tarefa para "Em curso"
                        lines[i] = $"{parts[0].Trim()},{parts[1].Trim()},Em curso,{clientId}";
                        File.WriteAllLines(filename, lines);

                        

                        return lines[i].Trim(); // Retorna a linha completa da tarefa
                    }
                }
            }
            else
            {
                Console.WriteLine("Arquivo de serviço não encontrado: " + filename);
            }

            Console.WriteLine("Nenhuma tarefa não alocada encontrada.");
            return null; // Retorna null se não houver tarefas não alocadas ou se o arquivo de serviço não for encontrado
        }

        

        // Método para obter a tarefa associada a um cliente dentro de um serviço
static string GetTaskForClient(string serviceId, string clientId)
{
    Console.WriteLine("Procurando tarefa para o cliente " + clientId + " no serviço " + serviceId);

    // Lógica para ler o arquivo de serviço correspondente
    string filename = serviceId + ".csv";
    Mutex mutex = GetMutexForService(serviceId);
    mutex.WaitOne(); // Aguardar o Mutex

    try
    {
        if (File.Exists(filename))
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 4 && parts[3].Trim().Equals(clientId, StringComparison.OrdinalIgnoreCase)) // Compara o ClienteID insensível a maiúsculas e minúsculas
                    {
                        Console.WriteLine("Tarefa encontrada para o cliente " + clientId + ": " + line.Trim());
                        return $"{parts[0].Trim()},{parts[1].Trim()},{parts[2].Trim()}"; // Retorna o ID e a descrição da tarefa associada ao cliente e o estado
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("Arquivo de serviço não encontrado: " + filename);
        }

        Console.WriteLine("Nenhuma tarefa encontrada para o cliente " + clientId + " no serviço " + serviceId);
        return null; // Retorna null se o cliente não tiver uma tarefa associada neste serviço
    }
    finally
    {
        mutex.ReleaseMutex(); // Liberar o Mutex após a leitura do arquivo
    }
}
    }
}
