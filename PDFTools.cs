using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.Export;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
using UglyToad.PdfPig.Util;

namespace Tools
{
    class PDFReader
    {

        public static Dictionary<string,string> read(string filepath, string[] categories)
        {

            PdfDocument document = PdfDocument.Open(@filepath);
            return resumeAnalysis(document);

        }

        private static Dictionary<string,string> resumeAnalysis(PdfDocument document)
        {
            
            var blocks = findBlocks(document,2.5);
            
            // Dictionary<string,TextBlock> d_categories = new Dictionary<string, TextBlock>();
            //Find titles
            var page = document.GetPage(1);
            // foreach (var page in document.GetPages())
            // {
                // creation of groups of words of the same size, sorted by size
                var words = page
                            .GetWords();
                words = from w in words
                        orderby w.Letters[0].FontSize
                        descending
                        select w;
                var gwords = words
                            .GroupBy(w =>w.Letters[0].FontSize).ToList();
            
                int idGroupTitles = 1; //  0 is The "Leo Filoche" headline, so 1 is the category titles 
                var cat_words = string.Join(" ",gwords[idGroupTitles].Select(w=>w.Text).ToArray());
                List<string> categories = new List<string>();
                foreach(var block in blocks){
                    if(cat_words.IndexOf(block.Text)!=-1){
                        categories.Add(block.Text);
                    }
                }
                categories.Add("Header"); // the abstract that is about me at the top of the resume
            // }
            return findResumeCategoriesContent(document,categories.ToArray());
        }

        // I followed the instructions on https://github.com/UglyToad/PdfPig/wiki/Document-Layout-Analysis
        // And modified them to fit with my analysis
        ///<summary> Automated analysis with UglyToad tools</summary>
        private static List<TextBlock> findBlocks(PdfDocument document, double precision){
            var blocks = new List<TextBlock>();
            foreach (var page in document.GetPages())
            {
                var rcopti = new RecursiveXYCut.RecursiveXYCutOptions();
                rcopti.MinimumWidth = page.Width / precision;
                rcopti.DominantFontWidthFunc = letters => letters.Select(l => l.GlyphRectangle.Width).Average();
                rcopti.DominantFontHeightFunc = letters => letters.Select(l => l.GlyphRectangle.Height).Average();

                var rxcut = new RecursiveXYCut(rcopti);
                var words = page.GetWords();
                blocks.AddRange((List<TextBlock>) rxcut.GetBlocks(words));
                xmlAnalysis(document, rxcut,page.Number); // <- graphic display of the page analysis 
            }
            return blocks;
        }

        /// <summary>Produce a file used for the visualisation of the analysis</summary> 
        // The produced xml file is used, with imports/blackwhite.tif & cv.png to generate a visual overview of the analysis (see exports/layoutvalblocks.png by example)
        // to generate yourself this image, follow the instructions on  https://github.com/UglyToad/PdfPig/wiki/Document-Layout-Analysis#using-layoutevalgui
        private static void xmlAnalysis(PdfDocument document, RecursiveXYCut rxcut,int pageNumber=1)
        {
            PageXmlTextExporter pageXmlTextExporter = new PageXmlTextExporter(
            DefaultWordExtractor.Instance,
            rxcut,
            UnsupervisedReadingOrderDetector.Instance
            );
            foreach (var page in document.GetPages())
            {
                string xml = pageXmlTextExporter.Get(page);
                File.WriteAllText("exports/document.pagexml"+(pageNumber==1?"":pageNumber)+".xml", xml);
            }
        }
        
        

        private static Dictionary<string, string> findResumeCategoriesContent(PdfDocument document, string[] categories)
        {
            Dictionary<string, string> d_categories = new Dictionary<string, string>();
            string current_category = "Header";
            string buffer = "";

            foreach (Page page in document.GetPages())
            {
                string spaces = "    ";
                var parts = page.Text.Split(spaces).Where(part => part.Trim().Length > 0).Select(part => part.TrimStart());
                foreach (string part in parts)
                {
                    bool categoryContainer = false;
                    foreach (string category in categories)
                    {
                        int index = part.IndexOf(category);
                        if (index != -1)
                        {
                            buffer += (buffer.Length==0?"":" ") + (index > 1 ? part.Substring(0, index - 1) : "");
                            buffer = buffer.Replace("‘", "'").Replace("’", "'");
                            d_categories[current_category] = buffer;
                            current_category = category;
                            buffer = part.Substring(index + category.Length).TrimStart();
                            categoryContainer = true;
                        }
                    }
                    if (!categoryContainer) buffer += (buffer.Length==0?"":" ") + part;
                }
            }
            d_categories[current_category] = buffer;
            return d_categories;
        }
    }
    
}