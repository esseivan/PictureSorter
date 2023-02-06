using System;
using System.Drawing;

namespace PictureSorter
{
    public abstract class ImageTools
    {
        public static Image FixRotation(Image image)
        {
            foreach (var prop in image.PropertyItems)
            {
                if (prop.Id == 0x0112)
                {
                    int orientationValue = image.GetPropertyItem(prop.Id).Value[0];
                    RotateFlipType rotateFlipType = getRotateFlipType(orientationValue);
                    image.RotateFlip(rotateFlipType);
                    break;
                }
            }

            return image;
        }

        //=== determine image rotation
        private static RotateFlipType getRotateFlipType(int rotateValue)
        {
            RotateFlipType flipType;
            switch (rotateValue)
            {
                case 1:
                    flipType = RotateFlipType.RotateNoneFlipNone;
                    break;
                case 2:
                    flipType = RotateFlipType.RotateNoneFlipX;
                    break;
                case 3:
                    flipType = RotateFlipType.Rotate180FlipNone;
                    break;
                case 4:
                    flipType = RotateFlipType.Rotate180FlipX;
                    break;
                case 5:
                    flipType = RotateFlipType.Rotate90FlipX;
                    break;
                case 6:
                    flipType = RotateFlipType.Rotate90FlipNone;
                    break;
                case 7:
                    flipType = RotateFlipType.Rotate270FlipX;
                    break;
                case 8:
                    flipType = RotateFlipType.Rotate270FlipNone;
                    break;
                default:
                    flipType = RotateFlipType.RotateNoneFlipNone;
                    break;
            }

            return flipType;
        }

        //=== image border
        public static Image reapplyBorderToImage(
            Image image,
            Color borderColor,
            float borderSizePercent
        )
        {
            if (borderSizePercent < 0)
                throw new ArgumentOutOfRangeException(
                    "borderSize",
                    "The border size must be greater or equal to 0"
                );

            int maxDimension = Math.Max(image.Width, image.Height);
            float maxRealDimension = maxDimension / (1 + borderSizePercent);
            int borderSizePixels = (int)Math.Round(borderSizePercent * maxRealDimension);
            //create a new square image
            Bitmap newImage = new Bitmap(image.Width, image.Height);

            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                //fill the new square with a color for the border
                graphics.FillRectangle(
                    new SolidBrush(borderColor),
                    0,
                    0,
                    newImage.Width,
                    newImage.Height
                );

                //put the original image on top of the new square
                Rectangle srcRect = new Rectangle(
                    borderSizePixels,
                    borderSizePixels,
                    image.Width - 2 * borderSizePixels,
                    image.Height - 2 * borderSizePixels
                );
                graphics.DrawImage(
                    image,
                    borderSizePixels,
                    borderSizePixels,
                    srcRect,
                    GraphicsUnit.Pixel
                );
            }

            //return the image
            return newImage;
        }

        //=== image border
        public static Image applyBorderToImage(
            Image image,
            Color borderColor,
            float borderSizePercent
        )
        {
            if (borderSizePercent < 0)
                throw new ArgumentOutOfRangeException(
                    "borderSize",
                    "The border size must be greater or equal to 0"
                );

            int maxDimension = Math.Max(image.Width, image.Height);
            int borderSizePixels = (int)Math.Round(borderSizePercent * maxDimension);

            //create a new square image
            Bitmap newImage = new Bitmap(
                image.Width + 2 * borderSizePixels,
                image.Height + 2 * borderSizePixels
            );

            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                //fill the new square with a color for the border
                graphics.FillRectangle(
                    new SolidBrush(borderColor),
                    0,
                    0,
                    newImage.Width,
                    newImage.Height
                );

                //put the original image on top of the new square
                graphics.DrawImage(
                    image,
                    borderSizePixels,
                    borderSizePixels,
                    image.Width,
                    image.Height
                );
            }

            //return the image
            return newImage;
        }
    }
}
