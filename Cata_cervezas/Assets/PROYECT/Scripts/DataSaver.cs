using System;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataSaver : MonoBehaviour
{
    [Header("Dropdowns")]
    public TMP_Dropdown desplegableComprar;
    public TMP_Dropdown comprarP1;
    public TMP_Dropdown comprarP2;
    public TMP_Dropdown comprarP3;

    [Header("Sliders")]
    public Slider precioP1;
    public Slider precioP2;
    public Slider precioP3;
    public Slider sliderSeguro;

    [Header("External reference")]
    public ProductOrderLoader productOrderLoader; //para saber el ID de usuario

    public void SaveData()
    {
        Debug.Log("[DataSaver] Guardando datos de usuario...");

        // 1️ Obtener ID de usuario
        int userID = productOrderLoader.sujeto;

        // 2️ Inicializar variables a 0

        int p1 = 0;
        int p2 = 0;
        int p3 = 0;

        float precio1 = 0f;
        float precio2 = 0f;
        float precio3 = 0f;

        float seguro = 0f;

        // 3 Obtener valores de los dropdowns y sliders dependiendo compra general

        int comprarGeneral = desplegableComprar.value;

        if (comprarGeneral == 1) // SI es Sí, guardas los valores de los productos, si es No se queda todo en 0 (inicializado arriba)
        {
            p1 = comprarP1.value;
            p2 = comprarP2.value;
            p3 = comprarP3.value;

            // 4 Aplicar lógica de precios (Solo si es Sí (1) guardas el precio)
            precio1 = (p1 == 1) ? precioP1.value : 0f;// Si es Sí (1), toma el valor del slider, si no 0
            precio2 = (p2 == 1) ? precioP2.value : 0f;
            precio3 = (p3 == 1) ? precioP3.value : 0f;

            seguro = sliderSeguro.value;
        }

        // 5 Crear CSV
        StringBuilder sb = new StringBuilder();

        sb.AppendLine(
            "OpcionComprar;ComprarP1;PrecioP1;ComprarP2;PrecioP2;ComprarP3;PrecioP3;SliderSeguro" //encabezados de las columnas
        );

        sb.AppendLine(
            $"{comprarGeneral};{p1};{precio1};{p2};{precio2};{p3};{precio3};{seguro}" //guardamos cada valor separado por comas en la fila correspondiente
        );

        // 6 Guardar datos en carpeta "Data"
        //string folderPath = Path.Combine(Application.persistentDataPath, "Data"); esto lo guarda en local en el pc, no en el proyecto, por eso no se ve y da error
        string folderPath = Path.Combine(Application.dataPath,"PROYECT","Data"); //editor / proyecto
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        // Generamos el timestamp con un formato apto para nombres de archivo
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // Combinamos el ID de usuario y el timestamp
        string fileName = $"{timestamp}_Data_U{userID}.csv";
        string filePath = Path.Combine(folderPath, fileName);

        File.WriteAllText(filePath, sb.ToString());

        Debug.Log("Datos del usuario guardados correctamente en: " + filePath);
    }
}

