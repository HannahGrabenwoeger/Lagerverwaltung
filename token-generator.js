const admin = require('firebase-admin');
const firebase = require('firebase/app');
require('firebase/auth');

// Firebase Admin initialisieren
const serviceAccount = require('./Secrets/service-account.json');

admin.initializeApp({
  credential: admin.credential.cert(serviceAccount),
});

// Firebase Client initialisieren (nicht Admin!)
const firebaseConfig = {
    apiKey: "AIzaSyDDktiumZEEgzAKdIwSJiN1nLLquHwCWtU",
    authDomain: "WarehouseManagement-backend.firebaseapp.com",
};

firebase.initializeApp(firebaseConfig);

async function generateAndExchangeToken() {
  try {
    const customToken = await admin.auth().createCustomToken('testUser123');
    console.log('✅ Custom Token:', customToken);

    const userCredential = await firebase.auth().signInWithCustomToken(customToken);
    const idToken = await userCredential.user.getIdToken();

    console.log('\n✅ ID Token (verwenden fürs Backend):', idToken);
  } catch (error) {
    console.error('❌ Fehler beim signInWithCustomToken:', error.message);
  }
}

generateAndExchangeToken();