using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AudioUnitTest
{
    [Test]
    public void TestHighPassFilter()
    {
        //Assing
        float cutoffFreq = 70;
        double[] fftSamples = new double[] { 1,1 };
        var audioFilter = new AudioFilter();

        //Act
        //Assert
    }
}
