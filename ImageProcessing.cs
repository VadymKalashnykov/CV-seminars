using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurContrastBrightnessImage
{
    static class ImageProcessing
    {
        #region Main
        private static int H, W;

        private static byte checkPixValue(double x) {
            return (byte)(x < 0 ? 0 : (x > 255 ? 255 : x));
        }

        private static double[] doubledArray(byte[] array) {
            double[] output = new double[array.Length];
            for (int i = 0; i < output.Length; i++) {
                output[i] = (double) array[i];
            }
            return output;
        }

        private static byte[] bytedArray(double[] array) {
            byte[] output = new byte[array.Length];
            for (int i = 0; i < output.Length; i++) {
                output[i] = checkPixValue(array[i]);
            }
            return output;
        }

        /// <summary>
        ///0.299 0.587 0.114
        ///B G R
        /// </summary>
        public static double[] setGrayScale(double[] image) {
            double[] grayscaled = new double[image.Length / 4];
            for (int i = 0; i < grayscaled.Length; i++) {
                grayscaled[i] = checkPixValue(0.299 * image[4 * i] + 0.587 * image[4 * i + 1] + 0.114 * image[4 * i + 2]);
            }
            return grayscaled;
        }

        private static double[] applyMatrix(double[] imageBytes, double[,] ker) {
            /// TODO: do smth with frames
            double[] output = new double[imageBytes.Length];
            int r = ker.GetLength(0);
            for (int i = r / 2; i < H - r / 2; i++) {
                for (int j = r / 2; j < W - r / 2; j++) {
                    for (int dx = -r / 2; dx <= r / 2; dx++) {// write 9 lines insdead of 2 fors, u lazy moron
                        for (int dy = -r / 2; dy <= r / 2; dy++) {
                            output[W * i + j] += ker[dx + r/2, dy + r/2] * imageBytes[W * (i + dy) + (j + dx)]; // donno why it fails sometimes ;(
                        }
                    }
                }
            }
            return output;
        }

        private static double[] substractArrays(double[] A, double[] B) {
            double[] output = new double[A.Length];
            for (int i = 0; i < output.Length; i++) {
                output[i] = A[i] - B[i];
            }
            return output;
        }

        public static void setHeightAndLength(int h, int w) {
            if ((h <= 0) || (w <= 0))
                throw new Exception("Invalid image parameters");
            H = h;
            W = w;
        }

        #endregion

        #region Edges
        private static double[] setDX(double[] imageBytes) {
            double[,] kerX = new double[3, 3] { { 1 / 8f, 0 / 8f, -1 / 8f }, 
                                                { 2 / 8f, 0 / 8f, -2 / 8f }, 
                                                { 1 / 8f, 0 / 8f, -1 / 8f } };
            double[] grayscaled = setGrayScale(imageBytes);
            return applyMatrix(grayscaled, kerX);
        }

        private static double[] setDY(double[] imageBytes) {
            double[,] kerY = new double[3, 3] { { 1 / 8f, 2 / 8f, 1 / 8f },
                                                { 0 / 8f, 0 / 8f, 0 / 8f },
                                                { -1 / 8f, -2 / 8f, -1 / 8f } };
            double[] grayscaled = setGrayScale(imageBytes);
            return applyMatrix(grayscaled, kerY);
        }

        private static double[] getWeird(double[] Ix, double[] Iy) {
            double[] output = new double[Ix.Length];

            SqMatrix[,] derivMatrixes = new SqMatrix[W, H];

            for (int i = 1; i < H - 1; i++) {
                for (int j = 1; j < W - 1; j++) {
                    derivMatrixes[j, i] = new SqMatrix(new double[,] {{ Ix[ j + i * W] * Ix[ j + i * W] ,  Ix[ j + i * W] * Iy[ j + i * W] } ,
                                                                     {  Ix[ j + i * W] * Iy[ j + i * W] ,  Iy[ j + i * W] * Iy[ j + i * W] }});
                }
            }

            for (int x = 4; x < W - 4; x++) {
                for (int y = 4; y < H - 4; y++) {//----------------------------------

                    SqMatrix M = new SqMatrix();
                    for (int u = -2; u < 3 ; u++) {
                        for (int v = -2; v < 3; v++) {
                            M += (2.5 - Math.Abs(u) - Math.Abs(v)) * derivMatrixes[x + u, y + v];
                        }
                    }

                    output[x + y * W] = (M.R);
                }//---------------------------------------------------------------------
            }

            return output;
        }


        private static double dist(int i, int j) {
            return (i % W - j % W) * (i % W - j % W) + (i / W - j / W) * (i / W - j / W);
        }

        private static List<int> getEdges(double[] weird) {
            List<int> Dots = new List<int>();
            for (int i = 0; i < weird.Length; i++) {
                if (weird[i] > 1200)
                    Dots.Add(i);
            }

            Dots = new List<int> (Dots.OrderByDescending(i => weird[i]).Take(5000));
            int count = 0; 
            do {
                Dots.RemoveAll(x => dist(x, Dots[count]) < 100 && x != Dots[count]);
                ++count;
            }
            while(count < Dots.Count);

            Dots = new List<int>(Dots.OrderByDescending(i => weird[i]).Take(200));
           // return  new List<int> (Dots.OrderByDescending(i => weird[i]).Take(Dots.Count > 100 ? 100 : Dots.Count));
            return Dots;
        }

        public static byte[] markEdges(byte[] image) {
            double[] dimage = doubledArray(image);
            double[] weird = getWeird(setDX(dimage), setDY(dimage));
            List<int> Dots = getEdges(weird);

            byte[] output = (byte[]) image.Clone();

            foreach (int index in Dots) {
                output[index * 4] = 0;
                output[index * 4 + 1] = 255;
                output[index * 4 + 2] = 0;

                output[index * 4 - W * 4] = 0;
                output[index * 4 + 1 - W * 4] = 255;
                output[index * 4 + 2 - W * 4] = 0;

                output[index * 4 + W * 4] = 0;
                output[index * 4 + 1 + W * 4] = 255;
                output[index * 4 + 2 + W * 4] = 0;

                output[index * 4 - 4] = 0;
                output[index * 4 + 1 - 4] = 255;
                output[index * 4 + 2 - 4] = 0;

                output[index * 4 + 4] = 0;
                output[index * 4 + 1 + 4] = 255;
                output[index * 4 + 2 + 4] = 0;
            }
            return output;
        }
        #endregion

        #region Blobs

        private const int Height = 15;

        public static byte[] getNiceBlobs(byte[] imageBytes, int k = 1) {
            double[] image = setGrayScale(doubledArray(imageBytes));
            double[,] gaussKer = new double[,] { { 1 / 16.0, 1 / 8.0, 1 / 16.0 }, 
                                                 { 1 / 8.0,  1 / 4.0, 1 / 8.0 },
                                                 { 1 / 16.0, 1 / 8.0, 1 / 16.0 }};
            
            double[][] smoothedImages = new double[Height][];

            smoothedImages[0] = image.Clone() as double[];
            for (int i = 1; i < Height; i++) {
                smoothedImages[i] = applyMatrix(smoothedImages[i - 1], gaussKer);
                for (int iter = 0; iter < 3; iter++) {
                    smoothedImages[i] = applyMatrix(smoothedImages[i], gaussKer);
                }
            }

            double[][] lapassedImages = new double[Height - 1][];
            for (int i = 0; i < Height - 1; i++) {
                lapassedImages[i] = substractArrays(smoothedImages[i + 1], smoothedImages[i]);
            }

            return bytedArray(Array.ConvertAll(lapassedImages[k], x => x + 120));
        }

        public static List<double[]> getBlobCoords(byte[] imageBytes, int n) {
            List<double[]> BLOBS = new List <double[]>();
            double[] image = setGrayScale(doubledArray(imageBytes));
            /*double[,] gaussKer = new double[,] { { 1 / 16.0, 1 / 8.0, 1 / 16.0 }, 
                                                 { 1 / 8.0,  1 / 4.0, 1 / 8.0 },
                                                 { 1 / 16.0, 1 / 8.0, 1 / 16.0 } };*/
            double[,] gaussKer = Kernels.getGaussian(1);
            double[][] smoothedImages = new double[Height][];

            smoothedImages[0] = applyMatrix(image, gaussKer);

            for (int i = 1; i < Height; i++) {
                smoothedImages[i] = applyMatrix(smoothedImages[i - 1], gaussKer);
            }

            double[][] lapassedImages = new double[Height - 1][]; 
            for (int i = 0; i < Height - 1; i++) {
                lapassedImages[i] = substractArrays(smoothedImages[i + 1], smoothedImages[i]);
            }

            List<double[]> OUTPUTBLOBS = new List<double[]>();

            for (int w = 4 ; w < W - 4; w++) {  
                for (int h = 4; h < H - 4; h++) {
                    for (int t = 1; t < Height - 2; t++) {
                        bool isBlob = true;
                        double current = lapassedImages[t][h * W + w];
                        for (int dw = -1; dw <= 1; dw++) {
                            for (int dh = -1; dh <= 1; dh++) {
                                for (int dt = -1; dt <= 1; dt++) {
                                    if ((lapassedImages[t + dt][(h + dh) * W + (w + dw)]) > current * 0.9 && ((t + dt < Height - 2) && (t - dt >= 1)))
                                        isBlob = false;
                                }
                            }
                        }
                        if (isBlob) {
                            OUTPUTBLOBS.Add(new double[3] { w / (W + 0.0), h / (H + 0.0), t / (W + 0.0) / Math.Pow(2, 2.5) });
                        }
                    }
                }
            }
            return OUTPUTBLOBS;
        }

        #endregion
    }
}
