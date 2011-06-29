using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Interferometry
{
    /// <summary>
    /// Interaction logic for ImagePreview.xaml
    /// </summary>
    public partial class ImagePreview : Window
    {
        public ImagePreview(ImageSource src)
        {
            InitializeComponent();
            imagePreview.Source = src;

            if (src is BitmapSource)
            {
                this.Width = ((BitmapSource)src).PixelWidth + 20;
                this.Height = ((BitmapSource)src).PixelHeight + 20;
            }
            else if (src is CachedBitmap)
            {
                this.Width = ((CachedBitmap)src).PixelWidth + 20;
                this.Height = ((CachedBitmap)src).PixelHeight + 20;
            }
        }
    }
}
