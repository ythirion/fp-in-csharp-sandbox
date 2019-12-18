using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp8
{
    class CoalescingExample
    {
        public void SampleInCSharp7(string value)
        {
            if (value == null)
            {
                value = "SpongeBob";
            }
            // Do something
        }

        public void SampleInCSharp8(string value)
        {
            value ??= "SpongeBob";
            // Do something
        }
    }
}
