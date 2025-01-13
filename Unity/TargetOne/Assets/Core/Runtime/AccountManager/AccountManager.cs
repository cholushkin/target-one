using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GameLib.Alg;
using NaughtyAttributes;
using UnityEngine;

public class AccountManager : Singleton<AccountManager>
{
    [Serializable]
    public class GameState // Save structure
    {
        public long Checkpoint;
        public long Coins;
    }

    [Serializable]
    public class Accounts
    {
        public byte ActiveAccountIndex;
        public GameState[] GameStates = new GameState[MaxAccounts];
    }

    private const int MaxAccounts = 3;
    private const string AccountsStorageFile = "Accounts.adb"; // Accounts database ciphered file

    // todo: Replace these with securely generated and stored values.
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("8635sG-3GKoD7;Ep"); // 16-byte key
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("x:GR904N6eIZ'22Z"); // 16-byte IV

    public Accounts ActiveAccounts;

    protected override void Awake()
    {
        base.Awake();
        DeserializeActiveAccounts();
    }

    void OnDestroy()
    {
        SaveAccounts();
    }

    private void DeserializeActiveAccounts()
    {
        string path = GetAccountsSavePath();
        if (File.Exists(path))
        {
            try
            {
                byte[] encryptedData = File.ReadAllBytes(path);
                string decryptedJson = Decrypt(encryptedData);
                ActiveAccounts = JsonUtility.FromJson<Accounts>(decryptedJson);
                Debug.Log($"Accounts loaded ({path})");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load accounts ({path}): {e.Message}");
                ActiveAccounts = CreateEmptyAccounts();
            }
        }
        else
        {
            Debug.Log($"No existing account data found ({path}). Initializing new accounts.");
            ActiveAccounts = CreateEmptyAccounts();
        }
    }

    public GameState GetAccountData(int accountIndex)
    {
        return ActiveAccounts.GameStates[accountIndex];
    }

    public bool IsAccountInitialized(int accountIndex)
    {
        if (accountIndex >= 0 && accountIndex < MaxAccounts)
        {
            var account = ActiveAccounts.GameStates[accountIndex];
            // Check if account has been initialized (can change criteria as needed)
            return account.Checkpoint != -1 || account.Coins != -1;
        }
        else
        {
            Debug.LogError("Invalid account index.");
            return false;
        }
    }

    public void ClearAccount(int accountIndex)
    {
        if (accountIndex >= 0 && accountIndex < MaxAccounts)
        {
            ActiveAccounts.GameStates[accountIndex] = new GameState { Checkpoint = -1, Coins = -1 };
            SaveAccounts();
            Debug.Log($"Account {accountIndex} has been cleared.");
        }
        else
        {
            Debug.LogError("Invalid account index.");
        }
    }

    public void SaveAccounts()
    {
        SerializeGameState();
    }

    private void SerializeGameState()
    {
        string path = GetAccountsSavePath();
        try
        {
            string json = JsonUtility.ToJson(ActiveAccounts, true);
            byte[] encryptedData = Encrypt(json);
            File.WriteAllBytes(path, encryptedData);
            Debug.Log($"Accounts saved successfully (({path})).");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save accounts ({path}): {e.Message}");
        }
    }

    private string GetAccountsSavePath()
    {
        return Path.Combine(Application.persistentDataPath, AccountsStorageFile);
    }

    private Accounts CreateEmptyAccounts()
    {
        Accounts newAccounts = new Accounts();
        for (int i = 0; i < newAccounts.GameStates.Length; i++)
            newAccounts.GameStates[i] = new GameState
            {
                Checkpoint = -1,
                Coins = -1
            };

        return newAccounts;
    }

    private string Decrypt(byte[] encryptedData)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;

            using (MemoryStream ms = new MemoryStream(encryptedData))
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(cs))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }

    private byte[] Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    using (StreamWriter writer = new StreamWriter(cs))
                    {
                        writer.Write(plainText);
                    }
                }

                return ms.ToArray();
            }
        }
    }

    
    [Button]
    public void DeleteAllAccounts()
    {
        string path = GetAccountsSavePath();
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
                ActiveAccounts = CreateEmptyAccounts();
                Debug.Log("All accounts have been deleted and reset.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete accounts file ({path}): {e.Message}");
            }
        }
        else
        {
            Debug.Log("No accounts file found to delete.");
        }
    }
}