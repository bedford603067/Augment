using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects
{
    /// <summary>
    /// Metadata for Validating a Property\Field value
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RestrictionAttribute : Attribute
    {
        #region Private Fields

        private string mstrPattern = string.Empty;
        private int mintMaxInclusiveLength = 8000;
        private int mintMinInclusiveLength = 0;

        private string mstrValidationMessage = string.Empty;

        #endregion

        #region Public Properties

        /// <summary>
        /// Regular Expression
        /// </summary>
        public string Pattern
        {
            get
            {
                return mstrPattern;
            }
            set
            {
                mstrPattern = value;
            }
        }

        public int MaxInclusiveLength
        {
            get
            {
                return mintMaxInclusiveLength;
            }
            set
            {
                mintMaxInclusiveLength = value;
            }
        }

        public int MinInclusiveLength
        {
            get
            {
                return mintMinInclusiveLength;
            }
            set
            {
                mintMinInclusiveLength = value;
            }
        }

        public bool AllowDbNull
        {
            get
            {
                return (mintMaxInclusiveLength > 0);
            }
        }

        public bool IsFixedLength
        {
            get
            {
                return (mintMaxInclusiveLength == mintMinInclusiveLength);
            }
        }

        /// <summary>
        /// User Message (to show when Regex fail detected) 
        /// </summary>
        public string ValidationMessage
        {
            get
            {
                return mstrValidationMessage;
            }
            set
            {
                mstrValidationMessage = value;
            }
        }

        #endregion

        #region Constructors

        public RestrictionAttribute(int maxInclusiveLength)
        {
            maxInclusiveLength = mintMaxInclusiveLength;
        }
        public RestrictionAttribute(string pattern)
        {
            mstrPattern = pattern;
        }
        public RestrictionAttribute(int maxInclusiveLength, string pattern)
        {
            maxInclusiveLength = mintMaxInclusiveLength;
            mstrPattern = pattern;
        }
        public RestrictionAttribute(int maxInclusiveLength, int minInclusiveLength)
        {
            maxInclusiveLength = mintMaxInclusiveLength;
            minInclusiveLength = mintMinInclusiveLength;
        }
        public RestrictionAttribute(int maxInclusiveLength, int minInclusiveLength, string pattern)
        {
            maxInclusiveLength = mintMaxInclusiveLength;
            minInclusiveLength = mintMinInclusiveLength;
            mstrPattern = pattern;
        }

        /// <summary>
        /// Utilised by CodeDom when adding to CustomAttributes
        /// </summary>
        public RestrictionAttribute(){}

        #endregion
    }
}
