using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimpiarPlayerPrefs : MonoBehaviour
{
    void Start()
    {
        // Eliminar la clave del avatar seleccionado si existe
        if (PlayerPrefs.HasKey("SelectedAvatarImageName"))
        {
            PlayerPrefs.DeleteKey("SelectedAvatarImageName");
            PlayerPrefs.Save();
            Debug.Log("EliminadoPefats");
        }
    }
}
