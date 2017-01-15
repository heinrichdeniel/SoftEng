﻿using KinectControl;
using System;
namespace MatrixInverse
{
    class MatrixInverseProgram
    {
        /*static void Main(string[] args)
        {
          Console.WriteLine("\nBegin matrix inverse using Crout LU decomp demo \n");

          float[][] m = MatrixCreate(4, 4); 
          m[0][0] = 3.0; m[0][1] = 7.0; m[0][2] = 2.0; m[0][3] = 5.0;
          m[1][0] = 1.0; m[1][1] = 8.0; m[1][2] = 4.0; m[1][3] = 2.0;
          m[2][0] = 2.0; m[2][1] = 1.0; m[2][2] = 9.0; m[2][3] = 3.0;
          m[3][0] = 5.0; m[3][1] = 4.0; m[3][2] = 7.0; m[3][3] = 1.0;


          Console.WriteLine("Original matrix m is ");
          Console.WriteLine(MatrixAsString(m));

          float d = MatrixDeterminant(m);
          if (Math.Abs(d) < 1.0e-5)
            Console.WriteLine("Matrix has no inverse");

          float[][] inv = MatrixInverse(m);

          Console.WriteLine("Inverse matrix inv is ");
          Console.WriteLine(MatrixAsString(inv));

          float[][] prod = MatrixProduct(m, inv);
          Console.WriteLine("The product of m * inv is ");
          Console.WriteLine(MatrixAsString(prod));

          Console.WriteLine("========== \n");

          float[][] lum;
          int[] perm;
          int toggle = MatrixDecompose(m, out lum, out perm);
          Console.WriteLine("The combined lower-upper decomposition of m is");
          Console.WriteLine(MatrixAsString(lum));

          float[][] lower = ExtractLower(lum);
          float[][] upper = ExtractUpper(lum);

          Console.WriteLine("The lower part of LUM is");
          Console.WriteLine(MatrixAsString(lower));

          Console.WriteLine("The upper part of LUM is");
          Console.WriteLine(MatrixAsString(upper));

          Console.WriteLine("The perm[] array is");
          ShowVector(perm);

          float[][] lowTimesUp = MatrixProduct(lower, upper);
          Console.WriteLine("The product of lower * upper is ");
          Console.WriteLine(MatrixAsString(lowTimesUp));


          Console.WriteLine("\nEnd matrix inverse demo \n");
          Console.ReadLine();

        } // Main
        */

        static void ShowVector(int[] vector)
        {
            Console.Write("   ");
            for (int i = 0; i < vector.Length; ++i)
                Console.Write(vector[i] + " ");
            Console.WriteLine("\n");
        }

        public static float[][] MatrixInverse(float[][] matrix)
        {
            // assumes determinant is not 0
            // that is, the matrix does have an inverse
            int n = matrix.Length;
            float[][] result = MatrixCreate(n, n); // make a copy of matrix
            for (int i = 0; i < n; ++i)
                for (int j = 0; j < n; ++j)
                    result[i][j] = matrix[i][j];

            float[][] lum; // combined lower & upper
            int[] perm;
            int toggle;
            toggle = MatrixDecompose(matrix, out lum, out perm);

            float[] b = new float[n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                    if (i == perm[j])
                        b[j] = 1.0f;
                    else
                        b[j] = 0.0f;

                float[] x = Helper(lum, b);

                for (int j = 0; j < n; ++j)
                    result[j][i] = x[j];

            }
            return result;
        } // MatrixInverse

        static int MatrixDecompose(float[][] m, out float[][] lum, out int[] perm)
        {
            // Crout's LU decomposition for matrix determinant and inverse
            // stores combined lower & upper in lum[][]
            // stores row permuations into perm[]
            // returns +1 or -1 according to even or odd number of row permutations
            // lower gets dummy 1.0s on diagonal (0.0s above)
            // upper gets lum values on diagonal (0.0s below)

            int toggle = +1; // even (+1) or odd (-1) row permutatuions
            int n = m.Length;

            // make a copy of m[][] into result lu[][]
            lum = MatrixCreate(n, n);
            for (int i = 0; i < n; ++i)
                for (int j = 0; j < n; ++j)
                    lum[i][j] = m[i][j];



            // make perm[]
            perm = new int[n];
            for (int i = 0; i < n; ++i)
                perm[i] = i;
            for (int j = 0; j < n - 1; ++j) // process by column. note n-1 
            {
                float max = Math.Abs(lum[j][j]);
                int piv = j;

                for (int i = j + 1; i < n; ++i) // find pivot index
                {
                    float xij = Math.Abs(lum[i][j]);
                    if (xij > max)
                    {
                        max = xij;
                        piv = i;
                    }
                } // i
                if (piv != j)
                {
                    float[] tmp = lum[piv]; // swap rows j, piv
                    lum[piv] = lum[j];
                    lum[j] = tmp;

                    int t = perm[piv]; // swap perm elements
                    perm[piv] = perm[j];
                    perm[j] = t;

                    toggle = -toggle;
                }

                float xjj = lum[j][j];
                if (xjj != 0.0f)
                {
                    for (int i = j + 1; i < n; ++i)
                    {
                        float xij = lum[i][j] / xjj;
                        lum[i][j] = xij;
                        for (int k = j + 1; k < n; ++k)
                            lum[i][k] -= xij * lum[j][k];
                    }
                }

            } // j
            return toggle;
        } // MatrixDecompose

        static float[] Helper(float[][] luMatrix, float[] b) // helper
        {
            int n = luMatrix.Length;
            float[] x = new float[n];
            b.CopyTo(x, 0);

            for (int i = 1; i < n; ++i)
            {
                float sum = x[i];
                for (int j = 0; j < i; ++j)
                {
                    sum -= luMatrix[i][j] * x[j];
                }
                x[i] = sum;
            }

            x[n - 1] /= luMatrix[n - 1][n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                float sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i][j] * x[j];

                x[i] = sum / luMatrix[i][i];
            }


            return x;
        } // Helper

        static float MatrixDeterminant(float[][] matrix)
        {
            float[][] lum;
            int[] perm;
            int toggle = MatrixDecompose(matrix, out lum, out perm);
            float result = toggle;
            for (int i = 0; i < lum.Length; ++i)
                result *= lum[i][i];
            return result;
        }

        // ----------------------------------------------------------------

        static float[][] MatrixCreate(int rows, int cols)
        {
            float[][] result = new float[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new float[cols];
            return result;
        }

        static float[][] MatrixProduct(float[][] matrixA,
          float[][] matrixB)
        {
            int aRows = matrixA.Length;
            int aCols = matrixA[0].Length;
            int bRows = matrixB.Length;
            int bCols = matrixB[0].Length;
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices");

            float[][] result = MatrixCreate(aRows, bCols);

            for (int i = 0; i < aRows; ++i) // each row of A
                for (int j = 0; j < bCols; ++j) // each col of B
                    for (int k = 0; k < aCols; ++k) // could use k < bRows
                        result[i][j] += matrixA[i][k] * matrixB[k][j];

            return result;
        }

        static string MatrixAsString(float[][] matrix)
        {
            string s = "";
            for (int i = 0; i < matrix.Length; ++i)
            {
                for (int j = 0; j < matrix[i].Length; ++j)
                    s += matrix[i][j].ToString("F3").PadLeft(8) + " ";
                s += Environment.NewLine;
            }
            return s;
        }

        static float[][] ExtractLower(float[][] lum)
        {
            // lower part of an LU Doolittle decomposition (dummy 1.0s on diagonal, 0.0s above)
            int n = lum.Length;
            float[][] result = MatrixCreate(n, n);
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    if (i == j)
                        result[i][j] = 1.0f;
                    else if (i > j)
                        result[i][j] = lum[i][j];
                }
            }
            return result;
        }

        static float[][] ExtractUpper(float[][] lum)
        {
            // upper part of an LU (lu values on diagional and above, 0.0s below)
            int n = lum.Length;
            float[][] result = MatrixCreate(n, n);
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    if (i <= j)
                        result[i][j] = lum[i][j];
                }
            }
            return result;
        }

        // ----------------------------------------------------------------

    } // Program 

} // ns
