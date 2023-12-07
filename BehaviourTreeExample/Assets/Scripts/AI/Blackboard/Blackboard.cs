using System.Collections.Generic;

public class Blackboard
{
    private Dictionary<string, object> dictionary = new();

    public T GetVariable<T> ( string name )
    {
        if ( dictionary.ContainsKey(name) )
        {
            return (T)dictionary[name];
        }
        return default;
    }

    public void SetVariable<T> ( string name, T variable )
    {
        if ( dictionary.ContainsKey(name) )
        {
            dictionary[name] = variable;
        }
        else
        {
            dictionary.Add(name, variable);
        }
    }

    public void ClearBlackBoard ( bool confirmation = false, bool confirmation2 = false )
    {
        if ( confirmation && confirmation2 )
        {
            dictionary.Clear();
        }
    }
}
