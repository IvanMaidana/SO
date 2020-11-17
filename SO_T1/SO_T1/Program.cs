﻿using System;
using System.Collections.Generic;
using System.IO;

namespace SO_T1
{
    class CPU
    {
        public Status status = new Status(); // estado da CPU

        public string[] programMemory; // memoria de programa
        public int[] dataMemory; // memoria de dados
    }

    class Status
    {
        public int PC { get; set; } // registrador | o contador de programa
        public int A { get; set; } // registrador | acumulador
        public int InterruptionCode { get; set; }// codigo de interrupcao // 0 normal | 1 instrucao ilegal | 2 violacao de memoria
    }

    class Program
    {
        public const int normal = 0;
        public const int ilegal = 1;
        public const int violacao = 2;

        public const bool debugMode = true;


        static void Main(string[] args)
        {
            string path = "C://teste/SO.txt"; // diretorio
            int[] dados = new int[4]; // // dados do programa

            CPU cpu = new CPU(); // instancia da CPU
            Status status = new Status(); // instancia dos status do CPU

            if (File.Exists(path)) // confere se o diretorio existe e le o seu conteudo
            {
                string content = File.ReadAllText(path);
                string[] array = content.Split("\n");
                SetCPUProgramMemory(cpu, array); // manda as instrucoes para a cpu
            }
            else
            {
                Console.WriteLine("Path " + path + " was not found.");
            }

            for (int i = 0; i < cpu.programMemory.Length; i++) // exibe o conteudo da memoria de programa [instrucoes]
            {
                Console.WriteLine(i + " : " + cpu.programMemory[i]);
            }

            InitializeCPU(status); // inicializa o estado da cpu

            UpdateCPUStatus(cpu, status); // altera o estado da cpu

            SetCPUDataMemory(cpu, dados); // envia os dados para a cpu

            while (GetCPUInterruptionCode(cpu) == normal)
            {
                ExecuteCPU(cpu); // executa as instrucoes caso a cpu esteja em estado normal
            }

            if(debugMode)
            {
                Console.WriteLine("----------");

                Console.WriteLine("CPU parou na instrução " + cpu.programMemory[cpu.status.PC] + " (deve ser PARA)");
                Console.WriteLine("O valor de m[0] é " + cpu.dataMemory[0] + " (deve ser 42)");
            }
        }

        // alterar o conteúdo da memória de programa (recebe um vetor de strings)
        static void SetCPUProgramMemory(CPU cpu, string[] newData)
        {
            cpu.programMemory = newData;
        }

        // alterar o conteúdo da memória de dados (recebe um vetor de inteiros, que é alterado pela execução das instruções)
        static void SetCPUDataMemory(CPU cpu, int[] newData)
        {
            cpu.dataMemory = newData;
        }

        // obter o conteúdo da memória de dados (retorna um vetor de inteiros que é o conteúdo atual da memória – não precisa desta função caso o vetor passado pela função acima seja alterado “in loco”)
        static int[] GetCPUDataMemory(CPU cpu)
        {
            return cpu.dataMemory;
        }

        // ler o modo de interrupção da CPU (normal ou um motivo de interrupção)
        static int GetCPUInterruptionCode(CPU c)
        {
            return c.status.InterruptionCode;
        }

        // colocar a CPU em modo normal (coresponde ao retorno de interrupção) – muda para modo normal e incrementa o PC; se já estiver em modo normal, não faz nada
        static int GetCPUInterruptionMode(CPU cpu)
        {
            return cpu.status.InterruptionCode;
        }

        // obter a instrução em PC (que pode ser inválida se PC estiver fora da memória de programa)
        static string GetPCInstruction(CPU cpu)
        {
            return cpu.programMemory[cpu.status.PC];
        }

        // obter o estado interno da CPU (retorna o valor de todos os registradores)
        static Status GetCPUStatus(CPU cpu)
        {
            return cpu.status;
        }

        // alterar o estado interno da CPU (copia para os registradores da cpu os valores recebidos)
        static void UpdateCPUStatus(CPU cpu, Status e)
        {
            cpu.status = e;
        }

        // inicializar o estado interno da CPU (PC=0, A=0, estado=normal)
        static void InitializeCPU(Status e)
        {
            e.PC = 0;
            e.A = 0;
            e.InterruptionCode = normal; // normal
        }

        // executar uma instrução (só executa se estiver em modo normal)
        static void ExecuteCPU(CPU cpu)
        {
            if(GetCPUInterruptionMode(cpu) == normal)
            {
                bool updatePC = true; // controla se o PC devera ser atualizado ao terminar a execucao da instrucao

                Status status = GetCPUStatus(cpu);

                string origem = GetPCInstruction(cpu); // retorna a instrucao atual do PC

                string[] instrucao = origem.Split(' ');
                string instruction = instrucao[0];

                string valueTmp = origem.Remove(0, origem.IndexOf(' ') + 1);

                int value = 0;
                if (Char.IsDigit(valueTmp[0]))
                    value = Convert.ToInt32(valueTmp);

                if (debugMode)
                {
                    Console.WriteLine("instruction: " + instruction);
                    Console.WriteLine("Value: " + value);
                }

                if (instruction == "CARGI") // coloca o valor n no acumulador (A=n)
                {
                    status.A = value;
                }

                else if (instruction == "CARGM") // coloca no acumulador o valor na posição n da memória de dados (A=M[n])
                {
                    int[] data = GetCPUDataMemory(cpu);
                    status.A = data[value];
                }

                else if (instruction == "CARGX") // coloca no acumulador o valor na posição que está na posição n da memória de dados (A=M[M[n]])
                {
                    int[] data = GetCPUDataMemory(cpu);
                    int pos = data[value];
                    status.A = pos;
                    //status.A = cpu.dataMemory[cpu.dataMemory[value]];
                }

                else if (instruction == "ARMM") // coloca o valor do acumulador na posição n da memória de dados (M[n]=A)
                {
                    int[] data = GetCPUDataMemory(cpu);
                    data[value] = status.A;
                    SetCPUDataMemory(cpu, data);
                    //cpu.dataMemory[value] = cpu.status.A;
                }

                else if (instruction == "ARMX") // 	coloca o valor do acumulador posição que está na posição n da memória de dados (M[M[n]]=A)
                {
                    int[] data = GetCPUDataMemory(cpu);
                    int pos = data[value];
                    data[pos] = status.A;
                    SetCPUDataMemory(cpu, data);
                    //cpu.dataMemory[cpu.dataMemory[value]] = cpu.status.A;
                }

                else if (instruction == "SOMA") // 	soma ao acumulador o valor no endereço n da memória de dados (A=A+M[n])
                {
                    status.A = status.A + cpu.dataMemory[value];
                }

                else if (instruction == "NEG") // 	inverte o sinal do acumulador (A=-A)
                {
                    status.A *= -1;
                }

                else if (instruction == "DESVZ") // se A vale 0, coloca o valor n no PC
                {
                    if (status.A == 0)
                    {
                        status.PC = value;
                        updatePC = false;
                    }
                }

                else // coloca a CPU em interrupção – instrução ilegal
                {
                    status.InterruptionCode = ilegal;
                    updatePC = false;
                }

                UpdateCPUStatus(cpu, status); // atualiza o estado da CPU com os novos dados

                if (updatePC) { cpu.status.PC++; }

            }
        }

    }
}
