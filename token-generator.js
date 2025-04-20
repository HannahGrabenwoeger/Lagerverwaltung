const admin = require('firebase-admin');
const { initializeApp } = require('firebase/app');
const { getAuth, signInWithEmailAndPassword } = require('firebase/auth');
const { getIdToken } = require('firebase/auth');

const serviceAccount = require('./Secrets/service-account.json');

admin.initializeApp
({
  credential: admin.credential.cert(serviceAccount),
});

const firebaseConfig = {
  apiKey: "AIzaSyDDktiumZEEgzAKdIwSJiN1nLLquHwCWtU",
  authDomain: "lagerverwaltung-backend.firebaseapp.com",
  projectId: "lagerverwaltung-backend",
};

const firebaseApp = initializeApp(firebaseConfig);
const auth = getAuth(firebaseApp);

async function getIdTokenFromUserLogin() {
  try {
    const userCred = await signInWithEmailAndPassword(auth, 'test@example.com', '123456');
    const idToken = await userCred.user.getIdToken();
    console.log('\nID Token (für Postman verwenden):\n');
    console.log(idToken);
  } catch (err) {
    console.error('Fehler beim Login:', err.message);
  }
}

getIdTokenFromUserLogin();