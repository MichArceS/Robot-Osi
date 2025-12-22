using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageShufflerUI : MonoBehaviour
{
    public GameObject imageContainer;
    private RectTransform[] imageTransforms;

    void Start()
    {
        
        imageTransforms = imageContainer.GetComponentsInChildren<RectTransform>();

        ShuffleImages();
    }

    void ShuffleImages()
    {
        int length = imageTransforms.Length - 1;

        Vector3[] positions = new Vector3[length];
        for (int i = 1; i < length + 1; i++)
        {
            positions[i - 1] = imageTransforms[i].localPosition;
        }

        for (int i = 0; i < length; i++)
        {
            Vector3 temp = positions[i];
            int randomIndex = Random.Range(i, length);
            positions[i] = positions[randomIndex];
            positions[randomIndex] = temp;
        }

        for (int i = 1; i < length + 1; i++)
        {
            imageTransforms[i].localPosition = positions[i - 1];
        }
    }
}