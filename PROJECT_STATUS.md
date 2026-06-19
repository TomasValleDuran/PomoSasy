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
| 1 | **Sistema de guardado / Continuar partida** | ✅ Terminado y probado (16/jun) | [CÓDIGO] ✅ + [UNITY] ✅ |
| 2 | **Sistema de oleadas (waves) con condición de victoria** | 🟢 Código ✅ + Unity cableándose (16/jun) | [CÓDIGO] ✅ + [UNITY] 🔄 |
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
| 💾 Guardado + Continuar + Menú | **Nosotros (vos + yo)** | Persistencia y cableado del menú | ✅ **Terminado (16/jun)** |

> ✅ **Sprint cerrado el 16/jun:** guardado/continuar andando, menú con título "Survivors",
> New Game, Continue (aparece solo si hay save) y Settings (stub). Loop probado: jugar →
> pausar/salir → continuar. Próximo: ver sección 4 (oleadas, IA, etc.).

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
**Estado: 🟢 Código ✅ + Unity cableado en curso (16/jun) — falta probar el loop completo**

> ✅ Código escrito: `WaveSpawner` (+ `WaveConfigJson`, `EnemyPool`), `WaveData` (SO de fallback),
> `WaveUI` y la victoria en `GameplayUI`. `GameManagerScript` ahora expone `TotalEnemiesAlive` +
> `OnEnemyCountChanged`. Las oleadas se configuran por **JSON** (`Assets/Config/waves.json`); los
> enemigos se referencian por id y se resuelven con el **Enemy Catalog** del `WaveSpawner`.
> El `WaveSpawner` spawnea cada oleada, espera a que mueran **todos** los enemigos, avanza, y al
> terminar la última dispara `OnAllWavesCompleted` (victoria → diálogo + borra el save). Reusa tu pooling.
>
> 🔄 **Unity (hecho hoy):** `WaveSpawner` en la escena con el JSON + catálogo, `SpawnerController`
> viejo desactivado, `WaveUI` agregado y `VictoryDialog` cableado. **Falta:** terminar de asignar
> los textos del `WaveUI` y probar el loop completo (oleada→oleada→victoria + continuar). Pasos en **3.6**.

<details><summary>Contexto original (lo que faltaba)</summary>

- **Ejemplo:** `WaveHandler` + `WaveData` (SO). Oleadas definidas, selección de enemigo por
  **peso/probabilidad**, delay entre enemigos y entre oleadas, lleva la cuenta de enemigos vivos,
  marca "oleada completa" y **"todas las oleadas completas" (victoria)**, y **guarda el progreso**.
- **Vos:** `SpawnerController` + `SpawnerData` — spawnea cada X segundos para siempre, con
  pooling (➕ bueno) y posición random alrededor del player. **No hay oleadas, ni fin, ni
  victoria.** El juego solo termina si te matan.

**Qué falta:** progresión por oleadas y condición de ganar. Es uno de los gaps grandes.

**Tipo: [CÓDIGO]** ✅ hecho **+ [UNITY]** (crear los `WaveData` assets y cablear — ver 3.6).

</details>

---

### 2.11 Guardado / Cargar / Continuar partida
**Estado: ✅ TERMINADO Y PROBADO (16/jun)**

> "Continuar" = retoma la partida (nivel, XP, vida, monedas, upgrades, loadout y posición del
> player; los enemigos arrancan de cero). **Actualizado:** ahora guarda **al inicio de cada
> oleada** (no al pausar/salir/cerrar) para evitar farmeo de XP, y al continuar te deja al
> principio de la oleada guardada. Al morir o ganar, borra el save (la corrida terminó). Detalle
> en 3.6 F.
>
> Archivos nuevos: `Save/SaveSystem.cs`, `Save/GameSaveData.cs`, `Save/GameSaveCoordinator.cs`.
> Tocados: `XpManagerScript`, `WalletManagerScript`, `HealthComponent`, `PlayerUpgradeModifiers`,
> `LevelUpSkillSelectionController`, `PlayerAttackLoadout`, `XpUI`, `GameplayUI`, `MainMenuUi`.
> Unity: `GameSaveCoordinator` en la escena Gameplay + botones cableados.
>
> **Bonus de la sesión:** `SceneLoader` ahora es un singleton *self-healing* (se crea solo si no
> existe), así no rompe cuando hacés Play directo en la escena Gameplay.

<details><summary>Contexto original (lo que faltaba)</summary>

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

</details>

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
**Estado: ✅ TERMINADO (16/jun)**

Menú armado con **título "Survivors"**, **New Game**, **Continue** y **Settings** (stub).
`MainMenuUi.cs`: **"Continuar"** marca `ContinueRequested` y retoma el save; **"Nueva partida"**
borra el save y carga limpio; el botón **"Continuar" aparece solo si hay save** (se oculta con
`SetActive`); **"Settings"** llama a `OpenSettings()` (no hace nada por ahora, como se pidió).

---

### 2.16 Escenas y Build Settings
**Estado: 🟢 Casi listo — falta solo sacar `Gameplay 1` del build**

- ✅ `Gameplay 2` renombrada a **`Gameplay`** (GUID preservado) y puesta en **Build index 1**.
  `MainMenu` en índice 0. El loop menú↔gameplay funciona.
- ⏳ **Pendiente menor:** `Gameplay 1` quedó colgando en el **build index 2**. Sacala de
  **File → Build Settings** y mové `Gameplay 1.unity` + `SampleScene.unity` a `Assets/_Old/`.

**Nota de conflictos con el compañero (sonido):** NO duplicar la escena para "evitar
conflicts" — dos escenas Unity divergidas no se pueden auto-mergear. Mejor: coordinar turnos
sobre la escena, que el sonido vaya en prefabs/AudioManager, y/o activar Unity Smart Merge.

**Acción [UNITY]:** pasos en la sección 3.3.

---

### 2.17 Pulido de UI (timer, leaderboard, diálogos, safe area)
**Estado: 🟢 Código ✅ (19/jun) — falta cablear en Unity (ver 3.7)**

Pasada de UI para que el juego se vea menos "template" y tenga score real:

- **Reloj de partida** ➕: `SurvivalTimer.cs` (cuenta mientras no estés en pausa/game over, se
  resetea por corrida) + `TimerUI.cs` (muestra `mm:ss`). **Se persiste al Continuar** (campo
  `elapsedSeconds` en `GameSaveData`, capturado/restaurado por `GameSaveCoordinator` al inicio
  de cada oleada, igual que el resto del estado).
- **Leaderboard local** ➕: `Scores/LeaderboardSystem.cs` (top 10 en su propio JSON
  `leaderboard.json`, sobrevive a muerte/Nueva partida; ordena por tiempo → nivel → monedas) +
  `UI/LeaderboardUI.cs` + `UI/LeaderboardRowView.cs`. Cada corrida se registra al morir/ganar y
  el resultado muestra el puesto ("New best!" / "Leaderboard #3").
- **Diálogos y wave banner animados**: `UI/DialogAnimator.cs` (fade + scale con tiempo
  *unscaled*, así anima aunque el juego esté en pausa). `GameplayUI` y `WaveUI` ahora lo usan via
  `DialogAnimator.Set(...)` — si el panel no tiene el componente, hace `SetActive` normal (100%
  retrocompatible). El banner además muestra subtítulo ("12 enemies incoming").
- **Stats de resultado más prolijos**: en vez de oraciones ("You reached level 5.") ahora son
  bloques cortos (`Level 7`, `1,240 XP`, `30`) + tiempo sobrevivido + puesto.
- **Safe Area (mobile)** ➕: `UI/SafeAreaFitter.cs` para que el HUD no quede bajo el notch.
- **Fix**: se sacó un `Debug.Log` por frame que estaba en `XpUI.UpdateXp`.

**Tipo: [UNITY]** — todo el código está; falta crear/asignar textos y agregar componentes en la
escena. Pasos en **3.7**.

> ⏳ **Pendiente de diseño (no-código):** sigue faltando una **fuente custom** (todo usa la
> LiberationSans default de TMP). Es el cambio visual más barato; ver nota en sección 4.

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

### 3.3 Renombrar Gameplay 2 → Gameplay y limpiar Build (para 2.16)
0. ⚠️ **Commiteá primero tus cambios de `Gameplay 2`** (tenés cambios sin guardar en git),
   así el rename queda limpio.
1. En la **Project window** de Unity: click derecho en `Gameplay 2` → **Rename** → `Gameplay`.
   Hacerlo dentro de Unity mantiene el GUID y el `.meta`, así **no se rompe el Build Settings**.
2. **File → Build Settings** (o **Build Profiles** en Unity 6): sacá `Gameplay 1`, dejá
   `Gameplay` en el índice **1** y `MainMenu` en el **0**.
   (Esto importa: `SceneLoader.LoadGameplay()` hace `LoadScene(1)`.)
3. Mové `Gameplay 1.unity` y `SampleScene.unity` a `Assets/_Old/` (no las borres de una).
4. **No hagas una copia de la escena** para trabajar en paralelo — coordinás turnos con tu
   compañero en su lugar (ver nota en 2.16).

### 3.5 Cablear el sistema de guardado (para 2.11 / 2.15) — ✅ HECHO (16/jun)
Queda como referencia de lo que se cableó:

**A) Build Settings (si no lo hiciste en 3.3):**
1. **File → Build Settings**: sacá `Gameplay 1`, dejá `Gameplay` en índice **1**, `MainMenu` en **0**.

**B) Agregar el coordinador a la escena de Gameplay:**
1. Abrí la escena `Gameplay`.
2. Click derecho en la jerarquía → **Create Empty**. Nombralo `SaveCoordinator`
   (o usá un objeto de managers que ya tengas, ej. el que tiene XpManager/WalletManager).
3. Con el objeto seleccionado → **Add Component** → buscá **GameSaveCoordinator** → agregalo.
   No tiene campos que asignar. Listo.
4. Guardá la escena.

**C) Cablear el botón "Continuar" del menú:**
1. Abrí la escena `MainMenu`.
2. Seleccioná el objeto que tiene el script **MainMenuUI**.
3. En el Inspector vas a ver un campo nuevo **Continue Button** → arrastrá ahí el botón
   "Continuar" de la jerarquía.
4. Verificá los **OnClick** de los botones (siguen igual que antes):
   - Botón "Continuar" → `MainMenuUI.ContinueGame`
   - Botón "Nueva partida" → `MainMenuUI.NewGame`
5. Guardá la escena.

**D) Chequeo del player (debería estar ya):**
- El player tiene el **Tag = "Player"**.
- Tiene los componentes `HealthComponent`, `PlayerUpgradeModifiers`, `PlayerAttackLoadout`.
  (Si falta `PlayerUpgradeModifiers`, el sistema igual no rompe: se saltea esa parte.)

**E) Probar el loop:**
1. Play en `MainMenu` → "Nueva partida" → jugá, subí de nivel, agarrá monedas.
2. Pausá (o salí al menú). Debería guardar.
3. Volvé al menú: el botón "Continuar" tiene que estar **habilitado**.
4. "Continuar" → tenés que aparecer con tu nivel/vida/monedas/upgrades/posición de antes.
5. Probá morir → volver al menú → "Continuar" tiene que estar **deshabilitado** (save borrado).
6. El save queda en `Application.persistentDataPath/savegame.json` (en Mac:
   `~/Library/Application Support/<compañía>/<juego>/savegame.json`) por si querés inspeccionarlo.

### 3.4 Animator: agregar estado de ataque (para 2.9)
1. Abrí el Animator Controller del player (en `Assets/AnimatorControllers/`).
2. En la pestaña **Parameters**, agregá un **Bool** llamado `isAttacking`.
3. Creá un estado/sub-state machine de ataque (o un blend tree de ataque por dirección).
4. Transición: `Cualquier estado → Ataque` con condición `isAttacking == true`; y
   `Ataque → Idle/Walk` con `isAttacking == false`.
5. Desmarcá **Has Exit Time** en esas transiciones para que responda al instante.
6. (Te paso el script que setea `isAttacking` cuando se ejecuta el ataque.)

### 3.6 Cablear el sistema de oleadas (para 2.10)
**A) Las oleadas se configuran por JSON** (`Assets/Config/waves.json`):
- Ya hay un ejemplo creado con 4 oleadas de dificultad creciente. Editás ese archivo y listo —
  al volver a Play, toma los cambios (Unity reimporta el `.json` al guardarlo).
- Formato: una lista `waves`, cada una con `waveName`, `startDelay` (respiro antes de arrancar),
  `spawnInterval` (segundos entre enemigos) y `entries` (qué enemigo y cuántos). Ejemplo:
  ```json
  { "waveName": "Mixed", "startDelay": 4.0, "spawnInterval": 1.0,
    "entries": [ { "enemy": "Skeleton", "amount": 8 }, { "enemy": "Vampire", "amount": 3 } ] }
  ```
- Los `enemy` son **ids de texto**, no prefabs (un JSON no puede referenciar prefabs). El mapeo
  id→prefab se hace una sola vez en el Inspector (paso B.4, **Enemy Catalog**). Para tocar
  cantidades/intervalos/orden de oleadas no hace falta abrir Unity; para agregar un **tipo nuevo**
  de enemigo, sumalo al catálogo una vez.
- *(El `WaveData` ScriptableObject sigue existiendo como fallback: si NO asignás un JSON, usa el SO.
  Si asignás el JSON, el SO se ignora.)*

**B) Poner el WaveSpawner en la escena:**
1. Abrí la escena `Gameplay`.
2. **Desactivá o borrá el objeto que tiene `SpawnerController`** (el spawner infinito). ⚠️ No
   dejes los dos corriendo: el `WaveSpawner` espera a que mueran *todos* los enemigos para
   avanzar, y el spawner infinito nunca lo dejaría llegar a cero.
3. Create Empty → nombralo `WaveSpawner` → **Add Component → WaveSpawner**.
4. Configuralo:
   - **Waves Json** → arrastrá `Assets/Config/waves.json`.
   - **Enemy Catalog** → agregá un elemento por cada enemigo: **Id** = el texto que usás en el
     JSON (`Skeleton`, `Vampire`…) y **Prefab** = el prefab correspondiente.
   - **Spawn Radius** (ej. 10, igual que el spawner viejo) y dejá **Use Enemy Pooling** tildado.

**C) UI del contador de oleada:**
1. En el Canvas de Gameplay, creá un **TMP Text** arriba (ej. "Wave 1 / 3"). Opcional: otro para
   "Enemies: N", y un objeto banner (panel con texto) que aparezca al empezar cada oleada.
2. Create Empty `WaveUI` (o usá el objeto del `GameplayUI`) → **Add Component → WaveUI**.
3. Asigná: **Wave Text** (obligatorio), **Enemies Text** y **Wave Banner**/**Wave Banner Text**
   (opcionales). **Wave Spawner** se autocompleta si lo dejás vacío, pero podés arrastrarlo.

**D) Diálogo de victoria:**
1. Duplicá tu `gameOverDialog`, renombralo `victoryDialog`, cambiá el texto a "¡Ganaste!".
2. Seleccioná el objeto con `GameplayUI` → asigná **Victory Dialog** = ese panel. Opcional:
   **Victory Level Text** / **Victory Money Text** para mostrar stats.
3. En el botón de salir del `victoryDialog`, en **OnClick** llamá a
   `GameplayUI.ExitToMainMenuFromVictory`.

**E) Probar:** Play → matá toda la oleada 1 → debería arrancar la 2 tras el delay → al limpiar
la última, aparece el diálogo de victoria y el save se borra.

**F) Nuevo comportamiento del guardado (anti-exploit):**
- El juego ahora guarda **al inicio de cada oleada** (solo se conserva el último snapshot). Ya
  **no** se guarda al pausar / salir al menú / cerrar la app.
- Por qué: antes podías casi limpiar una oleada (juntando XP/levels), salir, y "Continuar" te
  dejaba todo ese progreso mientras los enemigos reaparecían → farmeo infinito de stats. Ahora,
  al continuar, retomás **desde el principio de la oleada en curso**: perdés el progreso parcial
  de esa oleada (es el costo intencional para cerrar el exploit).
- "Continuar" además te reposiciona en la oleada guardada (no arranca siempre de la 1).

### 3.7 Cablear el pulido de UI (para 2.17)
Todo el código ya está. Pasos en la escena (todo [UNITY]):

**A) Reloj de partida:**
1. En la escena `Gameplay`: Create Empty → `SurvivalTimer` → **Add Component → SurvivalTimer**.
   (No tiene campos; se autorregistra.)
2. En el Canvas, creá un **TMP Text** arriba al centro (ej. "00:00"). Add Component → **TimerUI**
   → arrastrá ese texto al campo **Timer Text**.

**B) Diálogos y banner animados:**
1. A cada panel — `pauseDialog`, `gameOverDialog`, `victoryDialog` y el `waveBanner` — agregale
   **Add Component → DialogAnimator** (te crea solo el `CanvasGroup`). Listo, ya animan.
   *(Si no le ponés el componente, el panel sigue funcionando con on/off normal.)*
2. Banner: creá un segundo **TMP Text** debajo del título del banner y arrastralo a
   `WaveUI → Wave Banner Subtitle` (muestra "X enemies incoming").

**C) Stats de resultado (en el objeto con `GameplayUI`):**
- Asigná los campos nuevos (todos opcionales): **Survived Time Text**, **Game Over Rank Text**,
  **Victory Time Text**, **Victory Rank Text**. Reusá/duplicá los textos que ya tengas en los
  diálogos de game over / victoria.

**D) Leaderboard:**
1. En `MainMenu`, creá un panel `LeaderboardPanel` (desactivado por default) → Add Component →
   **LeaderboardUI**.
2. **Modo rápido:** arrastrá un **TMP Text** multilinea a **Fallback Text**. Con eso ya muestra
   la tabla. **Modo lindo:** hacé un prefab de fila (3 TMP: rank/time/detail) con
   **LeaderboardRowView** y asignale sus 3 campos; después en `LeaderboardUI` poné **Row Prefab**
   y **Content** (un objeto con *Vertical Layout Group*). (Opcional: **Empty State** = un texto
   "Sin scores todavía".)
3. Un botón "Leaderboard" en el menú: en su **OnClick** poné el `LeaderboardPanel` →
   `GameObject.SetActive(true)` (y un botón "Cerrar" con `SetActive(false)`). Se refresca solo al
   activarse.

**E) Safe Area (mobile):**
1. Envolvé tu HUD en un panel raíz que llene la pantalla (anchors 0–1) y agregale
   **Add Component → SafeAreaFitter**. Movó los elementos del HUD (y el joystick si querés)
   dentro de ese panel. En notch/encoder ya no se cortan.

**F) Probar:** jugá un rato (el reloj corre) → morí → el diálogo debe mostrar tiempo + puesto →
volvé al menú → abrí el leaderboard y verificá que aparezca la corrida. Probá también
pausar→Continuar y confirmá que el reloj **retoma** (no vuelve a 00:00).

## 4. Plan sugerido (orden de prioridad)

### ✅ Sprint cerrado (16/jun) — Menú + Guardado + Continuar

| Paso | Tarea | Tipo | Estado |
|------|-------|------|--------|
| 0 | Renombrar `Gameplay 2` → `Gameplay` (3.3) | [UNITY] | ✅ Hecho (GUID preservado) |
| 0b | `Gameplay` en Build index 1 | [UNITY] | ✅ Hecho (falta sacar `Gameplay 1`, ver 2.16) |
| 1 | `SaveSystem` + `GameSaveData` | [CÓDIGO] | ✅ Hecho |
| 2 | Captura de estado (XP, nivel, vida, monedas, upgrades, loadout, posición) | [CÓDIGO] | ✅ Hecho |
| 3 | Restaurar al continuar + guardar al pausar/salir/cerrar | [CÓDIGO] | ✅ Hecho |
| 4 | `MainMenuUi` + menú (título, New Game, Continue condicional, Settings stub) | [MIXTO] | ✅ Hecho |
| 5 | Cablear en Unity (coordinator + botones) — sección **3.5** | [UNITY] | ✅ Hecho |
| 6 | Probar el loop: jugar → pausar → salir → continuar | [UNITY] | ✅ Probado |

**Único colgado menor:** sacar `Gameplay 1` del Build Settings (ver 2.16).

### 🔊 En paralelo — tu compañero
| Tarea | Tipo | Notas |
|-------|------|-------|
| Audio / SFX / música | [MIXTO] | **No tocar.** Lane separada |

### ⏭️ Después del sprint (orden sugerido)
| Prioridad | Tarea | Tipo | Por qué |
|-----------|-------|------|---------|
| 🟢 Casi (probar) | Sistema de oleadas + victoria (2.10) | [CÓDIGO] ✅ + [UNITY] 🔄 (3.6) | Define el "loop" del juego |
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
| Oleadas + condición de victoria | ✓ | ✓ | 🟢 código ✅ + Unity en curso, falta probar (3.6) |
| Pooling de enemigos | ✗ | ✓ | ➕ |
| Guardar / Cargar / Continuar | ✓ | ✓ | ✅ terminado y probado (16/jun) |
| Pausa | ✓ | ✓✓ | ➕ pause stacking |
| Game Over con stats | ✗ | ✓ | ➕ |
| Economía de monedas / loot | ✗ | ✓ | ➕ |
| Sistema de upgrades (pasivos/ataque) | parcial | ✓✓ | ➕ |
| Menú principal | ✓ | ✓ | ✅ terminado (título, New/Continue/Settings) |
| Reloj de partida (timer) | ✗ | ✓ | ➕ código ✅ (persiste al Continuar), cablear (3.7) |
| Leaderboard local (top 10) | ✗ | ✓ | ➕ código ✅, cablear (3.7) |
| Diálogos / banner animados | ✗ | ✓ | ➕ código ✅, cablear (3.7) |
| Safe Area (mobile) | ✗ | ✓ | ➕ código ✅, cablear (3.7) |
| Fuente custom (no default TMP) | — | ✗ | ❌ pendiente de diseño (ver sección 4) |
| Escenas / Build ordenado | ✓ | ✓ | 🟢 casi (falta sacar `Gameplay 1` del build) |
| Audio / SFX / música | ✗ | en progreso | 🔄 compañero (no tocar) |

---

*Decime por cuál querés arrancar y lo encaramos. Para las tareas [CÓDIGO] las hago yo;
para las [UNITY] te voy guiando paso a paso como en la sección 3.*
