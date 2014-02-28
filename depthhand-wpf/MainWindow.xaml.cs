using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using Microsoft.Kinect;

namespace depthhand_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private KinectSensor sensor;
        private DepthImagePixel[] depthPixels;
        private byte[] colorPixels;
        private WriteableBitmap colorBitmap;
        private short[] pixelData;
        private short[] pixeldata;

        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        /// 
        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            {

                if (StartStopButton.Content.ToString() == "Start")
                {
                    if (KinectSensor.KinectSensors.Count > 0)
                    {
                        sensor = KinectSensor.KinectSensors[0];
                    }

                    if (null != this.sensor)
                    {
                        //turn on sensor
                        this.sensor.Start();
                        //turn on color stream utk menerima rgb
                        this.sensor.ColorStream.Enable();
                        // Turn on the depth stream to receive depth frames
                        this.sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                        // Allocate space to put the depth pixels we'll receive
                        this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];

                        // Allocate space to put the color pixels we'll create
                        this.colorPixels = new byte[this.sensor.DepthStream.FramePixelDataLength * sizeof(int)];

                        // This is the bitmap we'll display on-screen
                        this.colorBitmap = new WriteableBitmap(this.sensor.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                        // Set the image we display to point to the bitmap where we'll put the image data
                        this.Image.Source = this.colorBitmap;

                        
                        // Add an event handler to be called whenever there is new depth frame data
                        this.sensor.DepthFrameReady += this.SensorDepthFrameReady;

                        convexHull = new ConvexHull();
                        
                        StartStopButton.Content = "Stop";
                    }
                }

                else
                {
                    //stop kinect
                    if (this.sensor != null && this.sensor.IsRunning)
                    {
                        this.sensor.Stop();
                        StartStopButton.Content = "Start";
                    }

                }
            }
        }
  //      private void pictureBox_Paint(object sender, PaintEventArgs e)
  //      {
  //          Rectangle ee = new Rectangle(10, 10, 30, 30);
  //          using (Pen pen = new Pen(Color.FromRgb(244, 0, 0), 2))
  //          {
  //              e.Graphics.DrawRectangle(pen, ee);
  //      }
  //}
        

            /// <summary>
            /// Execute shutdown tasks
            /// </summary>
            /// <param name="sender">object sending the event</param>
            /// <param name="e">event arguments</param>
            private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
            {
                if (null != this.sensor)
                {
                    this.sensor.Stop();
                }
            }

            /// <summary>
            /// Event handler for Kinect sensor's DepthFrameReady event
            /// </summary>
            /// <param name="sender">object sending the event</param>
            /// <param name="e">event arguments</param>
            /// 
            private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
            {
                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    if (depthFrame != null)
                    {

                        pixeldata = new short[depthFrame.PixelDataLength];
                        depthFrame.CopyPixelDataTo(pixeldata);
                        //int x = 120; //specify x value here
                        //int y = 170; //specify y value here

                        //int w = depthFrame.Width;
                        //// this is distance in mm
                        //int d = pixeldata[x + y * w];
                        //d = d >> 3;
                        //label.Content = d;

                        



                        // Copy the pixel data from the image to a temporary array
                        depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                        // Get the min and max reliable depth for the current frame
                        int minDepth = 825;
                        int maxDepth = 925;

                        


                        // Convert the depth to RGB
                        int colorPixelIndex = 0;
                        for (int i = 0; i < this.depthPixels.Length; ++i)
                        {
                            // Get the depth for this pixel
                            short depth = depthPixels[i].Depth;

                            // To convert to a byte, we're discarding the most-significant
                            // rather than least-significant bits.
                            // We're preserving detail, although the intensity will "wrap."
                            // Values outside the reliable depth range are mapped to 0 (black).

                            // Note: Using conditionals in this loop could degrade performance.
                            // Consider using a lookup table instead when writing production code.
                            // See the KinectDepthViewer class used by the KinectExplorer sample
                            // for a lookup table example.
                            byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);
                            //minim 0,8 - maks 1,5m


                            // Write out blue byte
                            this.colorPixels[colorPixelIndex++] = intensity;

                            // Write out green byte
                            this.colorPixels[colorPixelIndex++] = intensity;

                            // Write out red byte                        
                            this.colorPixels[colorPixelIndex++] = intensity;

                            // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                            // If we were outputting BGRA, we would write alpha here.
                            ++colorPixelIndex;

                        }   
                        
                        // Write the pixel data into our bitmap
                        this.colorBitmap.WritePixels(
                            new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                            this.colorPixels,
                            this.colorBitmap.PixelWidth * sizeof(int),
                            0);
                        
                    }

                }
            }




            internal ConvexHull convexHull { get; set; }
    }


    }





/*
 * public class AllMethods
{
    public static void Method2()
    {
        // code here
    }
}

class Caller
{
    public static void Main(string[] args)
    {
        AllMethods.Method2();
    }
}
 * 
 * 
 * 
 * public class MyClass
{
    public void InstanceMethod() 
    { 
        // ...
    }
}

public static void Main(string[] args)
{
    var instance = new MyClass();
    instance.InstanceMethod();
}
 * 
*/