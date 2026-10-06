using System.Collections.Generic;
using UnityEngine;

// data that a kid carries

public class Order
{
    // how many of each does the kid want
    public Dictionary<CandyData, int> Items = new Dictionary<CandyData, int>();

    // total candy in order
    public int TotalCount { get; private set; }

    public static Order Generate(CandyData[] candyPool, int minItems, int maxItems)
    {
        Order order = new Order();
        int count = Random.Range(minItems, maxItems + 1);

        for (int i = 0; i < count; i++)
        {
            CandyData candy = candyPool[Random.Range(0, candyPool.Length)];
            order.Items.TryGetValue(candy, out int current); // curent = 0 if value not in dict
            order.Items[candy] = current + 1;
        }

        order.TotalCount = count;
        return order;
    }
}