using System.Collections;
using System.Collections.Generic;

using System;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class GuardarDatos : MonoBehaviour
{
    public Slider slider;
    public TMP_Dropdown dropdown;
    public GameObject ventanafinal;


    public void GuardarDatosArchivo()
    {
        int valorSlider = Mathf.RoundToInt(slider.value);
        string n_catador = dropdown.options[dropdown.value].text;

        string datos = $"Numero de catador;Intensidad de salado \n {n_catador};{valorSlider}"; //opcion csv
        //string datos = $"Número de catador: {n_catador}, Intensidad de salado: {valorSlider}"; //opcion txt
        // Generar nombre de archivo con timestamp
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string nombreArchivo = $"respuesta_usuario_{timestamp}.csv"; //cambiar extension segun opcion seleccionada
        string ruta = Path.Combine(Application.persistentDataPath, nombreArchivo);

        File.WriteAllText(ruta, datos);

        string mensaje = $" Datos guardados correctamente en:\n{ruta}";
        Debug.Log(mensaje); //mostrar mensaje en el log unity

        // Abrir ventana final
        if (ventanafinal != null)
        {
            ventanafinal.SetActive(true);
        }
    }
}
