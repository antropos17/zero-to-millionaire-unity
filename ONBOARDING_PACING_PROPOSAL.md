# Next authorized slice: onboarding, modest story and gradual pacing

Use heroine portrait from the approved current sprites and a small speech bubble
in the room area, above the controls. Reels, meters and BUY remain accessible.
No voice/TTS/service, new cast, new room, payment, timer or new luxury tier.

Russian dialogue and dialogue buttons; retain current English brand/task labels.
Localize the three primary action buttons to СПИН / КУПИТЬ / МЕНЮ so instructions
refer to the visible controls. Existing font covers the sample except uppercase
П; verify/fix coverage with the full same-family OFL source before rendering.

Exact contextual copy:
1. «Пока здесь неуютно, но всё можно исправить понемногу. Начнём с пола».
2. «Здесь можно получить игровые монеты и фрагменты вещей. Попробуем один спин».
3. «Для покупки нужны и монеты, и фрагменты именно этой вещи. Посмотрим, сколько уже собрано».
4. Only when ready: «На ремонт пола всего хватает. Нажми „Купить“, когда будешь готов».
5. After committed purchase: «Уже лучше! Следующая задача уже появилась. Двигайся в своём темпе».

Buttons: Далее, Пропустить обучение. Menu: Повторить обучение.
Next advances text only. Normal SPIN/manual purchase remains the sole resource
path. Skip persists without penalty; replay is help-only and grants nothing.
Store version/step/skip and viewed-story mask independently from resource fields
in the same atomic saved state. Resume derives relevant step from actual state.
Experienced existing saves skip onboarding and mark past milestones viewed.
Story waits for spin/reveal completion and appears only after real purchases:
- floor + surfaces: «У комнаты появилась основа. Теперь легче представить, какой она станет».
- wallpaper + door + window: «Теперь комната выглядит совсем иначе. Осталось устроить место для отдыха».
- mattress + lamp: «Теперь есть где отдохнуть и почитать при свете. Ещё табурет, и всё будет на своих местах».
- final: «Девять квадратных метров стали уютным домом. Спасибо за помощь».

Working target: typical whole-room42–60spins, first upgrade2–4spins, later steps
progressively longer. Proposed parts20/30/38/44/50/58/62/70 and prices
60/115/155/195/235/300/315/355; equal cash/current-symbol pool stays unchanged.
These are a starting tuning hypothesis, not measured or owner-approved numbers.
Simulate many seeds and report actual percentiles/per-task intervals, plus time
assumptions. No imposed play duration. Guarantee a matching part on otherwise
all-cash collection rolls to prevent fragment starvation, preserving1icon=1part.
Base payout remains above spin cost; cash-only catch-up still works.

Preserve cash, all fragment/excess counters and ownership. Grandfather the active
legacy task's goal/price in migration so an already-ready old task does not
become unready; subsequent unowned tasks use the slower curve. Persist per-task
requirements, validate raw IDs/statuses before migration, and consume only the
actual required resources on manual purchase. No silent reset of user save.

Tests: fresh/existing/resume/skip/replay, no duplicate rewards, milestone-once,
legacy ready task/owned/excess preservation, both gates, recovery/finite symbols,
full completion and many-seed balance distribution. Feature code freeze13:15UTC;
final regression/native release/source delivery by14:05UTC. This file proposes
the next slice; none of these features is claimed in baseline v0.4f candidate.