using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurContrastBrightnessImage
{
    static class Kernels
    {
        public enum KernelType
        {
            Gauss,
            Laplass
        }

        private static Dictionary<double, double[,]> gaussian = new Dictionary<double, double[,]>();
        
        public static double[,] getGaussian(double sigma) {
            if (sigma <= 0)
                throw new Exception("Invalid nonpositive sigma");
            if (!gaussian.Keys.Contains(sigma)) {
                int r = ((int)(-Math.Floor(-sigma))) * 6 + 1;
                double[,] ker = new double[r, r];
                for (int i = -r / 2; i <= r / 2; i++) {
                    for (int j = -r / 2; j <= r / 2; j++) {
                        ker[r / 2 + i, r / 2 + j] = 1 / (2 * Math.PI * sigma * sigma) * Math.Pow(Math.E, (-i * i - j * j) / (2 * sigma * sigma));
                    }
                }
                gaussian.Add(sigma, ker);
                return ker;
            }
            else {
                return gaussian[sigma];
            }
        }

        private static Dictionary<double, double[,]> laplacian = new Dictionary<double, double[,]>();

        public static double[,] getLaplacian(double sigma) {
            if (sigma <= 0)
                throw new Exception("Invalid nonpositive sigma");
            if (!laplacian.Keys.Contains(sigma)) {
                int r = ((int)(-Math.Floor(-sigma))) * 6 + 1;
                double[,] ker = new double[r, r];
                for (int i = -r / 2; i <= r / 2; i++) {
                    for (int j = -r / 2; j <= r / 2; j++) {
                        double sigma1 = sigma - 0.1;
                        double sigma2 = sigma + 0.1;
                        double G1 = 1 / (2 * Math.PI * sigma1 * sigma1) * Math.Pow(Math.E, (-i * i - j * j) / (2 * sigma1 * sigma1));
                        double G2 = 1 / (2 * Math.PI * sigma2 * sigma2) * Math.Pow(Math.E, (-i * i - j * j) / (2 * sigma2 * sigma2));
                        ker[r / 2 + i, r / 2 + j] =  (G1 - G2) / 0.2;
                    }
                }
                laplacian.Add(sigma, ker);
                return ker;
            }
            else {
                return laplacian[sigma];
            }
        }
    }
}
