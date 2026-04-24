using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.UI;

public class ProductOrderLoader : MonoBehaviour
{
    [Header("Configuración")]
    public int sujeto = 0;  // Número del sujeto
    public string csvFileName = "orden_productos.csv";

    [Header("Productos (A,B,C)")]
    public GameObject productoA;
    public GameObject productoB;
    public GameObject productoC;

    [Header("Posiciones de los productos")]
    public Transform pos1;
    public Transform pos2;
    public Transform pos3;

    [Header("Audios enlazados al cerrar productos")]

    public GameObject textoYAudioActivo;
    public GameObject textoYAudioCentral;
    public GameObject textoYAudioFinal;
    public GameObject textoYAudioEleccion;

    [Header("Objetos de la ventana encuesta")]
    public GameObject ventanaSiNo;
    public GameObject bocadillo;
    public Sprite spriteA; //arrastrar textura sabor
    public Sprite spriteB; //arrastrar textura nutri
    public Sprite spriteC; //arrastrar textura planet 
    public GameObject etiquetaProducto1; //objeto donde poner textura sabor
    public GameObject etiquetaProducto2; //objeto donde poner textura nutri
    public GameObject etiquetaProducto3; //objeto donde poner textura planet

    [Header("Audio explicación cierre")]
    public GameObject audioExplicacionCerrar;


    private Dictionary<int, string[]> ordenPorSujeto = new Dictionary<int, string[]>();
   // private XRSimpleInteractable interactableB;
    private int indiceActualInteractable = 0; // comienza en el primero del orden
    private string[] ordenActual; // guardamos el orden cargado del CSV para este sujeto (variable global, fuera de funciones para acceder desde otros métodos)
    private bool audioExplicacionReproducido = false;
    void Start()
    {
        DesactivarProductos();

        if (sujeto == 0) //no se ha puesto el numero de sujeto
        {
            Debug.LogWarning("[ProductOrderLoader] No se ha asignado número de sujeto. No se cargará ningún producto.");
            return;
        }

        CargarCSV();


        if (!ordenPorSujeto.ContainsKey(sujeto)) //  numero de sujeto no encontrado en la lista
        {
            Debug.LogError("[ProductOrderLoader] El sujeto " + sujeto + " no está en el CSV.");
            return;
        }

        string[] orden = ordenPorSujeto[sujeto]; // obtenemos el orden para el sujeto

        if (orden.Length != 3)
        {
            Debug.LogError("[ProductOrderLoader] Orden inválido para el sujeto " + sujeto);
            return;
        }

        ColocarProductos(orden); // ponemos los productos en las posiciones

        AsignarEtiquetasEncuesta(); // asignar las texturas de las imagenes de la ventana encuesta
    }

    void CargarCSV()
    {
        string filePath = Path.Combine(Application.dataPath, "PROYECT/Scripts/TFM", csvFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError("[ProductOrderLoader] NO se encontró el CSV en: " + filePath);
            return;
        }

        string[] lines = File.ReadAllLines(filePath);

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

            // primer campo: ID de sujeto
            if (!int.TryParse(parts[0].Trim(), out int id)) 
            {
                Debug.LogError("[CSV] No se pudo leer el ID de sujeto en: " + parts[0]);
                continue;
            }

            // Segundo campo: orden de productos
            string ordenRaw = parts[1].Trim();

            // ELIMINAR cualquier comilla y retorno de carro/espacios invisibles
            //ordenRaw = ordenRaw.Replace("\"", "").Replace("\r", "").Replace("\n", "").Trim();

            // Dividir por comas
            string[] orden = ordenRaw.Split(',');

            for (int j = 0; j < orden.Length; j++)
                orden[j] = orden[j].Trim();

            ordenPorSujeto[id] = orden;

            //Debug.Log("[CSV] Guardado sujeto " + id + " => " + string.Join(",", orden));
        }

        Debug.Log("[ProductOrderLoader] CSV cargado correctamente.");
    }

    void ColocarProductos(string[] orden)
    {
        ordenActual = orden; // guardamos el orden para usar después
        //colocar los producrtos en las posiciones según el orden
        Debug.Log($"[ProductOrderLoader] Orden sujeto {sujeto}: {orden[0]}-{orden[1]}-{orden[2]}"); // debug del orden A B C

        PosicionProducto(orden[0], pos1);
        ActivarInteractable(orden[0]); // activar interactable del primer producto nada mas aparecer
        PosicionProducto(orden[1], pos2);
        PosicionProducto(orden[2], pos3);
    }
    void ActivarInteractable(string letra)
    {

        //obtener el componente SimpleInteractable y activarlo
        GameObject producto = ObtenerProducto(letra); // obtener el GameObject del producto
        XRSimpleInteractable interactable = producto.GetComponent<XRSimpleInteractable>(); // obtener el componente XRSimpleInteractable

        if (interactable == null)
        {
            Debug.LogError("[ProductOrderLoader] El producto " + producto.name + " NO tiene XRSimpleInteractable.");
            return;
        }
        interactable.enabled = true; // ACTIVARLO
        Debug.Log("[ProductOrderLoader] Interactable ACTIVADO en " + producto.name);

        // Solo para el primer producto, reproducir audio al tocarlo
        if (indiceActualInteractable == 0 && !audioExplicacionReproducido)
        {
            interactable.selectEntered.AddListener(_ =>
            {   //suscribirse al evento de selección
                if (audioExplicacionCerrar != null)
                {
                    audioExplicacionCerrar.gameObject.SetActive(true);
                    audioExplicacionReproducido = true;
                }
                else
                {
                    Debug.LogWarning("[ProductOrderLoader] Audio explicación no asignado.");
                }
            });
        }
    }
    void DesactivarInteractable(string letra)
    {

        //obtener el componente SimpleInteractable y adesactivarlo
        GameObject producto = ObtenerProducto(letra);

        XRSimpleInteractable interactable = producto.GetComponent<XRSimpleInteractable>();

        if (interactable == null)
        {
            Debug.LogError("[ProductOrderLoader] El producto " + producto.name + " NO tiene XRSimpleInteractable.");
            return;
        }

        interactable.enabled = false; // DESACTIVARLO
        Debug.Log("[ProductOrderLoader] Interactable DESACTIVADO en " + producto.name);

    }


    void PosicionProducto(string letra, Transform destino)
    {
        GameObject obj = ObtenerProducto(letra);

        if (obj != null && destino != null) // si el producto y destino son válidos
        {
            obj.transform.position = destino.position; // movemos a la posición
            obj.SetActive(true);// activamos el producto
        }
    }

    GameObject ObtenerProducto(string letra)  // devuelve el GameObject correspondiente a la letra
    {
        switch (letra.ToUpper())
        {
            case "A": return productoA;
            case "B": return productoB;
            case "C": return productoC;
            default:
                Debug.LogError("[ProductOrderLoader] Letra desconocida: " + letra);
                return null;
        }
    }

    void DesactivarProductos()
    {
        if (productoA) productoA.SetActive(false);
        if (productoB) productoB.SetActive(false);
        if (productoC) productoC.SetActive(false);
    }
    private void CerrarBotonVentana()
    {
        // Índice 1 → activar Texto (producto central)
        if (indiceActualInteractable == 1)
        {
            textoYAudioActivo.SetActive(false);
            textoYAudioCentral.SetActive(true);
        }

        // Índice 2 → activar Texto (producto final)
        else if (indiceActualInteractable == 2)
        {
            textoYAudioCentral.SetActive(false);
            textoYAudioFinal.SetActive(true);

        }
        // Índice 3 → activar Texto (subasta) + ventana Eleccion Sí/No
        else if (indiceActualInteractable == 3)
        {
            textoYAudioFinal.SetActive(false);
            textoYAudioEleccion.SetActive(true);
            ventanaSiNo.SetActive(true); // Abrir ventana SiNo cuando todos los productos han sido cerrados
            Canvas canvas = bocadillo.GetComponent<Canvas>();
            canvas.enabled = false;  //desactivar el bocadillo del avatar, informacion en ventana

        }

        if (textoYAudioActivo == null || textoYAudioCentral == null || textoYAudioFinal == null)
        {
            Debug.LogWarning("[ProductOrderLoader] Alguno de los objetos de texto y audio no están asignados.");
        }
    }

    // Función que se llama al cerrar la ventana de un producto
    public void InteractableSiguienteProducto()
    {
        // Desactivar interactable del producto actual
        DesactivarInteractable(ordenActual[indiceActualInteractable]); 

        // Pasar al siguiente producto
        indiceActualInteractable++;

        // ACTIVAR texto y audio según índice, y abrir ventana 
        CerrarBotonVentana();

        if (indiceActualInteractable < ordenActual.Length)
        {
            // Activar interactable del siguiente producto
            ActivarInteractable(ordenActual[indiceActualInteractable]);
            // ACTIVAR texto y audio según índice
            //ActivarTextoYaudio();
        }
        else
        {
            Debug.Log("[ProductOrderLoader] Todos los productos ya han sido pulsados.");
        }
    }
     public void AsignarEtiquetasEncuesta()
    {
        //asignamos la textura al gameobject correspondiente (Image->SourceImage) según el orden

        //ordenActual contiene el orden de los productos para este sujeto
        ordenActual = ordenPorSujeto[sujeto];

        // Array con los GameObjects de las etiquetas
        GameObject[] etiquetas = new GameObject[] { etiquetaProducto1, etiquetaProducto2, etiquetaProducto3 };

        for (int i = 0; i < ordenActual.Length; i++)
        {
            GameObject etiquetaGO = etiquetas[i];
            Image img = etiquetaGO.GetComponent<Image>();
            if (img == null)
            {
                Debug.LogError("[ProductOrderLoader] No hay componente Image en " + etiquetaGO.name);
                continue;
            }
            // Elegimos la textura según la letra A/B/C

            switch (ordenActual[i].ToUpper())
            {
                case "A": img.sprite = spriteA; break;
                case "B": img.sprite = spriteB; break;
                case "C": img.sprite = spriteC; break;
                default:
                    Debug.LogWarning("Letra desconocida para asignar textura en orden: " + ordenActual[i]);
                    break;
            }
        }

    }

}
