using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadFromGP5 : MonoBehaviour
{
    public void LoadGP5()
    {
        using (StreamReader reader = new StreamReader("C:\\Users\\peer\\UnityProjects\\TabGenerator\\Assets\\AudioFiles\\the-eagles-hotel_california.gp3"))
        {
            string content = reader.ReadToEnd();

            print(content);
        }

    }
}