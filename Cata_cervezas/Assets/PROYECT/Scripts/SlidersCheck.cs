using UnityEngine;
using UnityEngine.UI;

public class SlidersCheck : MonoBehaviour
{
    public Slider[] sliders;
    public Button button;

    private bool[] slidersChanged;

    void Start()
    {
        button.interactable = false;

        slidersChanged = new bool[sliders.Length];

        for (int i = 0; i < sliders.Length; i++)
        {
            int index = i; // importante para evitar bugs
            sliders[i].onValueChanged.AddListener((value) => OnSliderChanged(index));
        }
    }

    void OnSliderChanged(int index)
    {
        slidersChanged[index] = true;
        CheckAllSliders();
    }

    void CheckAllSliders()
    {
        foreach (bool changed in slidersChanged)
        {
            if (!changed) return;
        }

        button.interactable = true;
    }
}