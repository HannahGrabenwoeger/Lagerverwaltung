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
    const userCredential = await signInWithEmailAndPassword(auth, "admin@email.com", "password");
    const token = await userCredential.user.getIdToken();
    console.log("Dein ID Token:\n");
    console.log(token);
  } catch (error) {
    console.error("Fehler beim Login:", error.message);
  }
}

getToken();