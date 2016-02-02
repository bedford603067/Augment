using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    /// <summary>
    /// To assist in working out Message Queue to send a work item (e.g. a Job) to.
    /// NB. Intentionally not static as expecting use in a Multithreaded context
    /// </summary>
    public class QueueAssistant
    {
        private int _numberOfQueuesAvailable = 1;

        public QueueAssistant(int numberOfQueuesAvailable)
        {
            _numberOfQueuesAvailable = numberOfQueuesAvailable;
        }

        /// <summary>
        /// Work out the appropriate Q Extension to add to chosen Q based on itemID 
        /// </summary>
        /// <param name="queueItemID"></param>
        /// <returns></returns>
        public string DeriveQueueExtensionFromItem(int queueItemID)
        {
            return DeriveQueueExtensionFromItem(queueItemID, null);
        }

        /// <summary>
        /// Work out the appropriate Q Extension to add to chosen Q based on itemID 
        /// Option to create Q Extension using other factors, e.g. Source, Area, etc
        /// </summary>
        /// <param name="queueItemID"></param>
        /// <param name="additionalFactors"></param>
        /// <returns></returns>
        public string DeriveQueueExtensionFromItem(int queueItemID, object[] additionalFactors)
        {
            int queueExtension = 0;
            int maxNoOfQueues = _numberOfQueuesAvailable;

            if (maxNoOfQueues < 2)
            {
                // Not appropriate to derive Extension, its Queue0 for all items.
                return queueExtension.ToString();
            }

            if (queueItemID < 100)
            {
                queueExtension = (queueItemID < maxNoOfQueues) ? queueItemID : (queueItemID - maxNoOfQueues);
                return queueExtension.ToString();
            }

            // ID is at least 3 digits. Get the last two digits.
            string tens = queueItemID.ToString().Substring(queueItemID.ToString().Length - 2, 1);
            string units = queueItemID.ToString().Substring(queueItemID.ToString().Length - 1, 1);

            queueExtension = int.Parse(string.Concat(new string[] { tens, units }));
            while (queueExtension > (maxNoOfQueues - 1))
            {
                queueExtension -= maxNoOfQueues;
            }
            if (queueExtension < 1)
            {
                queueExtension = 0;
            }

            return queueExtension.ToString();
        }
    }
}
