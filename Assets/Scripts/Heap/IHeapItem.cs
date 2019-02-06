﻿using UnityEngine;
using UnityEditor;
using System;

public interface IHeapItem<T> : IComparable<T> {
    int heapIndex {
        get;
        set;
    }
}