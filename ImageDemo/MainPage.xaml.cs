using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SharpCL;
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;

// Il modello di elemento Pagina vuota è documentato all'indirizzo https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x410

namespace ImageDemo
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Context context;
        private CommandQueue commandQueue;
        private Dictionary<string, Kernel> kernels;

        private const string kernelsCode = @"
             __kernel void blur(read_only image2d_t source, write_only image2d_t destination) {
                // Get pixel coordinate
                int2 coord = (int2)(get_global_id(0), get_global_id(1));

                // Create a sampler that use edge color for coordinates outside the image
                const sampler_t sampler = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;

                // Blur using colors in a 7x7 square
                uint4 color = (uint4)(0, 0, 0, 255);
                for(int u=-3; u<=3; u++) {
                    for(int v=-3; v<=3; v++) {
                        color += read_imageui(source, sampler, coord + (int2)(u, v));
                    }
                }
                color /= 49;

                // Write blurred pixel in destination image
                write_imageui(destination, coord, color);
             }

            __kernel void invert(read_only image2d_t source, write_only image2d_t destination) {
                // Get pixel coordinate
                int2 coord = (int2)(get_global_id(0), get_global_id(1));

                // Read color ad invert it (except for alpha value)
                uint4 color = read_imageui(source, coord);
                color.xyz = (uint3)(255,255,255) - color.xyz;

                // Write inverted pixel in destination image
                write_imageui(destination, coord, color);
             }
        ";

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Get a context for the first GPU platform found
            context = Context.AutomaticContext(DeviceType.GPU);
            if (context == null)
            {
                ContentDialog dialog = new ContentDialog()
                {
                    Title = "No OpenCL compatible GPU found!",
                    Content = "Please install or update you GPU driver and retry.",
                    CloseButtonText = "Quit"
                };
                await dialog.ShowAsync();
                Application.Current.Exit();
                return;
            }

            // Get a command queue for the first available device in the context
            commandQueue = context.CreateCommandQueue();
            if(commandQueue == null)
            {
                ContentDialog dialog = new ContentDialog()
                {
                    Title = "Error!",
                    Content = "Can't create a command queue for the current context.",
                    CloseButtonText = "Quit"
                };
                await dialog.ShowAsync();
                Application.Current.Exit();
                return;
            }

            // Build all kernels from source code
            kernels = context.BuildAllKernels(kernelsCode);
            if(context.Error)
            {
                ContentDialog dialog = new ContentDialog()
                {
                    Title = "Error!",
                    Content = "Can't compile kernels, please check source code.",
                    CloseButtonText = "Quit"
                };
                await dialog.ShowAsync();
                Application.Current.Exit();
                return;
            }
        }

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                    WriteableBitmap writeableBitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                    writeableBitmap.SetSource(fileStream);
                    SourceImage.Source = writeableBitmap;

                    BlurButton.IsEnabled = true;
                    InvertButton.IsEnabled = true;
                }
            }
        }

        private void Blur_Click(object sender, RoutedEventArgs e)
        {
            ExecuteKernel("blur");
        }

        private void Invert_Click(object sender, RoutedEventArgs e)
        {
            ExecuteKernel("invert");
        }

        private async void ExecuteKernel(string kernelName)
        {
            // Get source pixel data
            WriteableBitmap sourceBitmap = SourceImage.Source as WriteableBitmap;
            byte[] sourceData = sourceBitmap.PixelBuffer.ToArray();

            // Create OpenCL images
            SharpCL.Image sourceImage = context.CreateImage2D(sourceData, (ulong)sourceBitmap.PixelWidth, (ulong)sourceBitmap.PixelHeight,
                MemoryFlags.ReadOnly | MemoryFlags.CopyHostPointer, ImageChannelOrder.BGRA, ImageChannelType.UnsignedInt8);
            SharpCL.Image destinationImage = context.CreateImage2D((ulong)sourceBitmap.PixelWidth, (ulong)sourceBitmap.PixelHeight, MemoryFlags.WriteOnly, ImageChannelOrder.BGRA, ImageChannelType.UnsignedInt8);

            // Run blur kernel
            kernels[kernelName].SetArgument(0, sourceImage);
            kernels[kernelName].SetArgument(1, destinationImage);
            Event kernelEvent = commandQueue.EnqueueKernel(kernels[kernelName], new ulong[] { (ulong)sourceBitmap.PixelWidth, (ulong)sourceBitmap.PixelHeight });
            if (commandQueue.Error)
            {
                ContentDialog dialog = new ContentDialog()
                {
                    Title = "Error!",
                    Content = "Can't enqueue kernel on the command queue.",
                    CloseButtonText = "Quit"
                };
                await dialog.ShowAsync();
                return;
            }

            byte[] destinationData = new byte[sourceBitmap.PixelWidth * sourceBitmap.PixelHeight * 4];
            commandQueue.EnqueueReadImage(destinationImage, destinationData, default, default, true, new List<Event> { kernelEvent });
            if (commandQueue.Error)
            {
                ContentDialog dialog = new ContentDialog()
                {
                    Title = "Error!",
                    Content = "Can't enqueue read image command on the command queue.",
                    CloseButtonText = "Quit"
                };
                await dialog.ShowAsync();
                return;
            }

            WriteableBitmap writeableBitmap = new WriteableBitmap(sourceBitmap.PixelWidth, sourceBitmap.PixelHeight);
            using (Stream stream = writeableBitmap.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(destinationData, 0, sourceBitmap.PixelWidth * sourceBitmap.PixelHeight * 4);
            }
            DestinationImage.Source = writeableBitmap;

        }

    }
}
