namespace WebApplication1.Mailkit
{
    public static class EmailTemplateHelper
    {
        public static string GetTemplate(string path, Dictionary<string, string> tokens)
        {
            string body = File.ReadAllText(path);

            foreach (var token in tokens)
            {
                body = body.Replace($"{{{{{token.Key}}}}}", token.Value);
            }

            return body;
        }
    }
}
