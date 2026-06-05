# Documentación del Sistema de Experiencia (Refactorización de Autoridad)

Esta documentación detalla la transición de un modelo de solicitud de experiencia basado en el cliente a un modelo de asignación autoritativa por parte del servidor dentro del entorno de Photon Fusion.

## 1. ¿Por qué el cambio?
El sistema anterior presentaba fallos críticos de fiabilidad y riesgos de seguridad:
* **Fiabilidad:** Los jugadores a menudo no recibían experiencia tras eliminar enemigos (especialmente con habilidades de área), ya que el flujo dependía de que el cliente "atrapase" un evento local que no siempre se sincronizaba correctamente.
* **Seguridad:** Al permitir que el cliente enviara un RPC (`RPC_RequestAddXP`) para pedir experiencia, el sistema quedaba expuesto a exploits donde un usuario malintencionado podría "inyectar" experiencia de forma artificial.
* **Arquitectura:** Se buscaba alinear el proyecto con las mejores prácticas de **Autoridad de Estado** de Photon Fusion, donde el servidor es el único responsable de modificar variables persistentes como el nivel o la XP.

## 2. Funcionamiento Anterior (Legacy)
El flujo dependía de una comunicación de ida y vuelta iniciada por el cliente:
1.  **Muerte del Enemigo:** El servidor ejecutaba la lógica de muerte.
2.  **Evento Local:** Se disparaba un `EnemyDiedEvent` que el cliente intentaba escuchar.
3.  **Solicitud del Cliente:** El cliente, tras recibir el evento, enviaba un `RPC_RequestAddXP` al servidor.
4.  **Problema:** Si el paquete del evento se perdía o la lógica de detección de la habilidad no pasaba la referencia del asesino (`killer`), el cliente nunca pedía la XP y el jugador no progresaba.

## 3. Funcionamiento Actual (Authoritative)
Se ha implementado un flujo unidireccional y seguro gestionado íntegramente por el servidor:
1.  **Daño Identificado:** Las habilidades (como `GroundSmashSkill`) ahora pasan obligatoriamente el `Object.InputAuthority` del jugador al método `TakeDamage`. Esto "firma" el ataque.
2.  **Muerte y Registro:** Cuando la vida del enemigo llega a cero en el servidor, este registra quién fue el último atacante (`killer`).
3.  **Escucha en el Servidor:** El `NetworkExperienceSystem` ahora escucha el `EnemyDiedEvent` **directamente en el servidor**.
4.  **Asignación Directa:** El servidor valida si el asesino es el dueño del componente de experiencia y suma los puntos directamente a la variable `[Networked] CurrentExp`.
5.  **Sincronización:** El cliente simplemente observa el cambio en su barra de UI gracias a la sincronización automática de Fusion, sin haber tenido que "pedir" nada.

## 4. Archivos Clave Modificados
* **`NetworkExperienceSystem.cs`**: Se eliminaron los RPCs de solicitud y se movió la lógica de escucha de eventos al servidor (`HasStateAuthority`).
* **`GroundSmashSkill.cs`**: Se actualizó la llamada a `TakeDamage` para incluir la referencia del atacante, asegurando que el servidor sepa a quién otorgar la recompensa.

---
*Documento generado para el equipo de desarrollo - Sistema de Habilidades y Progresión.*
