using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

public class BTClearArray<T> : BTBaseNode
{
    private T[] array;

    public BTClearArray ( ref T[] array )
    {
        this.array = array;
    }

    protected override void OnEnter ()
    {
        for ( int i = 0; i < array.Length; i++ )
        {
            array[i] = default;
        }
    }

    protected override TaskStatus OnUpdate ()
    {
        return TaskStatus.Success;
    }
}