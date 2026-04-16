using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CustomButtonAction : MonoBehaviour
{
    public InputActionReference customButton; // Acción boton A
    public InputActionReference resetButton; // Acción boton b
    //public string sceneToLoad; //Cargar escena al pulsar A
    public string initialScene; // Escena inicial para resetear al presionar el botón B

    private void OnEnable()
    {
        // Habilitar la acción y suscribirse al evento
        if (customButton != null && customButton.action != null)
        {
            customButton.action.Enable();
            customButton.action.performed += ButtonAWasPressed;
            
        }
        if (resetButton != null && resetButton.action != null)
        {
            resetButton.action.Enable();
            resetButton.action.performed += OnButtonBPressed;
        }
    }

    private void OnDisable()
    {
        // Deshabilitar la acción y desuscribirse del evento
        if (customButton != null && customButton.action != null)
        {
            customButton.action.started -= ButtonAWasPressed;
            customButton.action.Disable(); //Evitar problemas en memoria, control manual
        }
        if (resetButton != null && resetButton.action != null)
        {
            resetButton.action.started -= OnButtonBPressed;
            resetButton.action.Disable();
        }
    }

    private void ButtonAWasPressed(InputAction.CallbackContext context)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0; // opcional: volver al inicio
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    private void OnButtonBPressed(InputAction.CallbackContext context)
    {
        // Resetear al presionar el botón B (volver a la escena inicial)
        if (!string.IsNullOrEmpty(initialScene))
        {
            SceneManager.LoadScene(initialScene);
        }
    }

    void Awake()
    {
        // Asegúrate de que el GameObject que contiene este script no se destruya al cambiar de escena
        DontDestroyOnLoad(gameObject);
    }
}
