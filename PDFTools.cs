using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.Export;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
using UglyToad.PdfPig.Util;

namespace Tools
{
    class PDFReader
    {

        public static void read(string filepath, string[] categories)
        {

            PdfDocument document = PdfDocument.Open(@filepath);

            textAnalysis(document);

        }

        // I followed the instructions on https://github.com/UglyToad/PdfPig/wiki/Document-Layout-Analysis
        // And modified them to fit with my analysis
        public static void textAnalysis(PdfDocument document)
        {
            for (var i = 0; i < document.NumberOfPages; i++)
            {
                var page = document.GetPage(i + 1);
                var words = page.GetWords();
                var rcopti = new RecursiveXYCut.RecursiveXYCutOptions();
                rcopti.MinimumWidth = page.Width / 3.0;
                rcopti.DominantFontWidthFunc = letters => letters.Select(l => l.GlyphRectangle.Width).Average();
                rcopti.DominantFontHeightFunc = letters => letters.Select(l => l.GlyphRectangle.Height).Average();
                var rxcut = new RecursiveXYCut(rcopti);
                var blocks = rxcut.GetBlocks(words);

                xmlAnalysis(document, rxcut);
            }
        }

        // The produced xml file is used, with imports/blackwhite.tif & cv.png to generate a visual overview of the analysis (see exports/layoutvalblocks.png by example)
        // to generate yourself this image, follow the instructions on  https://github.com/UglyToad/PdfPig/wiki/Document-Layout-Analysis#using-layoutevalgui
        public static void xmlAnalysis(PdfDocument document, RecursiveXYCut rxcut)
        {
            PageXmlTextExporter pageXmlTextExporter = new PageXmlTextExporter(
                DefaultWordExtractor.Instance,
                rxcut,
                UnsupervisedReadingOrderDetector.Instance
                );

            {
                for (var i = 0; i < document.NumberOfPages; i++)
                {
                    var page = document.GetPage(i + 1);
                    string xml = pageXmlTextExporter.Get(page);
                    File.WriteAllText("exports/document.pagexml.xml", xml);
                }
            }
        }
        /*

        PREVIOUOS METHODS OF ANALYSIS (will be deleted soon)

        /// <summary>Need to continue on working on the date detction</summary>
        private static void findDate(string content)
        {
            string months = monthDetection(content);
            List<int> numbers = yearsDetection(content);

        }

        private static string monthDetection(string content)
        {
            // Names of months in english. - For the system language's months names switch InvariantInfo to CurrentInfo
            string[] months = DateTimeFormatInfo.InvariantInfo.MonthNames;
            string detected = "";
            foreach (string month in months)
            {
                int index = content.IndexOf(month);
                if (index != -1) detected += month + " ";
            }
            return detected;
        }

        private static List<int> yearsDetection(string content)
        {
            List<int> years = new List<int>();
            string[] numbers = Regex.Split(content, @"\D+");
            foreach (string number in numbers)
            {
                int n;
                bool isNumeric = int.TryParse(number, out n);
                if (isNumeric && n > 2000)
                {
                    years.Add(n);
                }
            }
            //return String.Join("", content.ToCharArray().Where(Char.IsDigit));
            return years;
        }

        private static Dictionary<string, string> findResumeCategoriesContent(PdfDocument document, string[] categories)
        {
            Dictionary<string, string> d_categories = new Dictionary<string, string>();
            string current_category = "Header";
            string buffer = "";

            foreach (Page page in document.GetPages())
            {
                string spaces = "       ";
                var parts = page.Text.Split(spaces).Where(part => part.Trim().Length > 0).Select(part => part.TrimStart());
                foreach (string part in parts)
                {
                    bool categoryContainer = false;
                    foreach (string category in categories)
                    {
                        int index = part.IndexOf(category);
                        if (index != -1)
                        {
                            buffer += index > 1 ? part.Substring(0, index - 1) : "";
                            buffer = buffer.Replace("‘", "'").Replace("’", "'");
                            d_categories[current_category] = buffer;
                            current_category = category;
                            buffer = part.Substring(index + category.Length).TrimStart();
                            categoryContainer = true;
                        }
                    }
                    if (!categoryContainer) buffer += part;
                }
            }
            d_categories[current_category] = buffer;
            return d_categories;
        }*/
    }
}