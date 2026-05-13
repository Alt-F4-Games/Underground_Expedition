# Estructura y Funcionamiento General del Sistema de Habilidades

Este sistema ha sido diseñado para operar en un entorno multijugador utilizando **Photon Fusion**, bajo una arquitectura desacoplada y orientada a datos (Data-Driven).

## Pilares de la Arquitectura

El sistema se divide en cuatro capas principales:

### 1. Capa de Datos (ScriptableObjects)
Utiliza `SkillData` como clase base para definir las propiedades inmutables y visuales de cada habilidad (Nombre, Descripción, Icono, Cooldown base, y Reducción de Cooldown). Esto permite a los diseñadores crear y balancear nuevas variantes sin tocar el código.

### 2. Capa de Lógica de Red (NetworkBehaviour)
La clase abstracta `NetworkSkill` hereda de `NetworkBehaviour` y gestiona el estado sincronizado en red (State Authority):
* **CurrentLevel**: Nivel actual de la habilidad.
* **CooldownEnd**: Un `TickTimer` que sincroniza el tiempo de recarga en todos los clientes.
* **ActiveEnd**: Un `TickTimer` para gestionar tiempos de casteo, duraciones activas o aturdimientos (ej. `SelfStunDuration`).

### 3. Capa de Gestión (PlayerSkillManager)
Centraliza las habilidades equipadas en el jugador (`_slot1`, `_slot2`). Escucha los inputs empaquetados (`NetworkInputPlayer`) y decide cuándo ejecutar una habilidad o solicitar un *Upgrade* al servidor. Además, actúa como puente único para el sistema de combate mediante el método polimórfico `GetModifiedDamage`.

### 4. Capa de Interfaz (UI)
Totalmente reactiva y desacoplada de la lógica del servidor. Los componentes de UI (`SkillSlotUI`, `InventorySkillSlotUI`) se inyectan dinámicamente y leen los datos directamente de los `NetworkBehaviour` locales. El sistema de progresión notifica los cambios de puntos de habilidad a través de eventos (`GameEvents`).

## Flujo de Ejecución

1. **Input**: El cliente local presiona una tecla. `NetworkController` lo captura y lo envía empaquetado en `OnInput`.
2. **Simulación**: `FixedUpdateNetwork` del `PlayerSkillManager` procesa el botón en el servidor y en los clientes (mediante predicción).
3. **Validación**: Se llama a `CanCast()`. Si hay nivel, no hay cooldown y no se está canalizando nada, se aprueba.
4. **Ejecución**: Se dispara `OnExecute()`, aplicando timers de recarga o efectos de la habilidad.
5. **Combate**: Al atacar, `NetworkPlayerAttack` solicita el daño final al manager, el cual lo pasa por todas las habilidades activas (ej. sumando multiplicadores de `EmpoweredStrikeSkill`) antes de aplicar el daño.