using Newtonsoft.Json;

namespace Tools
{
    class FileWriter
    {
        public static void write(string path, string content){
            File.WriteAllText(@path, content);
        }

        public static void write(Dictionary<string,string> d){
            write("result.json",JsonConverter.convertToJsonFormat(d));
        }
    }

    class JsonConverter
    {
        public static string convertToJsonFormat(Dictionary<string,string> d){
           return JsonConvert.SerializeObject(d,Formatting.Indented);
        }
    }
}