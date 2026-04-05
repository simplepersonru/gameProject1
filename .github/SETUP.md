# 🛠️ Настройка CI/CD для Unity игры

В этом репозитории настроен GitHub Actions для автоматической сборки и публикации игры.  
Ниже описано, что нужно сделать **один раз**, чтобы пайплайн заработал полностью.

---

## Что делает пайплайн

| Событие | Результат |
|---------|-----------|
| Push в `main`/`master` | Сборка WebGL + Android → публикация WebGL на GitHub Pages → создание GitHub Release с APK |
| Pull Request | Сборка WebGL + Android → комментарий в PR со ссылками на артефакты |

---

## Шаг 1 — Получить Unity License файл

Пайплайн использует [GameCI](https://game.ci) для сборки Unity проектов.  
Для этого нужна активация лицензии Unity (работает с бесплатной Personal).

### Способ 1: Автоматическая активация через GameCI

1. Запустите workflow `Acquire Unity Activation File` (он требует ручного запуска)  
   *(или создай его отдельно — см. документацию: https://game.ci/docs/github/activation)*
2. Скачайте `.alf` файл из артефактов этого запуска
3. Зайдите на https://license.unity3d.com и активируйте лицензию — скачается `.ulf` файл
4. Добавьте содержимое `.ulf` файла в секрет `UNITY_LICENSE` (см. Шаг 2)

### Способ 2: Через GitHub Actions (рекомендуется)

Добавьте следующий workflow **один раз** для получения `.alf`:

```yaml
# .github/workflows/activation.yml
name: Acquire activation file
on:
  workflow_dispatch:
jobs:
  activation:
    name: Request manual activation file
    runs-on: ubuntu-latest
    steps:
      - uses: game-ci/unity-request-activation-file@v2
        with:
          unityVersion: 6000.3.12f1
      - uses: actions/upload-artifact@v4
        with:
          name: unity-activation-file
          path: ./*.alf
```

---

## Шаг 2 — Добавить секреты в репозиторий

Перейдите в **Settings → Secrets and variables → Actions → New repository secret**:

| Секрет | Описание |
|--------|----------|
| `UNITY_LICENSE` | Содержимое `.ulf` файла лицензии Unity (полный XML) |
| `UNITY_EMAIL` | Email от аккаунта Unity |
| `UNITY_PASSWORD` | Пароль от аккаунта Unity |

> **Важно**: Никогда не коммитьте `.ulf` файл или пароль в репозиторий!

---

## Шаг 3 — Включить GitHub Pages

1. Перейдите в **Settings → Pages**
2. В разделе **Source** выберите **GitHub Actions**
3. Сохраните

После этого, при каждом пуше в `main`/`master`, игра будет автоматически публиковаться по адресу:  
```
https://<ваш-логин>.github.io/<название-репозитория>/
```

---

## Шаг 4 — Проверить результат

После успешного запуска пайплайна:

- **WebGL**: перейдите по ссылке GitHub Pages (см. выше) — игра должна запуститься в браузере
- **Android APK**: перейдите в **Releases** репозитория и скачайте `.apk` файл

---

## Советы по публикации WebGL в будущем

### Бесплатные хостинги для WebGL/WASM игр

| Сервис | Бесплатный план | Особенности |
|--------|----------------|-------------|
| **GitHub Pages** *(уже настроен)* | ✅ Бесплатно | Прямая интеграция с GitHub; нет серверной логики |
| **itch.io** | ✅ Бесплатно | Лучшая платформа для инди-игр; аудитория геймеров |
| **Netlify** | ✅ 100 GB трафика/мес | CI/CD, превью деплои на каждый PR |
| **Vercel** | ✅ Бесплатно | Быстро, CDN по всему миру |
| **Cloudflare Pages** | ✅ Неограниченный трафик | Очень быстрый CDN |

### Публикация на itch.io (рекомендуется для игр)

Чтобы в будущем настроить автоматическую публикацию на itch.io:

1. Создайте аккаунт на https://itch.io
2. Создайте новую игру → выберите **WebGL**
3. Установите [Butler CLI](https://itch.io/docs/butler/)
4. Добавьте секрет `BUTLER_CREDENTIALS` в GitHub (API key от itch.io)
5. Добавьте в workflow шаг:
   ```yaml
   - name: Deploy to itch.io
     uses: josephbmanley/butler-publish-itchio-action@master
     env:
       BUTLER_CREDENTIALS: ${{ secrets.BUTLER_CREDENTIALS }}
       CHANNEL: html5
       ITCH_GAME: <имя-игры>
       ITCH_USER: <ваш-логин>
       PACKAGE: build/WebGL/WebGL
   ```

### Проблемы с WebGL в Unity 6

- Убедитесь, что **Compression Format** в WebGL Player Settings установлен в `Disabled` или `Gzip` (не Brotli, т.к. требует специального сервера)
- GitHub Pages поддерживает Gzip-компрессию
- Для больших игр используйте **Addressables** для ленивой загрузки ресурсов

---

## Что предоставить Copilot в следующий раз

Если потребуется помощь с CI/CD или публикацией, подготовьте:

1. ✅ Репозиторий с настроенными секретами (см. Шаг 2)
2. ✅ GitHub Pages включён (см. Шаг 3)
3. ✅ Логи неудачного запуска workflow (если что-то пошло не так)
4. 💡 Аккаунт на желаемом хостинге (itch.io, Netlify и т.д.) если нужна публикация туда
