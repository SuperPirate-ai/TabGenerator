using Accord.Compat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

public class CustomLogger
{
    private static string msg = "";

    private static string UnfoldObject(object obj)
    {
        string temp = "";
        switch (obj.GetType())
        {
            case Type t when t == typeof(int):
                temp += (int)obj + " ";
                break;
            case Type t when t == typeof(string):
                temp += (string)obj + " ";
                break;
            case Type t when t == typeof(float):
                temp += ((float)obj).ToString("f7") + " ";
                break;
            case Type t when t == typeof(double):
                temp += (double)obj + " ";
                break;
            case Type t when t == typeof(bool):
                temp += (bool)obj + " ";
                break;
            case Type t when t == typeof(int[]):
                temp += "[";
                foreach (object item in (Array)obj)
                {
                    temp += UnfoldObject(item) + ", ";
                }
                temp += "]";
                break;
            case Type t when t == typeof(string[]):
                temp += "[";
                foreach (object item in (Array)obj)
                {
                    temp += UnfoldObject(item) + ", ";
                }
                temp += "]";
                break;
            case Type t when t == typeof(float[]):
                temp += "[";
                foreach (object item in (Array)obj)
                {
                    temp += UnfoldObject(item) + ", ";
                }
                temp += "]";
                break;
            case Type t when t == typeof(double[]):
                temp += "[";
                foreach (object item in (Array)obj)
                {
                    temp += UnfoldObject(item) + ", ";
                }
                temp += "]";
                break;
            case Type t when t == typeof(bool[]):
                temp += "[";
                foreach (object item in (Array)obj)
                {
                    temp += UnfoldObject(item) + ", ";
                }
                temp += "]";
                break;
            case Type t when t == typeof(Array):
                temp += "[";
                foreach (object item in (Array)obj)
                {
                    temp += UnfoldObject(item) + ", ";
                }
                temp += "]";
                break;

            case Type t when t.FullName.StartsWith("System.Collections.Generic.List"):
                temp += "[";
                for (int i = 0; i < ((System.Collections.IList)obj).Count; i++)
                {
                    temp += UnfoldObject(((System.Collections.IList)obj)[i]) + ", ";
                }
                temp += "]";
                break;
            case Type t when t.FullName.StartsWith("System.Collections.Generic.Dictionary"):
                temp += "{";
                var casted = (System.Collections.IDictionary)obj;
                foreach (object value in casted.Values)
                {
                    temp += UnfoldObject(value) + ", ";
                }
                temp += "}";
                break;
            case Type t when t == typeof(Vector2):
                temp += "(";
                temp += UnfoldObject(((Vector2)obj).X) + ", " + UnfoldObject(((Vector2)obj).Y);
                temp += ")";
                break;
            case Type t when t == typeof(Vector3):
                temp += "(";
                temp += UnfoldObject(((Vector3)obj).X) + ", " + UnfoldObject(((Vector3)obj).Y) + ", " + UnfoldObject(((Vector3)obj).Z);
                temp += ")";
                break;
            case Type t when t == typeof(Enumerable):
                temp += "[";
                foreach (object item in (Array)obj)
                {
                    temp += UnfoldObject(item) + ", ";
                }
                temp += "]";
                break;

            case Type t when t.FullName.StartsWith("System.Tuple"):
                temp += obj.ToString();
                break;
            case Type t when t.FullName.StartsWith("System.ValueTuple"):
                var tuple = obj;
                var fields = tuple.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

                foreach (var field in fields)
                {
                    var value = field.GetValue(tuple);
                    temp += $"{UnfoldObject(value)} ,";
                }
                
                break;
            default:
                temp += "UNSUPPORTED " + obj.GetType().FullName + " " + obj.ToString() + " ";
                break;
        }
        return temp;
    }

    public static void Log<T>(T arg)
    {
        msg += " " + UnfoldObject(arg) + " |";
    }

    public static string Flush()
    {
        string temp = msg;
        if (temp.Length > 0)
        { 
            UnityEngine.Debug.Log(msg); 
        }
        
        msg = "";
        return temp;
    }

    public static string Clear()
    {
        string temp = msg;
        msg = "";
        return temp;
    }
}
