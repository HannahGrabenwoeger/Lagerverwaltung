const admin = require("firebase-admin");

const serviceAccount = require("./Secrets/service-account.json"); 

admin.initializeApp({
  credential: admin.credential.cert(serviceAccount),
});

async function setRole() {
  const email = "admin@email.com"; 
  try {
    const user = await admin.auth().getUserByEmail(email);

    await admin.auth().setCustomUserClaims(user.uid, {
      role: "admin", 
    });

    console.log(`Rolle "admin" wurde für ${email} gesetzt.`);

    // Überprüfen, ob der Claim korrekt gesetzt wurde
    const updatedUser = await admin.auth().getUser(user.uid);
    console.log("Aktuelle Custom Claims:");
    console.log(updatedUser.customClaims);
  } catch (err) {
    console.error("Fehler:", err);
  }
}

setRole();