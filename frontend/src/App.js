import React, { useEffect, useState } from "react";

function App() {
  const [articles, setArticles] = useState([]); // Zustand für Artikeldaten

  // Daten vom Backend abrufen
  useEffect(() => {
    fetch("http://localhost:5094/api/articles") // Backend-URL
      .then((response) => response.json())
      .then((data) => setArticles(data))
      .catch((error) => console.error("Fehler beim Abrufen der Daten:", error));
  }, []);

  return (
    <div>
      <h1>Artikel Übersicht</h1>
      <ul>
        {articles.map((article) => (
          <li key={article.id}>
            <strong>{article.name}</strong> - Bestand: {article.quantity} - Lager: {article.location}
          </li>
        ))}
      </ul>
    </div>
  );
}

export default App;