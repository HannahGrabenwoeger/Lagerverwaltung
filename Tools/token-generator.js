const { initializeApp } = require("firebase/app");
const { getAuth, signInWithEmailAndPassword } = require("firebase/auth");

const firebaseConfig = {
  apiKey: "AIzaSyBfpghFMu5DCVgXr7tx35_uipcwpUlb5ZY",
  authDomain: "lagerverwaltung-backend-10629.firebaseapp.com",
  projectId: "lagerverwaltung-backend-10629",
};

const app = initializeApp(firebaseConfig);
const auth = getAuth(app);

async function getToken() {
  try {
    await auth.signOut();

    const userCredential = await signInWithEmailAndPassword(auth, "admin@email.com", "password");

    const tokenResult = await userCredential.user.getIdTokenResult(true); 

    console.log("Dein ID Token:");
    console.log(tokenResult.token);

    console.log("Custom Claims im Token:");
    console.log(tokenResult.claims);

    if (!tokenResult.claims.role) {
      console.warn("ACHTUNG: Kein 'role' Claim im Token vorhanden!");
    }
  } catch (error) {
    console.error("Fehler beim Login oder Tokenabruf:", error.message);
  }
}

getToken();