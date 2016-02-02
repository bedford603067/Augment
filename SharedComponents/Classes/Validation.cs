using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects
{
    /// <summary>
    /// Exception Class for recording a Validation error
    /// </summary>
    public class ValidationException : Exception
    {
        public string PropertyName;
        public Type PropertyType;

        public ValidationException(string propertyName, string errorMessage)
            : base(errorMessage)
        {
            PropertyName = propertyName;
        }
        public ValidationException(string propertyName, Type propertyType, string errorMessage)
            : base(errorMessage)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
        }
    }

    public class ValidationExceptionCollection : System.Collections.Generic.List<ValidationException> { }

    public static class ValidationClass
    {
        /// <summary>
        /// Validate all Properties of the passed Instance
        /// </summary>
        public static bool Validate(object typeInstance, out ValidationExceptionCollection errorMessages)
        {
            errorMessages = new ValidationExceptionCollection();
            ValidationException objException = null;

            // Validate using rules defined against each Property via RestrictionAttribute
            foreach (System.Reflection.PropertyInfo objInfo in typeInstance.GetType().GetProperties())
            {
                Validate(typeInstance, objInfo, out objException);
                if (objException != null)
                {
                    errorMessages.Add(objException);
                }
            }

            // TODO: Check the BaseType as well
            /*
            if (typeInstance.GetType().BaseType != null && 
                typeInstance.GetType().BaseType.Namespace.IndexOf("BusinessObjects") > -1)
            {
                foreach (System.Reflection.PropertyInfo objInfo in typeInstance.GetType().BaseType.GetProperties())
                {
                    Validate(typeInstance.GetType().BaseType, objInfo, out objException);
                    if (objException != null)
                    {
                        errorMessages.Add(objException);
                    }
                }
            }
            */ 

            return (errorMessages.Count == 0);
        }

        /// <summary>
        /// Validate a single Property of the passed Instance
        /// </summary>
        public static bool Validate(object typeInstance, string propertyName, out ValidationException errorMessage)
        {
            System.Type objType = typeInstance.GetType();
            System.Reflection.PropertyInfo objInfo = objType.GetProperty(propertyName);

            return Validate(typeInstance, objInfo, out errorMessage); ;
        }

        /// <summary>
        /// Validate a single Property of the passed Instance
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static bool Validate(object typeInstance, System.Reflection.PropertyInfo propertyInfo, out ValidationException errorMessage)
        {
            bool blnIsValid = true;
            errorMessage = null;
            object objPropertyValue = null;

            object[] arrAttributes = propertyInfo.GetCustomAttributes(typeof(RestrictionAttribute), false);
            if (arrAttributes.Length > 0)
            {
                RestrictionAttribute objAttribute = (RestrictionAttribute)arrAttributes[0];
                if (objAttribute.Pattern != string.Empty || objAttribute.AllowDbNull == false)
                {
                    // Get value
                    objPropertyValue = propertyInfo.GetValue(typeInstance, null);
                }
                if (objAttribute.AllowDbNull == false)
                {
                    // Check whether value has been specified
                    if (objPropertyValue == null || objPropertyValue.ToString() == string.Empty)
                    {
                        blnIsValid = false;
                    }
                    if (blnIsValid == false)
                    {
                        errorMessage = new ValidationException(propertyInfo.Name, "Value is mandatory and has not been specified");
                        return blnIsValid;
                    }
                }
                if (objAttribute.Pattern != string.Empty)
                {
                    if (objPropertyValue != null && objPropertyValue.ToString() != string.Empty)
                    {
                        // Check whether value specified matches Regular Expression
                        System.Text.RegularExpressions.Regex objRegex = new System.Text.RegularExpressions.Regex(objAttribute.Pattern);
                        if (objPropertyValue == null)
                        {
                            blnIsValid = false;
                        }
                        else
                        {
                            blnIsValid = objRegex.IsMatch(objPropertyValue.ToString());
                        }
                        if (blnIsValid == false)
                        {
                            errorMessage = new ValidationException(propertyInfo.Name, "Value does not match regular expression required." + " " + objAttribute.Pattern);
                        }
                    }
                }
            }

            return blnIsValid;
        }


        /// <summary>
        /// Get RegularExpression from ValidationAttribute for a single Property of the given Type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string GetValidationRegularExpression(Type type, string propertyName)
        {
            System.Reflection.PropertyInfo objInfo = type.GetProperty(propertyName);
            string strRegularExpression = string.Empty;

            object[] arrAttributes = objInfo.GetCustomAttributes(typeof(RestrictionAttribute), false);
            if (arrAttributes.Length > 0)
            {
                RestrictionAttribute objAttribute = (RestrictionAttribute)arrAttributes[0];
                if (objAttribute.Pattern != null)
                {
                    strRegularExpression = objAttribute.Pattern;
                }
            }

            return strRegularExpression;
        }
    }

    [Serializable]
    public class ValidationBase
    {
        /// <summary>
        /// Validate all Properties of the current instance
        /// </summary>
        public virtual bool Validate(out ValidationExceptionCollection errorMessages)
        {
            return ValidationClass.Validate(this, out errorMessages);
        }

        /// <summary>
        /// Validate a single Property of the current instance
        /// </summary>
        public virtual bool Validate(string propertyName, out ValidationException errorMessage)
        {
            return ValidationClass.Validate(this, propertyName, out errorMessage); 
        }

        /// <summary>
        /// Validate a single Property of the current instance
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public virtual bool Validate(System.Reflection.PropertyInfo propertyInfo, out ValidationException errorMessage)
        {
            return ValidationClass.Validate(this, propertyInfo, out errorMessage); 
        }
    }
}
