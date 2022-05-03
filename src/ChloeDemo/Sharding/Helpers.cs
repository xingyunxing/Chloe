using Chloe;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChloeDemo.Sharding
{
    internal class Helpers
    {
        public static void PrintSplitLine()
        {
            Console.WriteLine("--------------------------------------------------------------------------------------");
        }
        public static void PrintResult(PagingResult<Order> result)
        {
            var dataList = result.DataList;

            Console.WriteLine($"Totals: {result.Totals} Takens: {result.DataList.Count}");

            Console.WriteLine(dataList[0].CreateTime.ToString("yyyy-MM-dd HH:mm"));
            Console.WriteLine(dataList[1].CreateTime.ToString("yyyy-MM-dd HH:mm"));
            Console.WriteLine(dataList[dataList.Count - 2].CreateTime.ToString("yyyy-MM-dd HH:mm"));
            Console.WriteLine(dataList[dataList.Count - 1].CreateTime.ToString("yyyy-MM-dd HH:mm"));
        }
    }
}
