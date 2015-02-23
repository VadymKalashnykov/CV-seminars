using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BlurContrastBrightnessImage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage originalImage;
        private byte[] originalImageBytes;
        private byte[] processedImageBytes;
        private int W, H;
        public MainWindow() {
            InitializeComponent();
        }

        // open file from the finder
        private void openFileButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg)|*.jpg; *.jpeg";
            if (openFileDialog.ShowDialog() == true) {
                showImage(openFileDialog.FileName);
                slidersGrid.IsEnabled = true;
            }
        }

        // show image on the window
        private void showImage(string filename) {
            originalImage = ImageConvertor.FilenameToImage(filename);
            originalImageBytes = ImageConvertor.ImageToByteArray(filename);
            originalPanel.Source = originalImage;
            W = (int)originalImage.PixelHeight;
            H = (int)originalImage.PixelWidth;
            ImageProcessing.setHeightAndLength(W, H);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            processedImageBytes = ImageProcessing.markEdges(originalImageBytes);
            grayscalePanel.Source = ImageConvertor.ByteArrayToImage(processedImageBytes, originalImage.PixelWidth, originalImage.PixelHeight, 4);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) {
            processedImageBytes = ImageProcessing.getDifferenceOfGaussins(originalImageBytes);
            grayscalePanel.Source = ImageConvertor.ByteArrayToImage(processedImageBytes, originalImage.PixelWidth, originalImage.PixelHeight, 1);
        }

        private void gaussSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (e.NewValue <= 1)
                return;
            processedImageBytes = ImageProcessing.getDifferenceOfGaussins(originalImageBytes, (int) e.NewValue);
            grayscalePanel.Source = ImageConvertor.ByteArrayToImage(processedImageBytes, originalImage.PixelWidth, originalImage.PixelHeight, 1);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            drawPanel.Children.Clear();
            
            grayscalePanel.Source = originalImage;

            List<double[]> Blobs = ImageProcessing.getBlobCoordsUsingLaplacianKernel(originalImageBytes);
            

            foreach (double[] coord in Blobs) {
                Ellipse el = new Ellipse();
                el.Stroke = System.Windows.Media.Brushes.Red;
                double r = coord[2] * originalPanel.ActualWidth;
                el.Width = 2 * r;
                el.Height = 2 * r;
                Canvas.SetLeft(el, coord[0] * originalPanel.ActualWidth - r);
                Canvas.SetTop(el, coord[1] * originalPanel.ActualHeight - r);
                drawPanel.Children.Add(el);
            }
            drawPanel.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
