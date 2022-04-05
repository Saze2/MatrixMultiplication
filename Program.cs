using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace MatrixMultiplication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var matrixA = GenerateMatrix(300, 300);
            var matrixB = GenerateMatrix(300, 300);
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            var result1 = MultiplyMatricesNaive(matrixA, matrixB);
            stopwatch.Stop();
            Console.WriteLine("Naive: " + stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Reset();

            stopwatch.Start();
            var result2 = MultiplyMatricesNaiveParallel(matrixA, matrixB);
            stopwatch.Stop();
            Console.WriteLine("Naive Parallel: " + stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Reset();
            //Console.WriteLine(AssertEqualMatrix(result1, result2));


            stopwatch.Start();
            var result3 = MultiplyMatricesStrassen(matrixA, matrixB);
            stopwatch.Stop();
            Console.WriteLine("Strassens: " + stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Reset();
            //Console.WriteLine(AssertEqualMatrix(result1, result3));


            stopwatch.Start();
            var result4 = MultiplyMatricesStrassenParallel(matrixA, matrixB);
            stopwatch.Stop();
            Console.WriteLine("Strassens Parallel: " + stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Reset();
            //Console.WriteLine(AssertEqualMatrix(result1, result4));
        }

        static double[,] GenerateMatrix(int n, int m)
        {
            double[,] matrixC = new double[n, m];
            Random random = new Random();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    matrixC[i, j] = random.Next(0, 1000);
                }
            }
            return matrixC;
        }

        static void PrintMatrix(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            int m = matrix.GetLength(1);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    Console.Write(matrix[i, j] + "\t");
                }
                Console.WriteLine();

            }
            Console.WriteLine();
        }

        static bool AssertEqualMatrix(double[,] matrixA, double[,] matrixB)
        {
            int rowA = matrixA.GetLength(0);
            int colA = matrixA.GetLength(1);
            int rowB = matrixB.GetLength(0);
            int colB = matrixB.GetLength(1);

            if (rowA != rowB || colA != colB) return false;

            bool result = true;
            for (int i = 0; i < rowA; i++)
            {
                for (int j = 0; j < colA; j++)
                {
                    if (matrixA[i, j] != matrixB[i, j])
                    {
                        result = false;
                        break;
                    }
                    
                }
            }
            return result;
        }

        static double[,] MultiplyMatricesNaive(double[,] matrixA, double[,] matrixB)
        {
            int rowA = matrixA.GetLength(0);
            int colA = matrixA.GetLength(1);
            int rowB = matrixB.GetLength(0);
            int colB = matrixB.GetLength(1);

            //if (colA != rowB) throw new ArithmeticException("Cannot multiply the given matrices!");
            double[,] matrixResult = new double[colA, rowB];

            for (int i = 0; i < rowA; i++)
            {
                for (int j = 0; j < colB; j++)
                {
                    matrixResult[i, j] = 0;
                    for (int k = 0; k < colA; k++)
                        matrixResult[i, j] += matrixA[i, k] * matrixB[k, j];
                }
            }

            return matrixResult;
        }

        static double[,] MultiplyMatricesNaiveParallel(double[,] matrixA, double[,] matrixB)
        {
            int rowA = matrixA.GetLength(0);
            int colA = matrixA.GetLength(1);
            int rowB = matrixB.GetLength(0);
            int colB = matrixB.GetLength(1);

            double[,] matrixResult = new double[colA, rowB];

            Parallel.For(0, rowA, i =>
            {
                for (int j = 0; j < colB; j++)
                {
                    double temp = 0;
                    for (int k = 0; k < colA; k++)
                    {
                        temp += matrixA[i, k] * matrixB[k, j];
                    }
                    matrixResult[i, j] = temp;

                }
            });
            return matrixResult;
        }

        public static double[,] MultiplyMatricesStrassenParallel(double[,] A, double[,] B)
        {
            int n = A.GetLength(0);

            int lengthPadded = (int)Math.Pow(2, (int)Math.Ceiling(Math.Log(n) / Math.Log(2)));

            double[,] PaddedA = new double[lengthPadded, lengthPadded];
            double[,] PaddedB = new double[lengthPadded, lengthPadded];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    PaddedA[i, j] = A[i, j];
                    PaddedB[i, j] = B[i, j];
                }
            }

            double[,] matrixResultPadded = MultiplyMatricesStrassenRecursiveParallel(PaddedA, PaddedB);
            double[,] matrixResult = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrixResult[i, j] = matrixResultPadded[i, j];
                }
            }

            return matrixResult;
        }

        public static double[,] MultiplyMatricesStrassenRecursiveParallel(double[,] A, double[,] B)
        {
            int n = A.GetLength(0);
            double[,] matrixResult = new double[n, n];

            if (n <= 128)
            {
                return MultiplyMatricesNaive(A, B);
            }
            else
            {

                double[,] A11 = new double[n / 2, n / 2];
                double[,] A12 = new double[n / 2, n / 2];
                double[,] A21 = new double[n / 2, n / 2];
                double[,] A22 = new double[n / 2, n / 2];
                double[,] B11 = new double[n / 2, n / 2];
                double[,] B12 = new double[n / 2, n / 2];
                double[,] B21 = new double[n / 2, n / 2];
                double[,] B22 = new double[n / 2, n / 2];

                SplitMatrix(A, A11, 0, 0);
                SplitMatrix(A, A12, 0, n / 2);
                SplitMatrix(A, A21, n / 2, 0);
                SplitMatrix(A, A22, n / 2, n / 2);
                SplitMatrix(B, B11, 0, 0);
                SplitMatrix(B, B12, 0, n / 2);
                SplitMatrix(B, B21, n / 2, 0);
                SplitMatrix(B, B22, n / 2, n / 2);

                List<Task> tasks = new List<Task>();

                Task<double[,]> task1 = Task<double[,]>.Factory.StartNew(() => 
                MultiplyMatricesStrassenRecursiveParallel(MatrixAdd(A11, A22), MatrixAdd(B11, B22)),
                CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
                tasks.Add(task1);

                Task<double[,]> task2 = Task<double[,]>.Factory.StartNew(() =>
                MultiplyMatricesStrassenRecursiveParallel(MatrixAdd(A21, A22), B11));
                tasks.Add(task2);

                Task<double[,]> task3 = Task<double[,]>.Factory.StartNew(() =>
                MultiplyMatricesStrassenRecursiveParallel(A11, MatrixSubtract(B12, B22)));
                tasks.Add(task3);

                Task<double[,]> task4 = Task<double[,]>.Factory.StartNew(() =>
                MultiplyMatricesStrassenRecursiveParallel(A22, MatrixSubtract(B21, B11)));
                tasks.Add(task4);

                Task<double[,]> task5 = Task<double[,]>.Factory.StartNew(() =>
                MultiplyMatricesStrassenRecursiveParallel(MatrixAdd(A11, A12), B22));
                tasks.Add(task5);

                Task<double[,]> task6 = Task<double[,]>.Factory.StartNew(() =>
                MultiplyMatricesStrassenRecursiveParallel(MatrixSubtract(A21, A11), MatrixAdd(B11, B12)));
                tasks.Add(task6);

                Task<double[,]> task7 = Task<double[,]>.Factory.StartNew(() =>
                MultiplyMatricesStrassenRecursiveParallel(MatrixSubtract(A12, A22), MatrixAdd(B21, B22)));
                tasks.Add(task7);

                Task.WaitAll(tasks.ToArray());

                double[,] C11 = MatrixAdd(MatrixSubtract(MatrixAdd(task1.Result, task4.Result), task5.Result), task7.Result);
                double[,] C12 = MatrixAdd(task3.Result, task5.Result);
                double[,] C21 = MatrixAdd(task2.Result, task4.Result);
                double[,] C22 = MatrixAdd(MatrixSubtract(MatrixAdd(task1.Result, task3.Result), task2.Result), task6.Result);

                JoinMatrices(C11, matrixResult, 0, 0);
                JoinMatrices(C12, matrixResult, 0, n / 2);
                JoinMatrices(C21, matrixResult, n / 2, 0);
                JoinMatrices(C22, matrixResult, n / 2, n / 2);
            }

            return matrixResult;
        }

        public static double[,] MultiplyMatricesStrassen(double[,] A, double[,] B)
        {
            int n = A.GetLength(0);
            int lengthPadded = (int)Math.Pow(2, (int)Math.Ceiling(Math.Log(n) / Math.Log(2)));

            double[,] PaddedA = new double[lengthPadded, lengthPadded];
            double[,] PaddedB = new double[lengthPadded, lengthPadded];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    PaddedA[i, j] = A[i, j];
                    PaddedB[i, j] = B[i, j];
                }
            }

            double[,] matrixResultPadded = MultiplyMatricesStrassenRecursive(PaddedA, PaddedB);
            double[,] matrixResult = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrixResult[i, j] = matrixResultPadded[i, j];
                }
            }

            return matrixResult;
        }

        public static double[,] MultiplyMatricesStrassenRecursive(double[,] A, double[,] B)
        {
            int n = A.GetLength(0);
            double[,] matrixResult = new double[n, n];

            if (n <= 128)
            {
                return MultiplyMatricesNaive(A, B);
            }
            else
            {
                double[,] A11 = new double[n / 2, n / 2];
                double[,] A12 = new double[n / 2, n / 2];
                double[,] A21 = new double[n / 2, n / 2];
                double[,] A22 = new double[n / 2, n / 2];
                double[,] B11 = new double[n / 2, n / 2];
                double[,] B12 = new double[n / 2, n / 2];
                double[,] B21 = new double[n / 2, n / 2];
                double[,] B22 = new double[n / 2, n / 2];

                SplitMatrix(A, A11, 0, 0);
                SplitMatrix(A, A12, 0, n / 2);
                SplitMatrix(A, A21, n / 2, 0);
                SplitMatrix(A, A22, n / 2, n / 2);
                SplitMatrix(B, B11, 0, 0);
                SplitMatrix(B, B12, 0, n / 2);
                SplitMatrix(B, B21, n / 2, 0);
                SplitMatrix(B, B22, n / 2, n / 2);

                double[,] M1 = MultiplyMatricesStrassenRecursive(MatrixAdd(A11, A22), MatrixAdd(B11, B22));
                double[,] M2 = MultiplyMatricesStrassenRecursive(MatrixAdd(A21, A22), B11);
                double[,] M3 = MultiplyMatricesStrassenRecursive(A11, MatrixSubtract(B12, B22));
                double[,] M4 = MultiplyMatricesStrassenRecursive(A22, MatrixSubtract(B21, B11));
                double[,] M5 = MultiplyMatricesStrassenRecursive(MatrixAdd(A11, A12), B22);
                double[,] M6 = MultiplyMatricesStrassenRecursive(MatrixSubtract(A21, A11), MatrixAdd(B11, B12));
                double[,] M7 = MultiplyMatricesStrassenRecursive(MatrixSubtract(A12, A22), MatrixAdd(B21, B22));
                double[,] C11 = MatrixAdd(MatrixSubtract(MatrixAdd(M1, M4), M5), M7);
                double[,] C12 = MatrixAdd(M3, M5);
                double[,] C21 = MatrixAdd(M2, M4);
                double[,] C22 = MatrixAdd(MatrixSubtract(MatrixAdd(M1, M3), M2), M6);

                JoinMatrices(C11, matrixResult, 0, 0);
                JoinMatrices(C12, matrixResult, 0, n / 2);
                JoinMatrices(C21, matrixResult, n / 2, 0);
                JoinMatrices(C22, matrixResult, n / 2, n / 2);
            }

            return matrixResult;
        }

        public static double[,] MatrixSubtract(double[,] matrixA, double[,] matrixB)
        {
            int n = matrixA.GetLength(0);
            double[,] matrixResult = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrixResult[i, j] = matrixA[i, j] - matrixB[i, j];

                }
            }
            return matrixResult;
        }
        public static double[,] MatrixAdd(double[,] matrixA, double[,] matrixB)
        {
            int n = matrixA.GetLength(0);
            double[,] matrixResult = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrixResult[i, j] = matrixA[i, j] + matrixB[i, j];
                }

            }
            return matrixResult;
        }

        public static void SplitMatrix(double[,] parentMatrix, double[,] childMatrix, int lowerBound, int upperBound)
        {
            for (int i = 0, j = lowerBound; i < childMatrix.GetLength(0); i++, j++)
            {
                for (int k = 0, m = upperBound; k < childMatrix.GetLength(0); k++, m++)
                {
                    childMatrix[i, k] = parentMatrix[j, m];
                }
            }
        }

        public static void JoinMatrices(double[,] childMatrix, double[,] parentMatrix, int iB, int jB)
        {
            for (int i1 = 0, i2 = iB; i1 < childMatrix.GetLength(0); i1++, i2++)
            {
                for (int j1 = 0, j2 = jB; j1 < childMatrix.GetLength(0); j1++, j2++)
                {
                    parentMatrix[i2, j2] = childMatrix[i1, j1];

                }

            }
        }
    }




}
