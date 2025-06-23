const { initializeApp } = require("firebase/app");
const { getAuth, signInWithEmailAndPassword, signOut } = require("firebase/auth");

const firebaseConfig = {
  apiKey: "AIzaSyBfpghFMu5DCVgXr7tx35_uipcwpUlb5ZY",
  authDomain: "lagerverwaltung-backend-10629.firebaseapp.com",
  projectId: "lagerverwaltung-backend-10629",
};

const app = initializeApp(firebaseConfig);
const auth = getAuth(app);

async function getToken() {
  try {
    await signOut(auth); // wichtig, damit das neue Token geladen wird!

    const userCredential = await signInWithEmailAndPassword(auth, "manager@email.com", "password");

    const tokenResult = await userCredential.user.getIdTokenResult(true); // ⬅ true ist extrem wichtig

    console.log("➡️ Token geladen:");
    console.log(tokenResult.token);

    console.log("➡️ Custom Claims:");
    console.log(tokenResult.claims);

  } catch (err) {
    console.error("❌ Fehler beim Login/Token:", err.message);
  }
}

getToken();