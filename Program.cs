using System;
using System.IO;
using System.Linq;
using Telerik.Windows.Documents.Extensibility;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Export;
using Telerik.Windows.Documents.Fixed.Model;
using Telerik.Windows.Documents.Fixed.Model.Editing;
using Telerik.Windows.Documents.Fixed.Model.Resources;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using ImageMagick;
using Telerik.Windows.Documents.Fixed.Model.Navigation;
using System.Net.Http;

namespace Telerik
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            JpegImageConverterBase customJpegImageConverter = new CustomJpegImageConverter();
            FixedExtensibilityManager.JpegImageConverter = customJpegImageConverter;

            
            RadFixedDocument document = new RadFixedDocument();
            PdfFormatProvider provider = new PdfFormatProvider();

            #region If file is in URL
            //var link = "https://files.alicereceptionist.com/inductionmaterial/Lippert/VISITOR%20CONFIDENTIALITY%20AGREEMENT.pdf";
            //using (HttpClient client = new HttpClient())
            //{
            //    HttpResponseMessage msg = await client.GetAsync(link);
            //    if (msg.IsSuccessStatusCode)
            //    {
            //        var contentStream = msg.Content.ReadAsStreamAsync().Result;
            //        document = provider.Import(contentStream);
            //    }
            //}
            #endregion

            #region If file is stored locally
            using (Stream input = System.IO.File.OpenRead(@"C:\Users\deepe\Desktop\original.pdf")){document = provider.Import(input);}
            #endregion

            #region Signature
            var signatureField = document.AcroForm.FormFields.Where(x => x.Name == "signature").FirstOrDefault();

            if (signatureField != null)
            {
                var signatureStream = new FileStream(@"C:\Users\deepe\Desktop\lion.png", FileMode.Open);

                #region If image is in bytes
                //var signature = _visitorInductionMaterialService.Get(28991).Signature;
                //ImageSource signatureStream = new ImageSource(new MemoryStream(signature));
                #endregion

                var location = signatureField.Widgets.FirstOrDefault();
                var editor = new FixedContentEditor(location.Parent as RadFixedPage);
                editor.Position.Translate(location.Rect.X, location.Rect.Y);
                editor.DrawImage(signatureStream, 200, 30);
            }
            #endregion

            #region Date
            var dateField = document.AcroForm.FormFields.Where(x => x.Name == "date").FirstOrDefault();
            if (signatureField != null)
            {
                var location = dateField.Widgets.FirstOrDefault();
                var editor = new FixedContentEditor(location.Parent as RadFixedPage);
                editor.Position.Translate(location.Rect.X, location.Rect.Y);
                editor.DrawText(DateTime.Now.ToString("dd/MM/yyyy"));
            }
            #endregion

            #region Print Name
            var printNameField = document.AcroForm.FormFields.Where(x => x.Name == "printName").FirstOrDefault();
            if (printNameField != null)
            {
                var location = printNameField.Widgets.FirstOrDefault();
                var editor = new FixedContentEditor(location.Parent as RadFixedPage);
                editor.Position.Translate(location.Rect.X, location.Rect.Y);
                editor.DrawText("vussan");
            }
            #endregion


            #region Company Name
            var companyNameField = document.AcroForm.FormFields.Where(x => x.Name == "companyName").FirstOrDefault();
            //document.NamedDestinations.TryGetValue("companyName", out NamedDestination companyNameDestination);

            if (companyNameField != null)
            {
                var location = companyNameField.Widgets.FirstOrDefault();
                //var editor = new FixedContentEditor(companyNameDestination.GoToAction.Destination.Page);
                var editor = new FixedContentEditor(location.Parent as RadFixedPage);
                editor.Position.Translate(location.Rect.X, location.Rect.Y);
                editor.DrawText("Wintech LLC");
            }
            #endregion

            #region Named Destination
            //document.NamedDestinations.Add("signature", new Location() { Page = document.Pages[1], Left = 200, Top = 485 });
            //document.NamedDestinations.Add("date", new Location() { Page = document.Pages[1], Left = 600, Top = 485 });

            //document.NamedDestinations.TryGetValue("signature", out NamedDestination destinationSignature);
            //document.NamedDestinations.TryGetValue("date", out NamedDestination destinatinDate);


            //if (destinationSignature != null)
            //{
            //    var loc = destinationSignature.Destination as Location;
            //    var editor = new FixedContentEditor(destinationSignature.Destination.Page);

            //    editor.Position.Translate(loc.Left.Value, loc.Top.Value);
            //    editor.DrawImage(imageSource,200,50);
            //}

            //if (destinatinDate != null)
            //{
            //    var loc = destinatinDate.Destination as Location;
            //    var editor = new FixedContentEditor(destinatinDate.Destination.Page);

            //    editor.Position.Translate(loc.Left.Value, loc.Top.Value);
            //    editor.DrawImage(new ImageSource(System.IO.File.Open(@"C:\Users\deepe\Desktop\lion.png", FileMode.Open)), 200, 50);
            //}
            #endregion

            string exportedDocument = @"C:\Users\deepe\Desktop\Signed-sample.pdf";
            System.IO.File.WriteAllBytes(exportedDocument, provider.Export(document));

            //using Stream output = File.OpenWrite(exportedDocument); provider.Export(document, output);
        }
    }

    internal class CustomJpegImageConverter : Telerik.Windows.Documents.Extensibility.JpegImageConverterBase
    {
        public override bool TryConvertToJpegImageData(byte[] imageData, ImageQuality imageQuality, out byte[] jpegImageData)
        {
            string[] magickImageFormats = Enum.GetNames(typeof(MagickFormat)).Select(x => x.ToLower()).ToArray();
            string imageFormat;
            if (this.TryGetImageFormat(imageData, out imageFormat) && magickImageFormats.Contains(imageFormat.ToLower()))
            {
                using (MagickImage magickImage = new MagickImage(imageData))
                {
                    magickImage.Format = MagickFormat.Jpeg;
                    magickImage.Alpha(AlphaOption.Remove);
                    magickImage.Quality = (int)imageQuality;

                    jpegImageData = magickImage.ToByteArray();
                }

                return true;
            }

            jpegImageData = null;
            return false;
        }
    }
}
