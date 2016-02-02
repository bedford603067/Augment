using System;

using System.Security.Cryptography;
using System.IO;

namespace FinalBuild
{
	public class Encryption
	{
		#region Private Fields

		// Key snd Vector for Encryption. Default values can be overridden using one of the constructors.
		private byte[] marrEncryptionKey = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
		private byte[] marrEncryptionVector = { 65, 110, 68, 26, 69, 178, 200, 219 };

		#endregion

		#region Enumerations

		public enum eCryptoMode
		{Encrypt,Decrypt}
		
		public enum eSymmetricCryptoAlgorithm
		{DES,RC2,Rinjdael,TripleDES}

		#endregion

		#region Public Methods

		public Encryption()
		{
			// Use default values for encryption Key and Vector
		}

		public Encryption(byte[] encryptionKey, byte[] encryptionVector)
		{
			// Specify values for encryption Key and Vector
			marrEncryptionKey = encryptionKey;
			marrEncryptionVector = encryptionVector;
		}

		/// <summary>
		/// Encrypt String using default Algorithm
		/// </summary>
		/// <param name="targetValue"></param>
		/// <returns></returns>
		public byte[] Encrypt(string targetValue)
		{
			return Encrypt(targetValue, eSymmetricCryptoAlgorithm.TripleDES);
		}

        /// <summary>
        /// Encrypt File content using default Algorithm
        /// </summary>
        /// <param name="targetValue"></param>
        /// <returns></returns>
        public byte[] Encrypt(FileStream decryptedFile)
        {
            return Encrypt(decryptedFile, eSymmetricCryptoAlgorithm.TripleDES);
        }

		/// <summary>
		/// Encrypt String using specified Algorithm
		/// </summary>
		/// <param name="targetValue"></param>
		/// <param name="cryptoAlgorithm"></param>
		/// <returns></returns>
		public byte[] Encrypt(string targetValue, eSymmetricCryptoAlgorithm cryptoAlgorithm)
		{
			byte[] arrEncryptedValue = null;

			if (targetValue.Length > 0)
			{
				arrEncryptedValue = SymmetricCrypto(cryptoAlgorithm,
													eCryptoMode.Encrypt,
													System.Text.Encoding.UTF8.GetBytes(targetValue),
													marrEncryptionKey,
													marrEncryptionVector);
			}

			return arrEncryptedValue;
		}

        /// <summary>
        /// Encrypt File using specified Algorithm
        /// </summary>
        /// <param name="targetValue"></param>
        /// <param name="cryptoAlgorithm"></param>
        /// <returns></returns>
        public byte[] Encrypt(FileStream decryptedFile, eSymmetricCryptoAlgorithm cryptoAlgorithm)
        {
            byte[] arrEncryptedValue = null;
            byte[] buffer = null;

            buffer = new byte[decryptedFile.Length];
            decryptedFile.Read(buffer, 0, buffer.Length);
            decryptedFile.Close();

            arrEncryptedValue = SymmetricCrypto(cryptoAlgorithm,
                                                eCryptoMode.Encrypt,
                                                buffer,
                                                marrEncryptionKey,
                                                marrEncryptionVector);

            return arrEncryptedValue;
        }

        /// <summary>
		/// Decrypt to String using default Algorithm
		/// </summary>
		/// <param name="targetValue"></param>
		/// <returns></returns>
		public string Decrypt(byte[] targetValue)
		{
			return Decrypt(targetValue, eSymmetricCryptoAlgorithm.TripleDES);
		}

        /// <summary>
        /// Decrypt specified File using default Algorithm
        /// </summary>
        /// <param name="targetValue"></param>
        /// <returns></returns>
        public byte[] DecryptFile(FileStream encryptedFile)
        {
            return DecryptFile(encryptedFile, eSymmetricCryptoAlgorithm.TripleDES);
        }

		/// <summary>
		/// Decrypt to String using specified Algorithm
		/// </summary>
		/// <param name="targetValue"></param>
		/// <param name="cryptoAlgorithm"></param>
		/// <returns></returns>
		public string Decrypt(byte[] targetValue, eSymmetricCryptoAlgorithm cryptoAlgorithm)
		{
			byte[] arrDecryptedValue = null;
			string strDecryptedValue = string.Empty;

			if (targetValue.Length >= 8)
			{
				arrDecryptedValue = SymmetricCrypto(cryptoAlgorithm,
												    eCryptoMode.Decrypt,
													targetValue,
													marrEncryptionKey,
													marrEncryptionVector);
				strDecryptedValue = System.Text.Encoding.UTF8.GetString(arrDecryptedValue);
			}

			return strDecryptedValue;
		}

        /// <summary>
        /// Decrypt specified File using specified Algorithm
        /// </summary>
        /// <param name="targetValue"></param>
        /// <param name="cryptoAlgorithm"></param>
        /// <returns></returns>
        public byte[] DecryptFile(FileStream encryptedFile, eSymmetricCryptoAlgorithm cryptoAlgorithm)
        {
            byte[] arrDecryptedValue = null;
            byte[] buffer = null;

            buffer = new byte[encryptedFile.Length];
            encryptedFile.Read(buffer, 0, buffer.Length);
            encryptedFile.Close();

            arrDecryptedValue = SymmetricCrypto(cryptoAlgorithm,
                                                eCryptoMode.Decrypt,
                                                buffer,
                                                marrEncryptionKey,
                                                marrEncryptionVector);

            return arrDecryptedValue;
        }

		/// <summary>
		/// Return a Key appropriate to the specified CryptoAlgorithm
		/// </summary>
		/// <param name="Algorithm"></param>
		/// <returns></returns>
	    public string GetSymmetricKey(eSymmetricCryptoAlgorithm Algorithm)
	   {
			SymmetricAlgorithm cryptEngine;
			string strReturn;

			// Use Crypto Engine and generate Key
			cryptEngine = GetSymmetricEncryptionProvider(Algorithm);
			strReturn = GetSymmetricKey(cryptEngine);
			cryptEngine = null;

			return strReturn;
		}

		/// <summary>
		/// Returns a Key from the specified Provider
		/// </summary>
		/// <param name="Provider"></param>
		/// <returns></returns>
		public string GetSymmetricKey(System.Security.Cryptography.SymmetricAlgorithm Provider)
		{
			string strReturn;

			// Generate Key
			Provider.GenerateKey();
			strReturn = System.Text.ASCIIEncoding.ASCII.GetString(Provider.Key);

			return strReturn;
		}

		/// <summary>
		/// Return a Vector appropriate to the specified CryptoAlgorithm
		/// </summary>
		/// <param name="Algorithm"></param>
		/// <returns></returns>
		public string GetSymmetricVector(eSymmetricCryptoAlgorithm Algorithm)
		{
			SymmetricAlgorithm cryptEngine;
			string strReturn;

			// Use Crypto Engine and generate Vector
			cryptEngine = GetSymmetricEncryptionProvider(Algorithm);
			strReturn = GetSymmetricVector(cryptEngine);
			cryptEngine = null;

			return strReturn;
		}

		/// <summary>
		/// Returns a Vector from the specified Provider
		/// </summary>
		/// <param name="Provider"></param>
		/// <returns></returns>
		public string GetSymmetricVector(System.Security.Cryptography.SymmetricAlgorithm Provider)
		{
			string strReturn;

			// Generate Vector
			Provider.GenerateIV();
			strReturn = System.Text.ASCIIEncoding.ASCII.GetString(Provider.IV);

			return strReturn;

		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Performs Encryption\Decryption using specified Algorithm, Key, and Vector
		/// </summary>
		/// <param name="cryptoAlgorithm"></param>
		/// <param name="cryptoMode"></param>
		/// <param name="inputData"></param>
		/// <param name="encryptionKey"></param>
		/// <param name="encryptionVector"></param>
		/// <returns></returns>
		private byte[] SymmetricCrypto(eSymmetricCryptoAlgorithm cryptoAlgorithm, eCryptoMode cryptoMode, byte[] inputData, byte[] encryptionKey, byte[] encryptionVector)
		{
			SymmetricAlgorithm cryptProvider;
			ICryptoTransform cryptEngine = null;
			MemoryStream objCryptoStream;
			CryptoStream objCryptoProcess;
			byte[] arrOutputData;

			// Create encryption provider - required for validation steps below
			// ----------------------------------------------------------------
			cryptProvider = GetSymmetricEncryptionProvider(cryptoAlgorithm);

			// Validate contents of byte arrays
			// --------------------------------
			if (ValidateSymmetricalEncryptionParameters(cryptProvider, ref inputData, ref encryptionKey, ref encryptionVector, cryptoMode) == false)
			{
				return null;
			}

			// Attach Key and Vector to cryptProvider
			cryptProvider.Key = encryptionKey;
			cryptProvider.IV = encryptionVector;

			// Encrypt\Decrypt
			objCryptoStream = new MemoryStream();
			switch (cryptoMode)
			{
				case eCryptoMode.Encrypt:
					{
						cryptEngine = cryptProvider.CreateEncryptor();
						break;
					}
				case eCryptoMode.Decrypt:
					{
						cryptEngine = cryptProvider.CreateDecryptor();
						break;
					}
			}

			// Encrypted\Decrypt value into a Stream
			objCryptoProcess = new CryptoStream(objCryptoStream, cryptEngine, CryptoStreamMode.Write);
			objCryptoProcess.Write(inputData, 0, inputData.Length);
			objCryptoProcess.Close();

			// Extract Encrypted\Decrypted value from Stream
			arrOutputData = objCryptoStream.ToArray();

			cryptEngine = null;
			cryptProvider = null;
			objCryptoStream = null;
			objCryptoProcess = null;

			return arrOutputData;
		}

		/// <summary>
		/// Get Encryption Provider appropriate to eSymmetricCryptoAlgorithm value specified
		/// </summary>
		/// <param name="Algorithm"></param>
		/// <returns></returns>
		private System.Security.Cryptography.SymmetricAlgorithm GetSymmetricEncryptionProvider(eSymmetricCryptoAlgorithm Algorithm)
		{
			// Return a provider to match our chosen encryption algorithm
			// ==========================================================
			SymmetricAlgorithm objReturn = null;

			// Generate crypto provider
			switch(Algorithm)
			{
				case eSymmetricCryptoAlgorithm.DES:
				{
					objReturn = new DESCryptoServiceProvider();
					break;
				}
				case eSymmetricCryptoAlgorithm.RC2:
				{
					objReturn = new RC2CryptoServiceProvider();
					break;
				}
				case eSymmetricCryptoAlgorithm.Rinjdael:
				{
					objReturn = new RijndaelManaged();
					break;
				}
				case eSymmetricCryptoAlgorithm.TripleDES:
				{
					objReturn = new TripleDESCryptoServiceProvider();
					break;
				}
			}

			// Apply standard parameters to this provider
			// (None to be applied at this time)

			// Return provider
			return objReturn;
		}

		/// <summary>
		/// Validates that specified Key and Vector are appropriate to the Algorithm to be used for the encryption 
		/// </summary>
		/// <param name="Provider"></param>
		/// <param name="Data"></param>
		/// <param name="Key"></param>
		/// <param name="Vector"></param>
		/// <param name="Mode"></param>
		/// <returns></returns>
		private bool ValidateSymmetricalEncryptionParameters(System.Security.Cryptography.SymmetricAlgorithm Provider,ref byte[] Data, ref byte[] Key, ref byte[] Vector, eCryptoMode Mode)
		{
			// DATA VALIDATION
			// ===============
			if (Data.Length == 0)
			{
				switch (Mode)
				{
					case eCryptoMode.Encrypt:
					{
						throw new SymmetricEncryptionDataValidationException("No data to encrypt");
						break;
					}
					case eCryptoMode.Decrypt:
					{
						throw new SymmetricEncryptionDataValidationException("No data to decrypt");
						break;
					}
				}
			}

			// KEY VALIDATION
			// ==============
			if (Key.Length == 0)
			{
				// Supply a key if one has not been supplied.
				if (Mode == eCryptoMode.Encrypt)
				{
					Key = System.Text.ASCIIEncoding.ASCII.GetBytes(this.GetSymmetricKey(Provider));
				}
			}
			else
			{
				// Pad the key if it is too short/trim the key if it is too long
				while (Key.Length * 8 != Provider.KeySize)
				{
					if (Key.Length * 8 < Provider.KeySize) 
					{
						// TODO
						// ReDim Preserve Key(Key.Length);
						Key[Key.Length - 1] = 0;
					}
					else
					{
						// TODO
						//ReDim Preserve Key(Key.Length - 2);
					}
				}
			}

			
			// Ensure that the key is strong - validation will error if a weak key is used.
			// Can only be checked for DES and TripleDES Keys
			if (Provider.GetType() == typeof(DESCryptoServiceProvider))
			{
				if ( DESCryptoServiceProvider.IsWeakKey(Key))
				{
					throw new SymmetricEncryptionDataValidationException("Known weak key supplied for DES algorithm.");
				}
			}
			if (Provider.GetType() == typeof(TripleDESCryptoServiceProvider)) 
			{
				if ( TripleDESCryptoServiceProvider.IsWeakKey(Key))
				{
					throw new SymmetricEncryptionDataValidationException("Known weak key supplied for TripleDES algorithm.");
				}
			}
			

			// VECTOR VALIDATION
			// =================
			if (Vector.Length == 0)
			{
				if (Mode == eCryptoMode.Encrypt)
				{
					Vector = System.Text.ASCIIEncoding.ASCII.GetBytes(this.GetSymmetricVector(Provider));
				}
			}

			// Assume validation will be passed if no errors are detected
			return true;
		}

		#endregion

	}
 
	public class SymmetricEncryptionDataValidationException:Exception
	{
		public SymmetricEncryptionDataValidationException(string exceptionMessage)
		{
		}

		public byte[] Data;
		public byte[] Key;
		public byte[] Vector;
	}

}
