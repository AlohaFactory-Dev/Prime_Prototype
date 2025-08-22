using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public static class SimpleInt
{
    private static int tempIndex;

    private static BigInteger quotient;

    // 소수점 n번째 자릿수까지 표시할 지.
    private const int NUMBER_OF_DECIMAL_PLACES = 1;
    private const int ORDER_VALUE = 1000;
    private static BigInteger tempResult;
    private static int tempDecimalPlaces;
    private static readonly List<string> SimpleIntStrings = new() { "", "K", "M", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };

    static string GetSimpleInt(BigInteger value, int tempIndex)
    {
        tempResult = GetBigIntegerResult(tempIndex);
        tempDecimalPlaces = (int)Math.Pow(10, NUMBER_OF_DECIMAL_PLACES);
        tempResult = BigInteger.Divide(value * tempDecimalPlaces, tempResult);
        float result = (float)tempResult / tempDecimalPlaces;
        return $"{result:F1}{SimpleIntStrings[tempIndex]}";
    }

    public static string ToSimpleInt(BigInteger value)
    {
        if (value <= 999)
        {
            return value.ToString();
        }

        return GetSimpleInt(value, GetSimpleIntStringIndex(value));

        static int GetSimpleIntStringIndex(BigInteger value)
        {
            for (tempIndex = 0; tempIndex < SimpleIntStrings.Count; tempIndex++)
            {
                quotient = BigInteger.Divide(value, BigInteger.Pow(ORDER_VALUE, tempIndex));
                if (quotient == 0)
                {
                    break;
                }
            }

            return tempIndex - 1;
        }
    }

    static BigInteger GetBigIntegerResult(int tempIndex1)
    {
        return BigInteger.Pow(ORDER_VALUE, tempIndex1);
    }

    public static BigInteger CelingToSimpleInt(float value)
    {
        return new BigInteger(Math.Ceiling((double)value));
    }
}