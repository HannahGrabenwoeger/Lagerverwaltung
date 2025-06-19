const admin = require("firebase-admin");

const serviceAccount = require("../backend/Secrets/service-account.json"); 

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

    const updatedUser = await admin.auth().getUser(user.uid);
    console.log("Aktuelle Custom Claims:");
    console.log(updatedUser.customClaims);
  } catch (err) {
    console.error("Fehler:", err);
  }
}

setRole();