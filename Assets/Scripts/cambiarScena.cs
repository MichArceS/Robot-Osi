using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class cambiarScena : MonoBehaviour
{
    public int indiceEscena = 0;

    public void CambiarEscena()
    {
        SceneManager.LoadSceneAsync(indiceEscena);
        // Verificar si la clave existe antes de intentar borrarla
        if (PlayerPrefs.HasKey("SelectedAvatarImageName"))
        {
            PlayerPrefs.DeleteKey("SelectedAvatarImageName");
            //Debug.Log("Se elimino el prefas");
        }
    }
   
}
