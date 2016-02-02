using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Security.Cryptography;
using System.Configuration;

namespace BusinessObjects.WorkManagement
{
    public partial class FileAssociation
    {
        /// <summary>
        /// Populate FileAssociation given a filePath to an actual File
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="associationType"></param>
        /// <param name="isFileEncrypted"></param>
        public void Populate(string filePath, eFileAssociationType associationType, bool isFileEncrypted)
        {
            mstrPath = filePath;
            mstrDescription = filePath.Substring(filePath.LastIndexOf(@"\") + 1);
            mobjAssociationType = associationType;
            using (FileStream inStream = new FileStream(mstrPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                mintSizeInBytes = (int)inStream.Length;
            }
            mstrHash = CalculateFileHash(mstrPath);
            mblnIsEncrypted = isFileEncrypted;
        }

        /// <summary>
        // Populate FileAssociation given metadata retrieved from SQL Database
        /// </summary>
        /// <param name="drMember"></param>
        public void Populate(System.Data.DataRow drMember)
        {
            if (!drMember["FilePath"].Equals(DBNull.Value))
            {
                mstrPath = drMember["FilePath"].ToString();
            }
            if (!drMember["FileDesc"].Equals(DBNull.Value))
            {
                mstrDescription = drMember["FileDesc"].ToString();
            }
            if (!drMember["AssociationType"].Equals(DBNull.Value))
            {
                mobjAssociationType = (eFileAssociationType)Enum.Parse(typeof(eFileAssociationType), drMember["AssociationType"].ToString());
            }
            if (!drMember["SizeInBytes"].Equals(DBNull.Value))
            {
                mintSizeInBytes = (int)drMember["SizeInBytes"];
            }
            if (!drMember["FileHash"].Equals(DBNull.Value))
            {
                mstrHash = drMember["FileHash"].ToString();
            }
            if (!drMember["IsEncrypted"].Equals(DBNull.Value))
            {
                mblnIsEncrypted = (bool)drMember["IsEncrypted"];
            }
        }

        public string DeriveDestinationPath(string targetDirectory)
        {
            string sourceDirectory = mstrPath.Substring(0, mstrPath.LastIndexOf(@"\"));

            return mstrPath.Replace(sourceDirectory, targetDirectory);
        }

        /// <summary>
        /// Calculate a Hash value representing the content of the File
        /// Note. Calculating Hash value for Files at remote locations can be slow
        /// </summary>
        private string CalculateFileHash(string filePath)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hashValue;

            using (FileStream inStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096))
            {
                hashValue = md5.ComputeHash(inStream);
            }

            return BitConverter.ToString(hashValue);
        }
    }
}
