using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.UI;
using TMPro;

public class ProductOrderLoader : MonoBehaviour
{
    [Header("Configuración")]
    private int sujeto;  // Número del sujeto
    public string csvFileName = "Panelistas_cerveza.csv";

    [Header("Muestras")]
    public GameObject Muestra104;
    public GameObject Muestra392;
    public GameObject Muestra432;
    public GameObject Muestra521;
    public GameObject Muestra719;

    [Header("Desplegable Panelista")]
    public TMP_Dropdown Panelista;

    [Header("Ventana final")]
    public GameObject VentanaFinal;

    private Dictionary<string, GameObject> mapaMuestras;
    private Dictionary<int, string[]> ordenPorSujeto = new Dictionary<int, string[]>();
    private Dictionary<string, MuestraUI> mapaUI;
    private int indiceActualInteractable = 0; // comienza en el primero del orden
    private string[] ordenActual; // guardamos el orden cargado del CSV para este sujeto (variable global, fuera de funciones para acceder desde otros métodos)

    // ---------------- DATOS ----------------
    [System.Serializable]
    public class DatosMuestra
    {
        public string muestra;
        public float sliderColor;
        public float sliderTurbidez;
    }

    private List<DatosMuestra> datos = new List<DatosMuestra>();

    // ---------------- START ----------------
    void Start()
    {
        DesactivarProductos(); //asegurar que estan todos desactivados

        // Crear mapa colores → muestras
        mapaMuestras = new Dictionary<string, GameObject>()
        {
            { "gris", Muestra104 },
            { "amarillo", Muestra392 },
            { "verde", Muestra432 },
            { "negro", Muestra521 },
            { "naranja", Muestra719 }
        };

        //para guardar datos de los sliders de cada muestra
        mapaUI = new Dictionary<string, MuestraUI>()
        {
            { "gris", Muestra104.GetComponent<MuestraUI>() },
            { "amarillo", Muestra392.GetComponent<MuestraUI>() },
            { "verde", Muestra432.GetComponent<MuestraUI>() },
            { "negro", Muestra521.GetComponent<MuestraUI>() },
            { "naranja", Muestra719.GetComponent<MuestraUI>() }
        };
    }
    void CargarExperimento()
    {
        CargarCSV();


        if (!ordenPorSujeto.ContainsKey(sujeto)) //  numero de sujeto no encontrado en la lista
        {
            Debug.LogError("[ProductOrderLoader] El sujeto " + sujeto + " no está en el CSV.");
            return;
        }

        ordenActual = ordenPorSujeto[sujeto]; // obtenemos el orden para el sujeto

        if (ordenActual.Length == 0)
        {
            Debug.LogError("[ProductOrderLoader] Orden vacío.");
            return;
        }

        // Activar primera muestra
        indiceActualInteractable = 0;
        ActivarProductoActual();
    }

    public void EmpezarExperimento()
    {
        //leer el sujeto segun el dropdown de la ventana inicial
        sujeto = Panelista.value;

        if (sujeto == 0)
        {
            Debug.LogWarning("Selecciona un panelista primero");
            return;
        }

        CargarExperimento();
    }
    void CargarCSV()
    {
        //string filePath = Path.Combine(Application.dataPath, "PROYECT/Scripts", csvFileName);

        //if (!File.Exists(filePath)){
        //Debug.LogError("[ProductOrderLoader] NO se encontró el CSV en: " + filePath);
        //return;
        //}

        //string[] lines = File.ReadAllLines(filePath);

        TextAsset csv = Resources.Load<TextAsset>("Panelistas_cerveza"); //“Todo lo que esté en Resources viaja dentro de la APK”

        if (csv == null)
        {
            Debug.LogError("[ProductOrderLoader] No se pudo cargar el CSV desde Resources");
            return;
        }

        string[] lines = csv.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // saltamos header
        {
            string line = lines[i].Trim(); // limpiar espacios
            if (string.IsNullOrEmpty(line)) continue; // saltar líneas vacías

            //Debug.Log("[CSV] Línea original: '" + line + "'"); // debug línea original

            // Dividir por punto y coma
            string[] parts = line.Split(';'); //dividir por ;

            if (parts.Length < 2) // verificar que hay al menos 2 campos
            {
                Debug.LogError("[CSV] Línea inválida (faltan campos): " + line);
                continue;
            }

            // primer campo: numero de panelista
            if (!int.TryParse(parts[0].Trim(), out int id)) 
            {
                Debug.LogError("[CSV] Error leyendo panelista: " + parts[0]);
                continue;
            }

            // Segundo campo: orden de productos
            string ordenRaw = parts[1].Trim();

            // LIMPIEZA
            ordenRaw = ordenRaw.Replace("[", "")
                               .Replace("]", "")
                               .Replace("'", "")
                               .Replace("\"", "");

            // Dividir por comas
            string[] orden = ordenRaw.Split(',');

            for (int j = 0; j < orden.Length; j++)
                orden[j] = orden[j].Trim().ToLower();

            ordenPorSujeto[id] = orden;
        }

        Debug.Log("[ProductOrderLoader] CSV cargado correctamente.");
    }

    


    // ---------------- FLUJO ----------------
    void ActivarProductoActual()
    {
        DesactivarProductos();

        string clave = ordenActual[indiceActualInteractable];

        if (!mapaMuestras.ContainsKey(clave))
        {
            Debug.LogError("[ProductOrderLoader] No existe muestra para: " + clave);
            return;
        }

        mapaMuestras[clave].SetActive(true);

        Debug.Log("Activando muestra: " + clave);
    }


    public void InteractableSiguienteProducto() //Llamar en onclick del ultimo boton de cada muestra
    {
        GuardarDatosMuestraActual();
        indiceActualInteractable++;

        if (indiceActualInteractable >= ordenActual.Length)
        {
            Debug.Log("Fin del experimento");
            GuardarCSVFinal();
            DesactivarProductos();

            if (VentanaFinal != null)
                VentanaFinal.SetActive(true);

            return;
        }

        ActivarProductoActual();
    }

    void GuardarDatosMuestraActual()
    {
        string clave = ordenActual[indiceActualInteractable];
        MuestraUI ui = mapaUI[clave];

        DatosMuestra d = new DatosMuestra();

        d.muestra = clave;
        d.sliderColor = ui.sliderColor.value;
        d.sliderTurbidez = ui.sliderTurbidez.value;

        datos.Add(d);

        Debug.Log("Guardado: " + clave);
    }
    void GuardarCSVFinal()
    {
        string path = Path.Combine(Application.dataPath, "PROYECT/Data");

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string nombreArchivo = timestamp + "_Resultados_" + sujeto + ".csv";
        //string filePath = Path.Combine(path, timestamp + "_Resultados_" + sujeto + ".csv"); para guardar en local del pc
        string filePath = Path.Combine(Application.persistentDataPath, nombreArchivo); //para guardar en local de gafas (/Android/data/tu.paquete.app/files/)

        List<string> lineas = new List<string>();

        lineas.Add("panelista;muestra;sliderColor;sliderTurbidez");

        foreach (var d in datos)
        {
            lineas.Add($"{sujeto};{d.muestra};{d.sliderColor};{d.sliderTurbidez}");
        }

        File.WriteAllLines(filePath, lineas);

        Debug.Log("CSV guardado en: " + filePath);
    }


    void DesactivarProductos()
    {
        if (Muestra104) Muestra104.SetActive(false);
        if (Muestra392) Muestra392.SetActive(false);
        if (Muestra521) Muestra521.SetActive(false);
        if (Muestra719) Muestra719.SetActive(false);
        if (Muestra432) Muestra432.SetActive(false);
    }

}
