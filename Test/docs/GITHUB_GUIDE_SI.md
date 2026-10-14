# Luma Player — GitHub / Windows EXE Guide (සිංහල)

## 1. අලුත් repository එක

[Create a new repository](https://github.com/new) → `Luma-Player` → Public → Create (initial README නොදමන්න).
ZIP එක extract කර එහි **ඇතුළත තිබෙන files සහ folders** root එකට upload කරන්න. `.github/workflows/windows.yml` path එක අනිවාර්යයි.

## 2. EXE build

Repo එකේ **Actions → Build Windows Installer → Run workflow → main → Run workflow**.
Success වුණාම ඒ run එකේ **Artifacts → Luma-Player-Windows-Setup** download කරන්න. ZIP extract කර `Luma-Player-Setup-1.0.0.exe` run කරන්න.

## 3. නව versions

`src/LumaPlayer/LumaPlayer.csproj` සහ `installer/LumaPlayer.iss` තුළ version එක update කර main branch එකට push කරන්න. Build success වූ පසු test කරලා GitHub Releases එකට installer manually upload කරන්න.

## 4. අවධානය

මෙම source එක Windows PC එකක තවම සම්පූර්ණයෙන් build/test කර නැත. SmartScreen හේතුවෙන් unsigned EXE එකට warning එන්න පුළුවන්. Defender disable කරන්න එපා. Lock-screen media controls මේ version එකේ නැත.
