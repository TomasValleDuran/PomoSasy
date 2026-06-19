# TP Integrador — Diseño de enemigos (variedad por atributos)

> Catálogo de enemigos a partir de los **atributos que ya tiene el proyecto**.
> La idea: con pocas piezas (políticas de movimiento × comportamientos de ataque × stats)
> se arman muchos enemigos distintos **sin escribir código nuevo por cada uno**.
>
> Etiquetas de costo:
> - ✅ **Ya se puede** — sólo crear/asignar ScriptableObjects en el editor (no toca código).
>   *(Requiere que estén las `MovementPolicy` de la Feature 1 del [PROJECT_STATUS](PROJECT_STATUS.md) §2.3.)*
> - 🔧 **Código chico** — un script/SO nuevo y acotado.
> - 🧱 **Código más grande** — toca varios sistemas; pensarlo si sobra tiempo.

---

## 1. La "mesa de mezclas": qué atributos tenés

Un enemigo en este proyecto = **un prefab** con `EnemyController` + `HealthComponent`, que
referencia tres ScriptableObjects. Cambiando esos SO (o creando variantes) cambia todo el
comportamiento. Estos son **todos los knobs reales** del código actual:

### `EnemyData` (`Scripts/Data/EnemyData.cs`)
| Campo | Qué hace | Rango útil |
|-------|----------|------------|
| `enemyType` | id/nombre (cae al nombre del asset si está vacío). Se usa en el conteo de vivos | texto |
| `moveSpeed` | velocidad de desplazamiento | 1 (tanque) – 5 (enjambre) |
| `attackData` | **referencia** al `AttackData` (define el ataque) | SO |
| `moneyOnDeath` | monedas que dropea | 0 – 50 |
| `xpOnDeath` | XP que da al morir | 1 – 30 |
| `movementPolicy` ⭐ | **(nuevo, Feature 1)** estrategia de movimiento. Null = perseguir | SO |

### `AttackData` (`Scripts/Attack/AttackData.cs`)
| Campo | Qué hace | Rango útil |
|-------|----------|------------|
| `damage` | daño por golpe | 5 – 40 |
| `cooldown` | segundos entre ataques | 0.3 (rápido) – 3 (lento/pesado) |
| `attackBehavior` | **cómo** pega (ver abajo) | SO |
| `attackRange` | distancia a la que se "engancha"/golpea. **También define cuándo deja de perseguir y ataca** | 0.8 (melee) – 9 (a distancia) |
| `windupDuration` | telegraph: tiempo quieto antes de pegar | 0 (instantáneo) – 1.2 (golpe pesado) |
| `attackSfx` | sonido (lo cablea el compañero, **no tocar audio**) | clip |

### `HealthData` (`Scripts/Health/HealthData.cs`)
| Campo | Qué hace |
|-------|----------|
| `maxHealth` | vida máxima |

> ⚠️ Hoy Skeleton y Vampire **comparten** `Prefabs/Entity/Health/HealthData.asset`. Para variar
> la vida por enemigo hay que **duplicar** ese asset (ej. `HealthData_Tank`, `HealthData_Swarm`)
> y asignarlo en el `HealthComponent` de cada prefab.

### Comportamientos de ataque (`AttackBehavior`) — **ya existen como assets** en `Prefabs/Entity/Attack/`
| Behavior | Cómo pega | Bueno para |
|----------|-----------|-----------|
| **Melee** | daño instantáneo si el target está dentro de `attackRange` | grunt, tanque, enjambre |
| **Projectile** | dispara un proyectil hacia el player (tiene `projectileSpeed`, `spawnOffset`, prefab) | **arquero / mago (a distancia)** |
| **Dash** | embiste en línea recta hacia el player y pega al contacto (tiene `dashSpeed`) | embestidor / kamikaze |
| **Aura** | daño de área sostenido alrededor del que ataca | enemigo "maldito" que lastima al acercarse |
| **Basic** | golpe de área (overlap). Pensado más para el player | jefe melee amplio |

### Políticas de movimiento (`MovementPolicy`) — **nuevas, Feature 1**
| Policy | Movimiento | Parámetros |
|--------|-----------|-----------|
| **Chase** | va derecho al player | — |
| **Flee** | huye del player | — |
| **MaintainDistance** | se acerca si está lejos, retrocede si está cerca, se queda en el "punto dulce" | `desiredDistance`, `tolerance` |

**La combinación clave:** `movementPolicy` (cómo se mueve) × `attackBehavior` (cómo pega) ×
stats. Eso es la fuente de variedad. Ejemplo: *MaintainDistance + Projectile = arquero* que
patea para mantener distancia mientras te dispara.

---

## 2. Arquetipos que se arman YA (sólo SO, sin código)

> Asumiendo Feature 1 (las 3 `MovementPolicy`). Cada fila = duplicar `EnemyData` + `AttackData`
> (+ `HealthData` si querés otra vida) y, casi siempre, reusar el prefab de Skeleton/Vampire con
> otro sprite/tinte. Valores **sugeridos**, ajustá jugando.

| # | Arquetipo | Movimiento | Ataque | HP | Speed | Daño | CD | Range | Windup | Idea de diseño | Costo |
|---|-----------|-----------|--------|----|----|------|----|-------|--------|----------------|-------|
| 1 | **Esqueleto** (base actual) | Chase | Melee | 100 | 2 | 10 | 0.8 | 1.0 | 0.2 | El grunt de referencia | ✅ ya está |
| 2 | **Enjambre** (Swarmer) | Chase | Melee | 30 | 4.5 | 5 | 0.6 | 0.8 | 0 | Débil pero viene en manada, te rodea | ✅ |
| 3 | **Acorazado** (Tank) | Chase | Melee | 350 | 1.2 | 25 | 1.5 | 1.2 | 0.6 | Esponja de daño, lento, gran recompensa | ✅ |
| 4 | **Arquero** (ranged) ⭐ | MaintainDistance (d≈6) | **Projectile** | 70 | 2.5 | 12 | 1.5 | 7 | 0.5 | **El enemigo a distancia que pediste.** Te mantiene a raya y dispara | ✅ |
| 5 | **Mago kiteador** | **Flee** | **Projectile** | 60 | 3 | 15 | 1.2 | 8 | 0.4 | Huye mientras te tira. Te obliga a perseguir | ✅ |
| 6 | **Embestidor** (Charger) | Chase | **Dash** | 90 | 2 | 20 | 2.5 | 4 | 0.8 | Se planta (windup) y embiste en línea. Telegraph claro | ✅ |
| 7 | **Maldito** (Aura) | Chase | **Aura** | 120 | 1.8 | 8/tick | — | 1.5 | 0 | Lastima sólo con estar cerca; te empuja a no amontonarte | ✅ |
| 8 | **Bruiser** (golpe pesado) | Chase | Melee | 200 | 1.5 | 40 | 2.5 | 1.4 | 1.2 | Pega fuerte y lento, todo está en leer el windup | ✅ |

**Mínimo recomendado para la entrega (profundidad, no cantidad):** 1, 4 y 6 (o 7).
Eso ya muestra **3 patrones de movimiento + 3 patrones de ataque** distintos, que es justo lo
que evalúa la cátedra vs. el ejemplo (`Chase`/`Flee`/`MaintainDistance` + melee/ranged/dash).

> 💡 **El arquero (#4) es el headline.** "Enemigos a distancia" sale 100% con sistemas que ya
> tenés (`ProjectileAttackBehavior` + `MaintainDistancePolicy`), sin una línea de código nueva.

---

## 3. Mecánicas que NO tenemos (propuestas, ordenadas por costo/impacto)

> Sólo si sobra tiempo. Priorizá las 🔧 chicas: dan mucha variedad por poco código.

### 🔧 A. Comportamiento al morir (`OnDeathBehavior` SO) — **alto impacto, bajo costo**
`EnemyController.HandleDeath()` ya es el único punto de muerte. Agregar un campo opcional
`OnDeathBehavior onDeath` (SO con `Execute(Transform, EnemyData)`) habilita de un saque:
- **Exploder / Bomber**: AoE de daño al morir (reusa la lógica de `AuraAttackBehavior`). Te castiga por matarlo encima tuyo.
- **Splitter**: al morir spawnea 2 minis (vía el `EnemyPool`). Da oleadas que "crecen".
- **Necromante**: al morir deja una lápida que revive esqueletos (combina con Splitter).

Es **un** hook nuevo que abre 3-4 enemigos. Mejor relación variedad/código.

### 🔧 B. Daño por contacto (touch damage) — **opcional, es muy "survivors"**
Hoy el enemigo **sólo** hace daño con el `AttackBehavior` (windup → golpe). En los survivors
clásicos, el cuerpo del enemigo lastima al rozarte. Sería un `ContactDamage` chico
(`OnCollisionStay2D`/trigger + tick de daño con cooldown a `IDamageable`). Decisión de diseño:
si lo agregás, el enjambre (#2) se vuelve mucho más amenazante. **Riesgo:** rebalancea todo.

### 🔧 C. Enrage por vida baja — **chico**
Al bajar de X% de HP, sube `moveSpeed`/baja `cooldown`. Un par de líneas en `EnemyController`
escuchando `HealthComponent.OnDamaged`. Da "momentos" sin enemigos nuevos.

### 🔧 D. Variedad de loot/recompensa por rareza — **chico, ya casi está**
Ya tenés `moneyOnDeath`/`xpOnDeath`. Definir "tiers" (común/élite/jefe) es sólo convención de
valores + quizá un tinte de color en el prefab. Cero código si lo dejás como guía de balance.

### 🧱 E. Escudo / armadura direccional — **grande**
Invulnerable de frente, vulnerable de espalda (o escudo temporal recargable). Toca
`HealthComponent`/`IDamageable` para filtrar daño por ángulo/estado. Lindo pero caro; dejarlo
fuera del scope de mañana.

### 🧱 F. Jefe de oleada (boss) — **grande**
Un enemigo con barra de vida grande, varias fases (cambia `attackBehavior`/`movementPolicy` por
fase) y que **bloquea el avance de oleada** (el `WaveSpawner` ya espera a que mueran todos, así
que un boss como única entrada de la última oleada ya funciona como "jefe final" 👀). La versión
barata: oleada final = 1 solo "Acorazado/Bruiser" con mucha HP. Eso es ✅ y ya se siente a jefe.

---

## 4. Receta: cómo crear un enemigo nuevo (paso a paso)

Tomando el **Arquero (#4)** como ejemplo concreto:

1. **AttackData del arquero**
   `Prefabs/Entity/Enemy/` → Create → Scriptable Objects → Attack → AttackData → `ArcherAttackData`.
   - `attackBehavior` = el asset **ProjectileAttack** (`Prefabs/Entity/Attack/ProjectileAttack.asset`).
   - `damage` 12, `cooldown` 1.5, `attackRange` 7, `windupDuration` 0.5.
2. **MovementPolicy** (de la Feature 1)
   Create → AI/Movement/MaintainDistance → `ArcherMaintainDistance` con `desiredDistance` 6, `tolerance` 0.5.
3. **(Opcional) HealthData propio**: duplicá `HealthData.asset` → `HealthData_Archer`, `maxHealth` 70.
4. **EnemyData**
   Create → Scriptable Objects → EnemyObject → `ArcherData`.
   - `enemyType` = `Archer`, `moveSpeed` 2.5, `attackData` = `ArcherAttackData`, `movementPolicy` = `ArcherMaintainDistance`, `moneyOnDeath`/`xpOnDeath` a gusto.
5. **Prefab**
   Duplicá `Skeleton.prefab` (o `Vampire`) → `Archer.prefab`. En su `EnemyController` poné `EnemyData = ArcherData`; en el `HealthComponent`, el `HealthData` que quieras. Cambiá el sprite/tinte para que se distinga.
6. **Catálogo + oleadas**
   En el `WaveSpawner` de la escena → `Enemy Catalog` → nueva entrada `id = "Archer"`, `prefab = Archer.prefab`.
   En `Assets/Config/waves.json`, sumá `{ "enemy": "Archer", "amount": 3 }` a alguna oleada.

Listo: enemigo nuevo, **cero código**.

---

## 5. Propuesta de oleadas con variedad

Reescritura sugerida de `waves.json` para que se note la progresión de tipos
(usa sólo arquetipos ✅; agregá los ids al catálogo del `WaveSpawner` a medida que crees prefabs):

| Oleada | Composición | Qué enseña al jugador |
|--------|-------------|------------------------|
| 1 — Warm-up | 5 Esqueleto | lo básico (perseguidores melee) |
| 2 — A distancia | 4 Esqueleto + 2 **Arquero** | aparece la amenaza ranged → moverse |
| 3 — Embestida | 6 Esqueleto + 2 **Embestidor** | leer telegraphs/windup |
| 4 — Presión | 10 Enjambre + 3 Arquero | manejar la manada y el fuego de lejos |
| 5 — Jefe | 1 **Acorazado/Bruiser** (mucha HP) + 4 Enjambre | clímax y condición de victoria |

---

*Relación con el resto del proyecto:* los arquetipos ✅ dependen sólo de la **Feature 1
(MovementPolicy)** del [PROJECT_STATUS.md](PROJECT_STATUS.md) §2.3. La **animación de ataque**
(§2.9) hace que el windup/embestida del Embestidor y el Bruiser se lean mucho mejor. Audio queda
para el compañero (no tocar).
