using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace коммивояжер
{
    class Path : IComparable
    {
        public int Length;
        public int[] path;
        public double Weight = 0;

        public Path(int[] enter_path)
        {
            path = new int[enter_path.Length];
            Length = enter_path.Length;
        }

        public void CalcWeight(double[,] RoadMat)
        {
            double path_weight = 0;
            for (int i = 1; i < path.Length; i++)
            {
                path_weight += RoadMat[path[i - 1], path[i]];
            }
            path_weight += RoadMat[path.Length - 1, 0];
            Weight = path_weight;
        }

        public void Clear()
        {
            Weight = 0;
            Array.Clear(path, 0, path.Length);
            path = new int[Length];
        }

        public Path Clone()
        {
            Path newPath = new Path(new int[Length]);
            newPath.Length = this.Length;
            newPath.Weight = this.Weight;

            Array.Copy(this.path, newPath.path, this.Length);

            return newPath;
        }

        public int CompareTo(object obj)
        {
            if (obj is Path)
            {
                Path otherPath = (Path)obj;
                return Weight.CompareTo(otherPath.Weight);
            }

            throw new ArgumentException("Object is not a Path", nameof(obj));
        }

        public int this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException("Index is out of range");
                }
                return path[index];
            }
            set
            {
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException("Index is out of range");
                }
                path[index] = value;
            }
        }

        public void Print()
        {
            for (int i = 0; i < Length; i++)
            {
                Console.Write(path[i]);
                Console.Write(" ");
            }
            Console.WriteLine(Weight);
        }
    }
    class AntColony
    {
        private double alpha, beta, evaporationRate, pheroIncreasingRate;
        private int verticesCount;
        private double[,] distanceMatrix;
        private double[,] pheromoneMatrix;

        public int antCount;
        public double initialPheromone = 0.2;

        public AntColony(double[,] distanceMatrix, double alpha, double beta, double evaporationRate, double pheroIncreasingRate)
        {
            this.distanceMatrix = distanceMatrix;
            this.alpha = alpha;
            this.beta = beta;
            this.evaporationRate = evaporationRate;
            this.pheroIncreasingRate = pheroIncreasingRate;
            verticesCount = distanceMatrix.GetLength(0);
            antCount = verticesCount;

            pheromoneMatrix = new double[verticesCount, verticesCount];
            for (int i = 0; i < verticesCount; i++)
            {
                for (int j = 0; j < verticesCount; j++)
                {
                    pheromoneMatrix[i, j] = initialPheromone;
                }
            }
        }

        private int SelectNextVertex(int currentVertex, bool[] visited)
        {
            double[] probabilities = new double[verticesCount];
            double total = 0.0;

            for (int i = 0; i < verticesCount; i++)
            {
                if (!visited[i])
                {
                    probabilities[i] = Math.Pow(pheromoneMatrix[currentVertex, i], alpha) * Math.Pow(1.0 / distanceMatrix[currentVertex, i], beta);
                    total += probabilities[i];
                }
            }

            double randomValue = new Random().NextDouble();
            double cumulativeProbability = 0.0;

            for (int i = 0; i < verticesCount; i++)
            {
                if (!visited[i])
                {
                    probabilities[i] /= total;
                    cumulativeProbability += probabilities[i];

                    if (randomValue <= cumulativeProbability)
                    {
                        return i;
                    }
                }
            }

            for (int i = 0; i < verticesCount; i++)
            {
                if (!visited[i])
                {
                    return i;
                }
            }

            return -1;
        }

        private void UpdatePheromones(int[] path, double weight)
        {
            for (int i = 0; i < verticesCount - 1; i++)
            {
                int from = path[i];
                int to = path[i + 1];
                pheromoneMatrix[from, to] = (1.0 - evaporationRate) * pheromoneMatrix[from, to] + pheroIncreasingRate / weight;
                pheromoneMatrix[to, from] = pheromoneMatrix[from, to]; // Симметричное обновление
            }
        }

        private int[] AntRun()
        {
            int[] path = new int[verticesCount];
            bool[] visited = new bool[verticesCount];

            int startVertex = new Random().Next(verticesCount);
            path[0] = startVertex;
            visited[startVertex] = true;

            for (int i = 1; i < verticesCount; i++)
            {
                int nextVertex = SelectNextVertex(path[i - 1], visited);
                path[i] = nextVertex;
                visited[nextVertex] = true;
            }

            return path;
        }

        public void Solve(int iterations)
        {
            for (int iter = 0; iter < iterations; iter++)
            {
                for (int ant = 0; ant < antCount; ant++)
                {
                    int[] antPath = AntRun();
                    double antWeight = CalculatePathWeight(antPath);

                    UpdatePheromones(antPath, antWeight);
                }
            }

            int[] bestPath = AntRun();
            double bestWeight = CalculatePathWeight(bestPath);

            Console.WriteLine("Best Path: " + string.Join(" ", bestPath));
            Console.WriteLine("Best Weight: " + bestWeight);
        }

        private double CalculatePathWeight(int[] path)
        {
            double weight = 0.0;
            for (int i = 0; i < verticesCount - 1; i++)
            {
                weight += distanceMatrix[path[i], path[i + 1]];
            }
            weight += distanceMatrix[path[verticesCount - 1], path[0]]; // Замыкаем путь

            return weight;
        }
    }
    class Genetic
    {
        double[,] mat;
        private static int PopSize, GensCount, MutationDegree, a;
        public List<Path> Population = new List<Path>();
        private Random r = new Random();
        public Path currentAnswer;
        private Path child1;
        private Path child2;

        public Genetic(double[,] roadmatrix, int popSize, int gensCount, int mutationDegree, int vertices_count)
        {
            mat = roadmatrix;
            PopSize = popSize;
            GensCount = gensCount;
            MutationDegree = mutationDegree;
            a = vertices_count;
            child1 = new Path(new int[a]);
            child2 = new Path(new int[a]);
            currentAnswer = new Path(new int[a]);
        }
        public void GeneratePop()
        {
            for (int chromosome_ind = 0; chromosome_ind < PopSize; chromosome_ind++)
            {
                Population.Add(new Path(new int[a]));
                Population[chromosome_ind][0] = 0;

                if (true)
                {
                    int[] other = new int[a - 1];
                    for (int i = 1; i < a; i++) other[i - 1] = i;
                    int n = other.Length;
                    //Тасование Фишера — Йетса
                    while (n > 1)
                    {
                        n--;
                        int k = r.Next(n + 1);
                        int value = other[k];
                        other[k] = other[n];
                        other[n] = value;
                    }
                    for (int i = 0; i < a - 1; i++)
                    {
                        Population[chromosome_ind][i + 1] = other[i];
                        Population[chromosome_ind].Weight += mat[Population[chromosome_ind][i + 1], Population[chromosome_ind][i]];
                    }
                }
                Population[chromosome_ind].Weight += mat[Population[chromosome_ind][a - 1], Population[chromosome_ind][0]];
            }
            Population.Sort();
        }
        public void Interbreeding()
        {
            child1.Clear();
            child2.Clear();
            int parent1ind = r.Next(0, PopSize), parent2ind;
            while (true)
            {
                parent2ind = r.Next(0, PopSize);
                if (parent1ind != parent2ind) break;
            }
            for (int i = 0; i < a / 2; i++)
            {
                child1[i] = Population[parent1ind][i];
                child2[i] = Population[parent2ind][i];
                if (i != 0)
                {
                    child1.Weight += mat[child1[i], child1[i - 1]];
                    child2.Weight += mat[child2[i], child2[i - 1]];
                }
            }
            int child1currind = a / 2, child2currind = a / 2;
            for (int i = a / 2; i < a; i++)
            {
                if (!child1.path.Contains(Population[parent2ind][i]))
                {
                    child1[child1currind] = Population[parent2ind][i];
                    child1.Weight += mat[child1[child1currind], child1[child1currind - 1]];
                    child1currind++;
                }
                if (!child2.path.Contains(Population[parent1ind][i]))
                {
                    child2[child2currind] = Population[parent1ind][i];
                    child2.Weight += mat[child2[child2currind], child2[child2currind - 1]];
                    child2currind++;
                }
            }
            for (int i = a / 2; i < a; i++)
            {
                if (child1currind != a && !child1.path.Contains(Population[parent1ind][i]))
                {
                    child1[child1currind] = Population[parent1ind][i];
                    child1.Weight += mat[child1[child1currind], child1[child1currind - 1]];
                }
                if (child2currind != a && !child2.path.Contains(Population[parent2ind][i]))
                {
                    child2[child2currind] = Population[parent2ind][i];
                    child2.Weight += mat[child2[child2currind], child2[child2currind - 1]];
                }
            }
            child1.Weight += mat[child1[a - 1], child1[0]];
            child2.Weight += mat[child2[a - 1], child2[0]];
        }
        private double PathWeight(int[] path)
        {
            double path_weight = 0;
            for (int i = 1; i < path.Length; i++)
            {
                path_weight += mat[path[i], path[i - 1]];
            }
            path_weight += mat[0, path.Length - 1];
            return path_weight;
        }
        public void Mutation()
        {
            if (r.Next(0, 101) < MutationDegree)
            {
                int pos1 = r.Next(1, a), pos2;
                while (true)
                {
                    pos2 = r.Next(1, a);
                    if (pos1 != pos2) break;
                }
                int tmp = child1[pos2];
                child1[pos2] = child1[pos1];
                child1[pos1] = tmp;
                child1.Weight = PathWeight(child1.path);
            }
            if (r.Next(0, 101) < MutationDegree)
            {
                int pos1 = r.Next(1, a), pos2;
                while (true)
                {
                    pos2 = r.Next(1, a);
                    if (pos1 != pos2) break;
                }
                int tmp = child2[pos2];
                child2[pos2] = child2[pos1];
                child2[pos1] = tmp;
                child2.Weight = PathWeight(child2.path);
            }
        }
        public void IntegrateNewGen()
        {
            Population.Add(child1.Clone());
            Population.Add(child2.Clone());
            Population.Sort();
            Population.RemoveAt(PopSize + 1);
            Population.RemoveAt(PopSize);
        }
        public void Evolution(int gensCount)
        {
            for (int i = 0; i < gensCount; i++)
            {
                Interbreeding();
                Mutation();
                IntegrateNewGen();
                currentAnswer = Population[0];
                //Info();
            }
        }
        public void Info(bool fullinfo = false)
        {
            if (fullinfo)
            {
                Console.WriteLine($"Population: [len : {Population.Count}]");
                for (int i = 0; i < Population.Count; i++)
                {
                    for (int j = 0; j <= a; j++)
                    {
                        if (j == a)
                        {
                            Console.WriteLine(Population[i].Weight);
                        }
                        else
                        {
                            Console.Write(Population[i][j]);
                            Console.Write(' ');
                        }
                    }
                }
                Console.WriteLine("Childs:");
                for (int i = 0; i < child1.Length; i++)
                {
                    Console.Write(child1[i]);
                    Console.Write(' ');
                }
                Console.WriteLine(child1.Weight);
                for (int i = 0; i < child2.Length; i++)
                {
                    Console.Write(child2[i]);
                    Console.Write(' ');
                }
                Console.WriteLine(child1.Weight);
                Console.WriteLine($"Current answer:");
                for (int i = 0; i < currentAnswer.Length; i++)
                {
                    Console.Write(currentAnswer[i]);
                    Console.Write(' ');
                }
            }
            Console.WriteLine(currentAnswer.Weight);
        }
    }
    
    class Program
    {

        static void Main(string[] args)
        {

            for (int starts = 0; starts < 30; starts++)
            {
                Console.WriteLine($"Start No. {starts}: ");
                //Инициализация матрицы смежности
                int a = 30;
                double[,] mat = new double[a, a];
                for (int i = 0; i < a; i++)
                {
                    for (int j = 0; j < a; j++)
                    {
                        mat[i, j] = new Random().Next(0, 999);
                    }
                }

                //Генетический алгоритм
                Genetic pop = new Genetic(mat, a, 5, 5, a);
                pop.GeneratePop();
                pop.Evolution(10000);
                pop.Info();

                Console.WriteLine();

                //Муравьиный алгоритм
                AntColony colony = new AntColony(mat, 1, 4, 0.2, 50);
                colony.antCount = a;
                colony.Solve(10000);
            }
            Console.ReadKey();
        }
    }
}
