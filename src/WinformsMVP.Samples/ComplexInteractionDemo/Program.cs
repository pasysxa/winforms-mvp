using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WinformsMVP.Samples.ComplexInteractionDemo.Models;
using WinformsMVP.Samples.ComplexInteractionDemo.OrderManagement;
using WinformsMVP.Samples.ComplexInteractionDemo.ProductSelector;
using WinformsMVP.Samples.ComplexInteractionDemo.OrderSummary;

namespace WinformsMVP.Samples.ComplexInteractionDemo
{
    /// <summary>
    /// Launcher for Complex Interaction Demo
    /// Demonstrates parent-child MVP pattern with two UserControls
    /// </summary>
    public static class ComplexInteractionDemoProgram
    {
        public static void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create sample product data
            var products = CreateSampleProducts();

            // Create the main form
            var form = new OrderManagementForm();

            // Create child control presenters
            var productSelectorPresenter = new ProductSelectorPresenter(form.ProductSelectorView);
            var orderSummaryPresenter = new OrderSummaryPresenter(form.OrderSummaryView);

            // Create main presenter with child presenters
            var mainPresenter = new OrderManagementPresenter(
                productSelectorPresenter,
                orderSummaryPresenter,
                products);

            // Attach view and initialize
            mainPresenter.AttachView(form);
            mainPresenter.Initialize();

            // Run the application
            Application.Run(form);
        }

        private static IList<Product> CreateSampleProducts()
        {
            return new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Laptop",
                    Category = "Electronics",
                    Price = 999.99m,
                    StockQuantity = 15
                },
                new Product
                {
                    Id = 2,
                    Name = "Mouse",
                    Category = "Accessories",
                    Price = 29.99m,
                    StockQuantity = 50
                },
                new Product
                {
                    Id = 3,
                    Name = "Keyboard",
                    Category = "Accessories",
                    Price = 79.99m,
                    StockQuantity = 30
                },
                new Product
                {
                    Id = 4,
                    Name = "Monitor",
                    Category = "Electronics",
                    Price = 299.99m,
                    StockQuantity = 20
                },
                new Product
                {
                    Id = 5,
                    Name = "USB Cable",
                    Category = "Accessories",
                    Price = 9.99m,
                    StockQuantity = 100
                },
                new Product
                {
                    Id = 6,
                    Name = "Webcam",
                    Category = "Electronics",
                    Price = 59.99m,
                    StockQuantity = 25
                },
                new Product
                {
                    Id = 7,
                    Name = "Headphones",
                    Category = "Audio",
                    Price = 149.99m,
                    StockQuantity = 40
                },
                new Product
                {
                    Id = 8,
                    Name = "Speakers",
                    Category = "Audio",
                    Price = 89.99m,
                    StockQuantity = 18
                }
            };
        }
    }
}
