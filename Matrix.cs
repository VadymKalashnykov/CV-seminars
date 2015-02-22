using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurContrastBrightnessImage
{
    public class Matrix
    {
        public readonly int X;
        public readonly int Y;

        private double[,] values = null;

        public double this [int i, int j] {
            get {
                return values[i, j];
            }
            set {
                values[i, j] = value;
            }
        }


        public Matrix(int x, int y) {
            X = x;
            Y = y;
            values = new double[x, y];
        }

        public double[,] getValues() {
            return (double[,]) values.Clone(); 
        }

        public Matrix(int x, int y, double[,] matrix) {
            X = x;
            Y = y;
            values = matrix;
        }

        public static Matrix operator + (Matrix m1, Matrix m2) {
            if (m1.X != m2.X || m1.Y != m2.Y)
                throw new Exception("Dimentions int '+' operator");
            double[,] newValues = new double[m1.X, m1.Y];
            for (int i = 0; i < m1.X; i++) {
                for (int j = 0; j < m1.Y; j++) {
                    newValues[i, j] = m1[i, j] + m2[i,j];
                }
            }
            return new Matrix(m1.X, m1.Y, newValues);
        }

        public static Matrix operator * (Matrix m1, double value) {
            double[,] newValues = new double[m1.X, m1.Y];
            for (int i = 0; i < m1.X; i++) {
                for (int j = 0; j < m1.Y; j++) {
                    newValues[i, j] = m1[i, j] * value;
                }
            }
            return new Matrix(m1.X, m1.Y, newValues);
        }

        public static Matrix operator *(Matrix m1, Matrix m2) {
            if (m1.Y != m2.X)
                throw new Exception("Dimentions int '*' operator");
            double[,] newValues = new double[m1.X, m2.Y];
            for (int i = 0; i < m1.X; i++) {
                for (int j = 0; j < m1.Y; j++) {
                    for (int k = 0; k < m1.Y; k++) {
                        newValues[i, j] += m1[i, k] * m2[k, j];
                    }
                }
            }
            return new Matrix(m1.X, m2.Y, newValues);
        }
    }

    public class SqMatrix : Matrix
    {
        public static SqMatrix operator *(double value, SqMatrix m1) {
            double[,] newValues = new double[m1.X, m1.Y];
            for (int i = 0; i < m1.X; i++) {
                for (int j = 0; j < m1.Y; j++) {
                    newValues[i, j] = m1[i, j] * value;
                }
            }
            return new SqMatrix(newValues);
        }

        public static SqMatrix operator +(SqMatrix m1, SqMatrix m2) {
            if (m1.X != m2.X || m1.Y != m2.Y)
                throw new Exception("Dimentions int '+' operator");
            double[,] newValues = new double[m1.X, m1.Y];
            for (int i = 0; i < m1.X; i++) {
                for (int j = 0; j < m1.Y; j++) {
                    newValues[i, j] = m1[i, j] + m2[i, j];
                }
            }
            return new SqMatrix(newValues);
        }

        public SqMatrix (double[,] values) : base (2, 2, values) {}

        public SqMatrix () : base(2,2) {}

         const double k = 0.05;
         public double R {
            get {
                return ((this[0,0] * this[1,1] - this[1, 0] * this[0,1]) - k * Math.Pow(this[0,0] + this[1,1], 2));
            }
        }
    }
}
