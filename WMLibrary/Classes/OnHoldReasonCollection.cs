using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    public partial class OnHoldReasonCollection
    {
        private OnHoldReasonCollection mColOnHoldReasonCollection;

        public OnHoldReasonCollection()
        {
            mColOnHoldReasonCollection = this;  // set the current instance         
             
        }
        /// <summary>
        /// Indexer to get the OnHoldReason Object which is present in the collection
        /// </summary>
        /// <param name="onHoldDescription"></param>
        /// <returns></returns>
        public OnHoldReason this[string onHoldDescription]
        {
            get
            {
                return GetOnHoldObject(onHoldDescription);
            }
        }

        /// <summary>
        /// Returns the matched onHoldReason Object
        /// </summary>
        /// <param name="onHoldDescription"></param>
        /// <returns></returns>
        private OnHoldReason GetOnHoldObject(string onHoldDescription)
        {
            foreach (OnHoldReason onHoldReason in mColOnHoldReasonCollection)
            {
                if(onHoldReason.Description.ToUpper().Equals(onHoldDescription.ToUpper()))
                {
                    return onHoldReason;
                }
            }
            return null;
        }
    }
}
