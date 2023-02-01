using Newtonsoft.Json;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Tools
{
    class PDFReader
    {
        public static Dictionary<string,string> read(string filepath,string [] categories)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();

            string current_category = "Header";
            string buffer = "";

            PdfDocument document = PdfDocument.Open(@filepath);
            foreach (Page page in document.GetPages())
            {
                var parts = page.Text.Split("    ").Where(part => part.Trim().Length > 0).Select(part => part.TrimStart());
                foreach (string part in parts)
                {
                    bool categoryContainer = false;
                    foreach (string category in categories)
                    {
                        int index = part.IndexOf(category);
                        if (index != -1)
                        {
                            buffer+= index>1?part.Substring(0,index-1):"";
                            buffer = buffer.Replace("‘","'").Replace("’","'");
                            d[current_category] = buffer;
                            current_category = category;
                            buffer = part.Substring(index+category.Length).TrimStart();
                            categoryContainer=true;
                        }
                    }
                    if(!categoryContainer)buffer+=part;
                }
            }
            d[current_category] = buffer;
            return d;
        }
    }
}