namespace App
{
    class App
    {
        static void Main(string[] args){
            //dotnet run imports/CV_January2023v2.pdf
            if(args.Length!=1)throw new ArgumentException("You must provide one argument that is referencing the resume's path");
            string[] categories = { "Header", "Education", "Professional Experience", "Languages", "Technical skills", "Events", "Hobbies", "Personal information", "Noteworthy projects" };
            var analysis = 
            Tools.PDFReader.read(args[0],categories);
            Tools.FileWriter.write(analysis);
        }
    }
}