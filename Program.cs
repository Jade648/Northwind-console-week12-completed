using System;
using NLog.Web;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NorthwindConsole.Model;

namespace NorthwindConsole
{
    class Program
    {
        // create static instance of Logger
        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();
        static void Main(string[] args)
        {
            logger.Info("Program started");

            try
            {
                string choice;
                do
                {
                    Console.WriteLine("1) Display Categories");
                    Console.WriteLine("2) Display Category and related products");
                    Console.WriteLine("3) Display all Categories and their related products");
                    Console.WriteLine("4) Add Product");
                    Console.WriteLine("5) Edit Product");
                    Console.WriteLine("6) Delete Product");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"Option {choice} selected");
                    if (choice == "1")
                    {
                        var db = new NorthwindContext();
                        var query = db.Categories.OrderBy(p => p.CategoryName);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{query.Count()} records returned");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName} - {item.Description}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    } else if (choice == "1")
                    {
                        Category category = new Category();
                        Console.WriteLine("Enter Category Name:");
                        category.CategoryName = Console.ReadLine();
                        Console.WriteLine("Enter the Category Description:");
                        category.Description = Console.ReadLine();

                        ValidationContext context = new ValidationContext(category, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(category, context, results, true);
                        if (isValid)
                        {
                            logger.Info("Validation passed");
                            var db = new NorthwindContext();
                            // check for unique name
                            if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                            {
                                // generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                            }
                            
                            else
                            {
                                // SDave category to db
                                db.AddCategory(category);
                                logger.Info("New catgeory added");
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }

                    }else if (choice == "2")
                    {
                        var db = new NorthwindContext();
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Select the category whose products you want to display:");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");
                        Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                        Console.WriteLine($"{category.CategoryName} - {category.Description}");
                        foreach (Product p in category.Products)
                        {
                            Console.WriteLine(p.ProductName);
                        }
                    }
                    else if (choice == "3")
                    {
                        var db = new NorthwindContext();
                        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName}");
                            foreach (Product p in item.Products)
                            {
                                Console.WriteLine($"\t{p.ProductName}");
                            }        
                        }
                    } else if (choice == "4") {
                        var db = new NorthwindContext();
                        // Add new product
                        Product NewProduct = InputProduct(db);
                        db.AddProduct(NewProduct);
                    } else if (choice == "5") {

                        Console.WriteLine("Choose a Product to Edit:");

                          var db = new NorthwindContext();
                          var product = GetProduct(db);

                        if (product != null){

                            Product UpdatedProduct = InputProduct(db);
                        
                            if (UpdatedProduct != null) {
                                UpdatedProduct.ProductId = product.ProductId; 
                                db.EditProduct(UpdatedProduct);
                                logger.Info($"Products (Id: {product.ProductId})updated");
                            }  
                        }
                        Console.WriteLine();
                    } else if (choice == "6") {
                        Console.WriteLine("Choose the product to delete:");
                        var db = new NorthwindContext();
                        var product = GetProduct(db);
                        if(product != null){
                            db.DeleteProduct(product);
                            logger.Info($"Product (id: {product.ProductId}) deleted");
                        }
                    }    

                } while (choice.ToLower() != "q");

            } catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }

        public static Product GetProduct(NorthwindContext db){

            var products = db.Products.OrderBy(p => p.ProductId);
            foreach(Product p in products){
                 Console.WriteLine($"{p.ProductId}: {p.ProductName}");
            }

         if (int.TryParse(Console.ReadLine(),out int  ProductId))
         {
            Product product = db.Products.FirstOrDefault(p => p.ProductId == ProductId);
            if (product != null){
                return product;
            }
         }
          logger.Error("Invalid ProductId");
           return null;
        }

        public  static Product InputProduct(NorthwindContext db){
            Product product = new Product();
            Console.WriteLine("Enter the Product name");
            product.ProductName = Console.ReadLine();

            Console.WriteLine("Select the catgeory:");
            foreach(Category c in db.Categories.OrderBy(c => c.CategoryName)){
                Console.WriteLine("{0}) {1}", c.CategoryId, c.CategoryName);
            }
            product.CategoryId = Convert.ToInt32(Console.ReadLine());
            
            ValidationContext context = new ValidationContext (product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();
             
            var isValid = Validator.TryValidateObject(product, context, results, true);
            if(isValid)
            {

                if(db.Products.Any(p => p.ProductName == product.ProductName))
                {
                    isValid = false;
                    results.Add(new ValidationResult("Product name exists",new string[] {"Name"}));
                }
                else
                {
                    logger.Info("Validation passed");
                }

                }

                if (!isValid)
                {
                    foreach( var result in results)
                    {

                    logger.Error($"{result.MemberNames.First()}: {result.ErrorMessage}");  
                }

                return null;
                    
            }

            return product;

        } 
    }
}

