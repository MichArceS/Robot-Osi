using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerInfo
{
    private static int nextId; 
    public string playerCode;
    public string playerName;
    public string selectedAvatarName;

    public PlayerInfo(string name, string avatarName)
    {
        LoadNextId();
        playerCode = GeneratePlayerCode();
        playerName = name;
        selectedAvatarName = avatarName;
        SaveNextId();
    }
   
    private string GeneratePlayerCode()
    {
        return nextId.ToString("D3");
    }

    private static void LoadNextId()
    {
        nextId = PlayerPrefs.GetInt("NextPlayerId", 1);
    }

    private static void SaveNextId()
    {
        nextId++;
        PlayerPrefs.SetInt("NextPlayerId", nextId);
    }

    public static int GetNextId()
    {
        LoadNextId();
        return nextId;
    }

    //Reiniciar nextId a 1
    public static void ResetNextId()
    {
        nextId = 1;
        PlayerPrefs.SetInt("NextPlayerId", nextId);
    }
}

[Serializable]
public class PlayerData
{
    public List<PlayerInfo> players = new List<PlayerInfo>();
}
