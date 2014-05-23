/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Text;
using System.Drawing;
using System.Configuration;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using MessagingToolkit.QRCode.Codec;
using Magix.Core;

namespace Magix.QR
{
    /*
     * QR codes controller
     */
    internal sealed class QRCodeCore : ActiveController
    {
        /*
         * creates a QR code according to the given input
         */
        [ActiveEvent(Name = "magix.qr.create")]
        private void magix_qr_create(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.QR",
                    "Magix.QR.hyperlisp.inspect.hl",
                    "[magix.qr.create-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.QR",
                    "Magix.QR.hyperlisp.inspect.hl",
                    "[magix.qr.create-sample]");
                return;
            }

            if (!e.Params.ContainsValue("url"))
                throw new ArgumentException("[magix.qr.create] requires a [url] property");

            // Retreiving attributes for QR Code...
            int scale = e.Params["scale"].Get<int>(6);
            string errCorrection = e.Params["err"].Get<string>("Q");
            string fullUrl = e.Params["url"].Get<string>();
            string explanation = e.Params["text"].Get<string>();
            int cornerRadius = e.Params["rounded-corners"].Get<int>(20);
            bool animate = e.Params["anti-pixelated"].Get<bool>(true);
            string backgroundImage = e.Params["bg-image"].Get<string>("media/images/textures/bumpy-light.png");
            string foregroundImage = e.Params["fg-image"].Get<string>("media/images/textures/bumpy-dark.png");

            // Image has presedence ...
            string backgroundColor =
                string.IsNullOrEmpty(backgroundImage) ?
                    e.Params["bg-color"].Get<string>() :
                    null;
            string foregroundColor =
                string.IsNullOrEmpty(foregroundImage) ?
                    e.Params["fg-color"].Get<string>() :
                    null;

            string fontName = e.Params["font-name"].Get<string>("Comic Sans MS");
            int fontSize = e.Params["font-size"].Get<int>(-1);
            string fontColor = e.Params["font-color"].Get<string>("#999999");
            int rotateAngle = e.Params["rotate"].Get<int>();
            string fileName = e.Params["file-name"].Get<string>();

            // if no file-name is given, a unique name is automatically assigned, and returned to caller
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = "tmp/qr-" + Guid.NewGuid().ToString() + ".png";

                // Need to RETURN the newly created path back to caller ...
                e.Params["file-name"].Value = fileName;
            }

            string physicalPath = Page.MapPath("~/" + fileName);


            CreateQRCode(
                scale, 
                errCorrection, 
                fullUrl, 
                explanation, 
                cornerRadius, 
                animate, 
                backgroundImage, 
                foregroundImage, 
                backgroundColor, 
                foregroundColor, 
                fontName, 
                fontSize, 
                fontColor, 
                rotateAngle, 
                physicalPath);
        }

        private void CreateQRCode(
            int scale, 
            string errCorrection, 
            string fullUrl, 
            string explanation, 
            int cornerRadius, 
            bool animate, 
            string backgroundImage, 
            string foregroundImage, 
            string backgroundColor, 
            string foregroundColor, 
            string fontName, 
            int fontSize, 
            string fontColor, 
            int rotateAngle, 
            string physicalPath)
        {
            // Setting up our QR Code Generator ...
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            qrCodeEncoder.QRCodeScale = scale;
            qrCodeEncoder.QRCodeErrorCorrect =
                (QRCodeEncoder.ERROR_CORRECTION)
                    Enum.Parse(typeof(QRCodeEncoder.ERROR_CORRECTION),
                    errCorrection);
            qrCodeEncoder.QRCodeBackgroundColor = Color.White;
            qrCodeEncoder.QRCodeForegroundColor = Color.Black;

            if (!string.IsNullOrEmpty(foregroundColor))
                qrCodeEncoder.QRCodeForegroundColor = System.Drawing.ColorTranslator.FromHtml(foregroundColor);

            if (!string.IsNullOrEmpty(backgroundColor))
                qrCodeEncoder.QRCodeBackgroundColor = System.Drawing.ColorTranslator.FromHtml(backgroundColor);

            // 'Guessing' the version number according to number of bytes ...
            qrCodeEncoder.QRCodeVersion = GetQRCodeVersion(fullUrl, errCorrection);


            // Encoding the QR Code, raw format ...
            using (Image qrRaw = qrCodeEncoder.Encode(fullUrl, Encoding.UTF8))
            {
                // Adding "white borders".
                // MUST be done before the 'effect' is applied ...
                using (Image qrScaled = GetAdditionalBorders(
                    qrRaw,
                    qrCodeEncoder.QRCodeScale,
                    !string.IsNullOrEmpty(explanation),
                    cornerRadius,
                    backgroundColor))
                {

                    // 'Animating' the QR Code [skews the black and white]
                    using (Image imgBlurred = GetAnimatedImage(
                        qrScaled,
                        qrCodeEncoder.QRCodeScale - 1,
                        animate))
                    {
                        using (Image imgBackgroundExchanged =
                            ExchangeColorWithTextureBrush(
                                imgBlurred,
                                backgroundImage,
                                Color.FromArgb(255, 255, 255, 255)))
                        {
                            using (Image imgForegroundExchanged =
                                ExchangeColorWithTextureBrush(
                                    imgBackgroundExchanged,
                                    foregroundImage,
                                    Color.FromArgb(255, 0, 0, 0)))
                            {
                                if (!string.IsNullOrEmpty(explanation))
                                {
                                    using (Graphics gText = Graphics.FromImage(imgForegroundExchanged))
                                    {
                                        if (fontSize == -1)
                                        {
                                            // Figuring out the optimal font size automatically
                                            int maxSize = imgForegroundExchanged.Width - 20;
                                            fontSize = 25; // default
                                            while (true)
                                            {
                                                using (Font font = new Font(fontName, fontSize))
                                                {
                                                    SizeF size = gText.MeasureString(explanation, font);
                                                    if (size.Width < maxSize)
                                                        break;
                                                    if (fontSize < 10)
                                                        break;
                                                    fontSize -= 1;
                                                }
                                            }
                                        }
                                        using (Font font = new Font(
                                                fontName,
                                                fontSize))
                                        {
                                            gText.PageUnit = GraphicsUnit.Pixel;
                                            SizeF size = gText.MeasureString(explanation, font);
                                            using (Brush b = CreateBrush(fontColor))
                                            {
                                                gText.DrawString(
                                                    explanation,
                                                    font,
                                                    b,
                                                    Math.Max(5,
                                                    (imgForegroundExchanged.Width / 2) - (int)size.Width / 2) +
                                                        fontSize / 4,
                                                    imgForegroundExchanged.Height -
                                                        ((qrCodeEncoder.QRCodeScale * 5) +
                                                            ((int)size.Height / 2)),
                                                    StringFormat.GenericTypographic);
                                            }
                                        }
                                    }
                                }
                                using (Bitmap last =
                                    GetRotateImage(
                                        (Bitmap)imgForegroundExchanged,
                                        rotateAngle))
                                {
                                    // This is our 'end product', though not entirely finished yet ...
                                    using (Image finalResult =
                                        new Bitmap(
                                            last.Width,
                                            last.Height,
                                            PixelFormat.Format32bppArgb))
                                    {
                                        using (Graphics g = Graphics.FromImage(finalResult))
                                        {
                                            ImageAttributes atrs = new ImageAttributes();
                                            atrs.SetColorKey(
                                                Color.FromArgb(255, 254, 254, 254),
                                                Color.FromArgb(255, 254, 254, 254));
                                            g.DrawImage(
                                                last,
                                                new Rectangle(0, 0, last.Width, last.Height),
                                                0,
                                                0,
                                                last.Width,
                                                last.Height,
                                                GraphicsUnit.Pixel,
                                                atrs);
                                        }
                                        finalResult.Save(physicalPath, ImageFormat.Png);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private Brush CreateBrush(string fontColor)
        {
            if (fontColor.IndexOf('#') == 0)
            {
                return new SolidBrush(ColorTranslator.FromHtml(fontColor));
            }
            else
            {
                return new SolidBrush(Color.FromName(fontColor));
            }
        }

        private static int[] _codeCapacityLevelH = new int[]
        {
            7,14,24,34,44,58,64,84,98,119,137,155,177,194,220,250,280,310,338,382,403,439,461,511,535,593,625,658,698,742,790,842,898,958,983,1051,1093,1139,1219,1273
        };

        private static int[] _codeCapacityLevelQ = new int[]
        {
            11,20,32,46,60,74,86,108,130,151,177,203,241,258,292,322,364,394,442,482,509,565,611,661,715,751,805,868,908,982,1030,1112,1168,1228,1283,1351,1423,1499,1579,1663
        };

        private static int[] _codeCapacityLevelM = new int[]
        {
            14,26,42,62,84,106,122,152,180,213,251,287,331,362,412,450,504,560,624,666,711,779,857,911,997,1059,1125,1190,1264,1370,1452,1538,1628,1722,1809,1911,1989,2099,2213,2331
        };

        private static int[] _codeCapacityLevelL = new int[]
        {
            17,32,53,78,106,134,154,192,230,271,231,367,425,458,520,568,644,718,792,858,929,1003,1091,1171,1273,1367,1465,1528,1628,1732,1840,1952,2068,2188,2303,2431,2563,2699,2809,2953
        };

        private int GetQRCodeVersion(string fullUrl, string errCorrection)
        {
            switch (errCorrection)
            {
                case "H":
                    return GetCode(fullUrl, _codeCapacityLevelH);
                case "Q":
                    return GetCode(fullUrl, _codeCapacityLevelQ);
                case "M":
                    return GetCode(fullUrl, _codeCapacityLevelM);
                case "L":
                    return GetCode(fullUrl, _codeCapacityLevelL);
            }
            throw new ArgumentException("Unknown QR Code Error Correction Level");
        }

        private int GetCode(string fullUrl, int[] capacities)
        {
            int stringLength = fullUrl.Length;
            for (int idx = 0; idx < capacities.Length; idx++)
            {
                if (capacities[idx] >= stringLength)
                    return idx + 1;
            }
            throw new ArgumentException("There is too much data in your QR Code for the current Error Correction Level");
        }

        private Image GetAdditionalBorders(
            Image imgOrig,
            int qrCodeSize,
            bool hasExplanation,
            int rounded,
            string backgroundColor)
        {
            int width = imgOrig.Width + (qrCodeSize * 10);
            int height = imgOrig.Height + (qrCodeSize * (hasExplanation ? 14 : 10));
            using (Bitmap bmp = new Bitmap(
                width,
                height))
            {
                Image afterRounded = GetRoundedRectangle(bmp, rounded, backgroundColor);
                using (Graphics g = Graphics.FromImage(afterRounded))
                {
                    g.DrawImage(
                        imgOrig,
                        new Rectangle(
                            qrCodeSize * 5,
                            qrCodeSize * 5,
                            imgOrig.Width,
                            imgOrig.Height),
                        new Rectangle(
                            0,
                            0,
                            imgOrig.Width,
                            imgOrig.Height),
                            GraphicsUnit.Pixel);
                    return afterRounded;
                }
            }
        }

        private Image GetRoundedRectangle(Bitmap bmp, int rounded, string backgroundColor)
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                using (Brush b = new SolidBrush(Color.FromArgb(255, 254, 254, 254)))
                {
                    if (rounded > 0)
                    {
                        g.FillRectangle(b, new Rectangle(0, 0, bmp.Width, bmp.Height));
                        using (Brush b2 = new SolidBrush(
                            string.IsNullOrEmpty(backgroundColor) ?
                                Color.White :
                                System.Drawing.ColorTranslator.FromHtml(backgroundColor)))
                        {
                            return DrawRoundedRectangle(
                                g,
                                b2,
                                new Rectangle(2, 2, bmp.Width - 4, bmp.Height - 4),
                                rounded,
                                RoundedCorners.All,
                                bmp);
                        }
                    }
                    else
                    {
                        using (Brush b2 = new SolidBrush(
                            string.IsNullOrEmpty(backgroundColor) ?
                                Color.White :
                                System.Drawing.ColorTranslator.FromHtml(backgroundColor)))
                        {
                            g.FillRectangle(b2, new Rectangle(0, 0, bmp.Width, bmp.Height));
                            return new Bitmap(bmp);
                        }
                    }
                }
            }
        }

        private Image GetAnimatedImage(Image img, int scale, bool blur)
        {
            if (blur)
            {
                AForge.Imaging.Filters.Median m = new AForge.Imaging.Filters.Median();
                m.Size = scale;
                return m.Apply((Bitmap)img);
            }
            return new Bitmap(img);
        }

        private Bitmap GetRotateImage(Bitmap vLast, int factor)
        {
            if (factor == 0)
                return new Bitmap(vLast);
            else
            {
                AForge.Imaging.Filters.RotateBilinear m = new AForge.Imaging.Filters.RotateBilinear(factor);
                m.FillColor = Color.FromArgb(0, 254, 254, 254);
                return m.Apply(vLast);
            }
        }

        private Image ExchangeColorWithTextureBrush(
            Image imgSrc,
            string textureFileName,
            Color colorToReplace)
        {
            if (string.IsNullOrEmpty(textureFileName))
                return new Bitmap(imgSrc);
            else
            {
                Image retVal = RemoveColor(imgSrc, colorToReplace, textureFileName);
                return retVal;
            }
        }

        private Image ExchangeColorWithTextureBrush(
            Image imgSrc,
            Color srcColor,
            Color destColor,
            Color destColorMax)
        {
            AdjustAlpha((Bitmap)imgSrc, destColor.A, destColor, srcColor, destColorMax);
            return new Bitmap(imgSrc);
        }

        private Bitmap RemoveColor(Image imgOrig, Color color, string brushImageFile)
        {
            if (string.IsNullOrEmpty(brushImageFile))
                return new Bitmap(imgOrig);

            string fullServerPathToBrushImage =
                Page.MapPath("~/" + brushImageFile);

            Bitmap bitmapOrig = (Bitmap)imgOrig;

            using (Bitmap bitmapBrush = new Bitmap(fullServerPathToBrushImage))
            {
                using (Graphics g = Graphics.FromImage(bitmapOrig))
                {
                    using (TextureBrush br2 = new TextureBrush(bitmapBrush))
                    {
                        for (int x = 0; x < bitmapOrig.Width; x++)
                        {
                            for (int y = 0; y < bitmapOrig.Height; y++)
                            {
                                if (bitmapOrig.GetPixel(x, y).Equals(color))
                                {
                                    g.FillRectangle(
                                        br2,
                                        new Rectangle(
                                            x,
                                            y,
                                            1,
                                            1));
                                }
                            }
                        }
                    }
                }
                return new Bitmap(imgOrig);
            }
        }

        private static void AdjustAlpha(
            Bitmap image,
            byte alpha,
            Color destColor,
            Color srcColor,
            Color destColorMax)
        {
            int offset = 0;

            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData =
                image.LockBits(
                    new Rectangle(
                        0,
                        0,
                        image.Width,
                        image.Height),
                    ImageLockMode.ReadWrite,
                    PixelFormat.Format32bppRgb);

            int stride = bmData.Stride;
            IntPtr Scan0 = bmData.Scan0;

            for (int y = 0; y < image.Height; ++y)
            {
                for (int x = 0; x < image.Width; ++x)
                {
                    byte red = Marshal.ReadByte(Scan0, offset + 2);
                    byte green = Marshal.ReadByte(Scan0, offset + 1);
                    byte blue = Marshal.ReadByte(Scan0, offset + 0);
                    if (red >= srcColor.R && red <= destColorMax.R &&
                        green >= srcColor.G && green <= destColorMax.G &&
                        blue >= srcColor.B && blue <= destColorMax.B)
                        Marshal.WriteByte(Scan0, offset + 3, (byte)alpha);
                    offset += 4;
                }
            }
            image.UnlockBits(bmData);
        }

        private static Image DrawRoundedRectangle(
            Graphics g,
            Brush b,
            Rectangle rec,
            int radius,
            RoundedCorners corners,
            Image img)
        {
            int x = rec.X;
            int y = rec.Y;
            int diameter = radius * 2;
            var horiz = new Rectangle(x, y + radius, rec.Width, rec.Height - diameter);
            var vert = new Rectangle(x + radius, y, rec.Width - diameter, rec.Height);

            g.FillRectangle(b, horiz);
            g.FillRectangle(b, vert);

            if ((corners & RoundedCorners.TopLeft) == RoundedCorners.TopLeft)
                g.FillEllipse(b, x, y, diameter, diameter);
            else
                g.FillRectangle(b, x, y, diameter, diameter);

            if ((corners & RoundedCorners.TopRight) == RoundedCorners.TopRight)
                g.FillEllipse(b, x + rec.Width - (diameter + 1), y, diameter, diameter);
            else
                g.FillRectangle(b, x + rec.Width - (diameter + 1), y, diameter, diameter);

            if ((corners & RoundedCorners.BottomLeft) == RoundedCorners.BottomLeft)
                g.FillEllipse(b, x, y + rec.Height - (diameter + 1), diameter, diameter);
            else
                g.FillRectangle(b, x, y + rec.Height - (diameter + 1), diameter, diameter);

            if ((corners & RoundedCorners.BottomRight) == RoundedCorners.BottomRight)
                g.FillEllipse(b, x + rec.Width - (diameter + 1), y + rec.Height - (diameter + 1), diameter, diameter);
            else
                g.FillRectangle(b, x + rec.Width - (diameter + 1), y + rec.Height - (diameter + 1), diameter,
                                diameter);
            AForge.Imaging.Filters.Blur blur = new AForge.Imaging.Filters.Blur();
            return blur.Apply((Bitmap)img);
        }

        internal enum RoundedCorners
        {
            None = 0x00,
            TopLeft = 0x02,
            TopRight = 0x04,
            BottomLeft = 0x08,
            BottomRight = 0x10,
            All = 0x1F
        }
    }
}