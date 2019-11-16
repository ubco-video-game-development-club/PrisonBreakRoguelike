using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<K, V> where V : IComparable<V>
{
    private List<PriorityNode> data;

    public PriorityQueue()
    {
        data = new List<PriorityNode>();
    }

    public void Add(K key, V value)
    {
        data.Add(new PriorityNode(key, value));

        // The index of the child (lower priority)
        int child = data.Count - 1;
        while (child > 0)
        {
            // The index of the parent (higher priority)
            int parent = (child - 1) / 2;
            
            // If the child's value is greater than the parent's value,
            // everything is as it should be and we can stop
            V cVal = data[child].value;
            V pVal = data[parent].value;
            if (cVal.CompareTo(pVal) >= 0)
            {
                break;
            }

            // If the child's value is less than the parent's value,
            // we need to swap the two values
            PriorityNode temp = data[parent];
            data[parent] = data[child];
            data[child] = temp;

            child = parent;
        }
    }

    public K Remove()
    {
        // Store the element to be returned
        K result = data[0].key;

        // Set the first element to the last element
        int last = data.Count - 1;
        data[0] = data[last];

        int parent = 0;
        while (parent < last)
        {
            // Get the index of the smaller child
            int left = (parent * 2) + 1;
            int right = (parent * 2) + 2;
            int child = left < right ? left : right;

            // If the parent's value is less than the child's value,
            // everything is as it should be and we can stop
            V pValue = data[parent].value;
            V cValue = data[child].value;
            if (pValue.CompareTo(cValue) <= 0)
            {
                break;
            }

            // If the parent's value is greater than the child's value,
            // we need to swap the two values
            PriorityNode temp = data[child];
            data[child] = data[parent];
            data[parent] = temp;

            parent = child;
        }

        return result;
    }

    public int Length()
    {
        return data.Count;
    }

    public override String ToString()
    {
        return data.ToString();
    }

    private class PriorityNode
    {
        public K key;
        public V value;

        public PriorityNode(K key, V value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
