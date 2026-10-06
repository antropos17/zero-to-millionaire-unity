# ZERO TO MILLIONAIRE — финальная проверка Windows v0.6

Игровой код и графика зафиксированы на 6bec3763f688e44cf4266044fd995a945f96ebed.
Позднейший Git HEAD добавляет только проверки, инструкции и упаковку. Сравнение
production-каталогов с этим commit должно оставаться пустым. Unity 6000.3.21f1.

Windows-v0.6-final: StandaloneWindows64, BuildOptions.None, успешно;
104593710 байт по BuildReport, время сборки 13.740 с. Development=false,
Standalone defines пустые, Mono; единственная сцена Assets/Scenes/Level1.unity.
Статический анализ готовой Assembly-CSharp.dll через Mono.Cecil подтвердил
отсутствие Qa/SetQa методов и автоматического игрового harness. Read-only observer
включается только явным private-profile аргументом; обычный запуск его не включает.
SHA256 DLL: f56c14b680149131d2b3cd50aecf19d501a3d09b81b3d69ca37ee15b781cc96a.

Проверки именно финального игрового кода:

- Unity Test Framework EditMode: 24 passed, 0 failed, 0 skipped.
- Модель экономики: 107769 assertions, 1000 полных уровней; 49–60 спинов,
  медиана 53, p95 57. Текущие пороги фрагментов: 20/30/38/44/50/58/62/70;
  цены: 60/115/155/195/235/300/315/355. Спин стоит 5, базовая награда 12,
  cash-символ +2, вертикальная тройка +10. Три бесплатных стартовых спина;
  прозрачный бесплатный recovery при cash<5. Экономика настраивается в Economy.cs.
- Навигация: 432000 samples, 1394 destinations, 1876.2 м; проходы и мебель чисты.
- Offline session: 52 checks; прерывания, rollback, taps, relaunch, reset backup.
- Миграция/позиции: 21106 assertions, 5664 legacy points, 1567 projections,
  максимум 0.491 м; деньги, предметы и фрагменты сохраняются.
- Persistence диалогов: 14 checks; store failure/retry, modal, relaunch,
  help и неизменность ресурсов.
- Реальный полный Editor UI цикл из свежего save без SetQaState: 60144 checks,
  55 спинов, 8 явных BUY, 4 явно закрытых сюжетных комментария, 8144 frames.
  Проверены точные награды/фрагменты, отдельные требования каждой задачи,
  20 повторных BUY на каждом этапе, сохранение каждого ремонта/предмета,
  полное завершение, настоящие SceneManager reload, calm reload, запрет
  поздних действий, reset cancel, confirmed reset, ещё один reload.
- Layout: 2624 assertions, 160 комбинаций, 320x640/360x640/360x800/480x960,
  все 8 задач, 5 вариантов ресурсов, intMax cash, BUY gates/repeat/reload.
- Motion/review/intro: моментальный STOP, settled ПРОПУСТИТЬ, точная награда,
  repeated taps, menu during animation, calm persistence, fast reset,
  удержание ремонта без диалога, ready counters, typography и replay проверены.
  Onboarding прошёл 745 advancing Editor frames.

Все UI проверки здесь — реальные offscreen кадры Unity Editor с синтетическими
callbacks и отдельными saves. Это не OS input, emulator или Android device test.
Editor timing полного цикла: 8133 samples, median 16.61 ms, p95 19.67 ms,
max 283.94 ms за 137.23 с; включает captures и tooling pauses, поэтому это
не standalone/device FPS benchmark. Первый full-level harness ошибочно ожидал
queued dialogue без задержки polling; исправлен только harness и пройден заново.
Оба исходных отчёта сохранены для прозрачности.

6 оригинальных reference PNG включены; вся игровая графика — отдельные PNG
слои комнаты, heroine rig, предметы, slot symbols. Это полноценный Unity проект,
не набор плоских скриншотов. Лицензии шрифтов включены.

Ограничения и состояние запуска:

- По просьбе пользователя финальный EXE запущен видимо, PID 42968; HWND
  подтверждён. Input desktop остаётся Screen-saver, поэтому foreground, реальные
  native pixels, OS input и последние native FPS не подтверждены. Игра оставлена
  открытой для пользователя; её не закрывать ради QA.
- Ранее native safety проверки f972 прошли, но относятся к более раннему build.
- Текущий Android APK отложен по приоритету Windows; старые APK сохранены.
  Нет проверки этого build на реальном Android устройстве или эмуляторе.
- Cloud browser заблокирован, WebGL persistence не проверен; WebGL не включён.
- Предупреждение Unity ComputeBuffer при выходе не доказано исправленным и
  не подавлялось. Нет заявленного native performance pass на заблокированном desktop.
- Пользовательский save не входил в тесты и архивы; до явного открытия пользователем
  hash оставался 93a5cdc9a2e7b5d7da598926a413bfc967b0738e1a92d6269b59d1052c5a125e.
  Теперь обычная игра может законно сохранять действия самого пользователя.
- Ничего не покупалось, не устанавливалось, не публиковалось, не push-илось;
  старые builds, v0.5, APK и посторонние файлы/процессы сохранены.

Архивы source и Windows разрезаются на стандартные .zip.001/.002/... части
не более 15000000 байт. Все части скачивать в одну папку, открыть первую в 7-Zip.
Склейка SHA256, CRC и настоящее извлечение установленным 7-Zip проверяются;
все извлечённые файлы сравниваются по SHA256. Точные результаты и Library IDs:
Deliverables/final-v0.6-delivery.json, final-v0.6-split-verification.json,
library-delivery.json. Инструкция запуска: RUN_WINDOWS_V06.md.