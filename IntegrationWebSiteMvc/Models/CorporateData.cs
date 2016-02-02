using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IntegrationWebSiteMvc.Models
{
    public class CorporateMetadata
    {
        public List<BusinessObjects.WorkManagement.Asset> Assets
        {
            get
            {
                List<BusinessObjects.WorkManagement.Asset> assets = null;

                CorporateData.ExternalDataServiceClient serviceProxy = new CorporateData.ExternalDataServiceClient();
                assets = serviceProxy.SearchAssets(new CorporateData.AssetSearchCriteria());
                serviceProxy.Close();

                return assets;
            }
        }

        public List<BusinessObjects.WorkManagement.Customer> Customers
        {
            get
            {
                List<BusinessObjects.WorkManagement.Customer> customers = null;

                CorporateData.ExternalDataServiceClient serviceProxy = new CorporateData.ExternalDataServiceClient();
                customers = serviceProxy.SearchCustomers(new CorporateData.CustomerSearchCriteria() );
                serviceProxy.Close();

                return customers;
            }
        }

        public List<BusinessObjects.WorkManagement.Material> Materials
        {
            get
            {
                List<BusinessObjects.WorkManagement.Material> materials = null;

                CorporateData.ExternalDataServiceClient serviceProxy = new CorporateData.ExternalDataServiceClient();
                materials = serviceProxy.SearchMaterials(string.Empty,0,null,null,null);
                serviceProxy.Close();

                return materials;
            }
        }

        public List<StockSummary> Stock
        {
            get
            {
                List<BusinessObjects.WorkManagement.Stock> stockContainer = null;

                CorporateData.ExternalDataServiceClient serviceProxy = new CorporateData.ExternalDataServiceClient();
                stockContainer = serviceProxy.SearchStock(string.Empty, 0, 0, null);
                serviceProxy.Close();

                List<StockSummary> stockList = new List<StockSummary>();
                for (int index = 0; index < stockContainer.Count; index++)
                {
                    stockList.Add(new StockSummary { Material = stockContainer[index].StockItems.First().Material, ItemCount = stockContainer[index].StockItems.Count });
                }

                return stockList;
            }
        }

        public List<BusinessObjects.WorkManagement.PerformanceMeasurement> Measurements
        {
            get
            {
                List<BusinessObjects.WorkManagement.PerformanceMeasurement> measurements = null;

                CorporateData.ExternalDataServiceClient serviceProxy = new CorporateData.ExternalDataServiceClient();
                measurements = serviceProxy.SearchMeasurements(string.Empty);
                serviceProxy.Close();

                return measurements;
            }
        }

        public List<BusinessObjects.WorkManagement.Asset> SearchForAsset(int assetID, bool includeMeasurements)
        {
            return SearchForAsset(assetID, true, includeMeasurements);
        }

        public List<BusinessObjects.WorkManagement.Asset> SearchForAsset(int assetID, bool includeChildAssets, bool includeMeasurements)
        {
            List<BusinessObjects.WorkManagement.Asset> assets = null;

            CorporateData.ExternalDataServiceClient serviceProxy = new CorporateData.ExternalDataServiceClient();

            CorporateData.AssetSearchCriteria criteria = new CorporateData.AssetSearchCriteria();
            criteria.ID = assetID;
            criteria.IncludeMeasurementHistory = includeMeasurements;
            criteria.IncludeLocation = true;

            assets = serviceProxy.SearchAssets(criteria);
            serviceProxy.Close();

            if (includeChildAssets)
            {
                if (assets != null && assets.Count == 1 && assets[0].Groups != null)
                {
                    BusinessObjects.WorkManagement.Asset parentAsset = assets[0];
                    List<BusinessObjects.WorkManagement.Asset> childAssets = new List<BusinessObjects.WorkManagement.Asset>();
                    for (int index = 0; index < parentAsset.Groups.Count; index++)
                    {
                        if (parentAsset.Groups[index].Assets != null)
                        {
                            for (int assetIndex = 0; assetIndex < parentAsset.Groups[index].Assets.Count; assetIndex++)
                            {
                                childAssets.Add(parentAsset.Groups[index].Assets[assetIndex]);
                            }
                        }
                    }

                    // Return childAssets if such are present
                    assets = childAssets;
                }
            }

            return assets;
        }

        public CorporateData.AssetCollection SaveAsset(CorporateData.AssetUpdate assetUpdate)
        {
            CorporateData.AssetCollection assets = null;

            CorporateData.ExternalDataServiceClient serviceProxy = new CorporateData.ExternalDataServiceClient();
            assets = serviceProxy.UploadAssetData(assetUpdate);
            serviceProxy.Close();

            return assets;
        }

        public CorporateData.AssetUpdate GenerateAssetUpdate(int assetID)
        {
            CorporateData.AssetUpdate assetUpdate = new CorporateData.AssetUpdate();
            BusinessObjects.WorkManagement.Asset selectedAsset = null;

            List<BusinessObjects.WorkManagement.Asset> assets = SearchForAsset(assetID, false, false); // Second arg "false" means don't return "child" Assets
            if (assets != null && assets.Count > 0)
            {
                selectedAsset = assets.Find(new System.Predicate<BusinessObjects.WorkManagement.Asset>(a => a.ID == assetID)); 

                assetUpdate.AssetID = assetID;
                assetUpdate.AssetType = selectedAsset.AssetType;
                // assetUpdate.Attributes = selectedAsset.Attributes;
                assetUpdate.Code = selectedAsset.Code;
                if (assets[0].ExtendedProperties != null)
                {
                    for (int index = 0; index < selectedAsset.ExtendedProperties.Count; index++)
                    {
                        if (assets[0].ExtendedProperties[index].Key == "CustomerID" &&
                           !string.IsNullOrEmpty(selectedAsset.ExtendedProperties[index].Value.ToString()))
                        {
                            assetUpdate.CustomerID = int.Parse(selectedAsset.ExtendedProperties[index].Value.ToString());
                            break;
                        }
                    }
                }
                assetUpdate.Description = selectedAsset.Description;
                // assetUpdate.Group = selectedAsset.;
                assetUpdate.Location = selectedAsset.Location;
            }

            return assetUpdate;
        }

        private static int _assetAttributeCount;
        public BusinessObjects.WorkManagement.AssetAttributeCollection GetAssetAttributes(int assetID, int startIndex, int pageSize, string sortBy)
        {
            BusinessObjects.WorkManagement.AssetAttributeCollection assetAttributes = new BusinessObjects.WorkManagement.AssetAttributeCollection();
            List<BusinessObjects.WorkManagement.Asset> assets = SearchForAsset(assetID, true, false);

            _assetAttributeCount = 0;
            if (assets != null && assets.Count > 0)
            {
                _assetAttributeCount = assets[0].Attributes != null ? assets[0].Attributes.Count : 0;
                if (_assetAttributeCount > 0)
                {
                    int endIndex = startIndex + (pageSize - 1);
                    if(endIndex > (_assetAttributeCount - 1))
                    {
                        endIndex = _assetAttributeCount - 1;
                    }
                    for (int index = 0; index < assets[0].Attributes.Count; index++)
                    {
                        if (index >= startIndex && index <= endIndex)
                        {
                            assetAttributes.Add(assets[0].Attributes[index]);
                        }
                    }
                }
            }

            return assetAttributes;
        }
        public static int GetAssetAttributesCount(int assetID)
        {
            return _assetAttributeCount;
        }
        public void SaveAssetAttribute(int assetID, BusinessObjects.WorkManagement.AssetAttribute instance)
        {
            Console.WriteLine(instance.Name);
        }

        public int SaveCustomer(BusinessObjects.WorkManagement.Customer customer, string userID)
        {
            List<int> customerIDs = null;

            CorporateData.ExternalDataServiceClient serviceProxy = new CorporateData.ExternalDataServiceClient();
            customerIDs = serviceProxy.UploadCustomerData(customer, userID);
            serviceProxy.Close();

            if (customerIDs != null && customerIDs.Count > 0)
            {
                return customerIDs[0];
            }

            return 0;
        }

        public BusinessObjects.WorkManagement.Contact CreateContactFromCustomerDetails(BusinessObjects.WorkManagement.Customer customer)
        {
            BusinessObjects.WorkManagement.Contact contact = null;

            contact = new BusinessObjects.WorkManagement.Contact { Surname = customer.Surname, TelephoneNo = customer.TelephoneNo, MobileNo = customer.MobileNo, EMail = customer.EMail,
            AlternativeTelephoneNo = customer.AlternativeTelephoneNo, CompanyName = customer.CompanyName, Forenames = customer.Forenames, Title = customer.Title};

            return contact;
        }

        public void SaveContact(int parentEntityID, string parentEntityType, BusinessObjects.WorkManagement.Contact contact)
        {
            CorporateData.ExternalDataServiceClient serviceProxy = new CorporateData.ExternalDataServiceClient();
            serviceProxy.SaveContactInfo(parentEntityID, parentEntityType, new List<BusinessObjects.WorkManagement.Contact>() { contact });
        }

        public BusinessObjects.WorkManagement.StockItemCollection QueryStock(int materialID)
        {
            List<BusinessObjects.WorkManagement.Stock> stockContainer = null;

            CorporateData.ExternalDataServiceClient serviceProxy = new CorporateData.ExternalDataServiceClient();
            stockContainer = serviceProxy.SearchStock(string.Empty, materialID, 0, null);
            serviceProxy.Close();

            if (stockContainer[0].StockItems != null)
            {
                return stockContainer[0].StockItems;
            }

            return new BusinessObjects.WorkManagement.StockItemCollection();
        }
    }

    public class StockSummary
    {
        public BusinessObjects.WorkManagement.Material Material;
        public int ItemCount;
    }
}