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
                        
                        GrahamScan gs = new GrahamScan();
                        List<Point> listPoints = new List<Point>();


                        int w = depthFrame.Width;
                        int h = depthFrame.Height;
                        
                        for (int x = 0; x < h; x++) 
                        {                            
                            for (int y = 0; y < w; y++)
                            {                                
                                    if (minDepth <  < maxDepth)
                                    {
                                        listPoints.Add(new Point(x, y));
                                    }                             
                            }   
                        }                                            
                        
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        gs.convexHull(listPoints, label);

                        stopwatch.Stop();
                        float elapsed_time = stopwatch.ElapsedMilliseconds;
                                                
                        Console.WriteLine("Elapsed time: {0} milliseconds", elapsed_time);
                        Console.WriteLine("Press enter to close...");
                        Console.ReadLine();

                        // Write the pixel data into our bitmap
                        this.colorBitmap.WritePixels(
                            new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                            this.colorPixels,
                            this.colorBitmap.PixelWidth * sizeof(int),
                            0);
                        
                    }

                }
            }
            public class Point
            {
                private int y;
                private int x;
                public Point(int _x, int _y)
                {
                    x = _x;
                    y = _y;
                }
                public int getX()
                {
                    return x;
                }
                public int getY()
                {
                    return y;
                }
            }
            public class GrahamScan
            {
                const int TURN_LEFT = 1;
                const int TURN_RIGHT = -1;
                const int TURN_NONE = 0;
                public int turn(Point p, Point q, Point r)
                {
                    return ((q.getX() - p.getX()) * (r.getY() - p.getY()) - (r.getX() - p.getX()) * (q.getY() - p.getY())).CompareTo(0);
                }

                public void keepLeft(List<Point> hull, Point r)
                {
                    while (hull.Count > 1 && turn(hull[hull.Count - 2], hull[hull.Count - 1], r) != TURN_LEFT)
                    {
                        Console.WriteLine("Removing Point ({0}, {1}) because turning right ", hull[hull.Count - 1].getX(), hull[hull.Count - 1].getY());
                        hull.RemoveAt(hull.Count - 1);
                    }
                    if (hull.Count == 0 || hull[hull.Count - 1] != r)
                    {
                        Console.WriteLine("Adding Point ({0}, {1})", r.getX(), r.getY());
                        hull.Add(r);
                    }
                    Console.WriteLine("# Current Convex Hull #");
                    foreach (Point value in hull)
                    {
                        Console.Write("(" + value.getX() + "," + value.getY() + ") ");
                    }
                    Console.WriteLine();
                    Console.WriteLine();

                }

                public double getAngle(Point p1, Point p2)
                {
                    float xDiff = p2.getX() - p1.getX();
                    float yDiff = p2.getY() - p1.getY();
                    return Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
                }

                public List<Point> MergeSort(Point p0, List<Point> arrPoint)
                {
                    if (arrPoint.Count == 1)
                    {
                        return arrPoint;
                    }
                    List<Point> arrSortedInt = new List<Point>();
                    int middle = (int)arrPoint.Count / 2;
                    List<Point> leftArray = arrPoint.GetRange(0, middle);
                    List<Point> rightArray = arrPoint.GetRange(middle, arrPoint.Count - middle);
                    leftArray = MergeSort(p0, leftArray);
                    rightArray = MergeSort(p0, rightArray);
                    int leftptr = 0;
                    int rightptr = 0;
                    for (int i = 0; i < leftArray.Count + rightArray.Count; i++)
                    {
                        if (leftptr == leftArray.Count)
                        {
                            arrSortedInt.Add(rightArray[rightptr]);
                            rightptr++;
                        }
                        else if (rightptr == rightArray.Count)
                        {
                            arrSortedInt.Add(leftArray[leftptr]);
                            leftptr++;
                        }
                        else if (getAngle(p0, leftArray[leftptr]) < getAngle(p0, rightArray[rightptr]))
                        {
                            arrSortedInt.Add(leftArray[leftptr]);
                            leftptr++;
                        }
                        else
                        {
                            arrSortedInt.Add(rightArray[rightptr]);
                            rightptr++;
                        }
                    }
                    return arrSortedInt;
                }

                public void convexHull(List<Point> points, Label label)
                {
                    Console.WriteLine("# List of Point #");
                    foreach (Point value in points)
                    {
                        
                        Console.Write("(" + value.getX() + "," + value.getY() + ") ");
                    }
                    Console.WriteLine();
                    Console.WriteLine();

                    Point p0 = null;
                    foreach (Point value in points)
                    {
                        if (p0 == null)
                            p0 = value;
                        else
                        {
                            if (p0.getY() > value.getY())
                                p0 = value;
                        }
                    }
                    List<Point> order = new List<Point>();
                    foreach (Point value in points)
                    {
                        if (p0 != value)
                            order.Add(value);
                    }

                    order = MergeSort(p0, order);
                    Console.WriteLine("# Sorted points based on angle with point p0 ({0},{1})#", p0.getX(), p0.getY());
                    foreach (Point value in order)
                    {
                        Console.WriteLine("(" + value.getX() + "," + value.getY() + ") : {0}", getAngle(p0, value));
                    }
                    List<Point> result = new List<Point>();
                    result.Add(p0);
                    result.Add(order[0]);
                    result.Add(order[1]);
                    order.RemoveAt(0);
                    order.RemoveAt(0);
                    Console.WriteLine("# Current Convex Hull #");
                    foreach (Point value in result)
                    {
                        Console.Write("(" + value.getX() + "," + value.getY() + ") ");
                    }
                    Console.WriteLine();
                    Console.WriteLine();
                    foreach (Point value in order)
                    {
                        keepLeft(result, value);
                    }
                    Console.WriteLine();
                    Console.WriteLine("# Convex Hull #");
                    string z = "";
                    foreach (Point value in result)
                    {
                        z += "(" + value.getX() + " + " + value.getY() + ")" ;
                        
                        //Console.Write("(" + value.getX() + "," + value.getY() + ") ");
                    }
                    label.Content = z;
                    Console.WriteLine();
                }


        }


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