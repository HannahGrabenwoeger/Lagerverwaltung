const manager = require("firebase-admin");

const serviceAccount = require("../backend/Secrets/service-account.json"); 

manager.initializeApp({
  credential: manager.credential.cert(serviceAccount),
});

async function setRole() {
  const email = "manager@email.com"; 
  try {
    const user = await manager.auth().getUserByEmail(email);

    await manager.auth().setCustomUserClaims(user.uid, {
      role: "manager", 
    });

    console.log(`Role "manager" was set for ${email}.`);

    const updatedUser = await manager.auth().getUser(user.uid);
    console.log("Current custom claims:");
    console.log(updatedUser.customClaims);
  } catch (err) {
    console.error("Error:", err);
  }
}

setRole();