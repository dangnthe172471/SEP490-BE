using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Helpers
{
    public class EmailTemplateHelper
    {
        public static string LoadTemplate(string fileName)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Không tìm thấy template: {filePath}");

            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        public static string RenderTemplate(string template, Dictionary<string, string> values)
        {
            foreach (var kv in values)
                template = template.Replace($"{{{{{kv.Key}}}}}", kv.Value ?? string.Empty);
            return template;
        }
    }
}
