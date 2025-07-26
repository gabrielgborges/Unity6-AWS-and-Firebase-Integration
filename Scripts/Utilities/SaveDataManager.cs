
// based on https://github.com/UnityTechnologies/UniteNow20-Persistent-Data

using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SaveDataManager
{
   public static void SaveJsonData(ISaveable saveable)
   {
      if (FileManager.WriteToFile(saveable.FileNameToUseForData(), saveable.ToJson()))
      {
         // Debug.Log("Save successful");
      }
   }

   public static void LoadJsonData(ISaveable saveable)
   {
      if (FileManager.LoadFromFile(saveable.FileNameToUseForData(), out var json))
      {
         saveable.LoadFromJson(json);
         // Debug.Log("Load complete");
      }
   }
   
   // Sets a string value in PlayerPrefs after encrypting it
    public static void SetString(string key, string value)
    {
        // Encrypt the value
        string encryptedValue = Encrypt(value); 
        // Store the encrypted value
        PlayerPrefs.SetString(key, encryptedValue); 
    }

    // Retrieves a string value from PlayerPrefs after decrypting it
    public static string GetString(string key, string defaultValue = "")
    {
        // Get the encrypted value
        string encryptedValue = PlayerPrefs.GetString(key, defaultValue);
        if (string.IsNullOrEmpty(encryptedValue))
        {
            return defaultValue;
        }
        // Decrypt and return the value
        return Decrypt(encryptedValue); 
    }

    // Encrypts a string using AES encryption
    private static string Encrypt(string text)
    {
        // Convert the text to a byte array
        var textBytes = Encoding.UTF8.GetBytes(text);

        // Create a new instance of the AES service provider
        using var aes = new AesCryptoServiceProvider(); 
    
        // Set the encryption key
        aes.Key = Encoding.UTF8.GetBytes("eX4mP1eK3yForT3st1n9OnlyS3cure!!"); //TODO: THIS IS A PLACEHOLDER.
        // Set cipher mode to CBC
        aes.Mode = CipherMode.CBC; 
        // Set padding mode
        aes.Padding = PaddingMode.PKCS7; 

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV); 
        // Encrypt the text bytes
        var encryptedBytes = encryptor.TransformFinalBlock(textBytes, 0, textBytes.Length); 
        // Return the IV and encrypted data as a base64 string
        return Convert.ToBase64String(aes.IV) + Convert.ToBase64String(encryptedBytes); 
    }

    // Decrypts a previously encrypted string
    private static string Decrypt(string encryptedText)
    {
        // Extract the first 24 characters as the IV
        var iv = encryptedText[..24]; 
        // The rest is the encrypted data
        encryptedText = encryptedText[24..]; 

        // Convert encrypted data to byte array
        var encryptedBytes = Convert.FromBase64String(encryptedText);
        // Convert IV to byte array 
        var ivBytes = Convert.FromBase64String(iv); 

        // Create a new instance of the AES service provider
        using var aes = new AesCryptoServiceProvider(); 
        // Set the decryption key
        aes.Key = Encoding.UTF8.GetBytes("eX4mP1eK3yForT3st1n9OnlyS3cure!!"); //TODO: THIS IS A PLACEHOLDER.
        aes.IV = ivBytes; // Set the IV
        aes.Mode = CipherMode.CBC; // Set cipher mode to CBC
        aes.Padding = PaddingMode.PKCS7; // Set padding mode

        // Create a decryptor object
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV); 
        // Decrypt the encrypted bytes
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length); 
        // Convert the decrypted bytes back to string and return
        return Encoding.UTF8.GetString(decryptedBytes); 
    }
}

public interface ISaveable
{
   string ToJson();
   void LoadFromJson(string a_Json);
   string FileNameToUseForData();
}