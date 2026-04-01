# 🐍 Змейка на C#

> Консольная игра "Змейка", созданная в учебных целях для изучения Git и GitHub.

[![.NET](https://img.shields.io/badge/.NET-8.0%2B-512BD4?style=flat-square&logo=.net)](https://dotnet.microsoft.com/)
[![Git](https://img.shields.io/badge/Git-F05032?style=flat-square&logo=git&logoColor=white)](https://git-scm.com/)
[![GitHub](https://img.shields.io/badge/GitHub-181717?style=flat-square&logo=github&logoColor=white)](https://github.com/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)

---

## 📋 Оглавление

- [🎮 Как играть](#-как-играть)
- [⚙️ Установка и запуск](#️-установка-и-запуск)

---

## 🎮 Как играть

| Клавиша | Действие |
|---------|---------|
| ⬆️ **Стрелка вверх** | Движение вверх |
| ⬇️ **Стрелка вниз** | Движение вниз |
| ⬅️ **Стрелка влево** | Движение влево |
| ➡️ **Стрелка вправо** | Движение вправо |

### 🎯 Элементы игры

| Символ | Описание |
|--------|----------|
| 🟢 `O` | Голова змейки |
| 🟢 `o` | Тело змейки |
| 🍎 `@` | Еда |
| 🧱 `#` | Стены |

### ✨ Цель игры

> Съесть как можно больше еды 🍎, не врезавшись в стены 🧱 и не наступив на свой хвост! 💀

---

## ⚙️ Установка и запуск

### 📋 Требования

- ✅ [.NET SDK](https://dotnet.microsoft.com/download) (версия 8.0 или выше)
- ✅ [Git](https://git-scm.com/downloads) (для клонирования репозитория)

### 🚀 Пошаговая инструкция

```bash
# 1️. Клонируем репозиторий
git clone https://github.com/ВАШ-ЛОГИН/SnakeGame.git

# 2️. Переходим в папку проекта
cd SnakeGame

# 3️. Запускаем игру
dotnet run
