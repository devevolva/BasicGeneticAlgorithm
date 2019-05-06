///////////////////////// BASIC GENETIC ALGORITHM /////////////////////////////
//
// Port from c++ -> c# of a basic genetic algorithm implementation by Mat 
// Buckland (aka fup) from www.ai-junkie.com
//
// john@devevolva.com
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

namespace Genetic_Algorithm
{
    public static class Extension
    {
        public static string ReplaceAt(this string input, int index, char newChar)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            char[] chars = input.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }
    }


    class Program
    {
        // const end of line comments are original defaults
        const double CROSSOVER_RATE = 0.7; // 0.7
        const double MUTATION_RATE = 0.100; // 0.001
        const int POP_SIZE = 300; // 100 (must be even)
        const int CHROMO_LENGTH = 300; // 300
        const int GENE_LENGTH = 4; // 4
        const int MAX_ALLOWABLE_GENERATIONS = 400; // 400

        private static Random rnd = new Random();

        public struct Chromosome
        {
            public string bits;
            public double fitness;

            public Chromosome(string bits, double fitness)
            {
                this.bits = bits;
                this.fitness = fitness;
            }
        }

        static string BuildChromosome(int length)
        {
            string bits = "";

            for (int i = 0; i < length; i++)
            {
                bits += rnd.Next(0, 2);
            }

            return bits;
        }

        public static void PrintChromosome(string bits)
        {
            int[] buffer = new int[(int)(CHROMO_LENGTH / GENE_LENGTH)];
            int numElements = ParseBits(ref bits, buffer);

            // added implicit 0 to beginning of printed chromosome so
            // when I manually verify them it isn't left out. 
            Console.Write("0 ");

            for (int i = 0; i < numElements; i++)
            {
                PrintGeneSymbol(buffer[i]);
            }
        }

        public static void PrintGeneSymbol(int gene)
        {
            if (gene < 10)
                Console.Write(gene + " ");
            else
            {
                switch (gene)
                {
                    case 10:
                        Console.Write("+");
                        break;
                    case 11:
                        Console.Write("-");
                        break;
                    case 12:
                        Console.Write("*");
                        break;
                    case 13:
                        Console.Write("/");
                        break;
                }

                Console.Write(" ");
            }
        }

        // Converts binary string into base10 number
        public static int BinToDec(string bits)
        {
            int value = 0;
            int valueToAdd = 1;

            for (int i = bits.Length; i > 0; i--)
            {
                if (bits.ElementAt(i - 1) == '1')
                    value += valueToAdd;
                valueToAdd *= 2;
            }

            return value;
        }

        // parse chromesome into base10 numbers and put into buffer
        public static int ParseBits(ref string bits, int[] buffer)
        {
            int bufferIndex = 0;
            bool isOperator = true;
            int thisGene = 0;

            for (int i = 0; i < CHROMO_LENGTH; i += GENE_LENGTH)
            {
                thisGene = BinToDec(bits.Substring(i, GENE_LENGTH));

                if (isOperator)
                {
                    if ((thisGene < 10) || (thisGene > 13))
                        continue;
                    else
                    {
                        isOperator = false;
                        buffer[bufferIndex++] = thisGene;
                        continue;
                    }
                }
                else
                {
                    if (thisGene > 9)
                        continue;
                    else
                    {
                        isOperator = true;
                        buffer[bufferIndex++] = thisGene;
                        continue;
                    }
                }
            }

            for (int i = 0; i < bufferIndex; i++)
            {
                if ((buffer[i] == 13) && (buffer[i + 1] == 0))
                    buffer[i] = 10;
            }

            return bufferIndex;
        }

        // given a chromosome and target value calculate fitness score
        public static double AssignFitness(string bits, int target)
        {
            int[] buffer = new int[(int)(CHROMO_LENGTH / GENE_LENGTH)];
            int numElements = ParseBits(ref bits, buffer);
            double result = 0.0d;

            for (int i = 0; i < numElements - 1; i += 2)
            {
                switch (buffer[i])
                {
                    case 10:
                        result += buffer[i + 1];
                        break;
                    case 11:
                        result -= buffer[i + 1];
                        break;
                    case 12:
                        result *= buffer[i + 1];
                        break;
                    case 13:
                        result /= buffer[i + 1];
                        break;
                }
            }

            if (result == (double)target)
                return 999.0d;
            else
                return 1 / (double)Math.Abs((double)(target - result));
        }

        public static class TestFitness
        {
            // Roulette method fitness test for reproduction
            public static string Roulette(int totalFitness, Chromosome[] population)
            {
                double slice = rnd.NextDouble();
                double currentFitness = 0.0d;

                for (int i = 0; i < POP_SIZE; i++)
                {
                    currentFitness += population[i].fitness;

                    if (currentFitness >= slice)
                        return population[i].bits;
                }
                return population[rnd.Next(0, POP_SIZE)].bits;
            }
        }

        // modifies genes during the process of reproduction
        public static class Polymerase
        {
            // gene crossover between parent chromosomes
            public static void Crossover(ref string offspring1, ref string offspring2)
            {
                if (rnd.NextDouble() < CROSSOVER_RATE)
                {
                    int crossover = (int)(rnd.NextDouble() * CHROMO_LENGTH);

                    string t1 = offspring1.Substring(0, crossover) + offspring2.Substring(crossover, CHROMO_LENGTH - crossover);
                    string t2 = offspring2.Substring(0, crossover) + offspring1.Substring(crossover, CHROMO_LENGTH - crossover);

                    offspring1 = t1;
                    offspring2 = t2;
                }
            }

            // gene mutation during reproduction process
            public static void Mutate(ref string bits)
            {
                for (int i = 0; i < bits.Length; i++)
                {
                    if (rnd.NextDouble() < MUTATION_RATE)
                    {
                        if (bits.ElementAt(i) == '1')
                            bits.ReplaceAt(i, '0');
                        else
                            bits.ReplaceAt(i, '1');
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            // Main Loop
            while (true)
            {
                double target = 0.0d;

                Console.WriteLine("Starting New Run ...");
                Console.WriteLine("Enter a target number: ");
                target = Double.Parse(Console.ReadLine());
                Console.WriteLine("");


                Chromosome[] population = new Chromosome[POP_SIZE];

                for (int i = 0; i < POP_SIZE; i++)
                {
                    population[i].bits = BuildChromosome(CHROMO_LENGTH);
                    population[i].fitness = 0.0d;
                }

                int numGenerationsTillSolution = 0;
                bool found = false;

                // GA Loop
                while (!found)
                {
                    double totalFitness = 0.0d;

                    for (int i = 0; i < POP_SIZE && !found; i++)
                    {
                        population[i].fitness = AssignFitness(population[i].bits, (int)target);

                        if (population[i].fitness == 999.0d)
                        {
                            Console.WriteLine("Solution found! Number of Generations till Solution = " + numGenerationsTillSolution);
                            Console.WriteLine("");
                            PrintChromosome(population[i].bits);

                            found = true;
                        }

                        totalFitness += population[i].fitness;
                    }

                    if (!found)
                    {
                        // No solution found so create new population based on fitness, roulette, crossover and mutation
                        Chromosome[] newPopulation = new Chromosome[POP_SIZE];
                        int popIndex = 0;

                        while (popIndex < POP_SIZE)
                        {
                            string offspring1 = TestFitness.Roulette((int)totalFitness, population);
                            string offspring2 = TestFitness.Roulette((int)totalFitness, population);

                            Polymerase.Crossover(ref offspring1, ref offspring2);

                            Polymerase.Mutate(ref offspring1);
                            Polymerase.Mutate(ref offspring2);

                            newPopulation[popIndex++] = new Chromosome(offspring1, 0.0d);
                            newPopulation[popIndex++] = new Chromosome(offspring2, 0.0d);
                        }

                        newPopulation.CopyTo(population, 0);

                        ++numGenerationsTillSolution;
                        if (numGenerationsTillSolution > MAX_ALLOWABLE_GENERATIONS)
                        {
                            found = true;
                            Console.WriteLine("Exiting run, no solutions found in " + MAX_ALLOWABLE_GENERATIONS + " generations.");
                            Console.WriteLine("");
                        }
                    }
                }// end GA Loop while

            }// end Main Loop while

        }// Main()

    }// class Program
}// Gentic Algorithm namespace
