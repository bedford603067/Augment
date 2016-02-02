using System;
using System.Collections;
using System.Reflection;
using System.Data;
using System.Web.UI.WebControls;


namespace BusinessObjects
    {

    /// <summary>
    /// This class is used to compare any type(property) of a class.
    /// This class automatically fetches the type of the property and compares.
    /// </summary>
    public sealed class GenericComparer : IGenericComparer
    {
        #region GenericComparer
        /// <summary>
        /// Sorting order
        /// </summary>
        public enum SortOrder
        {
            Ascending = 0,
            Descending = 1
        }

        Type objectType;
        /// <summary>
        /// Type of the object to be compared.
        /// </summary>
        public Type ObjectType
        {
            get { return objectType; }
            set { objectType = value; }
        }


        string sortcolumn = "";
        /// <summary>
        /// Column(public property of the class) to be sorted.
        /// </summary>
        public string SortColumn
        {
            get { return sortcolumn; }
            set { sortcolumn = value; }
        }


        int sortingOrder = 0;
        /// <summary>
        /// Sorting order.
        /// </summary>
        public int SortingOrder
        {
            get { return sortingOrder; }
            set { sortingOrder = value; }
        }

        /// <summary>
        /// Compare interface implementation
        /// </summary>
        /// <param name="x">Object 1</param>
        /// <param name="y">Object 2</param>
        /// <returns>Result of comparison</returns>
        public int Compare(object x, object y)
        {
            string[] sorts = sortcolumn.Split(',');

            foreach (string sort in sorts)
            {
                string[] aryProps = sort.Split('.');

                PropertyInfo propertyInfo;
                IComparable obj1;
                IComparable obj2;

                //special case "Book.ReadFrequency.Description"
                if (aryProps.Length > 1)
                {
                    object objSub1 = null;
                    object objSub2 = null;

                    objSub1 = x;
                    objSub2 = y;

                    //use either of the object to get the type 
                    propertyInfo = objSub1.GetType().GetProperty(aryProps[0]);


                    //loop the objects and get the sub object in question
                    for (int i = 0; i < aryProps.Length - 1; i++)
                    {


                        objSub1 = propertyInfo.GetValue(objSub1, null);
                        objSub2 = propertyInfo.GetValue(objSub2, null);
                        //use either of the object to get the type 
                        propertyInfo = objSub1.GetType().GetProperty(aryProps[i + 1]);

                    }


                    obj1 = (IComparable)propertyInfo.GetValue(objSub1, null);
                    obj2 = (IComparable)propertyInfo.GetValue(objSub2, null);



                }
                else
                {
                    //Dynamically get the protery info based on the property name
                    propertyInfo = ObjectType.GetProperty(sortcolumn);
                    //Get the value of the instance
                    obj1 = (IComparable)propertyInfo.GetValue(x, null);
                    obj2 = (IComparable)propertyInfo.GetValue(y, null);

                }

                if (obj1 == null && obj2 == null)
                    continue;

                //Compare based on the sorting order.
                switch (sortingOrder)
                {
                    case (int)GenericComparer.SortOrder.Ascending:

                        if (obj1 == null)
                            return -1;
                        else
                        {
                            int result = obj1.CompareTo(obj2);
                            if (result == 0)
                            {
                                continue;
                            }
                            else
                            {
                                return result;
                            }
                        }

                    case (int)GenericComparer.SortOrder.Descending:
                        if (obj2 == null)
                            return 1;
                        else
                        {
                            int result = obj2.CompareTo(obj1);
                            if (result == 0)
                            {
                                continue;
                            }
                            else
                            {
                                return result;
                            }
                        }
                }
            }
            return 0;
		}

		#endregion
    }



    /// <summary>
    /// IGenericComparer - Generic interface for object comparison
    /// </summary>
    public interface IGenericComparer : IComparer
    {
        Type ObjectType
        {
            get;
            set;
        }
        string SortColumn
        {
            get;
            set;
        }
        int SortingOrder
        {
            get;
            set;
        }
    }

    
     /// <summary>
    /// ISortable - Generic interface for sortable collection
    /// </summary>
    public interface ISortable
    {
        string SortColumn
        {
            get;
            set;
        }
        GenericComparer.SortOrder SortingOrder
        {
            get;
            set;
        }
        Type SortObjectType
        {
            get;
            set;
        }
        void Sort();
    }

    /// <summary>
    /// Abstract implementation of Sortable collection.
    /// </summary>
    public abstract class SortableCollectionBase : CollectionBase,ISortable
    {
        string sortcolumn="";
        
        public string SortColumn
        {
            get{return sortcolumn;}
            set{
                //reset to ascending
                if (value == sortcolumn)
                {
                    //if this is the second request for the same item swap the order
                    if (sortingOrder == GenericComparer.SortOrder.Ascending)
                    {
                        sortingOrder = GenericComparer.SortOrder.Descending;
                    }
                    else
                    {
                        sortingOrder = GenericComparer.SortOrder.Ascending;
                    }
                }
                else
                {
                    sortingOrder = GenericComparer.SortOrder.Ascending;
                    sortcolumn = value;
                }
            
            }
        }
        GenericComparer.SortOrder sortingOrder = GenericComparer.SortOrder.Ascending;

        public GenericComparer.SortOrder SortingOrder
        {
            get{return sortingOrder;}set{sortingOrder = value;}
        }

        Type sortObjectType;
        
        public Type SortObjectType
        {
            get{return sortObjectType;} set{sortObjectType = value;} 
        }

        public virtual void Sort() 
        {
            if(sortcolumn == "") 
                throw new Exception("Sort column required."); 
            if(SortObjectType == null) 
                throw new Exception("Sort object type required."); 
            IGenericComparer sorter = new GenericComparer();
            sorter.ObjectType = sortObjectType;
            sorter.SortColumn = sortcolumn;
            sorter.SortingOrder = (int)sortingOrder;
            InnerList.Sort(sorter);
        }
    }


    
}