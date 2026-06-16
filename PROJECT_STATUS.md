# TP Integrador — Estado del proyecto vs. Austral-Survivors (ejemplo de cátedra)

> Comparación generada analizando el código y los assets de ambos repos.
> Para cada ítem se indica si lo que falta es **[CÓDIGO]** (te lo puedo escribir yo)
> o **[UNITY]** (trabajo manual en el editor; abajo van los pasos).

---

## 0. Cómo leer este documento

- ✅ **Completo** — existe y es funcionalmente equivalente (o superior) al ejemplo.
- 🟡 **Incompleto** — existe pero está a medias, tiene bugs o no está cableado.
- ❌ **Falta** — no existe en tu repo y el ejemplo sí lo tiene.
- ➕ **Extra** — lo tenés vos y el ejemplo NO. Suma, no resta.

Etiquetas de tipo de trabajo:
- **[CÓDIGO]** = archivos `.cs`. Me lo pedís y lo hago.
- **[UNITY]** = configuración en el editor (escenas, prefabs, Inspector, Animator, Build Settings). Te dejo el paso a paso.
- **[MIXTO]** = necesita las dos cosas.

---

## 1. Resumen ejecutivo

Tu proyecto **no es una copia del ejemplo**: tomaste otra arquitectura, en varias áreas
más avanzada (sistema de ataques por `ScriptableObject`, economía de monedas, pooling de
enemigos, sistema de upgrades). Pero te faltan **tres pilares grandes** que el ejemplo sí tiene
y que probablemente la cátedra evalúe:

| # | Pilar | Estado | Tipo |
|---|-------|--------|------|
| 1 | **Sistema de guardado / Continuar partida** | ❌ Falta entero | [CÓDIGO] + [UNITY] |
| 2 | **Sistema de oleadas (waves) con condición de victoria** | ❌ Falta (tenés spawner infinito) | [CÓDIGO] + [UNITY] |
| 3 | **Variedad de IA de movimiento de enemigos (Chase/Flee/MaintainDistance)** | 🟡 Solo Chase | [CÓDIGO] |

Y un puñado de cosas chicas incompletas (animación de ataque, barra de vida de enemigos,
menú principal que no distingue Nueva/Continuar, escenas duplicadas).

**Lo que ya está sólido:** input + joystick, movimiento del player, ataques del player,
XP/nivel, upgrades al subir de nivel, vida, pausa, game over, economía de monedas.

---

## 1.5 Sprint actual y división de trabajo

> Acordado: tu compañero está con **sonido** ahora mismo. **No tocamos audio.**
> Nosotros arrancamos por **Menú + Guardado + Continuar partida** (son una sola
> feature conectada: "Continuar" necesita "Guardar", y el menú cablea las dos).

| Lane | Quién | Tarea | Estado |
|------|-------|-------|--------|
| 🔊 Audio / SFX / música | **Compañero** | Agregando sonidos | 🔄 En progreso — **NO TOCAR** |
| 💾 Guardado + Continuar + Menú | **Nosotros (vos + yo)** | Persistencia y cableado del menú | ▶️ Arrancando ahora |

**Por qué van juntas Menú + Guardado + Continuar:**
1. **Guardado** = un `SaveManager` que escribe a disco el estado de la partida.
2. **Continuar** = al cargar Gameplay, restaurar ese estado.
3. **Menú** = el botón "Continuar" se habilita solo si hay save; "Nueva partida" borra el save.

⚠️ **Antes de empezar — decisión de escena:** el Build Settings apunta a `Gameplay 1`, pero
vos venís editando `Gameplay 2` (tiene cambios sin commitear). El guardado/continuar tiene que
apuntar a la escena **correcta**. Hay que resolver esto primero (ver 2.16 y 3.3).

⚠️ **Zona de contacto con audio:** para no pisarnos con tu compañero, cuando cableemos el menú
y los diálogos **no agregamos `AudioSource` ni botones de sonido**; si un botón nuevo necesita
sonido, lo dejamos sin asignar y tu compañero lo conecta después.

---

## 2. Comparación por sistema

### 2.1 Input (movimiento táctil + teclado)
**Estado: ✅ Completo** — `InputHandler.cs` + `VirtualJoystick.cs` son casi idénticos al ejemplo.
De hecho tu joystick es **mejor**: aparece donde tocás la pantalla y se oculta al soltar.
Nada que hacer.

---

### 2.2 Movimiento del Player
**Estado: ✅ Completo** — `Controllers/PlayerController.cs`.
- Ejemplo: usa `MovementHandler` + `SpeedData` (SO) + `PlayerStats` para el multiplicador.
- Vos: `PlayerData.MoveSpeed` + `PlayerUpgradeModifiers.MoveSpeedMultiplier`. Equivalente y bien.

Detalle menor 🟡: `PlayerData.dashSpeed` existe pero **no se usa** (el dash real usa el
`dashSpeed` de `DashAttackBehavior`). No es un error, es un campo muerto. **[CÓDIGO]** — lo
puedo borrar si querés limpiar.

---

### 2.3 IA / Movimiento de enemigos
**Estado: 🟡 Incompleto (solo persiguen)**

- **Ejemplo:** patrón Strategy con `MovementPolicy` (ScriptableObject) y 3 variantes:
  `ChasePolicy`, `FleePolicy`, `MaintainDistancePolicy`. Cada enemigo elige su política en el
  Inspector → distintos comportamientos sin tocar código.
- **Vos:** `EnemyController.cs` tiene una FSM (Chase → WindUp → Attack → Cooldown) que está
  buena para atacar, pero **el movimiento siempre es perseguir** (`UpdateChase` va derecho al
  player). No hay "huir" ni "mantener distancia".

**Qué falta:** la variedad de movimiento. Para un survivor con enemigos a distancia
(arqueros, magos) querés `MaintainDistance` y `Flee`.

**Tipo: [CÓDIGO]** — Te puedo agregar un `MovementPolicy` (SO) idéntico en concepto al del
ejemplo e integrarlo en tu FSM (en `State.Chase`, en vez de ir siempre hacia el player, pedir
la dirección a la policy). Es el cambio más "de diseño" de la lista.

---

### 2.4 Sistema de ataque del Player
**Estado: ➕ Extra / superior al ejemplo**

- **Ejemplo:** `PlayerAttack.cs` — lista de `AttackData`, cooldowns, niveles, proyectil simple.
- **Vos:** arquitectura por estrategia mucho más rica:
  - `AttackBehavior` (SO abstracto) con implementaciones: **Basic (área), Melee, Projectile,
    Aura, Dash**.
  - `AttackSlot` (cooldown + multiplicadores de upgrade por ataque) + `PlayerAttackLoadout`
    (equipar/quitar) + `PlayerAttacker` (busca el enemigo más cercano y dispara).
  - Visuales por ataque (`IAttackVisual`, `BasicAttackAreaVisual`).

No te falta nada acá; está por encima del ejemplo. ✅

Detalle 🟡: `Player/PlayerAttackAreaVisualizer.cs` está marcado como *legacy* y se auto-desactiva.
Es código muerto. **[CÓDIGO]** — lo puedo eliminar.

---

### 2.5 Ataque de enemigos
**Estado: ✅ Completo (con otro enfoque)**

- **Ejemplo:** `EnemyAttack.cs` — melee con rango + cooldown, dispara evento de animación.
- **Vos:** integrado en la FSM de `EnemyController` usando el mismo `AttackBehavior` que el
  player (un enemigo puede tener melee, proyectil, dash…). Tenés además **WindUp** (telegraph
  antes de pegar), que el ejemplo no tiene. ✅➕

Detalle 🟡: tu enemigo dispara `AttackPerformed`/anim solo con un bool `isWalking`. No hay
animación de "atacando" (ver 2.9).

---

### 2.6 Vida / Daño
**Estado: 🟡 Casi completo, falta barra de vida de enemigos**

- **Ejemplo:** separa `PlayerHealth` (con barra `Image` + integración con stats) y `EnemyHealth`
  (con barra `Image` + da XP al morir). Interfaz `IDamageable` con `TakeDamage(int)` y
  `TakeDamage(int, Transform source)`.
- **Vos:** un único `HealthComponent` (float, con `HealthData` SO) reutilizable para player y
  enemigo. Más limpio (DRY). `IDamageable.TakeDamage(float)`. ✅ buen diseño.

**Lo que falta vs ejemplo:**
1. **Barra de vida flotante sobre el enemigo** ❌. Tu `Skeleton.prefab` no tiene Canvas/Image
   de HP. El ejemplo sí. — **[MIXTO]**: te puedo hacer un mini script `EnemyHealthBar.cs` que
   escuche `OnDamaged`, pero el armado del Canvas world-space sobre el prefab es **[UNITY]**.
2. El XP/loot al morir lo maneja `EnemyController.HandleDeath` (vos) en vez de `EnemyHealth`
   (ejemplo). Es equivalente, no es un problema.

---

### 2.7 XP y subida de nivel
**Estado: ✅ Completo**

- **Ejemplo:** `PlayerExperience` (XP/nivel + evento `LeveledUp`) + `PlayerStats` (escala
  vida/daño/velocidad por nivel).
- **Vos:** `XpManagerScript` (singleton XP/nivel + escala la vida máxima al subir) +
  `PlayerUpgradeModifiers`. Equivalente.

⚠️ **Posible bug a revisar 🟡 [CÓDIGO]:** `XpManagerScript.RestorePlayerHealth` multiplica la
vida máxima **en cada** subida de nivel, y además el upgrade `MaxHealthMultiplier` también la
multiplica. Si das ese upgrade, la vida puede crecer doble. Conviene decidir una sola fuente
de verdad. Te lo puedo auditar/arreglar.

---

### 2.8 Selección de mejoras al subir de nivel (Level-Up popup)
**Estado: ✅ Completo (otro enfoque)**

- **Ejemplo:** `LevelUpPopupUI/View/WeaponOptionView` — elegís **armas** (equipar/upgradear).
- **Vos:** `LevelUpSkillSelectionController` + `SkillSelectionUI` + `SkillOptionCardView` +
  `UpgradeDefinition` + `PlayerUpgradeApplier`. Ofrece N upgrades random (pasivos: velocidad,
  XP, vida; o de ataque: daño/cooldown/rango; o desbloquear un ataque nuevo). Pausa el juego
  mientras elegís, buffer de varias subidas seguidas. Está muy completo. ✅➕

Solo asegurate (parte **[UNITY]**) de tener `UpgradeDefinition` assets creados y asignados en
la lista `availableUpgrades` del controller en la escena (ver 3.2).

---

### 2.9 Animación
**Estado: 🟡 Incompleto (falta estado de ataque)**

- **Ejemplo:** `PlayerAnimation`/`EnemyAnimation` manejan: caminar, **atacar (isAttacking con
  duración)**, dirección en 8 sentidos, flip de sprite.
- **Vos:** `PlayerAnimator` (blend tree por VelocityX/Y/Speed — bien para caminar/idle) y
  `EnemyAnimator` (solo bool `isWalking`). **No hay animación de ataque** en ninguno.

**Qué falta:** disparar una animación/transición de "ataque" cuando el player o el enemigo
pegan.

**Tipo: [MIXTO]**
- [CÓDIGO]: exponer un evento `AttackPerformed` desde `AttackSlot`/`EnemyController` y un script
  de animación que ponga `isAttacking` por X segundos (puedo portar la lógica del ejemplo).
- [UNITY]: agregar el parámetro `isAttacking` y los estados/transiciones en los Animator
  Controllers (`AnimatorControllers/`).

---

### 2.10 Spawning de enemigos — **oleadas vs spawner infinito**
**Estado: ❌ Falta el sistema de oleadas / condición de victoria**

- **Ejemplo:** `WaveHandler` + `WaveData` (SO). Oleadas definidas, selección de enemigo por
  **peso/probabilidad**, delay entre enemigos y entre oleadas, lleva la cuenta de enemigos vivos,
  marca "oleada completa" y **"todas las oleadas completas" (victoria)**, y **guarda el progreso**.
- **Vos:** `SpawnerController` + `SpawnerData` — spawnea cada X segundos para siempre, con
  pooling (➕ bueno) y posición random alrededor del player. **No hay oleadas, ni fin, ni
  victoria.** El juego solo termina si te matan.

**Qué falta:** progresión por oleadas y condición de ganar. Es uno de los gaps grandes.

**Tipo: [CÓDIGO]** (la lógica) **+ [UNITY]** (crear los `WaveData` assets y cablear).
Te puedo escribir un `WaveSpawner` que reaproveche tu pooling actual pero organizado en
oleadas con condición de victoria, manteniendo tu estilo.

---

### 2.11 Guardado / Cargar / Continuar partida
**Estado: ❌ Falta entero — es el gap más grande**

- **Ejemplo:** `GameStateManager` serializa a JSON en disco (`Application.persistentDataPath`):
  nivel, XP, vida, posición, armas equipadas y estado de oleadas. Permite **Continuar** desde
  el menú (el botón "Continue" se habilita solo si hay save), arranca nueva partida borrando el
  save, y restaura todo al cargar la escena.
- **Vos:** No existe nada de esto. `SceneLoader`/`GameManagerScript` solo cargan escenas y
  resetean estado en memoria. En el menú, **"Continue" y "New Game" hacen exactamente lo mismo**
  (`MainMenuUi.cs`).

**Qué falta:** todo el sistema de persistencia + el cableado del menú.

**Tipo: [CÓDIGO]** (un `SaveManager` que capture/restaure XP, nivel, vida, monedas, upgrades,
loadout) **+ [UNITY]** (botón Continue que se habilite/deshabilite según haya save).
Puedo portar/adaptar la idea del `GameStateManager` del ejemplo a tu arquitectura (singletons
`XpManagerScript`, `WalletManagerScript`, `PlayerUpgradeModifiers`, `PlayerAttackLoadout`).

---

### 2.12 Pausa
**Estado: ✅ Completo (mejor que el ejemplo)**

- **Ejemplo:** `PauseManager` con un solo flag.
- **Vos:** `GameManagerScript` con **conteo de solicitudes de pausa** (pause stacking), así la
  pausa del menú y la del level-up no se pisan. Más robusto. ✅➕

---

### 2.13 Game Over
**Estado: ✅ Completo (extra vs ejemplo)**

`GameplayUI` muestra diálogo de game over con stats (nivel, XP, monedas), bloquea controles y
pausa. El ejemplo apenas hace `Debug.Log("Player died")`. ✅➕

---

### 2.14 Economía de monedas / Loot
**Estado: ➕ Extra (el ejemplo no tiene nada de esto)**

`LootManagerScript` + `Utils.LootGenerator` (reparto de valor en monedas con peso) +
`CoinPickupScript` (scatter + imán hacia el player) + `WalletManagerScript` + `WalletUI`.
Sistema completo y vistoso. ✅➕ Nada que hacer.

---

### 2.15 Menú principal
**Estado: 🟡 Incompleto**

`MainMenuUi.cs`: `ContinueGame()` y `NewGame()` **llaman a lo mismo** (`LoadGameplay`). Sin
sistema de guardado, "Continuar" no tiene sentido todavía. Se arregla junto con 2.11.
**Tipo: [MIXTO]**.

---

### 2.16 Escenas y Build Settings
**Estado: 🟡 Desprolijo**

- En Build Settings tenés `MainMenu` (índice 0) y **`Gameplay 1`** (índice 1).
- Pero en el repo hay **`Gameplay 1`**, **`Gameplay 2`** (más grande y más reciente — 5/jun) y
  `SampleScene`. No queda claro cuál es la buena. `Gameplay 2` parece la versión en la que
  estás trabajando, pero **no está en el build**.

**Acción [UNITY]:** decidir cuál gameplay es la canónica, ponerla en Build Settings y borrar
las otras (o moverlas a una carpeta `_Old`). Pasos en la sección 3.3.

---

## 3. Guías paso a paso para el trabajo de Unity

### 3.1 Agregar barra de vida flotante a un enemigo (para 2.6)
1. Abrí el prefab `Assets/Prefabs/Entity/Enemy/Skeleton/Skeleton.prefab` (doble click → modo prefab).
2. Click derecho sobre el objeto raíz → **UI → Canvas** (se crea un Canvas hijo).
3. Seleccioná el Canvas → en el Inspector, **Render Mode = World Space**.
4. Escalá el Canvas chiquito (ej. Scale `0.01, 0.01, 0.01`) y posicionalo arriba del sprite
   (Pos Y ≈ `0.8`).
5. Dentro del Canvas, click derecho → **UI → Image**. Esta es el fondo (gris).
6. Duplicá la Image, renombrala `Fill`, ponela verde, y arriba en el Inspector poné
   **Image Type = Filled, Fill Method = Horizontal, Fill Origin = Left**.
7. Cuando te pase el script `EnemyHealthBar.cs` (te lo hago yo), arrastrá la Image `Fill` al
   campo correspondiente.
8. Guardá el prefab (Ctrl/Cmd+S). Repetí para `Vampire.prefab`.

### 3.2 Confirmar que los Upgrades están cableados (para 2.8)
1. En el menú de assets: click derecho → **Create → Scriptable Objects → Upgrades →
   UpgradeDefinition**. Creá varios (ej. "+Velocidad", "+Daño", "Desbloquear Aura").
2. Completá cada uno en el Inspector (tipo Passive o Attack, multiplicadores, ícono, descripción).
3. En la escena Gameplay, seleccioná el objeto que tenga `LevelUpSkillSelectionController`.
4. En el campo **Available Upgrades**, arrastrá todos los `UpgradeDefinition` que creaste.
5. Verificá que `SkillSelectionUI` y sus `optionCards` (los `SkillOptionCardView`) estén
   asignados.

### 3.3 Limpiar escenas / Build Settings (para 2.16)
1. **File → Build Settings** (o **Build Profiles** en Unity 6).
2. Mirá qué escena de gameplay querés como definitiva (`Gameplay 1` o `Gameplay 2`).
3. Si es `Gameplay 2`: arrastrala a la lista de **Scenes In Build**, sacá `Gameplay 1`.
4. En el Project, mové las escenas que ya no uses a una carpeta `Assets/_Old/` (no las borres
   de una si no estás seguro).
5. Asegurate de que `MainMenu` quede en índice **0** y el gameplay en índice **1** (así
   `SceneLoader.LoadMainMenu()` / `LoadGameplay()` siguen funcionando).

### 3.4 Animator: agregar estado de ataque (para 2.9)
1. Abrí el Animator Controller del player (en `Assets/AnimatorControllers/`).
2. En la pestaña **Parameters**, agregá un **Bool** llamado `isAttacking`.
3. Creá un estado/sub-state machine de ataque (o un blend tree de ataque por dirección).
4. Transición: `Cualquier estado → Ataque` con condición `isAttacking == true`; y
   `Ataque → Idle/Walk` con `isAttacking == false`.
5. Desmarcá **Has Exit Time** en esas transiciones para que responda al instante.
6. (Te paso el script que setea `isAttacking` cuando se ejecuta el ataque.)

---

## 4. Plan sugerido (orden de prioridad)

### 🎯 Sprint actual — Menú + Guardado + Continuar (nosotros)
Hacer en este orden (cada paso depende del anterior):

| Paso | Tarea | Tipo | Notas |
|------|-------|------|-------|
| 0 | Decidir escena canónica de gameplay y ponerla en Build (2.16 / 3.3) | [UNITY] | Bloquea todo lo demás |
| 1 | `SaveManager` que capture estado (XP, nivel, vida, monedas, upgrades, loadout) | [CÓDIGO] | Yo lo escribo |
| 2 | Guardar a disco (JSON en `persistentDataPath`) + borrar save en "Nueva partida" | [CÓDIGO] | Yo lo escribo |
| 3 | Restaurar el estado al cargar Gameplay (Continuar) | [CÓDIGO] | Yo lo escribo |
| 4 | `MainMenuUi`: "Continuar" habilitado solo si hay save; "Nueva" borra save (2.15) | [MIXTO] | Código yo + cablear botón vos |
| 5 | Probar el loop completo: jugar → guardar → salir → continuar | [UNITY] | Test manual juntos |

### 🔊 En paralelo — tu compañero
| Tarea | Tipo | Notas |
|-------|------|-------|
| Audio / SFX / música | [MIXTO] | **No tocar.** Lane separada |

### ⏭️ Después del sprint (orden sugerido)
| Prioridad | Tarea | Tipo | Por qué |
|-----------|-------|------|---------|
| 🔴 Alta | Sistema de oleadas + victoria (2.10) | [CÓDIGO]+[UNITY] | Define el "loop" del juego |
| 🟠 Media | Variedad de IA de enemigos (2.3) | [CÓDIGO] | Da profundidad y es del ejemplo |
| 🟠 Media | Animación de ataque (2.9) | [MIXTO] | Pulido visible |
| 🟡 Baja | Barra de vida de enemigos (2.6) | [MIXTO] | Pulido visible |
| ⚪ Trivial | Borrar código muerto (dashSpeed, PlayerAttackAreaVisualizer) | [CÓDIGO] | Limpieza |
| ⚪ Revisar | Doble multiplicación de vida al subir nivel (2.7) | [CÓDIGO] | Posible bug |

---

## 5. Tabla resumen final

| Sistema | Ejemplo | Vos | Estado |
|---------|:-------:|:---:|--------|
| Input + joystick | ✓ | ✓ | ✅ |
| Movimiento player | ✓ | ✓ | ✅ |
| IA movimiento enemigos (varias policies) | ✓ | parcial | 🟡 solo chase |
| Ataque player | ✓ | ✓✓ | ➕ superior |
| Ataque enemigos | ✓ | ✓ | ✅ |
| Vida / daño | ✓ | ✓ | ✅ (falta barra HP enemigo) |
| Barra HP enemigo | ✓ | ✗ | ❌ |
| XP / nivel | ✓ | ✓ | ✅ |
| Popup de mejoras al subir nivel | ✓ | ✓ | ✅ |
| Animación caminar/idle | ✓ | ✓ | ✅ |
| Animación de ataque | ✓ | ✗ | ❌ |
| Oleadas + condición de victoria | ✓ | ✗ | ❌ (spawner infinito) |
| Pooling de enemigos | ✗ | ✓ | ➕ |
| Guardar / Cargar / Continuar | ✓ | ✗ | ❌ |
| Pausa | ✓ | ✓✓ | ➕ pause stacking |
| Game Over con stats | ✗ | ✓ | ➕ |
| Economía de monedas / loot | ✗ | ✓ | ➕ |
| Sistema de upgrades (pasivos/ataque) | parcial | ✓✓ | ➕ |
| Menú principal | ✓ | parcial | 🟡 |
| Escenas / Build ordenado | ✓ | desprolijo | 🟡 |
| Audio / SFX / música | ✗ | en progreso | 🔄 compañero (no tocar) |

---

*Decime por cuál querés arrancar y lo encaramos. Para las tareas [CÓDIGO] las hago yo;
para las [UNITY] te voy guiando paso a paso como en la sección 3.*
