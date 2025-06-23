# Lagerverwaltung – Backend

Dies ist das Backend eines Lagerverwaltungssystems mit Funktionen wie Produktverwaltung, Lagerstand, Restock Queue und Audit Logging.

## 🚀 Features

- Benutzerbasierte Rechtevergabe (Manager)
- Produkt- und Lagerverwaltung
- Restock Queue & automatische Verarbeitung
- Audit Logs
- Firebase Authentifizierung via Token

## 🧾 Anforderungen

- .NET 8 SDK
- SQLite
- Firebase-Projekt mit Service Account

## Firebase-Konfiguration

Im Ordner `Backend/Secrets` befindet sich die Datei `ServiceAccount.json`, allerdings nur mit Platzhalterwerten.

Für die Nutzung des Systems ist ein eigener Firebase-Service-Account erforderlich. Dafür gibt es zwei Möglichkeiten:

1. **Ich sende meine vollständige `ServiceAccount.json`-Datei** auf Anfrage privat zu.
2. **Alternativ kann ein eigener Firebase-Service-Account erstellt werden**:
   - Neues Firebase-Projekt unter https://console.firebase.google.com anlegen
   - In den Projekteinstellungen unter **Service Accounts** einen neuen privaten Schlüssel generieren
   - Die heruntergeladene Datei in `Backend/Secrets/ServiceAccount.json` einfügen (bestehende Datei ersetzen)