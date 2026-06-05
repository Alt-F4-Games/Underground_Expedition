
# Guía para Artistas (VFX, Shaders y Animaciones)

El sistema de combate está basado en **Autoridad de Estado** (Servidor). Esto significa que la lógica de "quién recibe daño" sucede exclusivamente en el backend. Sin embargo, los efectos visuales y auditivos deben verse fluidos e instantáneos en las pantallas de todos los jugadores.

 **REGLA DE ORO:** NUNCA instancies partículas (`Instantiate`), reproduzcas sonidos o cambies materiales dentro de `OnExecute()` o `FixedUpdateNetwork()`. 

## Uso del ChangeDetector (Eventos Visuales)
Para disparar un efecto (como una explosión, un destello o un sonido de impacto), usamos variables sincronizadas (`[Networked]`) que cambian en el servidor. Luego, los clientes detectan ese cambio utilizando el método **`Render()`**.

### Ejemplo de flujo (Ver `GroundSmashSkill.cs`):
1. **Servidor:** El jugador golpea el suelo. El código hace `SmashCount++`.
2. **Cliente (Render):** El `ChangeDetector` nota que el valor de `SmashCount` cambió este frame.
3. **Ejecución:** Se dispara la función `OnSmashExecuted()`. **Aquí es donde debes activar tus sistemas de partículas (`Play()`) o instanciar tus prefabs de VFX.**

### Efectos Persistentes (Auras / Buffs)
Si un efecto debe durar un tiempo prolongado (ej. puños en llamas por 3 ataques), revisa `EmpoweredStrikeSkill.cs`. Observa la variable `RemainingStrikes`. En el `Render()`, si esta variable es `> 0`, activas el GameObject de los puños de fuego; si es `<= 0`, lo apagas.

## Shaders y Barras de Progreso (Tiempo de Casteo)
Si un shader de disolución o una barra circular en la UI necesita saber cuánto falta para que se ejecute la habilidad, utiliza el método `GetActiveProgress(runner)`.
* Devuelve siempre un valor normalizado (Float de **0.0 a 1.0**).
* Úsalo en el `Update()` de tus scripts visuales para modificar parámetros como `_material.SetFloat("_Progress", valor);`.

## Animaciones
* **Aturdimientos de Casteo:** Si una habilidad tiene `SelfStunDuration`, el personaje se inmovilizará solo. No necesitas programarlo.
* **Triggers de Animación:** Aprovecha el mismo método `OnSmashExecuted()` (o el evento que detectes en el `Render`) para mandar el `animator.SetTrigger("CastSkill")`. Esto asegura que la animación se reproduzca al mismo tiempo en todas las pantallas.