using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace FinalBuild
{
    public class ValidatePostCode
    {
        /// <summary>
        /// validates formats defined at
        /// http://www.govtalk.gov.uk/gdsc/html/frames/PostCode.htm
        /// format:             example:        regEx:
        /// AN NAA              M1 1AA          [a-zA-Z][0-9]{1,2}
        /// ANN NAA             M60 1NW         [a-zA-Z][0-9]{1,2}
        /// AAN NAA             CR2 6XH         [a-zA-Z]{2}[0-9]{1,2}
        /// AANN NAA            DN55 1PT        [a-zA-Z]{2}[0-9]{1,2}
        /// ANA NAA             W1A 1HQ         [a-zA-Z]{1,2}[0-9][a-zA-Z]
        /// AANA NAA            EC1A 1BB        [a-zA-Z]{1,2}[0-9][a-zA-Z]
        /// </summary>
        /// <param name="postcode"></param>
        public static bool Validate(string outboundCode, string inboundCode)
        {
            try
            {
                outboundCode = outboundCode.Trim();
                

                ArrayList outboundFormats = new ArrayList();

                outboundFormats.Add(new Regex(@"^[a-zA-Z][0-9]{1,2}$"));
                outboundFormats.Add(new Regex(@"^[a-zA-Z]{2}[0-9]{1,2}$"));
                outboundFormats.Add(new Regex(@"^[a-zA-Z]{1,2}[0-9][a-zA-Z]$"));
                
                bool rtnMatch = false;

                foreach (Regex outboundFormat in outboundFormats)
                {
                    if (outboundFormat.IsMatch(outboundCode))
                    {
                        rtnMatch = true;
                        break;
                    }
                }
                if (!rtnMatch)
                {
                    return false;
                }

                if (inboundCode != null)
                {
                    inboundCode = inboundCode.Trim();

                    if (inboundCode!=string.Empty)
                    {
                        Regex inboundFormat = new Regex(@"^[0-9][a-zA-Z]{2}$");
                        if (!inboundFormat.IsMatch(inboundCode))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Test(){


            if (!Validate("M1 ","1AA")) return false;
            if (!Validate("M60 ","1NW")) return false;
            if (!Validate("CR2"," 6XH")) return false;
            if (!Validate("DN55"," 1PT")) return false;
            if (!Validate(" W1A   "," 1HQ ")) return false;
            if (!Validate("EC1A " ,"1BB ")) return false;
            if (!Validate("EC1A ", null)) return false;
            if (!Validate("EC1A ", " ")) return false;
            if (!Validate("EC1A ", "")) return false;

            return true;
            

        }
    }
}
