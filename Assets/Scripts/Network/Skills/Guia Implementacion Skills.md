Guia Implementacion Skills
# Guía de Implementación de Nuevas Habilidades

Para añadir una nueva habilidad funcional al sistema, sigue este flujo de trabajo estandarizado de 4 pasos:

## 1. Definición de Datos (ScriptableObject)
Crea una clase que herede de `SkillData`. Define aquí las variables estáticas y las funciones que calculan el escalado por nivel.

```csharp
[CreateAssetMenu(fileName = "NuevaHabilidadData", menuName = "Skills/Nueva Habilidad")]
public class NuevaHabilidadData : SkillData
{
public int DañoBase = 50;
public int DañoPorNivel = 10;
    public int CalcularDaño(int nivel) {
        return DañoBase + (DañoPorNivel * (nivel - 1));
    }
}
```

## 2. Lógica de la Habilidad (NetworkSkill)
   Crea un script que herede de NetworkSkill.

* **OnExecute (Requerido):** Define la lógica inmediata al presionar el botón (ej. iniciar el Timer de ActiveEnd).

* **FixedUpdateNetwork (Opcional):** Úsalo si necesitas esperar a que termine un Timer para hacer daño de área (como en GroundSmashSkill).

* **ModifyAttackDamage (Opcional):** Sobrescribe esto si la habilidad es un Buff que altera el ataque básico del jugador.

## 3. Configuración en Unity
1.  **Haz clic derecho en el proyecto y crea tu nuevo ScriptableObject (Create -> Skills -> Nueva Habilidad).** Llénalo con el icono, descripción y estadísticas.
2.  **Ve al Prefab del Jugador y añádele tu nuevo script de lógica (el NetworkBehaviour).**
3.  **En el inspector de tu nuevo script, arrastra el ScriptableObject a la variable _skillData.**
4.  **En el componente PlayerSkillManager del jugador, arrastra tu nuevo script al hueco Slot 1 o Slot 2.**

## 4. Registro en el Inventario (UI)
   Para que las estadísticas personalizadas (como el daño o los multiplicadores) se vean en el panel de inventario, debes actualizar el script InventorySkillSlotUI.cs.

Ve al método UpdateUI() y añade el caso de tu nueva data:

```csharp
else if (_trackedSkill.Data is NuevaHabilidadData nuevaData)
{
stats = $"ATK: {nuevaData.CalcularDaño(_trackedSkill.CurrentLevel)}  CD: {nuevaData.GetCooldown(_trackedSkill.CurrentLevel):F1}s";
}
```