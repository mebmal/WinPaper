# 🌄 WinPaper - Fond d'écran dynamique pour Windows

**WinPaper** est une application Windows développée en C# (.NET) permettant de définir automatiquement une image depuis Internet comme fond d'écran. L'utilisateur peut choisir entre un mode **statique** (image fixe) ou un mode **dynamique** avec une fréquence de rafraîchissement configurable.

---

## ✨ Fonctionnalités

- 🖼️ Définir une image comme fond d'écran depuis une URL
- 🔁 Mode dynamique : rafraîchit le fond d'écran automatiquement à intervalle régulier
- 💾 Enregistrement des paramètres utilisateur dans un fichier `settings.json`
- 🚀 Démarrage automatique avec Windows
- 🧰 Icône dans la zone de notification (systray)
- 🪟 Fermeture dans la barre système (pas d'arrêt réel)
- 📌 Création automatique de raccourci dans le menu Démarrer
- 📋 Enregistrement dans la liste des applications installées de Windows

---

## 🖼️ Aperçu

![image](https://github.com/user-attachments/assets/37d05068-4a82-49e5-a3ea-699112ae8b31)

---

## 🧑‍💻 Utilisation

1. Ouvrez l'application.
2. Collez l'URL d'une image dans le champ prévu.
3. (Optionnel) Activez le **mode dynamique** pour changer automatiquement le fond d'écran.
4. Définissez l’**intervalle de mise à jour** (en secondes).
5. Cliquez sur **Appliquer**.

📌 La fenêtre se masque à la fermeture (croix), l'application reste active en arrière-plan. Cliquez sur l'icône de la zone de notification pour la rouvrir.

---

## 🔧 Installation

### 1. Prérequis

- Windows 10 ou 11
- [.NET Desktop Runtime]([https://dotnet.microsoft.com/en-us/download](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-9.0.5-windows-x64-installer)) 

### 2. Compilation

- Ouvre le projet `WinPaper.sln` dans Visual Studio
- Build la solution : `Ctrl + Maj + B`

### 3. Exécution

- Lance `WinPaper.exe` depuis le dossier `bin/Release` ou `bin/Debug`

---

## 📁 Structure du projet

| Fichier/Classe                   | Rôle                                                                 |
|----------------------------------|----------------------------------------------------------------------|
| `MainWindow.xaml`                | Interface principale                                                 |
| `WallpaperManager.cs`           | Applique le fond d'écran via SystemParametersInfo                    |
| `DynamicWallpaperService.cs`    | Gère le timer et les changements automatiques d'images              |
| `SettingsManager.cs`            | Sauvegarde et lecture des paramètres depuis un fichier JSON         |
| `App.xaml.cs`                   | Point d'entrée principal de l’application                           |

---

## 🗑️ Désinstallation

- Supprimez le fichier exécutable
- Supprimez les éléments suivants :
  - Raccourci dans :  
    `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\WinPaper.lnk`
  - Clé de registre :  
    `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Uninstall\WinPaper`
  - (Optionnel) Paramètres utilisateur : `settings.json`

---

## 👤 Auteur

Développé par **mebmal**

> Pour toute suggestion ou amélioration, n'hésitez pas à ouvrir une issue ou une pull request.

