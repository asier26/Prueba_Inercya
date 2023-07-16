using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace Customers
{
    // Crear una tabla en SQL Server que se corresponda con los datos del archivo Customers.csv
    // CREATE DATABASE Exercises;

    // CREATE TABLE Customers(
    //  Id VARCHAR(5) PRIMARY KEY IDENTITY(1,1),
    //  Name VARCHAR(100),
    //  Address VARCHAR(200),
    //  City VARCHAR(50),
    //  Country VARCHAR(50),
    //    PostalCode INT,
    //  Phone VARCHAR(20)
    // );
    public class Customer
    {
        public Customer(string id, string name, string address, string city, string country, int postalCode, string phone)
        {
            Id = id;
            Name = name;
            Address = address;
            City = city;
            Country = country;
            PostalCode = postalCode;
            Phone = phone;
        }

        public string Id { get; }
        public string Name { get; }
        public string Address { get; }
        public string City { get; }
        public string Country { get; }
        public int PostalCode { get; }
        public string Phone { get; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string csvFilePath = "Customers.csv";
            string connectionString = "Server=(local);Initial Catalog=Exercises;Integrated Security=True";

            List<Customer> customers = ReadCustomersFromCsv(csvFilePath);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "Customers";

                    // Maper
                    bulkCopy.ColumnMappings.Add("Id", "Id");
                    bulkCopy.ColumnMappings.Add("Name", "Name");
                    bulkCopy.ColumnMappings.Add("Address", "Address");
                    bulkCopy.ColumnMappings.Add("City", "City");
                    bulkCopy.ColumnMappings.Add("Country", "Country");
                    bulkCopy.ColumnMappings.Add("PostalCode", "PostalCode");
                    bulkCopy.ColumnMappings.Add("Phone", "Phone");

                    bulkCopy.BatchSize = 1000;
                    bulkCopy.BulkCopyTimeout = 60;

                    try
                    {
                        // Escribe los datos en la tabla "Customers" en SQL Server
                        DataTable customersDataTable = ToDataTable(customers);
                        bulkCopy.WriteToServer(customersDataTable);
                        Console.WriteLine("Datos cargados exitosamente en la tabla Customers.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error al cargar los datos en la tabla Customers: " + ex.Message);
                    }
                }
            }
        }

        static List<Customer> ReadCustomersFromCsv(string filePath)
        {
            List<Customer> customers = new List<Customer>();

            using (var reader = new StreamReader(filePath))
            {
                // Omite la primera línea si contiene encabezados
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    Customer customer = new Customer(values[0], values[1], values[2], values[3], values[4], int.Parse(values[5]), values[6]);

                    customers.Add(customer);
                }
            }

            return customers;
        }

        static DataTable ToDataTable(List<Customer> customers)
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("Id", typeof(string));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Address", typeof(string));
            dataTable.Columns.Add("City", typeof(string));
            dataTable.Columns.Add("Country", typeof(string));
            dataTable.Columns.Add("PostalCode", typeof(int));
            dataTable.Columns.Add("Phone", typeof(string));

            foreach (Customer customer in customers)
            {
                dataTable.Rows.Add(customer.Id, customer.Name, customer.Address, customer.City, customer.Country, customer.PostalCode, customer.Phone);
            }

            return dataTable;
        }
    }
}