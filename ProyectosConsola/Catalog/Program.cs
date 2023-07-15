using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Serialization;

namespace Catalog
{
    interface IDataReader<T>
    {
        List<T> ReadData(string filePath, Encoding encoding);
    }
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Product> Products { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    class CsvDataReader<T> : IDataReader<T>
    {
        public List<T> ReadData(string filePath, Encoding encoding)
        {
            List<T> data = new List<T>();

            using (var reader = new StreamReader(filePath, encoding))
            {
                string headerLine = reader.ReadLine();
                string[] headers = headerLine.Split(';');

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(';');

                    T record = CreateRecordInstance<T>(headers, values);
                    data.Add(record);
                }
            }

            return data;
        }

        private T CreateRecordInstance<T>(string[] headers, string[] values)
        {
            T record = Activator.CreateInstance<T>();

            for (int i = 0; i < headers.Length; i++)
            {
                var property = typeof(T).GetProperty(headers[i]);
                if (property != null)
                {
                    var convertedValue = Convert.ChangeType(values[i], property.PropertyType);
                    property.SetValue(record, convertedValue);
                }
            }
            return record;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string categoriesFile = "Categories.csv";
            string productsFile = "Products.csv";
            string jsonOutputFile = "Catalog.json";
            string xmlOutputFile = "Catalog.xml";

            IDataReader<Category> categoryDataReader = new CsvDataReader<Category>();
            IDataReader<Product> productDataReader = new CsvDataReader<Product>();

            List<Category> categories = categoryDataReader.ReadData(categoriesFile, Encoding.UTF8);
            List<Product> products = productDataReader.ReadData(productsFile, Encoding.Latin1);

            var data = categories.Select(category => new
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Products = products.Where(p => p.CategoryId == category.Id).ToList()
            });

            var categoryData = data.Select(category => new Category
            {
                Description = category.Description,
                Id = category.Id,
                Name = category.Name,
                Products = category.Products
            }).ToList();

            // Exportar a JSON
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string jsonData = JsonSerializer.Serialize(categoryData, options);
            File.WriteAllText(jsonOutputFile, jsonData, Encoding.Default);

            Console.WriteLine($"Archivo JSON generado: {jsonOutputFile}");

            // Exportar a XML
            using (StreamWriter writer = new StreamWriter(xmlOutputFile, false, Encoding.Default))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Category>), new XmlRootAttribute("Data"));
                xmlSerializer.Serialize(writer, categoryData);
            }

            Console.WriteLine($"Archivo XML generado: {xmlOutputFile}");
        }
    }
}

