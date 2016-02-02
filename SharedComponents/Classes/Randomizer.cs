using System;
using System.Collections.Generic;
using System.Text;

namespace FinalBuild
{
    public class Randomizer
    {
        System.Random _random = new System.Random(Convert.ToInt32(System.DateTime.Now.Ticks % System.Int32.MaxValue));

        public int GetRandomNumber(int minimumValue, int maximumValue)
        {
            return _random.Next(minimumValue, maximumValue + 1);
        }
    }
}
