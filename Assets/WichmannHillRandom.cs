using System;
using UnityEngine;

public class WichmannHillRandom {

    int num1;
    int num2;
    int num3;

	public WichmannHillRandom()
    {
        num1 = 100;
        num2 = 100;
        num3 = 100;
    }
	public WichmannHillRandom(int s1, int s2, int s3)
    {
        num1 = s1;
        num2 = s2;
        num3 = s3;
    }
    public int[] GetNums()
    {
        return new[] { num1, num2, num3 };
    }
    public double Next()
    {
        num1 = 171 * num1 % 30269;
        num2 = 172 * num2 % 30307;
        num3 = 170 * num3 % 30323;
        var outputtingValue = num1 / 30269d + num2 / 30307d + num3 / 30323d;
        return outputtingValue % 1.0;
    }
    /**
     * <summary>Returns a random integer between minValue (inclusive) and maxValue (exclusive)</summary>
     * <exception cref="ArgumentOutOfRangeException">Thrown if minValue is greater than maxValue.</exception>
     */
    public int Next(int minValue, int maxValue)
    {
        if (minValue > maxValue)
            throw new ArgumentOutOfRangeException("minValue");
        var obtainedDouble = Next();
        return minValue + (int)(obtainedDouble * (maxValue - minValue));
    }

}
