namespace App
{
    class App
    {
        public static bool devMode = false;
        static void Main(string[] args){
            if(devMode){ // to avoid providing args at each execution
                string [] devArgs = {"CV_January2023v2.pdf"};
                args = devArgs;
            }
            if(args.Length!=1)throw new ArgumentException("You must provide one argument that is referencing the resume's path");
            string[] categories = { "Header", "Education", "Professional Experience", "Languages", "Technical skills", "Events", "Hobbies", "Personal information", "Noteworthy projects" };
            var analysis = Tools.PDFReader.read(args[0],categories);
            Tools.FileWriter.write(analysis);
        }
    }
}