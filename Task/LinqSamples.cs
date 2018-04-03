// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
    [Title("LINQ Module")]
    [Prefix("Linq")]
    public class LinqSamples : SampleHarness
    {

        private DataSource dataSource = new DataSource();

        [Category("Restriction Operators")]
        [Title("Task 1")]
        [Description("Lists all clients whose total turnover (the sum of all orders) exceeds some value X")]
        public void Linq1()
        {
            decimal[] X = { 10000.0000M, 20000.0000M, 3000.0000M };

            foreach (var x in X)
            {
                foreach (var customer in dataSource.Customers.Where(customer => customer.Orders.Sum(order => order.Total) > x))
                    ObjectDumper.Write(customer);

                Console.WriteLine($"X:{x}");
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 2_WithGroupBy_1")]
        [Description("A list of suppliers in the same country and city is made for each customer")]

        public void Linq2_WithGroupBy_1()
        {
            var supplierGroup = dataSource.Suppliers.GroupBy(s => s.City);

            foreach (var customer in dataSource.Customers)
            {
                ObjectDumper.Write(customer);
                Console.WriteLine("List:");

                foreach (var supplier in supplierGroup.Where(group => group.Key == customer.City))
                    ObjectDumper.Write(supplier);

                Console.WriteLine();
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 2_WithGroupBy_2")]
        [Description("A list of suppliers in the same country and city is made for each customer")]

        public void Linq2_WithGroupBy_2()
        {
            var customersLists = dataSource.Customers
                .GroupBy(customer => dataSource.Suppliers
                        .Where(supplier => supplier.City == customer.City).ToList()
                ).ToList();

            foreach (var customer in customersLists)
            {
                ObjectDumper.Write(customer);
                Console.WriteLine("List:");
                ObjectDumper.Write(customer.Key);
                Console.WriteLine();
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 2_WithGroupBy_3")]
        [Description("A list of suppliers in the same country and city is made for each customer")]

        public void Linq2_WithGroupBy_3()
        {
            var customersLists = dataSource.Customers
                .GroupBy(customer => new
                {
                    SuppliersList = dataSource.Suppliers
                        .Where(supplier => supplier.City == customer.City).ToList()
                }).ToList();

            foreach (var customer in customersLists)
            {
                ObjectDumper.Write(customer);
                Console.WriteLine("List:");
                ObjectDumper.Write(customer.Key.SuppliersList);
                Console.WriteLine();
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 2_WithoutGroupBy_1")]
        [Description("A list of suppliers in the same country and city is made for each customer")]

        public void Linq2_WithoutGroupBy_1()
        {
            foreach (var customer in dataSource.Customers)
            {
                ObjectDumper.Write(customer);
                Console.WriteLine("List:");

                foreach (var supplier in dataSource.Suppliers.Where(supplier => supplier.City == customer.City))
                    ObjectDumper.Write(supplier);

                Console.WriteLine();
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 2_WithoutGroupBy_2")]
        [Description("A list of suppliers in the same country and city is made for each customer")]

        public void Linq2_WithoutGroupBy_2()
        {
            var suppliersLists = dataSource.Customers
                .Select(customer => new List<Supplier>(
                dataSource.Suppliers
                .Where(supplier => supplier.City == customer.City))
                ).ToList();

            int i = 0;
            foreach (var customer in dataSource.Customers)
            {
                ObjectDumper.Write(customer);
                Console.WriteLine("List:");
                ObjectDumper.Write(suppliersLists[i]);
                Console.WriteLine();
                i++;
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 3")]
        [Description("Found all customers who had orders that exceed the amount of X")]
        public void Linq3()
        {
            decimal X = 20000.0000M;

            foreach (var customer in dataSource.Customers.Where(customer => customer.Orders.Where(order => order.Total > X).Any()))
                ObjectDumper.Write(customer);
        }

        [Category("Restriction Operators")]
        [Title("Task 4")]
        [Description("List of customers with indication of the month of which year they became customers (accepted for such month and year of the first order)")]
        public void Linq4()
        {
            foreach (var customer in dataSource.Customers)
            {
                if (customer.Orders.Count() == 0) continue;

                var firstOrderDate = customer.Orders.Min(order => order?.OrderDate);
                ObjectDumper.Write($"{firstOrderDate?.Month}/{firstOrderDate?.Year}");
                ObjectDumper.Write(customer);
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 5")]
        [Description("The list of clients with the indication, starting from what month of what year they became clients (accepted for such month and year of the first order)" +
            "sorted by year, month, turnover of the client (from maximal to minimum) and the name of the client")]
        public void Linq5()
        {
            foreach (var customer in dataSource.Customers
                .OrderBy(customer => customer.Orders.Min(order => order?.OrderDate)?.Year)
                .ThenBy(customer => customer.Orders.Min(order => order?.OrderDate)?.Month)
                .ThenByDescending(customer => customer.Orders.Sum(order => order.Total))
                .ThenBy(customer => customer.CompanyName))
            {
                if (customer.Orders.Count() == 0) continue;

                ObjectDumper.Write(customer.Orders.Min(order => order?.OrderDate));
                ObjectDumper.Write(customer);
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 6")]
        [Description("All clients that have a non-numeric postal code or are not populated by a region or the phone does not have an operator code " +
            "(for simplicity, we think that this is equivalent to no round parentheses in the beginning)")]
        public void Linq6()
        {
            Regex postalCodeRegex = new Regex(@"\d");
            foreach (var customer in dataSource.Customers
                .Where(customer =>
                customer.PostalCode == null
                || customer.PostalCode.Any(symbol => !postalCodeRegex.IsMatch(symbol.ToString()))
                || customer.Region == null
                || customer.Phone[0] != '('))
            {
                ObjectDumper.Write(customer);
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 7")]
        [Description("All products are grouped by category, in-by availability in stock, in the last group sort by cost")]
        public void Linq7()
        {
            foreach (var productCategoryGroup in dataSource.Products.GroupBy(product => product.Category))
            {
                Console.WriteLine(productCategoryGroup.Key);

                foreach (var productUnitsInStockGroup in productCategoryGroup.GroupBy(product => product.UnitsInStock))
                {
                    Console.WriteLine(productUnitsInStockGroup.Key);

                    foreach (var product in productUnitsInStockGroup.OrderBy(product => product.UnitPrice))
                    {
                        ObjectDumper.Write(product);
                    }
                }
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 8_1")]
        [Description("The products are grouped in the cheap, average price, expensive groups")]
        public void Linq8_1()
        {
            foreach (var productCheapGroup in dataSource.Products.GroupBy(p => p.UnitPrice < 5.0000M))
            {
                if (productCheapGroup.Key)
                {
                    Console.WriteLine("Group of cheap products: product < 5.0000M");
                    ObjectDumper.Write(productCheapGroup);
                }
                else
                    foreach (var productNotCheapGroup in productCheapGroup.GroupBy(p => p.UnitPrice > 10.0000M))
                    {
                        if (productNotCheapGroup.Key)
                            Console.WriteLine("Group of expensive products: product > 10.0000M");
                        else
                            Console.WriteLine("Group of average products: 5.0000M <= product <= 10.0000M");
                        ObjectDumper.Write(productNotCheapGroup);
                    }
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 8_2")]
        [Description("The products are grouped in the cheap, average price, expensive groups")]
        public void Linq8_2()
        {
            foreach (var productGroup in dataSource.Products
                .GroupBy(product => new
                {
                    Cheap = product.UnitPrice < 5.0000M,
                    Average = product.UnitPrice >= 5.0000M && product.UnitPrice <= 10.0000M,
                    Expensive = product.UnitPrice > 10.0000M
                }))
            {
                if (productGroup.Key.Cheap)
                {
                    Console.WriteLine("Group of cheap products: product < 5.0000M");
                    ObjectDumper.Write(productGroup);
                }
                if (productGroup.Key.Average)
                {
                    Console.WriteLine("Group of average products: 5.0000M <= product <= 10.0000M");
                    ObjectDumper.Write(productGroup);
                }
                if (productGroup.Key.Expensive)
                {
                    Console.WriteLine("Group of expensive products: product > 10.0000M");
                    ObjectDumper.Write(productGroup);
                }
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 9")]
        [Description("Average profitability of each city is calculated (average order amount for all clients from the given city) " +
            "and medium intensity (average number of orders per client from each city)")]
        public void Linq9()
        {
            foreach (var customerCityGroup in dataSource.Customers.GroupBy(customer => customer.City))
            {
                Console.WriteLine(customerCityGroup.Key);
                ObjectDumper.Write(customerCityGroup.SelectMany(customer => customer.Orders.Select(order => order.Total)).Average());
                ObjectDumper.Write(customerCityGroup.Select(customer => customer.Orders.Count()).Average());
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 10")]
        [Description("The average annual statistics of clients activity by months (excluding year), " +
            "statistics by years, by years and months (i.e. when one month in different years has its value) are made")]
        public void Linq10()
        {
            Console.WriteLine("Month");

            foreach (var month in dataSource.Customers
                .SelectMany(customer => customer.Orders
                .Select(order => order.OrderDate.Month))
                .Distinct()
                .OrderBy(d => d))
            {
                Console.WriteLine(month);
                ObjectDumper.Write(dataSource.Customers
                    .Select(customer => customer.Orders
                    .Where(order => order.OrderDate.Month == month)
                    .Count()).Average());
            }

            Console.WriteLine("Year");

            foreach (var year in dataSource.Customers
                .SelectMany(customer => customer.Orders
                .Select(order => order.OrderDate.Year))
                .Distinct()
                .OrderBy(d => d))
            {
                Console.WriteLine(year);
                ObjectDumper.Write(dataSource.Customers
                    .Select(customer => customer.Orders
                    .Where(order => order.OrderDate.Year == year)
                    .Count()).Average());
            }

            Console.WriteLine("Month + Year");

            foreach (var year in dataSource.Customers
                .SelectMany(customer => customer.Orders
                .Select(order => order.OrderDate.Year))
                .Distinct()
                .OrderBy(d => d))

                foreach (var month in dataSource.Customers
                    .SelectMany(customer => customer.Orders
                    .Select(order => order.OrderDate.Month))
                    .Distinct()
                    .OrderBy(d => d))
                {
                    Console.WriteLine($"{month}/{year}");

                    ObjectDumper.Write(dataSource.Customers
                        .Select(customer => customer.Orders
                        .Where(order => order.OrderDate.Month == month && order.OrderDate.Year == year)
                        .Count()).Average());
                }
        }
    }
}
