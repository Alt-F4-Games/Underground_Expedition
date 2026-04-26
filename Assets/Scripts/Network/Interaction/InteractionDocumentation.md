# Documentación: Sistema de Interacción - Underground Expedition

Este documento detalla la arquitectura, implementación y uso del sistema de interacción diseñado para **Underground Expedition** utilizando **Photon Fusion 2**.

## 1. Sistema
El sistema está diseñado bajo una arquitectura desacoplada y orientada a la red (Server-Authoritative). Se divide en dos responsabilidades claras:
* **Cliente (Visual/Input):** Detecta qué está mirando el jugador y proporciona feedback inmediato (Outlines, UI).
* **Servidor (Lógica/Físicas):** Valida la posición del jugador, gestiona los tiempos de espera (TickTimers) y ejecuta la lógica final de juego.

---

## 2. Componentes Principales

### I. `IInteractable.cs` (El Contrato)
Es la interfaz base. Define qué métodos **debe** tener cualquier objeto para ser considerado interactuable.
* `GetInteractPrompt()`: Retorna el texto para la UI.
* `GetInteractionDuration()`: Retorna el tiempo (en segundos) de interacción.
* `CanInteract()`: Validación de seguridad (Servidor).
* `OnInteract()`: Ejecución de la lógica (Servidor).
* `OnHoverEnter/Exit()`: Callbacks visuales locales (Cliente).

### II. `InteractableBase.cs` (El Molde)
Clase abstracta que implementa la interfaz y proporciona variables básicas para el Inspector de Unity.
> **Nota:** Todos los nuevos objetos deben heredar de esta clase.

### III. `PlayerInteractionScanner.cs` (Los Ojos del Cliente)
Componente local que corre en `Render()`. Tira un Raycast constante para detectar objetos en la layer `Interactable`.
* Gestiona el **Hover**: Avisa al objeto cuándo el jugador lo mira o deja de mirarlo.
* Provee información al **HUD** para mostrar los textos dinámicos.

### IV. `NetworkPlayerInteractor.cs` (El Cerebro del Servidor)
Componente sincronizado que corre en `FixedUpdateNetwork()`.
* Procesa el input de red.
* Gestiona el `TickTimer` para acciones que requieren mantener presionada la tecla.
* Valida la distancia física para evitar trampas.

---

## 3. Guía para Crear Nuevos Objetos

Para añadir un objeto interactuable al juego:

1.  **Crear el Script:** Crea una clase nueva (ej: `Cofre.cs`) que herede de `InteractableBase`.
2.  **Implementar `OnInteract`:**
    ```csharp
    public override void OnInteract(NetworkPlayerController player)
    {
        // Lógica que solo ocurre en el servidor
        Debug.Log("Cofre abierto por: " + player.Object.InputAuthority);
        AbrirCofre(); 
    }
    ```
3.  **Configuración en Unity:**
    * Asegúrate de que el GameObject tenga un **Collider**.
    * Cambia la **Layer** del objeto a `Interactable`.
    * En el Inspector, configura el `Prompt Message` (ej: "Abrir") y la `Interaction Duration`.

---

## 4. Guía de Efectos Visuales

* **`OnHoverEnter(NetworkPlayerController player)`**: Se dispara cuando el jugador enfoca el objeto. Ideal para activar el Outline.
* **`OnHoverExit(NetworkPlayerController player)`**: Se dispara cuando el jugador quita la vista. Ideal para desactivar el Outline.

**Ejemplo de uso:**
```csharp
public override void OnHoverEnter(NetworkPlayerController player)
{
    // Código para encender el resaltado
    _myRenderer.material.EnableKeyword("_EMISSION");
}

public override void OnHoverExit(NetworkPlayerController player)
{
    // Código para apagar el resaltado
    _myRenderer.material.DisableKeyword("_EMISSION");
}