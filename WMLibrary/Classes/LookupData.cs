using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using System.Configuration;
using System.Collections;

using FinalBuild;

namespace BusinessObjects.WorkManagement
{
    #region LostTimeReason Class

    public partial class LostTimeReason
    {
        #region Overrides

        public override string ToString()
        {
            return this.Description;
        }

        #endregion

        #region Constructors

        public LostTimeReason()
        {
        }

        internal LostTimeReason(DataRow dataRow)
        {
            this.Populate(dataRow);
        }

        #endregion

        #region Private Methods

        private void Populate(DataRow dataRow)
        {
            this.ID = int.Parse(dataRow["ID"].ToString());
            this.Description = dataRow["Description"].ToString();
            this.Travel = bool.Parse(dataRow["Travel"].ToString());
            this.IsNew = false;
            this.IsDirty = false;
        }

        #endregion
    }

    #endregion

    #region IncompleteReason Class

    public partial class IncompleteReason
    {
        #region Overrides

        public override string ToString()
        {
            return this.Description;
        }

        #endregion

        #region Constructors

        public IncompleteReason()
        {
        }

        internal IncompleteReason(DataRow dataRow)
        {
            this.Populate(dataRow);
        }

        #endregion

        #region Private Methods

        private void Populate(DataRow dataRow)
        {
            this.ID = int.Parse(dataRow["ID"].ToString());
            this.Description = dataRow["Description"].ToString();
            this.ReassignJob = bool.Parse(dataRow["ReassignJob"].ToString());
            this.CompleteJob = bool.Parse(dataRow["CompleteJob"].ToString());
            this.IsCarryover = bool.Parse(dataRow["IsCarryover"].ToString());
            this.IsNew = false;
            this.IsDirty = false;
        }

        #endregion
    }

    #endregion

    #region LostTimeReasonCollection Class

    public partial class LostTimeReasonCollection
    {
        #region Constructors

        public LostTimeReasonCollection()
        {
        }

        public LostTimeReasonCollection(DataTable dataSet)
        {
            if (dataSet != null)
            {
                this.Populate(dataSet);
            }
        }

        #endregion

        #region Private Methods

        private void Populate(DataTable dataTable)
        {
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Populate(dataRow);
            }
        }

        private void Populate(DataRow dataRow)
        {
            this.Add(new LostTimeReason(dataRow));
        }

        #endregion

        #region Public Methods

        public LostTimeReason Find(int iD)
        {
            foreach (LostTimeReason lostTimeReason in this)
            {
                if (lostTimeReason.ID == iD)
                {
                    return lostTimeReason;
                }
            }
            return null;
        }

        #endregion
    }

    #endregion

    #region IncompleteReasonCollection Class

    public partial class IncompleteReasonCollection
    {
        #region Constructors

        public IncompleteReasonCollection()
        {
        }

        public IncompleteReasonCollection(DataTable dataSet)
        {
            if (dataSet != null)
            {
                this.Populate(dataSet);
            }
        }

        #endregion

        #region Private Methods

        private void Populate(DataTable dataTable)
        {
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Populate(dataRow);
            }
        }

        private void Populate(DataRow dataRow)
        {
            this.Add(new IncompleteReason(dataRow));
        }

        #endregion

        #region Public Methods

        public IncompleteReason Find(int iD)
        {
            foreach (IncompleteReason incompleteReason in this)
            {
                if (incompleteReason.ID == iD)
                {
                    return incompleteReason;
                }
            }
            return null;
        }

        #endregion
    }

    #endregion
}

namespace BusinessObjects.WorkManagement
{
    #region AssetLookupData Class

    public partial class AssetLookupData : BusinessObjects.WorkManagement.LookupContainer
    {
        #region Enumerations

        #endregion

        #region Constants

        internal const string C_SelectLookupDataSP = "selAssetLookupData";

        #endregion

        #region Constructors

        public AssetLookupData()
        {
            this.SourceSystem = eWMSourceSystem.AssetManagement;
            this.PopulateLastUpdatedDate();
        }

        internal AssetLookupData(DataSet dataSet)
        {
            this.SourceSystem = eWMSourceSystem.AssetManagement;
            this.PopulateLookupData(dataSet);
            this.PopulateLastUpdatedDate();
        }

        #endregion

        #region Private Methods

        private void PopulateLookupData(string userID)
        {
            this.PopulateLookupData(AssetLookupData.GetLookupDataSet());
        }

        private void PopulateLookupData(DataSet dataSet)
        {
            /*
            DataTable dt = null;
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                dt = dataSet.Tables["ServiceStatus"];
                if (dt != null)
                {
                    this.mColServiceStatus = PopulateReferenceTypeCollection(dt);
                }
                dt = dataSet.Tables["Ownership"];
                if (dt != null)
                {
                    this.mColOwnership = PopulateReferenceTypeCollection(dt);
                }
                dt = dataSet.Tables["AssetWarrantyType"];
                if (dt != null)
                {
                    this.mColAssetWarrantyType = PopulateReferenceTypeCollection(dt);
                }
 
                this.IsNew = false;
                this.IsDirty = false;
            }
            */

            mColAssetTypes = PopulatePrimaryAndSecondaryAssetTypes();
            mColCodeLists = GetAssetAttributeLookupValues();
        }

        private ReferenceTypeCollection PopulateReferenceTypeCollection(DataTable dt)
        {
            ReferenceTypeCollection lReturn = null;
            if (dt != null)
            {
                lReturn = new ReferenceTypeCollection();
                foreach (DataRow dr in dt.Rows)
                {
                    ReferenceType referenceType = new ReferenceType();
                    referenceType.Code = dr["CODE"].ToString();
                    referenceType.Description = dr["DESCRIPT"].ToString();
                    lReturn.Add(referenceType);
                }
            }
            return lReturn;
        }

        private void PopulateLastUpdatedDate()
        {
            this.LastUpdated = DateTime.Now;
        }

        #endregion

        #region Internal Methods

        #region Static Internal Methods

        internal static AssetLookupData Fetch()
        {
            AssetLookupData lookupData = new AssetLookupData();
            lookupData.PopulateLookupData(AssetLookupData.GetLookupDataSet());
            return lookupData;
        }

        internal static DataSet GetLookupDataSet()
        {
            DataAccess dataAccess = Domain.GetADOInstance(Domain.eConnectionName.Corporate);
            DataSet dataSet = new DataSet();
            // SqlDataReader dataReader = null;
            
            /*
            try
            {
                dataReader = dataAccess.GetDataReader(AssetLookupData.C_SelectLookupDataSP, new SqlParameter[]) ; // { wmSourceSystemParameter });
                dataSet.Load(dataReader, LoadOption.OverwriteChanges,
                                         "ServiceStatus",
                                         "Ownership",
                                         "AssetWarrantyType");
            }
            catch (Exception ex)
            {
                if (dataReader != null && !dataReader.IsClosed)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                }
                if (dataSet != null)
                {
                    dataSet = null;
                }
                if (dataAccess != null)
                {
                    dataAccess = null;
                }
                throw ex;
            }
            finally
            {
                if (dataReader != null && !dataReader.IsClosed)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                }
                if (dataAccess != null)
                {
                    dataAccess = null;
                }
            }
            */

            return dataSet;
        }

        internal static BusinessObjects.WorkManagement.AssetAttributeCodeListCollection GetAssetAttributeLookupValues()
        {
            BusinessObjects.WorkManagement.AssetAttributeCodeListCollection lookupData = new BusinessObjects.WorkManagement.AssetAttributeCodeListCollection();
            BusinessObjects.WorkManagement.AssetAttributeCodeList codeList = null;
            ReferenceType item = null;

            DataSet dsResults = null;
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Corporate);
            string storedProcedure = "selAssetAttributeLookupValues";
            string currentAttributeName = string.Empty;

            dsResults = objADO.GetDataSet(storedProcedure, null);   // If slow then set commandTimeout : (storedProcedure, null, 600)

            foreach (DataTable resultTable in dsResults.Tables)
            {
                if (resultTable.Rows.Count > 0)
                {
                    for (int index = 0; index < resultTable.Rows.Count; index++)
                    {
                        // NB: For simplicity this logic is relying on the data returned being ORDER BY AttributeName
                        if (string.IsNullOrEmpty(currentAttributeName) || 
                            resultTable.Rows[index]["AttributeName"].ToString() != currentAttributeName)
                        {
                            currentAttributeName = resultTable.Rows[index]["AttributeName"].ToString();
                            codeList = new BusinessObjects.WorkManagement.AssetAttributeCodeList();
                            codeList.Items = new ReferenceTypeCollection();
                            codeList.AttributeName = currentAttributeName;
                            lookupData.Add(codeList);
                        }
                        item = new ReferenceType();
                        // item.Code = resultTable.Rows[index]["Code"].ToString();
                        item.Description = resultTable.Rows[index]["AttributeValue"].ToString();
                        codeList.Items.Add(item);
                    }
                }
            }

            return lookupData;
        }

        internal static AssetTypeInfoCollection PopulatePrimaryAndSecondaryAssetTypes()
        {
            AssetTypeInfoCollection assetTypes = new AssetTypeInfoCollection();
            DataAccess dataAccess = Domain.GetADOInstance(Domain.eConnectionName.Corporate);
            DataSet dsResults = null;
            DataRow[] childRows = null;
            string storedProcedure = "selAssetTypes";
            string filterExpression = string.Empty;

            dsResults = dataAccess.GetDataSet(storedProcedure, null);
            dsResults.Tables[0].TableName = "PrimaryTypes";
            dsResults.Tables[1].TableName = "Attributes";
            foreach (DataRow drPrimaryType in dsResults.Tables["PrimaryTypes"].Rows)
            {
                assetTypes.Add(new AssetTypeInfo());
                assetTypes[assetTypes.Count - 1].Description = drPrimaryType["AssetTypeDesc"].ToString();
                assetTypes[assetTypes.Count - 1].Code = drPrimaryType["AssetTypeCode"].ToString();

                filterExpression = string.Format("AssetTypeCode='{0}'", assetTypes[assetTypes.Count - 1].Code);
                childRows = dsResults.Tables["Attributes"].Select(filterExpression);
                if(childRows != null && childRows.Length > 0)
                {
                    assetTypes[assetTypes.Count - 1].Attributes = new AssetAttributeCollection();
                    for (int attributeIndex = 0; attributeIndex < childRows.Length; attributeIndex++)
                    {
                        assetTypes[assetTypes.Count - 1].Attributes.Add(new AssetAttribute());
                        assetTypes[assetTypes.Count - 1].Attributes[attributeIndex].Type = eAssetAttributeType.Dynamic;
                        assetTypes[assetTypes.Count - 1].Attributes[attributeIndex].Name = childRows[attributeIndex]["AttributeName"].ToString();
                        if (!childRows[attributeIndex]["AttributeValue"].Equals(DBNull.Value))
                        {
                            assetTypes[assetTypes.Count - 1].Attributes[attributeIndex].Value = childRows[attributeIndex]["AttributeValue"].ToString();
                        }
                        assetTypes[assetTypes.Count - 1].Attributes[attributeIndex].IsEditable = true;
                        // Add metadata to support data entry Validation 
                        if (!childRows[attributeIndex]["DataType"].Equals(DBNull.Value))
                        {
                            assetTypes[assetTypes.Count - 1].Attributes[attributeIndex].DataType = childRows[attributeIndex]["DataType"].ToString();
                        }
                        else
                        {
                            assetTypes[assetTypes.Count - 1].Attributes[attributeIndex].DataType = "string";
                        }
                        if (!childRows[attributeIndex]["Regex"].Equals(DBNull.Value))
                        {
                            // Should be a string - generate Regex constraining Max characters (. allows all characters bar line breaks)
                            assetTypes[assetTypes.Count - 1].Attributes[attributeIndex].ValidationExpression = childRows[attributeIndex]["Regex"].ToString();
                        }
                        if (!childRows[attributeIndex]["RegexMessage"].Equals(DBNull.Value))
                        {
                            assetTypes[assetTypes.Count - 1].Attributes[attributeIndex].ValidationMessage = childRows[attributeIndex]["RegexMessage"].ToString();
                        }
                    }

                    assetTypes[assetTypes.Count - 1].Attributes.Sort("Name", System.ComponentModel.ListSortDirection.Ascending);
                }

                /*
                assetTypes[assetTypes.Count - 1].SecondaryTypes = new ReferenceTypeCollection();
                filterExpression = string.Format("PrimaryCode='{0}'", assetTypes[assetTypes.Count - 1].Code);
                secondaryTypeRows = dsResults.Tables["SecondaryTypes"].Select(filterExpression);
                foreach (DataRow drSecondaryType in secondaryTypeRows)
                {
                    secondaryType = new ReferenceType();
                    secondaryType.Code = drSecondaryType["SecondaryCode"].ToString();
                    secondaryType.Description = drSecondaryType["SecondaryDesc"].ToString();
                    assetTypes[assetTypes.Count - 1].SecondaryTypes.Add(secondaryType);
                }
                */ 
            }

            // Sort by Description alpabetically
            assetTypes.Sort("Description", System.ComponentModel.ListSortDirection.Ascending);

            return assetTypes;
        }

        #endregion

        #endregion

        #region Public Methods

        #region Static Public Methods

        public static BusinessObjects.WorkManagement.AssetLookupData FetchBy()
        {
            return AssetLookupData.Fetch();
        }

        #endregion

        #endregion
    }

    #endregion
}

